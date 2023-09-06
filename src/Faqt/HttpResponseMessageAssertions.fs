namespace Faqt

open System.Net
open System.Net.Http
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type HttpResponseMessageAssertions =


    /// Asserts that the response has the specified status code.
    [<Extension>]
    static member HaveStatusCode
        (
            t: Testable<HttpResponseMessage>,
            statusCode: HttpStatusCode,
            ?because
        ) : And<HttpResponseMessage> =
        use _ = t.Assert()

        if isNull t.Subject then
            nullArg "subject"

        if t.Subject.StatusCode <> statusCode then
            t
                .With("Expected", statusCode)
                .With("But was", t.Subject.StatusCode)
                .With("Response", t.Subject)
                .With("Request", t.Subject.RequestMessage)
                .Fail(because)

        And(t)


    [<Extension>]
    static member private BeStatusCode
        (
            t: Testable<HttpResponseMessage>,
            lower: HttpStatusCode,
            upper: HttpStatusCode,
            ?because
        ) : And<HttpResponseMessage> =
        if isNull t.Subject then
            nullArg "subject"

        if t.Subject.StatusCode < lower || t.Subject.StatusCode > upper then
            t
                .With("But was", t.Subject.StatusCode)
                .With("Response", t.Subject)
                .With("Request", t.Subject.RequestMessage)
                .Fail(because)

        And(t)


    [<Extension>]
    static member private BeStatusCode
        (
            t: Testable<HttpResponseMessage>,
            statusCode: HttpStatusCode,
            ?because
        ) : And<HttpResponseMessage> =
        t.BeStatusCode(statusCode, statusCode, ?because = because)


    /// Asserts that the response has a status code between 100 and 199.
    [<Extension>]
    static member Be1XXInformational(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(enum 100, enum 199, ?because = because)


    /// Asserts that the response has a status code between 200 and 299.
    [<Extension>]
    static member Be2XXSuccessful(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(enum 200, enum 299, ?because = because)


    /// Asserts that the response has a status code between 300 and 399.
    [<Extension>]
    static member Be3XXRedirection(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(enum 300, enum 399, ?because = because)


    /// Asserts that the response has a status code between 400 and 499.
    [<Extension>]
    static member Be4XXClientError(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(enum 400, enum 499, ?because = because)


    /// Asserts that the response has a status code between 500 and 599.
    [<Extension>]
    static member Be5XXServerError(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(enum 500, enum 599, ?because = because)


    /// Asserts that the response has status code 100 Continue.
    [<Extension>]
    static member Be100Continue(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Continue, ?because = because)


    /// Asserts that the response has status code 101 Switching Protocols.
    [<Extension>]
    static member Be101SwitchingProtocols(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.SwitchingProtocols, ?because = because)


    /// Asserts that the response has status code 200 OK.
    [<Extension>]
    static member Be200Ok(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.OK, ?because = because)


    /// Asserts that the response has status code 201 Created.
    [<Extension>]
    static member Be201Created(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Created, ?because = because)


    /// Asserts that the response has status code 202 Accepted.
    [<Extension>]
    static member Be202Accepted(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Accepted, ?because = because)


    /// Asserts that the response has status code 203 Non-Authoritative Information.
    [<Extension>]
    static member Be203NonAuthoritativeInformation
        (
            t: Testable<HttpResponseMessage>,
            ?because
        ) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NonAuthoritativeInformation, ?because = because)


    /// Asserts that the response has status code 204 No Content.
    [<Extension>]
    static member Be204NoContent(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NoContent, ?because = because)


    /// Asserts that the response has status code 205 Reset Content.
    [<Extension>]
    static member Be205ResetContent(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.ResetContent, ?because = because)


    /// Asserts that the response has status code 206 Partial Content.
    [<Extension>]
    static member Be206PartialContent(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.PartialContent, ?because = because)


    /// Asserts that the response has status code 300 Multiple Choices.
    [<Extension>]
    static member Be300MultipleChoices(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.MultipleChoices, ?because = because)


    /// Asserts that the response has status code 301 Moved Permanently.
    [<Extension>]
    static member Be301MovedPermanently(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.MovedPermanently, ?because = because)


    /// Asserts that the response has status code 302 Found.
    [<Extension>]
    static member Be302Found(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Found, ?because = because)


    /// Asserts that the response has status code 303 See Other.
    [<Extension>]
    static member Be303SeeOther(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.SeeOther, ?because = because)


    /// Asserts that the response has status code 304 Not Modified.
    [<Extension>]
    static member Be304NotModified(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NotModified, ?because = because)


    /// Asserts that the response has status code 307 Temporary Redirect.
    [<Extension>]
    static member Be307TemporaryRedirect(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.TemporaryRedirect, ?because = because)


    /// Asserts that the response has status code 308 Permanent Redirect.
    [<Extension>]
    static member Be308PermanentRedirect(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.PermanentRedirect, ?because = because)


    /// Asserts that the response has status code 400 Bad Request.
    [<Extension>]
    static member Be400BadRequest(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.BadRequest, ?because = because)


    /// Asserts that the response has status code 401 Unauthorized.
    [<Extension>]
    static member Be401Unauthorized(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Unauthorized, ?because = because)


    /// Asserts that the response has status code 403 Forbidden.
    [<Extension>]
    static member Be403Forbidden(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Forbidden, ?because = because)


    /// Asserts that the response has status code 404 Not Found.
    [<Extension>]
    static member Be404NotFound(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NotFound, ?because = because)


    /// Asserts that the response has status code 405 Method Not Allowed.
    [<Extension>]
    static member Be405MethodNotAllowed(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.MethodNotAllowed, ?because = because)


    /// Asserts that the response has status code 406 Not Acceptable.
    [<Extension>]
    static member Be406NotAcceptable(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NotAcceptable, ?because = because)


    /// Asserts that the response has status code 407 Proxy Authentication Required.
    [<Extension>]
    static member Be407ProxyAuthenticationRequired
        (
            t: Testable<HttpResponseMessage>,
            ?because
        ) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.ProxyAuthenticationRequired, ?because = because)


    /// Asserts that the response has status code 408 Request Timeout.
    [<Extension>]
    static member Be408RequestTimeout(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.RequestTimeout, ?because = because)


    /// Asserts that the response has status code 409 Conflict.
    [<Extension>]
    static member Be409Conflict(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Conflict, ?because = because)


    /// Asserts that the response has status code 410 Gone.
    [<Extension>]
    static member Be410Gone(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.Gone, ?because = because)


    /// Asserts that the response has status code 411 Length Required.
    [<Extension>]
    static member Be411LengthRequired(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.LengthRequired, ?because = because)


    /// Asserts that the response has status code 412 Precondition Failed.
    [<Extension>]
    static member Be412PreconditionFailed(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.PreconditionFailed, ?because = because)


    /// Asserts that the response has status code 413 Content Too Large.
    [<Extension>]
    static member Be413ContentTooLarge(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.RequestEntityTooLarge, ?because = because)


    /// Asserts that the response has status code 414 URI Too Long.
    [<Extension>]
    static member Be414UriTooLong(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.RequestUriTooLong, ?because = because)


    /// Asserts that the response has status code 415 Unsupported Media Type.
    [<Extension>]
    static member Be415UnsupportedMediaType(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.UnsupportedMediaType, ?because = because)


    /// Asserts that the response has status code 416 Range Not Satisfiable.
    [<Extension>]
    static member Be416RangeNotSatisfiable(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.RequestedRangeNotSatisfiable, ?because = because)


    /// Asserts that the response has status code 417 Expectation Failed.
    [<Extension>]
    static member Be417ExpectationFailed(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.ExpectationFailed, ?because = because)


    /// Asserts that the response has status code 421 Misdirected Request.
    [<Extension>]
    static member Be421MisdirectedRequest(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.MisdirectedRequest, ?because = because)


    /// Asserts that the response has status code 426 Upgrade Required.
    [<Extension>]
    static member Be426UpgradeRequired(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.UpgradeRequired, ?because = because)


    /// Asserts that the response has status code 428 Precondition Required.
    [<Extension>]
    static member Be428PreconditionRequired(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.PreconditionRequired, ?because = because)


    /// Asserts that the response has status code 429 Too Many Requests.
    [<Extension>]
    static member Be429TooManyRequests(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.TooManyRequests, ?because = because)


    /// Asserts that the response has status code 431 Request Header Fields Too Large.
    [<Extension>]
    static member Be431RequestHeaderFieldsTooLarge
        (
            t: Testable<HttpResponseMessage>,
            ?because
        ) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.RequestHeaderFieldsTooLarge, ?because = because)


    /// Asserts that the response has status code 451 Unavailable For Legal Reasons.
    [<Extension>]
    static member Be451UnavailableForLegalReasons
        (
            t: Testable<HttpResponseMessage>,
            ?because
        ) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.UnavailableForLegalReasons, ?because = because)


    /// Asserts that the response has status code 500 Internal Server Error.
    [<Extension>]
    static member Be500InternalServerError(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.InternalServerError, ?because = because)


    /// Asserts that the response has status code 501 Not Implemented.
    [<Extension>]
    static member Be501NotImplemented(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NotImplemented, ?because = because)


    /// Asserts that the response has status code 502 Bad Gateway.
    [<Extension>]
    static member Be502BadGateway(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.BadGateway, ?because = because)


    /// Asserts that the response has status code 503 Service Unavailable.
    [<Extension>]
    static member Be503ServiceUnavailable(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.ServiceUnavailable, ?because = because)


    /// Asserts that the response has status code 504 Gateway Timeout.
    [<Extension>]
    static member Be504GatewayTimeout(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.GatewayTimeout, ?because = because)


    /// Asserts that the response has status code 505 HTTP Version Not Supported.
    [<Extension>]
    static member Be505HttpVersionNotSupported(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.HttpVersionNotSupported, ?because = because)


    /// Asserts that the response has status code 506 Variant Also Negotiates.
    [<Extension>]
    static member Be506VariantAlsoNegotiates(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.VariantAlsoNegotiates, ?because = because)


    /// Asserts that the response has status code 510 Not Extended.
    [<Extension>]
    static member Be510NotExtended(t: Testable<HttpResponseMessage>, ?because) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NotExtended, ?because = because)


    /// Asserts that the response has status code 511 Network Authentication Required.
    [<Extension>]
    static member Be511NetworkAuthenticationRequired
        (
            t: Testable<HttpResponseMessage>,
            ?because
        ) : And<HttpResponseMessage> =
        use _ = t.Assert()
        t.BeStatusCode(HttpStatusCode.NetworkAuthenticationRequired, ?because = because)
