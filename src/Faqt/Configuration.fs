namespace Faqt.Configuration

open System
open System.Collections.Generic
open System.Net.Http
open System.Text.RegularExpressions
open System.Threading


module private UrlMasking =


    let private masked = "***"


    let private secretParameters =
        HashSet(
            [
                "code"
                "key"
                "sig"
                "signature"
                "token"
                "access_token"
                "id_token"
                "refresh_token"
                "client_secret"
                "api_key"
                "apikey"
                "password"
                "subscription-key"
                "X-Amz-Signature"
                "X-Amz-Credential"
                "X-Amz-Security-Token"
                "X-Goog-Signature"
                "X-Goog-Credential"
            ],
            StringComparer.OrdinalIgnoreCase
        )


    let private maskParameter (parameter: string) =
        match parameter.IndexOf '=' with
        | i when i > 0 && i < parameter.Length - 1 ->
            let name = parameter.Substring(0, i)

            if secretParameters.Contains(Uri.UnescapeDataString name) then
                name + "=" + masked
            else
                parameter
        | _ -> parameter


    let private maskParameters (parameters: string) =
        parameters.Split('&') |> Array.map maskParameter |> String.concat "&"


    let private maskUserInfo (beforeQuery: string) =
        let authorityStart =
            if beforeQuery.StartsWith("//", StringComparison.Ordinal) then
                // Protocol-relative URL, such as in a Location header.
                2
            else
                match beforeQuery.IndexOf("://", StringComparison.Ordinal) with
                | -1 -> -1
                | schemeEnd -> schemeEnd + 3

        if authorityStart < 0 then
            beforeQuery
        else
            let authorityEnd =
                match beforeQuery.IndexOf('/', authorityStart) with
                | -1 -> beforeQuery.Length
                | i -> i

            match beforeQuery.LastIndexOf('@', authorityEnd - 1, authorityEnd - authorityStart) with
            | -1 -> beforeQuery
            | at ->
                match beforeQuery.IndexOf(':', authorityStart, at - authorityStart) with
                | -1 -> beforeQuery
                | colon -> beforeQuery.Substring(0, colon + 1) + masked + beforeQuery.Substring(at)


    /// Masks the password in the user info and the values of known secret query and fragment parameters.
    let mask (url: string) =
        let beforeFragment, fragment =
            match url.IndexOf '#' with
            | -1 -> url, None
            | i -> url.Substring(0, i), Some(url.Substring(i + 1))

        let beforeQuery, query =
            match beforeFragment.IndexOf '?' with
            | -1 -> beforeFragment, None
            | i -> beforeFragment.Substring(0, i), Some(beforeFragment.Substring(i + 1))

        maskUserInfo beforeQuery
        + (query |> Option.map (fun q -> "?" + maskParameters q) |> Option.defaultValue "")
        + (fragment
           |> Option.map (fun f -> "#" + maskParameters f)
           |> Option.defaultValue "")


module private HttpHeaderMasking =


    let private masked = "***"


    let private credentialsHeaders =
        HashSet([ "Authorization"; "Proxy-Authorization" ], StringComparer.OrdinalIgnoreCase)


    let private secretHeaders =
        HashSet(
            [
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


    /// Masks the value of a "name=value" cookie pair, keeping the name and any surrounding whitespace. Empty values,
    /// such as when deleting a cookie, are kept, since they are not secret.
    let private maskCookiePair (pair: string) =
        match pair.IndexOf '=' with
        | -1 when String.IsNullOrWhiteSpace pair -> pair
        | -1 -> pair.Substring(0, pair.Length - pair.TrimStart().Length) + masked
        | i when String.IsNullOrWhiteSpace(pair.Substring(i + 1)) -> pair
        | i -> pair.Substring(0, i + 1) + masked


    /// Masks one Set-Cookie cookie. Only the first pair is the cookie; the rest are attributes such as Path and
    /// HttpOnly, which are not secret.
    let private maskSetCookie (cookie: string) =
        match cookie.IndexOf ';' with
        | -1 -> maskCookiePair cookie
        | i -> maskCookiePair (cookie.Substring(0, i)) + cookie.Substring(i)


    /// Matches the separator between cookies when several are folded into one Set-Cookie value. A comma is only a
    /// separator when followed by "name=", so the comma in e.g. "Expires=Thu, 01 Jan 1970" is not.
    let private foldedSetCookieSeparator =
        Regex(@"(,\s*)(?=[^;=\s,]+=)", RegexOptions.CultureInvariant)


    let mask (name: string) (value: string) =
        // C# callers and F# callers without nullness checking can pass a null expected value or header name to header
        // assertions.
        if isNull (box value) then
            value
        elif String.Equals(name, "Cookie", StringComparison.OrdinalIgnoreCase) then
            value.Split(';') |> Array.map maskCookiePair |> String.concat ";"
        elif String.Equals(name, "Set-Cookie", StringComparison.OrdinalIgnoreCase) then
            // Split keeps the captured separators at odd indexes.
            foldedSetCookieSeparator.Split(value)
            |> Array.mapi (fun i part -> if i % 2 = 0 then maskSetCookie part else part)
            |> String.concat ""
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
    mapHttpUrls: string -> string
} with


    /// Returns the default configuration. Note that changes to this configuration is not considered a breaking change.
    /// The configuration is immutable; all instance methods return a new instance.
    static member Default = {
        httpContentMaxLength = 1024 * 1024
        formatHttpContent = true
        shouldPreviewHttpRequestContent = fun _ -> true
        shouldPreviewHttpResponseContent = fun _ -> true
        mapHttpHeaderValues = HttpHeaderMasking.mask
        mapHttpUrls = UrlMasking.mask
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
    /// returns the new value. This replaces the default, which masks credentials in common headers such as
    /// Authorization (except the scheme) and X-Api-Key, and cookie values (keeping names and attributes) in Cookie and
    /// Set-Cookie. To extend the default, call FaqtConfig.Default.MapHttpHeaderValues from your function. To show all
    /// values unchanged, use fun _ value -> value. For the Location, Content-Location and Referer headers, the function
    /// receives the value after MapHttpUrls has been applied.
    member this.SetMapHttpHeaderValues(mapHttpHeaderValues) =
        if isNull (box mapHttpHeaderValues) then
            nullArg (nameof mapHttpHeaderValues)

        {
            this with
                mapHttpHeaderValues = mapHttpHeaderValues
        }


    /// Gets the function used to format (e.g. mask) URLs in HTTP request lines and in the Location, Content-Location
    /// and Referer headers.
    member this.MapHttpUrls = this.mapHttpUrls


    /// Sets the function used to format (e.g. mask) URLs in HTTP request lines and in the Location, Content-Location
    /// and Referer headers. The function accepts the URL as rendered, which may be relative, and returns the new value.
    /// For headers, MapHttpHeaderValues is applied to the result. This replaces the default, which masks the password
    /// in the user info and the values of known secret query and fragment parameters, such as code, sig, access_token
    /// and X-Amz-Signature. To extend the default, call FaqtConfig.Default.MapHttpUrls from your function. To show URLs
    /// unchanged, use id.
    member this.SetMapHttpUrls(mapHttpUrls: string -> string) =
        if isNull (box mapHttpUrls) then
            nullArg (nameof mapHttpUrls)

        { this with mapHttpUrls = mapHttpUrls }


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
