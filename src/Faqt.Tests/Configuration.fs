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
  Cookie: a=***; b=***
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
Value: session=***
Response: |-
  HTTP/0.5 200 OK
  Set-Cookie: session=***; Path=/
  Set-Cookie: other=***
Request: null
"""


[<Theory>]
[<InlineData("authorization")>]
[<InlineData("PROXY-AUTHORIZATION")>]
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


[<Theory>]
[<InlineData("Cookie", "a=1; b=2", "a=***; b=***")>]
[<InlineData("Cookie", "a=1;b=", "a=***;b=")>]
[<InlineData("Cookie", "secret", "***")>]
[<InlineData("Set-Cookie", "session=1; Path=/; HttpOnly", "session=***; Path=/; HttpOnly")>]
[<InlineData("Set-Cookie",
             "session=; Expires=Thu, 01 Jan 1970 00:00:00 GMT",
             "session=; Expires=Thu, 01 Jan 1970 00:00:00 GMT")>]
[<InlineData("Set-Cookie", "secret; Path=/", "***; Path=/")>]
[<InlineData("cookie", "a=secret", "a=***")>]
[<InlineData("SET-COOKIE", "a=secret", "a=***")>]
[<InlineData("Cookie", ";; a= ", ";; a= ")>]
[<InlineData("Cookie", "a=1; secret", "a=***; ***")>]
[<InlineData("Set-Cookie",
             "a=1; Expires=Thu, 01 Jan 1970 00:00:00 GMT, b=2; Path=/,c=3",
             "a=***; Expires=Thu, 01 Jan 1970 00:00:00 GMT, b=***; Path=/,c=***")>]
let ``The default HTTP header mapping masks only cookie values`` name (value: string) (expected: string) =
    Assert.Equal(expected, FaqtConfig.Default.MapHttpHeaderValues name value)


[<Theory>]
[<InlineData("https://user:pass@example.com/p?code=abc&page=2#access_token=t&state=s",
             "https://user:***@example.com/p?code=***&page=2#access_token=***&state=s")>]
[<InlineData("https://example.com/?SIG=abc&Key=k&token=t&X-Amz-Signature=s&x-goog-credential=c",
             "https://example.com/?SIG=***&Key=***&token=***&X-Amz-Signature=***&x-goog-credential=***")>]
[<InlineData("https://example.com/?signature=1&id_token=2&refresh_token=3&client_secret=4",
             "https://example.com/?signature=***&id_token=***&refresh_token=***&client_secret=***")>]
[<InlineData("https://example.com/?api_key=1&apikey=2&password=3&X-Amz-Credential=4&X-Amz-Security-Token=5",
             "https://example.com/?api_key=***&apikey=***&password=***&X-Amz-Credential=***&X-Amz-Security-Token=***")>]
[<InlineData("https://example.com/?X-Goog-Signature=1", "https://example.com/?X-Goog-Signature=***")>]
[<InlineData("https://example.com/?Subscription-Key=1", "https://example.com/?Subscription-Key=***")>]
[<InlineData("/relative?code=abc", "/relative?code=***")>]
[<InlineData("https://example.com/?code=&codes=1&page", "https://example.com/?code=&codes=1&page")>]
[<InlineData("https://user@example.com/a@b:c", "https://user@example.com/a@b:c")>]
[<InlineData("https://example.com/#section", "https://example.com/#section")>]
let ``The default HTTP URL mapping masks secret URL parts`` (url: string) (expected: string) =
    Assert.Equal(expected, FaqtConfig.Default.MapHttpUrls url)


[<Theory>]
[<InlineData("/cb?c%6Fde=secret&a+b=c", "/cb?c%6Fde=***&a+b=c")>]
[<InlineData("//user:pass@cdn.example.com/x?sig=s", "//user:***@cdn.example.com/x?sig=***")>]
let ``The default URL masking matches unescaped names and protocol-relative URLs`` (value: string) (expected: string) =
    Assert.Equal(expected, FaqtConfig.Default.MapHttpUrls value)


[<Fact>]
let ``The default HTTP header mapping leaves URL masking to the URL mapping`` () =
    let url = "https://example.com/cb?code=abc"
    Assert.Equal(url, FaqtConfig.Default.MapHttpHeaderValues "Location" url)


[<Fact>]
let ``A request without a URI is still rendered`` () =
    use request = new HttpRequestMessage()
    request.RequestUri <- null

    let error = assertFails (fun () -> request.Should().FailWith("Value", request))

    Assert.Contains("[HttpRequestMessage.RequestUri not initialized]", error.Message)


[<Fact>]
let ``HTTP request URLs and URL headers are masked by default`` () =
    fun () ->
        let x = new HttpResponseMessage(HttpStatusCode.Redirect)
        x.Version <- Version.Parse("0.5")
        x.Headers.Location <- Uri("https://example.com/cb?code=secret&state=visible")
        x.Content <- new ByteArrayContent([||])
        x.Content.Headers.ContentLocation <- Uri("https://example.com/doc?sig=secret")

        let request =
            new HttpRequestMessage(HttpMethod.Get, "https://example.com/api?code=secret&page=2")

        request.Version <- Version.Parse("0.5")
        request.Headers.Referrer <- Uri("https://example.com/from?token=secret")
        x.RequestMessage <- request
        x.Should().HaveStatusCode(HttpStatusCode.OK)
    |> assertExnMsg
        """
