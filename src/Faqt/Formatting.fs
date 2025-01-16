﻿namespace Faqt.Formatting

open System
open System.Globalization
open System.IO
open System.Net
open System.Net.Http
open System.Text
open System.Text.Encodings.Web
open System.Text.Json
open System.Text.Json.Serialization
open System.Text.RegularExpressions
open System.Threading
open YamlDotNet.Core
open YamlDotNet.Core.Events
open YamlDotNet.RepresentationModel
open Faqt
open Faqt.Configuration


/// Wrap values in TryFormat to catch serialization exceptions and use a safe fallback serialization format for the
/// wrapped value. Not needed for top-level values added in With(...). Useful e.g. when wrapping values in records or
/// when serializing individual items in a sequence.
[<Struct>]
type TryFormat =
    /// Wrap values in TryFormat to catch serialization exceptions and use a safe fallback serialization format for the
    /// wrapped value. Not needed for top-level values added in With(...). Useful e.g. when wrapping values in records
    /// or when serializing individual items in a sequence.
    | TryFormat of (obj | null)


/// Contains the data used to render an assertion failure.
type FailureData = {

    /// The subject names. Contains the original subject name and any transformations.
    Subject: string list

    /// A user-supplied message.
    Because: string option

    /// The name of the failed assertion method.
    Should: string

    /// Any additional key-value pairs to add to the assertion failure message.
    Extra: (string * obj) list
}


module internal HttpContent =


    let private tryFormatJson (str: string) =
        try
            let serializerOptions =
                JsonSerializerOptions(WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping)

            let formatter =
                FracturedJson.Formatter(Options = FracturedJson.FracturedJsonOptions(OmitTrailingWhitespace = true))

            let doc = JsonDocument.Parse(str)
            formatter.Serialize(doc, 0, serializerOptions).Trim() |> Some
        with _ ->
            None


    let private tryFormat str =
        [ tryFormatJson ]
        |> List.tryPick (fun f -> f str)
        |> Option.map (sprintf "[content has been formatted]\n%s")
        |> Option.defaultValue str


    let serializeAppend formatContent maxLength (sb: StringBuilder) (c: HttpContent) =
        try
            if not (isNull c) && c.Headers.ContentLength <> Nullable(0L) then
                for h in c.Headers do
                    for v in h.Value do
                        sb.AppendLine().Append(h.Key).Append(": ").Append(v) |> ignore

                let s = c.ReadAsStream()
                let a = Array.zeroCreate (int s.Length)
                s.Seek(0, SeekOrigin.Begin) |> ignore
                s.Read(Span(a)) |> ignore

                let strContent =
                    Encoding.UTF8.GetString(a)
                    |> if formatContent then tryFormat else id
                    |> String.truncate $"…\n[content truncated after %i{maxLength} characters]" maxLength

                sb.AppendLine().AppendLine().Append(strContent) |> ignore
        with
        | :? ObjectDisposedException ->
            sb.AppendLine().AppendLine().Append("[content is disposed and cannot be read]")
            |> ignore
        | ex ->
            sb
                .AppendLine()
                .AppendLine()
                .AppendLine("An exception occured trying to get the content:")
                .Append(ex.ToString())
            |> ignore


module internal HttpRequestMessage =


    let serialize formatContent maxLength (mapHeaderValues: string -> string -> string) (m: HttpRequestMessage) =
        let sb = StringBuilder()

        sb
            .Append(m.Method.ToString())
            .Append(" ")
            .Append(m.RequestUri.ToString())
            .Append(" HTTP/")
            .Append(m.Version.ToString())
        |> ignore

        for h in m.Headers do
            for v in h.Value do
                sb.AppendLine().Append(h.Key).Append(": ").Append(mapHeaderValues h.Key v)
                |> ignore

        HttpContent.serializeAppend formatContent maxLength sb m.Content

        sb.ToString()


