module Tests

open System.Text.Encodings.Web
open System.Text.Json
open Faqt
open Xunit


let jsonFormat (x: 'a) =
    try
        JsonSerializer.Serialize(
            x,
            JsonSerializerOptions(Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true)
        )
    with _ ->
        $"%A{x}"


[<Fact>]
let ``Format config works`` () =
    // Everything is in a single test to ensure correct order, since config is global

    Formatting.addFormatter (fun (x: int) -> $"INT:%i{x}")

    // addFormatter works
    fun () ->
        let x = 1
        x.Should().Be(2)
    |> assertExnMsg
        """
x
    should be
INT:2
    but was
INT:1
"""

    // addFormatter works only when type is top-level
    fun () ->
        let x = {| X = 1 |}
        x.Should().Be({| X = 2 |})
    |> assertExnMsg
        """
x
    should be
{ X = 2 }
    but was
{ X = 1 }
"""

    Formatting.setDefaultFormatter jsonFormat

    // setDefaultFormatter works
    fun () ->
        let x = {|
            A = {| X = 1; Y = [ 'a'; 'b'; 'c' ] |}
            B = "foo"
        |}

        x.Should().NotBe(x)
    |> assertExnMsg
        """
x
    should not be
{
  "A": {
    "X": 1,
    "Y": [
      "a",
      "b",
      "c"
    ]
  },
  "B": "foo"
}
    but the values were equal.
"""
