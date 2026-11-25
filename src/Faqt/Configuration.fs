namespace Faqt.Configuration

open System
open System.Collections.Generic
open System.Net.Http
open System.Threading


module private HttpHeaderMasking =


    let private masked = "***"


    let private credentialsHeaders =
        HashSet([ "Authorization"; "Proxy-Authorization" ], StringComparer.OrdinalIgnoreCase)


    let private secretHeaders =
        HashSet(
            [
                "Cookie"
                "Set-Cookie"
                "X-Api-Key"
                "Api-Key"
                "X-Auth-Key"
                "X-Auth-Token"
                "X-Access-Token"
                "X-CSRF-Token"
                "X-XSRF-Token"
                "X-Amz-Security-Token"
                "X-Goog-Api-Key"
                "Ocp-Apim-Subscription-Key"
                "X-Functions-Key"
                "X-Vault-Token"
                "Private-Token"
                "X-Gitlab-Token"
            ],
            StringComparer.OrdinalIgnoreCase
        )


    /// Whether the text looks like an authentication scheme (e.g. "Bearer" or "AWS4-HMAC-SHA256") rather than part of a
    /// credential without a scheme.
    let private isScheme (s: string) =
        let isLetter c =
            (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')

        s.Length <= 20
        && isLetter s[0]
        && s |> Seq.forall (fun c -> isLetter c || (c >= '0' && c <= '9') || c = '-')


    let mask (name: string) (value: string) =
        // C# callers and F# callers without nullness checking can pass a null expected value to header assertions.
        if isNull (box value) then
            value
        elif credentialsHeaders.Contains name then
            // Keep the scheme, which helps debugging and is not secret.
            match value.IndexOf ' ' with
            | i when i > 0 && isScheme (value.Substring(0, i)) -> value.Substring(0, i + 1) + masked
            | _ -> masked
        elif secretHeaders.Contains name then
            masked
        else
            value


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
        mapHttpHeaderValues = HttpHeaderMasking.mask
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
    /// returns the new value. This replaces the default, which masks credentials and cookies in common headers such
    /// as Authorization (except the scheme), Cookie, Set-Cookie and X-Api-Key. To extend the default, call
    /// FaqtConfig.Default.MapHttpHeaderValues from your function. To show all values unchanged, use
    /// fun _ value -> value.
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
