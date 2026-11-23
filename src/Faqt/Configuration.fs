namespace Faqt.Configuration

open System
open System.Net.Http
open System.Threading


type FaqtConfig = private {
    httpContentMaxLength: int
    formatHttpContent: bool
    shouldPreviewHttpRequestContent: HttpRequestMessage -> bool
    shouldPreviewHttpResponseContent: HttpResponseMessage -> bool
    mapHttpHeaderValues: string -> string -> string
} with


    /// Returns the default configuration. Note that changes to this configuration is not considered a breaking change.
    /// The configuration is immutable; all instance methods return a new instance.
    static member Default = {
        httpContentMaxLength = 1024 * 1024
        formatHttpContent = true
        shouldPreviewHttpRequestContent = fun _ -> true
        shouldPreviewHttpResponseContent = fun _ -> true
        mapHttpHeaderValues = fun _ v -> v
    }


    /// Gets the HTTP content preview limit: bytes captured before decoding and characters rendered after formatting.
    /// Obtaining the stream may first buffer generated content in full; this is not a total memory limit.
    member this.HttpContentMaxLength = this.httpContentMaxLength


    /// Sets the HTTP content preview limit: bytes captured before decoding and characters rendered after formatting.
    /// Obtaining the stream may first buffer generated content in full; this is not a total memory limit.
    /// Zero omits the body without reading it. Stream previews may read one additional byte to detect truncation.
    /// Seekable stream positions are restored; nonseekable bodies may be consumed.
    member this.SetHttpContentMaxLength(length) =
        if length < 0 then
            invalidArg (nameof length) "The length must be non-negative"

        {
            this with
                httpContentMaxLength = length
        }


    /// Gets whether to attempt formatting HttpContent in assertion failure output.
    member this.FormatHttpContent = this.formatHttpContent


    /// Sets whether to attempt formatting HttpContent in assertion failure output.
    member this.SetFormatHttpContent(shouldFormat) = {
        this with
            formatHttpContent = shouldFormat
    }


    /// Gets the predicate that controls whether the default formatter may read an HTTP request body for a preview.
    member this.ShouldPreviewHttpRequestContent = this.shouldPreviewHttpRequestContent


    /// Sets the predicate that controls whether the default formatter may read an HTTP request body for a preview.
    /// Returning false or throwing omits the preview. Headers and other message details are still rendered.
    /// A zero preview limit skips the predicate. The predicate does not apply to separately asserted body values.
    member this.SetShouldPreviewHttpRequestContent(shouldPreview: HttpRequestMessage -> bool) =
        if isNull (box shouldPreview) then
            nullArg (nameof shouldPreview)

        {
            this with
                shouldPreviewHttpRequestContent = shouldPreview
        }


    /// Gets the predicate that controls whether the default formatter may read an HTTP response body for a preview.
    member this.ShouldPreviewHttpResponseContent = this.shouldPreviewHttpResponseContent


    /// Sets the predicate that controls whether the default formatter may read an HTTP response body for a preview.
    /// Returning false or throwing omits the preview. Headers and other message details are still rendered.
    /// A zero preview limit skips the predicate. The predicate does not apply to separately asserted body values.
    /// The response's RequestMessage property may be null.
    member this.SetShouldPreviewHttpResponseContent(shouldPreview: HttpResponseMessage -> bool) =
        if isNull (box shouldPreview) then
            nullArg (nameof shouldPreview)

        {
            this with
                shouldPreviewHttpResponseContent = shouldPreview
        }


    /// Gets the function used to format (e.g. mask) HTTP headers. The function accepts the header name and value, and
    /// returns the new value.
    member this.MapHttpHeaderValues = this.mapHttpHeaderValues


    /// Sets the function used to format (e.g. mask) HTTP headers. The function accepts the header name and value, and
    /// returns the new value.
    member this.SetMapHttpHeaderValues(mapHttpHeaderValues) =
        if isNull (box mapHttpHeaderValues) then
            nullArg (nameof mapHttpHeaderValues)

        {
            this with
                mapHttpHeaderValues = mapHttpHeaderValues
        }


/// Allows changing the current formatter, either temporarily (for the current thread) or globally.
type Config private () =


    static let mutable globalConfig: FaqtConfig = FaqtConfig.Default


    static let localConfig: AsyncLocal<FaqtConfig | null> = AsyncLocal()


    static member Current =
        match localConfig.Value with
        | null -> globalConfig
        | config -> config


    /// Sets the specified config as the default global config.
    static member Set(config) =
        if isNull (box config) then
            nullArg (nameof config)

        globalConfig <- config


    /// Sets the specified config as the config for the current thread. When the returned value is disposed, the old
    /// config is restored.
    static member With(config) =
        if isNull (box config) then
            nullArg (nameof config)

        let oldLocalConfig = localConfig.Value
        localConfig.Value <- config

        { new IDisposable with
            member _.Dispose() = localConfig.Value <- oldLocalConfig
        }