module internal HttpResponseMessage =


    let serialize formatContent maxLength (mapHeaderValues: string -> string -> string) (m: HttpResponseMessage) =
        let sb = StringBuilder()

        sb
            .Append("HTTP/")
            .Append(m.Version.ToString())
            .Append(" ")
            .Append(int m.StatusCode)
            .Append(" ")
            .Append(m.ReasonPhrase)
        |> ignore

        for h in m.Headers do
            for v in h.Value do
                sb.AppendLine().Append(h.Key).Append(": ").Append(mapHeaderValues h.Key v)
                |> ignore

        HttpContent.serializeAppend formatContent maxLength sb m.Content

        sb.ToString()


type private MappedValueConverter<'a, 'b>(mapping: 'a -> 'b, includeSubtypes) =
    inherit JsonConverter<'a>()

    override this.CanConvert(t: Type) =
        if includeSubtypes then
            typeof<'a>.IsAssignableFrom(t)
        else
            base.CanConvert(t)

    override this.Read(_, _, _) =
        raise <| NotSupportedException("Can only write")

    override this.Write(writer, value, options) =
        JsonSerializer.Serialize(writer, mapping value, options)


type private FailureDataConverter() =
    inherit JsonConverter<FailureData>()

    override this.Read(_, _, _) =
        raise <| NotSupportedException("Can only write")

    override this.Write(writer, value, options) =
        writer.WriteStartObject()
        writer.WritePropertyName("Subject")

        if value.Subject.Length > 1 then
            writer.WriteStartArray()

        for x in value.Subject do
            writer.WriteStringValue(x)

        if value.Subject.Length > 1 then
            writer.WriteEndArray()

        match value.Because with
        | None
        | Some "" -> ()
        | Some bc ->
            writer.WritePropertyName("Because")
            writer.WriteStringValue(bc)

        writer.WritePropertyName("Should")
        writer.WriteStringValue(value.Should)

        for k, v in value.Extra do
            writer.WritePropertyName(k)
            JsonSerializer.Serialize(writer, TryFormat v, options)

        writer.WriteEndObject()


type private TryFormatConverter(fallback: exn -> obj -> obj) =
    inherit JsonConverter<TryFormat>()

    override this.Read(_, _, _) =
        raise <| NotSupportedException("Can only write")

    override this.Write(writer, TryFormat value, options) =
        try
            let t = if isNull value then typeof<obj> else value.GetType()
            // Serialize once without the writer first to ensure it can be serialized, then use the writer. This
            // is needed because if we use the writer and it fails, we cannot continue writing.
            JsonSerializer.Serialize(value, t, options) |> ignore<string>
            JsonSerializer.Serialize(writer, value, t, options)
        with ex ->
            let fallback = fallback ex value
            let t = if isNull fallback then typeof<obj> else fallback.GetType()
            JsonSerializer.Serialize(writer, fallback, t, options)

    override this.WriteAsPropertyName(writer, TryFormat value, options) =
        let str = JsonSerializer.Serialize(value, value.GetType(), options)

        let str =
            if str.StartsWith('"') && str.EndsWith('"') then
                str.Substring(1, str.Length - 2)
            else
                str

        writer.WritePropertyName(str)


type private NoOpYamlVisitor() =
    inherit YamlVisitorBase()


type private JsonToYamlConverterVisitor(doc: YamlDocument) =
    inherit YamlVisitorBase()

    let isNumeric (x: string) =
        // https://stackoverflow.com/a/6425559/2978652
        Regex.IsMatch(x, @"^-?(0|[1-9][0-9]*)(\.[0-9]+)?([eE][+-]?[0-9]+)?$", RegexOptions.Compiled)

    override _.Visit(scalar: YamlScalarNode) =
        // Since we transform from JSON, DoubleQuoted are JSON strings (and keys), as opposed to numbers/null
        if scalar.Style = ScalarStyle.DoubleQuoted then
            if scalar.Value.Contains("\n") then
                scalar.Style <- ScalarStyle.Literal
            elif
                scalar.Value = "null"
                || scalar.Value = "true"
                || scalar.Value = "false"
                || isNumeric scalar.Value
            then
                scalar.Style <- ScalarStyle.SingleQuoted
            else
                scalar.Style <- ScalarStyle.Plain

        base.Visit(scalar)

    override _.Visit(sequence: YamlSequenceNode) =
        let hasOnlyScalarChildren =
            sequence.Children |> Seq.forall (fun x -> x.NodeType = YamlNodeType.Scalar)

        let isValueOfTopLevelSubjectNode =
            match doc.RootNode with
            | :? YamlMappingNode as n ->
                n
                |> Seq.exists (fun kvp -> kvp.Key.ToString() = "Subject" && kvp.Value = sequence)
            | _ -> false

        if hasOnlyScalarChildren && not isValueOfTopLevelSubjectNode then
            sequence.Style <- SequenceStyle.Flow
        else
            sequence.Style <- SequenceStyle.Block

        base.Visit(sequence)

    override _.Visit(mapping: YamlMappingNode) =
        mapping.Style <- MappingStyle.Block
        base.Visit(mapping)


