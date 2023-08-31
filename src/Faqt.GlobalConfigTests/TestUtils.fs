[<AutoOpen>]
module TestUtils

open Faqt
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)

    Assert.Equal(
        ("\n\n" + msg.Replace("\r\n", "\n").Trim() + "\n") :> obj, // Cast to obj to force full output
        ("\n\n" + ex.Message.Replace("\r\n", "\n").Trim() + "\n")
    )
