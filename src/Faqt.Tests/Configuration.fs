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
let ``Default config`` () =
    Assert.Equal(FaqtConfig.Default.HttpContentMaxLength, Config.Current.HttpContentMaxLength)
    Assert.Equal(FaqtConfig.Default.FormatHttpContent, Config.Current.FormatHttpContent)
    Assert.Equal(1024 * 1024, Config.Current.HttpContentMaxLength)
