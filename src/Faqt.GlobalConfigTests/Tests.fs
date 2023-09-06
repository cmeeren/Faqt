module Tests

open System
open System.Net.Http
open System.Runtime.CompilerServices
open Faqt
open Faqt.AssertionHelpers
open Faqt.Configuration
open Faqt.Formatting
open Xunit


[<Extension>]
type Assertions =


    [<Extension>]
    static member FailWith(t: Testable<'a>, key: string, value: 'b) : And<'a> =
        use _ = t.Assert()
        t.With(key, value).Fail(None)


[<Fact>]
let ``Can set, override and restore custom global config and formatter`` () =
    Config.Set(FaqtConfig.Default.SetHttpContentMaxLength(10))

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

    do
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(20))

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

  lorem ipsum dolor si…
  [content truncated after 20 characters]
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

  lorem ipsu…
  [content truncated after 10 characters]
"""


    Formatter.Set(fun _ -> "GLOBAL FORMATTER")

    fun () ->
        let x = 1
        x.Should().Be(2)
    |> assertExnMsg
        """
GLOBAL FORMATTER
"""

    do
        (use _ = Formatter.With(fun _ -> "OVERRIDDEN FORMATTER")

         fun () ->
             let x = 1
             x.Should().Be(2)
         |> assertExnMsg
             """
        OVERRIDDEN FORMATTER
    """)

    fun () ->
        let x = 1
        x.Should().Be(2)
    |> assertExnMsg
        """
GLOBAL FORMATTER
"""
