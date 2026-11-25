module Configuration

open System
open System.IO
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open Faqt
open Faqt.Configuration
open Xunit


[<Fact>]
let ``Can override and restore the default config`` () =

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Content <- new StringContent("lorem ipsum dolor sit amet")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsum dolor sit amet
"""

    do
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(10))

        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Get, "/")
            x.Version <- Version.Parse("0.5")
            x.Content <- new StringContent("lorem ipsum dolor sit amet")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsu…
  [content truncated after 10 bytes]
"""

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Content <- new StringContent("lorem ipsum dolor sit amet")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsum dolor sit amet
"""


[<Fact>]
let ``Can override and restore the default formatter in async code`` () =
    async {

        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Get, "/")
            x.Version <- Version.Parse("0.5")
            x.Content <- new StringContent("lorem ipsum dolor sit amet")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsum dolor sit amet
"""

        do!
            async {
                use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(10))
                do! Async.SwitchToNewThread()

                fun () ->
                    let x = new HttpRequestMessage(HttpMethod.Get, "/")
                    x.Version <- Version.Parse("0.5")
                    x.Content <- new StringContent("lorem ipsum dolor sit amet")
                    x.Should().FailWith("Value", x)
                |> assertExnMsg
                    """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsu…
  [content truncated after 10 bytes]
"""
            }

        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Get, "/")
            x.Version <- Version.Parse("0.5")
            x.Content <- new StringContent("lorem ipsum dolor sit amet")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsum dolor sit amet
"""
    }


[<Fact>]
let ``HTTP content formatting is on by default`` () =
    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        let content = new StringContent("{\"a\":1}")
        content.Headers.ContentType <- MediaTypeHeaderValue("application/json")
        x.Content <- content
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: application/json
  Content-Length: 7

  [content has been formatted]
  {"a": 1}
"""


[<Fact>]
let ``HTTP content formatting can be turned off`` () =
    use _ = Config.With(FaqtConfig.Default.SetFormatHttpContent(false))

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        let content = new StringContent("{\"a\":1}")
        content.Headers.ContentType <- MediaTypeHeaderValue("application/json")
        x.Content <- content
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: application/json
  Content-Length: 7

  {"a":1}
"""


[<Fact>]
let ``HTTP content has expected max length`` () =
    let expectedMaxLength = 1024 * 1024
    let bodyLength = expectedMaxLength * 2
    let body = String.replicate bodyLength "a"

    let expectedBody =
        String.replicate expectedMaxLength "a"
        + $"…\n  [content truncated after {expectedMaxLength} bytes]"

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Content <- new StringContent(body)
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        $"""
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: %i{bodyLength}

  %s{expectedBody}
"""


[<Fact>]
let ``HTTP content max length can be adjusted`` () =
    use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(10))

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Content <- new StringContent("lorem ipsum dolor sit amet")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsu…
  [content truncated after 10 bytes]
"""


[<Fact>]
let ``Setting negative HTTP content max length throws ArgumentException`` () =
    Assert.Throws<ArgumentException>(fun () -> FaqtConfig.Default.SetHttpContentMaxLength(-1) |> ignore)


[<Fact>]
let ``Setting null HTTP header mapper throws ArgumentNullException`` () =
    Assert.Throws<ArgumentNullException>(fun () ->
        FaqtConfig.Default.SetMapHttpHeaderValues(Unchecked.defaultof<string -> string -> string>)
        |> ignore
    )
    |> ignore


[<Fact>]
let ``Setting a null request or response preview predicate throws ArgumentNullException`` () =
    Assert.Throws<ArgumentNullException>(fun () ->
        FaqtConfig.Default.SetShouldPreviewHttpRequestContent(Unchecked.defaultof<HttpRequestMessage -> bool>)
        |> ignore
    )
    |> ignore

    Assert.Throws<ArgumentNullException>(fun () ->
        FaqtConfig.Default.SetShouldPreviewHttpResponseContent(Unchecked.defaultof<HttpResponseMessage -> bool>)
        |> ignore
    )
    |> ignore