Subject: x
Should: HaveStatusCode
Expected: 200 OK
But was: 302 Found
Response: |-
  HTTP/0.5 302 Found
  Location: https://example.com/cb?code=***&state=visible
  Content-Location: https://example.com/doc?sig=***
  Content-Length: 0
Request: |-
  GET https://example.com/api?code=***&page=2 HTTP/0.5
  Referer: https://example.com/from?token=***
"""


[<Fact>]
let ``A custom HTTP URL mapping applies to request lines and URL headers before header mapping`` () =
    use _ =
        Config.With(
            FaqtConfig.Default
                .SetMapHttpUrls(fun url -> FaqtConfig.Default.MapHttpUrls(url).Replace("custom=secret", "custom=[url]"))
                .SetMapHttpHeaderValues(fun name value -> if name = "Location" then value + " [header]" else value)
        )

    use request =
        new HttpRequestMessage(HttpMethod.Get, "https://example.com/api?code=secret&custom=secret")

    use response = new HttpResponseMessage(HttpStatusCode.Redirect)
    response.RequestMessage <- request
    response.Headers.Location <- Uri("https://example.com/cb?custom=secret")

    let error =
        assertFails (fun () -> response.Should().HaveHeaderValue("Location", "/cb?custom=secret&code=secret"))

    Assert.Contains("GET https://example.com/api?code=***&custom=[url] HTTP/", error.Message)
    Assert.Contains("Location: https://example.com/cb?custom=[url] [header]", error.Message)

    Assert.Equal(
        box "/cb?custom=[url]&code=*** [header]",
        error.FailureData.Extra |> List.find (fst >> (=) "Value") |> snd
    )


[<Fact>]
let ``The default HTTP URL masking can be turned off`` () =
    use _ = Config.With(FaqtConfig.Default.SetMapHttpUrls(id))

    use request =
        new HttpRequestMessage(HttpMethod.Get, "https://example.com/api?code=secret")

    request.Headers.Referrer <- Uri("https://example.com/from?token=secret")

    let error = assertFails (fun () -> request.Should().FailWith("Value", request))

    Assert.Contains("GET https://example.com/api?code=secret HTTP/", error.Message)
    Assert.Contains("Referer: https://example.com/from?token=secret", error.Message)


[<Fact>]
let ``A throwing HTTP URL mapping does not expose URLs or stop rendering`` () =
    use _ =
        Config.With(FaqtConfig.Default.SetMapHttpUrls(fun url -> failwith $"Cannot map %s{url}"))

    use request =
        new HttpRequestMessage(HttpMethod.Get, "https://example.com/sensitive")

    request.Headers.Referrer <- Uri("https://example.com/sensitive-referrer")
    request.Headers.Add("Other", "visible")

    let error = assertFails (fun () -> request.Should().FailWith("Value", request))

    Assert.Contains("GET [request URL omitted: URL mapping failed with System.Exception] HTTP/", error.Message)
    Assert.Contains("Referer: [header value omitted: URL mapping failed with System.Exception]", error.Message)
    Assert.Contains("Other: visible", error.Message)
    Assert.DoesNotContain("sensitive", error.Message)


[<Fact>]
let ``HaveHeaderValue fails normally with a null expected value for a URL header`` () =
    use response = new HttpResponseMessage(HttpStatusCode.OK)

    let error =
        assertFails (fun () -> response.Should().HaveHeaderValue("Location", Unchecked.defaultof<string>))

    Assert.Null(error.FailureData.Extra |> List.find (fst >> (=) "Value") |> snd)


[<Fact>]
let ``The default HTTP header mapping accepts a null header name`` () =
    Assert.Equal("value", FaqtConfig.Default.MapHttpHeaderValues (Unchecked.defaultof<string>) "value")


[<Fact>]
let ``NotHaveHeader masks URL headers by default`` () =
    use response = new HttpResponseMessage(HttpStatusCode.Redirect)
    response.Headers.Location <- Uri("https://example.com/cb?code=secret")

    let error = assertFails (fun () -> response.Should().NotHaveHeader("Location"))

    Assert.Equal(
        box "https://example.com/cb?code=***",
        error.FailureData.Extra
        |> List.find (fst >> (=) "But was present with value")
        |> snd
    )

    Assert.DoesNotContain("secret", error.Message)


[<Fact>]
let ``Setting null HTTP URL mapper throws ArgumentNullException`` () =
    Assert.Throws<ArgumentNullException>(fun () ->
        FaqtConfig.Default.SetMapHttpUrls(Unchecked.defaultof<string -> string>)
        |> ignore
    )
    |> ignore


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
But was present with value: session=***
Response: |-
  HTTP/0.5 200 OK
  Set-Cookie: session=***
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
