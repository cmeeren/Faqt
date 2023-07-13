[<AutoOpen>]
module TestUtils

open Faqt
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)
    Assert.Equal(msg.ReplaceLineEndings("\n").Trim(), ex.Message.ReplaceLineEndings("\n").Trim().Replace("\t", "    "))