[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``HTTP failures honor the body preview predicate for requests and responses`` nested predicateThrows =
    let mutable reads = 0

    let body text =
        let stream =
            { new MemoryStream(Encoding.UTF8.GetBytes(text: string)) with
                override this.Read(buffer: byte[], offset, count) =
                    reads <- reads + 1
                    base.Read(buffer, offset, count)
            }

        let content = new StreamContent(stream)
        content.Headers.ContentType <- MediaTypeHeaderValue("text/plain")
        content

    use request = new HttpRequestMessage(HttpMethod.Post, "/sensitive?sample=visible")
    request.Content <- body "sample request secret"
    use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    response.RequestMessage <- request
    response.Content <- body "sample response secret"
    use unrelatedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
    unrelatedResponse.Content <- new StringContent("ordinary response")

    let shouldPreview (relatedRequest: HttpRequestMessage | null) =
        if Object.ReferenceEquals(request, relatedRequest) then
            if predicateThrows then
                failwith "sample predicate secret"

            false
        else
            true

    use _ =
        Config.With(
            FaqtConfig.Default
                .SetShouldPreviewHttpRequestContent(fun req -> shouldPreview req)
                .SetShouldPreviewHttpResponseContent(fun res -> shouldPreview res.RequestMessage)
        )

    let error =
        assertFails (fun () ->
            if nested then
                response
                    .Should()
                    .SatisfyAll(
                        [
                            fun r -> r.Should().HaveStatusCode(HttpStatusCode.OK) |> ignore
                        ]
                    )
                |> ignore
            else
                response.Should().HaveStatusCode(HttpStatusCode.OK) |> ignore
        )

    let marker =
        if predicateThrows then
            "[content omitted: preview predicate failed with System.Exception]"
        else
            "[content omitted by configuration]"

    // SatisfyAll also renders the outer response as its subject value.
    let expectedPreviews = if nested then 3 else 2
    Assert.Equal(expectedPreviews, error.Message.Split(marker).Length - 1)
    Assert.DoesNotContain("sample request secret", error.Message)
    Assert.DoesNotContain("sample response secret", error.Message)
    Assert.DoesNotContain("sample predicate secret", error.Message)
    Assert.Contains("POST /sensitive?sample=visible", error.Message)
    Assert.Contains("400 Bad Request", error.Message)
    Assert.Contains("Content-Type: text/plain", error.Message)
    Assert.Equal(0, reads)

    let ordinaryFailure =
        assertFails (fun () -> unrelatedResponse.Should().HaveStatusCode(HttpStatusCode.OK))

    Assert.Contains("ordinary response", ordinaryFailure.Message)


[<Theory>]
[<InlineData(true)>]
[<InlineData(false)>]
let ``Request and response previews can be configured independently`` suppressRequest =
    use request = new HttpRequestMessage(HttpMethod.Post, "/token")
    request.Content <- new StringContent("sample request body")
    use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    response.RequestMessage <- request
    response.Content <- new StringContent("sample response body")

    let config =
        if suppressRequest then
            FaqtConfig.Default.SetShouldPreviewHttpRequestContent(fun _ -> false)
        else
            FaqtConfig.Default.SetShouldPreviewHttpResponseContent(fun res ->
                match res.RequestMessage with
                | null -> true
                | req -> req.Method <> HttpMethod.Post
            )

    use _ = Config.With(config)

    let error =
        assertFails (fun () -> response.Should().HaveStatusCode(HttpStatusCode.OK))

    if suppressRequest then
        Assert.DoesNotContain("sample request body", error.Message)
        Assert.Contains("sample response body", error.Message)
    else
        Assert.Contains("sample request body", error.Message)
        Assert.DoesNotContain("sample response body", error.Message)


[<Theory>]
[<InlineData(0, false, "[content omitted: preview limit is 0]")>]
[<InlineData(10, true, "[content omitted by configuration]")>]
let ``Omitting a preview skips computing the HTTP body length`` maxLength shouldCallPredicate (marker: string) =
    let mutable predicateCalled = false
    let mutable lengthComputed = false

    use _ =
        Config.With(
            FaqtConfig.Default
                .SetHttpContentMaxLength(maxLength)
                .SetShouldPreviewHttpResponseContent(fun _ ->
                    predicateCalled <- true
                    false
                )
        )

    let stream =
        { new MemoryStream() with
            override _.Length =
                lengthComputed <- true
                failwith "sample response secret"
        }

    use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    response.Content <- new StreamContent(stream)

    let error =
        assertFails (fun () -> response.Should().HaveStatusCode(HttpStatusCode.OK))

    Assert.Contains(marker, error.Message)
    Assert.DoesNotContain("sample response secret", error.Message)
    Assert.Equal(shouldCallPredicate, predicateCalled)
    Assert.False(lengthComputed)


[<Fact>]
let ``Setting null global config throws ArgumentNullException`` () =
    try
        Assert.Throws<ArgumentNullException>(fun () -> Config.Set(Unchecked.defaultof<FaqtConfig>))
        |> ignore
    finally
        Config.Set(FaqtConfig.Default)


[<Fact>]
let ``Setting null local config throws ArgumentNullException`` () =
    Assert.Throws<ArgumentNullException>(fun () -> Config.With(Unchecked.defaultof<FaqtConfig>) |> ignore)
    |> ignore


[<Fact>]
let ``Credential and cookie request headers are masked by default`` () =
    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Headers.Add("Authorization", "Bearer secret")
        x.Headers.Add("Proxy-Authorization", "secret")
        x.Headers.Add("Cookie", "a=secret; b=secret")
        x.Headers.Add("x-api-key", "secret")
        x.Headers.Add("X-Auth-Token", "secret")
        x.Headers.Add("Ocp-Apim-Subscription-Key", "secret")
        x.Headers.Add("Other", "visible")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Authorization: Bearer ***
  Proxy-Authorization: ***
  Cookie: ***
  x-api-key: ***
  X-Auth-Token: ***
  Ocp-Apim-Subscription-Key: ***
  Other: visible
"""


[<Fact>]
let ``Set-Cookie response headers are masked by default, also in HaveHeaderValue`` () =
    fun () ->
        let x = new HttpResponseMessage(HttpStatusCode.OK)
        x.Version <- Version.Parse("0.5")
        x.Headers.Add("Set-Cookie", "session=secret; Path=/")
        x.Headers.Add("Set-Cookie", "other=secret")
        x.Should().HaveHeaderValue("Set-Cookie", "session=expected")
    |> assertExnMsg
        """
Subject: x
Should: HaveHeaderValue
Header: Set-Cookie
Value: '***'
Response: |-
  HTTP/0.5 200 OK
  Set-Cookie: ***
  Set-Cookie: ***
Request: null
"""


[<Theory>]
[<InlineData("authorization")>]
[<InlineData("PROXY-AUTHORIZATION")>]
[<InlineData("cookie")>]
[<InlineData("SET-COOKIE")>]
[<InlineData("x-api-key")>]
[<InlineData("API-KEY")>]
[<InlineData("x-auth-key")>]
[<InlineData("X-AUTH-TOKEN")>]
[<InlineData("x-access-token")>]
[<InlineData("X-CSRF-TOKEN")>]
[<InlineData("x-xsrf-token")>]
[<InlineData("X-AMZ-SECURITY-TOKEN")>]
[<InlineData("x-goog-api-key")>]
[<InlineData("OCP-APIM-SUBSCRIPTION-KEY")>]
[<InlineData("x-functions-key")>]
[<InlineData("X-VAULT-TOKEN")>]
[<InlineData("private-token")>]
[<InlineData("X-GITLAB-TOKEN")>]
let ``The default HTTP header mapping masks each listed header case-insensitively`` (name: string) =
    Assert.Equal("***", FaqtConfig.Default.MapHttpHeaderValues name "secret")


[<Theory>]
[<InlineData("Bearer secret", "Bearer ***")>]
[<InlineData("AWS4-HMAC-SHA256 Credential=secret", "AWS4-HMAC-SHA256 ***")>]
[<InlineData("secret", "***")>]
[<InlineData("secret1! secret2", "***")>]
[<InlineData("1secret secret2", "***")>]
[<InlineData("averylongsecretwithoutspaces secret2", "***")>]
[<InlineData(" secret", "***")>]
let ``The default HTTP header mapping keeps only an authentication scheme`` (value: string) (expected: string) =
    Assert.Equal(expected, FaqtConfig.Default.MapHttpHeaderValues "Authorization" value)
    Assert.Equal(expected, FaqtConfig.Default.MapHttpHeaderValues "Proxy-Authorization" value)


[<Fact>]
let ``The default HTTP header mapping does not mask other headers`` () =
    Assert.Equal("Bearer visible", FaqtConfig.Default.MapHttpHeaderValues "Idempotency-Key" "Bearer visible")


[<Fact>]
let ``HaveHeaderValue fails normally with a null expected value for a masked header`` () =
    use response = new HttpResponseMessage(HttpStatusCode.OK)

    let error =
        assertFails (fun () -> response.Should().HaveHeaderValue("Authorization", Unchecked.defaultof<string>))

    Assert.Null(error.FailureData.Extra |> List.find (fst >> (=) "Value") |> snd)


[<Fact>]
let ``NotHaveHeader masks listed headers by default`` () =
    fun () ->
        let x = new HttpResponseMessage(HttpStatusCode.OK)
        x.Version <- Version.Parse("0.5")
        x.Headers.Add("Set-Cookie", "session=secret")
        x.Should().NotHaveHeader("Set-Cookie")
    |> assertExnMsg
        """
Subject: x
Should: NotHaveHeader
Header: Set-Cookie
But was present with value: '***'
Response: |-
  HTTP/0.5 200 OK
  Set-Cookie: ***
Request: null
"""


[<Fact>]
let ``A throwing header mapping does not replace header assertion failures`` () =
    use _ =
        Config.With(FaqtConfig.Default.SetMapHttpHeaderValues(fun _ value -> failwith $"Cannot map %s{value}"))

    use response = new HttpResponseMessage(HttpStatusCode.OK)
    response.Headers.Add("X-Secret", "sensitive-1")
    response.Headers.Add("X-Multi", "sensitive-2")
    response.Headers.Add("X-Multi", "sensitive-3")

    let failures = [
        assertFails (fun () -> response.Should().HaveHeaderValue("X-Secret", "sensitive-expected"))
        assertFails (fun () -> response.Should().NotHaveHeader("X-Secret"))
        assertFails (fun () -> response.Should().NotHaveHeader("X-Multi"))
    ]

    for error in failures do
        Assert.Contains("[header value omitted: header mapping failed with System.Exception]", error.Message)
        Assert.DoesNotContain("sensitive", error.Message)


[<Fact>]
let ``The default HTTP header masking can be turned off`` () =
    use _ = Config.With(FaqtConfig.Default.SetMapHttpHeaderValues(fun _ value -> value))

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Headers.Add("Authorization", "Bearer foobar")
        x.Headers.Add("Cookie", "foobar")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Authorization: Bearer foobar
  Cookie: foobar
"""


[<Fact>]
let ``The default HTTP header masking can be extended`` () =
    let mask = FaqtConfig.Default.MapHttpHeaderValues

    use _ =
        Config.With(
            FaqtConfig.Default.SetMapHttpHeaderValues(fun name value ->
                if name = "X-Custom-Secret" then
                    "[masked]"
                else
                    mask name value
            )
        )

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Headers.Add("Authorization", "Bearer foobar")
        x.Headers.Add("X-Custom-Secret", "foobar")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Authorization: Bearer ***
  X-Custom-Secret: [masked]
"""


[<Fact>]
let ``HTTP headers can be changed`` () =
    use _ =
        Config.With(
            FaqtConfig.Default.SetMapHttpHeaderValues(fun name value ->
                match name with
                | "Authorization"
                | "Cookie" -> "[masked]"
                | "Multi"
                | "Comma"
                | "CommaSpace"
                | "Semicolon"
                | "Accept-Encoding" -> value + "!"
                | _ -> value
            )
        )

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Headers.Add("Authorization", "foobar")
        x.Headers.Add("Cookie", "foobar")
        x.Headers.Add("Multi", "A")
        x.Headers.Add("Multi", "B")
        x.Headers.Add("Comma", "A,B")
        x.Headers.Add("CommaSpace", "A, B")
        x.Headers.Add("Semicolon", "A;B")
        x.Headers.Add("Accept-Encoding", "gzip, deflate")
        x.Headers.Add("Other", "test")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Authorization: [masked]
  Cookie: [masked]
  Multi: A!
  Multi: B!
  Comma: A,B!
  CommaSpace: A, B!
  Semicolon: A;B!
  Accept-Encoding: gzip!
  Accept-Encoding: deflate!
  Other: test
"""


[<Fact>]
let ``HTTP content headers can be changed`` () =
    use _ =
        Config.With(
            FaqtConfig.Default.SetMapHttpHeaderValues(fun name value -> if name = "Content-Type" then "***" else value)
        )

    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        let content = new StringContent("body")
        content.Headers.ContentType <- MediaTypeHeaderValue("application/example")
        x.Content <- content
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: ***
  Content-Length: 4

  body
"""


[<Theory>]
[<InlineData(false, false, false)>]
[<InlineData(false, false, true)>]
[<InlineData(false, true, false)>]
[<InlineData(false, true, true)>]
[<InlineData(true, false, false)>]
[<InlineData(true, false, true)>]
[<InlineData(true, true, false)>]
[<InlineData(true, true, true)>]
let ``Throwing header mapping does not expose values or stop rendering`` response contentHeader throwFirst =
    let secret = "sensitive-header-value"
    let failingValue = "sensitive-failing-value"

    use _ =
        Config.With(
            FaqtConfig.Default.SetMapHttpHeaderValues(fun name value ->
                if name = "X-Failing" && value = failingValue then
                    failwith $"Cannot map %s{value}"
                elif name = "X-Secret" then
                    "[masked]"
                else
                    value
            )
        )

    use request = new HttpRequestMessage(HttpMethod.Post, "/")
    use reply = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
    use content = new StringContent("retained body")

    let message, headers =
        if response then
            reply.Content <- content
            box reply, reply.Headers :> HttpHeaders
        else
            request.Content <- content
            box request, request.Headers :> HttpHeaders

    let headers =
        if contentHeader then
            content.Headers :> HttpHeaders
        else
            headers

    let addFailure () =
        headers.Add("X-Failing", [| "retained first"; failingValue; "retained last" |])

    if throwFirst then
        addFailure ()

    headers.Add("X-Secret", secret)

    if not throwFirst then
        addFailure ()

    headers.Add("X-Other", "retained other")

    for value in [ message; box [ Faqt.Formatting.TryFormat message ] ] do
        let error = assertFails (fun () -> (0).Should().FailWith("Value", value))
        Assert.DoesNotContain(secret, error.Message)
        Assert.DoesNotContain(failingValue, error.Message)
        Assert.Contains("X-Secret: [masked]", error.Message)
        Assert.Contains("X-Failing: [header value omitted: header mapping failed with System.Exception]", error.Message)
        Assert.Contains("X-Failing: retained first", error.Message)
        Assert.Contains("X-Failing: retained last", error.Message)
        Assert.Contains("X-Other: retained other", error.Message)
        Assert.Contains("retained body", error.Message)
