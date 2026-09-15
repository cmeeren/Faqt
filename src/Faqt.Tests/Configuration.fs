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
