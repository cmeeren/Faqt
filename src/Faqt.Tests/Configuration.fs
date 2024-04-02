module Configuration

open System
open System.Net.Http
open System.Net.Http.Headers
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
  [content truncated after 10 characters]
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
  [content truncated after 10 characters]
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
        x.Content <- new StringContent("{\"a\":1}")
        x.Content.Headers.ContentType <- MediaTypeHeaderValue("application/json")
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
        x.Content <- new StringContent("{\"a\":1}")
        x.Content.Headers.ContentType <- MediaTypeHeaderValue("application/json")
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
        + $"…\n  [content truncated after {expectedMaxLength} characters]"

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
  [content truncated after 10 characters]
"""


[<Fact>]
let ``HTTP headers are unchanged by default`` () =
    fun () ->
        let x = new HttpRequestMessage(HttpMethod.Get, "/")
        x.Version <- Version.Parse("0.5")
        x.Headers.Add("Authorization", "foobar")
        x.Headers.Add("Cookie", "foobar")
        x.Should().FailWith("Value", x)
    |> assertExnMsg
        """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Authorization: foobar
  Cookie: foobar
"""


[<Fact>]
let ``HTTP headers can be changed`` () =
    use _ =
        Config.With(
            FaqtConfig.Default.SetMapHttpHeaderValues(fun name value ->
                match name with
                | "Authorization"
                | "Cookie" -> "***"
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
  Authorization: ***
  Cookie: ***
  Multi: A!
  Multi: B!
  Comma: A,B!
  CommaSpace: A, B!
  Semicolon: A;B!
  Accept-Encoding: gzip!
  Accept-Encoding: deflate!
  Other: test
"""
