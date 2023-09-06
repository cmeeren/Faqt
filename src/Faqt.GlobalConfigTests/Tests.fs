module Tests

open Faqt
open Faqt.Formatting
open Xunit


[<Fact>]
let ``Can set, override and restore custom global formatter`` () =
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
