[<AutoOpen>]
module TestUtils

open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)
    Assert.Equal(msg.ReplaceLineEndings("\n").Trim(), ex.Message.ReplaceLineEndings("\n").Trim().Replace("\t", "    "))


[<Extension>]
type Assertions =


    [<Extension>]
    static member TestDerived(t: Testable<'a>, pass) : AndDerived<'a, 'a> =
        use _ = t.Assert()

        if not pass then
            Fail(t, None).Throw("{subject}")

        AndDerived(t, t.Subject)


    [<Extension>]
    static member Test(t: Testable<'a>, pass) : And<'a> =
        use _ = t.Assert()

        if not pass then
            Fail(t, None).Throw("{subject}")

        And(t)


    [<Extension>]
    static member PassDerived(t: Testable<'a>) : AndDerived<'a, 'a> =
        use _ = t.Assert()
        t.TestDerived(true)


    [<Extension>]
    static member Pass(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()
        t.Test(true)


    [<Extension>]
    static member FailDerived(t: Testable<'a>) : AndDerived<'a, 'a> =
        use _ = t.Assert()
        t.TestDerived(false)


    [<Extension>]
    static member Fail(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()
        t.Test(false)
