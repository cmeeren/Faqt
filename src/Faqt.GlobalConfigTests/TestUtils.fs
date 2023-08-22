[<AutoOpen>]
module TestUtils

open Faqt
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)

    Assert.Equal(
        ("\n\n" + msg.ReplaceLineEndings("\n").Trim() + "\n") :> obj, // Cast to obj to force full output
        ("\n\n" + ex.Message.ReplaceLineEndings("\n").Trim() + "\n")
    )
