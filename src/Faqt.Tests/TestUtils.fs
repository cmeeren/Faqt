[<AutoOpen>]
module TestUtils

open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)
    Assert.Equal(msg.ReplaceLineEndings("\n").Trim(), ex.Message.ReplaceLineEndings("\n").Trim().Replace("\t", "    "))


[<Extension; ContainsFaqtAssertions>]
type Assertions =


    [<Extension>]
    static member TestDerived(t: Testable<'a>, pass) : AndDerived<'a, 'a> =
        if not pass then
            fail (sub ())

        AndDerived(t, t.Subject)


    [<Extension>]
    static member Test(t: Testable<'a>, pass) : And<'a> =
        if not pass then
            fail (sub ())

        And(t)


    [<Extension>]
    static member PassDerived(t: Testable<'a>) : AndDerived<'a, 'a> = t.TestDerived(true)


    [<Extension>]
    static member Pass(t: Testable<'a>) : And<'a> = t.Test(true)


    [<Extension>]
    static member FailDerived(t: Testable<'a>) : AndDerived<'a, 'a> = t.TestDerived(false)


    [<Extension>]
    static member Fail(t: Testable<'a>) : And<'a> = t.Test(false)
