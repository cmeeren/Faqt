module AssertionFailedException

open Faqt
open Faqt.Formatting
open Xunit


[<Fact>]
let ``Has expected members`` () =
    // Compile-time check only
    fun () ->
        let x = Unchecked.defaultof<AssertionFailedException>
        x.FailureData |> ignore<FailureData>
    |> ignore