[<AutoOpen>]
module private FormattingHelpers =


    let formatAsYaml getYamlVisitor (json: string) =
        let yaml = YamlStream()
        yaml.Load(new StringReader(json))
        yaml.Accept(getYamlVisitor yaml.Documents[0])
        let outputYaml = new StringWriter()
        yaml.Save(outputYaml, false)
        outputYaml.Flush()
        outputYaml.ToString().Trim().Trim('.', '-').Trim()


    let getFormatter configureOptions formatAsYaml =
        let options = JsonSerializerOptions()
        configureOptions options
        fun data -> JsonSerializer.Serialize(data, options) |> formatAsYaml


/// A type to help configure the default YAML-based assertion message format.
type YamlFormatterBuilder = private {
    configureJsonSerializerOptions: YamlFormatterBuilder -> JsonSerializerOptions -> unit
    getJsonFSharpOptions: unit -> JsonFSharpOptions
    tryFormatFallback: exn -> obj -> obj
    getYamlVisitor: YamlDocument -> YamlVisitorBase
} with


    member private this.ConfigureJsonSerializerOptions'
        (configure: YamlFormatterBuilder -> JsonSerializerOptions -> unit)
        =
        {
            this with
                configureJsonSerializerOptions =
                    fun builder opts ->
                        this.configureJsonSerializerOptions builder opts
                        configure builder opts
        }


    /// Adds a function to configure JsonSerializerOptions. Multiple calls are allowed and will be run in order.
    member this.ConfigureJsonSerializerOptions(configure: JsonSerializerOptions -> unit) =
        this.ConfigureJsonSerializerOptions'(fun _ -> configure)


    /// Adds a function to configure JsonFSharpOptions. Multiple calls are allowed and will be run in order.
    member this.ConfigureJsonFSharpOptions(configure: JsonFSharpOptions -> JsonFSharpOptions) = {
        this with
            getJsonFSharpOptions = this.getJsonFSharpOptions >> configure
    }


    member private this.AddConverter'(getConverter: YamlFormatterBuilder -> #JsonConverter) =
        this.ConfigureJsonSerializerOptions'(fun builder c -> c.Converters.Insert(0, getConverter builder))


    /// Adds the specified JSON converter. If multiple converters return true for CanConvert, the last one added takes
    /// precedence.
    member this.AddConverter(converter: #JsonConverter) = this.AddConverter'(fun _ -> converter)


    /// Adds a converter that serializes a transformed value instead of the original value. Only the last added
    /// converter for any given input type will take effect. Subtypes of the input type are included.
    member this.SerializeAs(projection: 'a -> 'b) =
        if typeof<'a> = typeof<'b> then
            invalidArg
                (nameof projection)
                "The projected type must be different from the input type, or a stack overflow would occur"

        this.AddConverter(MappedValueConverter(projection, true))


    /// Adds a converter that serializes a transformed value instead of the original value. Only the last added
    /// converter for any given input type will take effect. Subtypes of the input type are not included.
    member this.SerializeExactAs(projection: 'a -> 'b) =
        if typeof<'a> = typeof<'b> then
            invalidArg
                (nameof projection)
                "The projected type must be different from the input type, or a stack overflow would occur"

        this.AddConverter(MappedValueConverter(projection, false))


    /// Sets the transformation (e.g. ToString) that is used when values wrapped in TryFormat fail serialization.
    /// Only the last call to this method will take effect.
    member this.TryFormatFallback(projection: exn -> obj -> obj) = {
        this with
            tryFormatFallback = projection
    }


    /// Specifies which YAML visitor is used when converting JSON to YAML. Only the last call to this method will take
    /// effect.
    member this.SetYamlVisitor(getYamlVisitor: YamlDocument -> YamlVisitorBase) = {
        this with
            getYamlVisitor = getYamlVisitor
    }


    /// Returns a formatter according to the current configuration.
    member this.Build() =
        let configure = this.configureJsonSerializerOptions this
        getFormatter configure (formatAsYaml this.getYamlVisitor)


    /// Returns a minimal builder. Note that changes to this format is not considered a breaking change. The builder is
    /// immutable; all instance methods return a new instance.
    static member Empty = {
        configureJsonSerializerOptions = fun _ _ -> ()
        getJsonFSharpOptions = fun _ -> JsonFSharpOptions.Default()
        tryFormatFallback =
            fun ex obj -> {|
                ``SERIALIZATION EXCEPTION`` = ex
                ToString = obj.ToString()
            |}
        getYamlVisitor = fun _ -> NoOpYamlVisitor()
    }


    /// Returns the default builder. Note that changes to this format is not considered a breaking change. The builder
    /// is immutable; all instance methods return a new instance.
    static member Default =
        YamlFormatterBuilder.Empty
            .ConfigureJsonFSharpOptions(fun opts ->
                opts
                    .WithUnionExternalTag()
                    .WithUnionNamedFields()
                    .WithUnionFieldNamesFromTypes()
                    .WithUnionUnwrapFieldlessTags()
                    .WithUnionUnwrapSingleFieldCases()
                    .WithUnwrapOption(false)
#if NET8_0_OR_GREATER
                    .WithMapFormat(MapFormat.Object)
#endif
            )
            .AddConverter'(fun builder -> builder.getJsonFSharpOptions () |> JsonFSharpConverter)
            .AddConverter'(fun builder -> TryFormatConverter(builder.tryFormatFallback))
            .AddConverter(FailureDataConverter())
            .AddConverter(JsonStringEnumConverter())
            .SerializeAs(string<Exception>)
            .SerializeAs(fun (t: Type) -> t.AssertionName)
            .SerializeAs(fun (ci: CultureInfo) -> if ci.Name = "" then "invariant" else ci.Name)
            .SerializeAs(fun x ->
                HttpRequestMessage.serialize
                    Config.Current.FormatHttpContent
                    Config.Current.HttpContentMaxLength
                    Config.Current.MapHttpHeaderValues
                    x
            )
            .SerializeAs(fun x ->
                HttpResponseMessage.serialize
                    Config.Current.FormatHttpContent
                    Config.Current.HttpContentMaxLength
                    Config.Current.MapHttpHeaderValues
                    x
            )
            .SerializeAs(fun (c: HttpStatusCode) ->
                let reasonPhrase =
                    match (new HttpResponseMessage(c)).ReasonPhrase with
                    | null
                    | "" -> string c
                    | s -> s

                if reasonPhrase = string (int c) then
                    string (int c)
                else
                    $"%i{int c} %s{reasonPhrase}"
            )
            .SerializeAs(fun (xs: seq<byte>) -> Convert.ToHexString(Seq.toArray xs))
            .SerializeAs(fun (xs: byte[]) -> Convert.ToHexString(xs))
            .SetYamlVisitor(fun doc -> JsonToYamlConverterVisitor(doc))


/// Allows changing the current formatter, either temporarily (for the current thread) or globally.
type Formatter private () =


    static let mutable globalFormatter: FailureData -> string =
        YamlFormatterBuilder.Default.Build()


    static let localFormatter: AsyncLocal<FailureData -> string> = AsyncLocal()


    static member internal Current =
        if isNull (box localFormatter.Value) then
            globalFormatter
        else
            localFormatter.Value


    /// Sets the specified formatter as the default global formatter.
    static member Set(format) = globalFormatter <- format


    /// Sets the specified formatter as the formatter for the current thread. When the returned value is disposed, the
    /// old formatter is restored.
    static member With(format) =
        let oldLocalFormatter = localFormatter.Value
        localFormatter.Value <- format

        { new IDisposable with
            member _.Dispose() =
                localFormatter.Value <- oldLocalFormatter
        }
