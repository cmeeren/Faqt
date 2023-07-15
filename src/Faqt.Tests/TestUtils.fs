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
    static member TestDerived(t: Testable<'a>, pass, ?methodNameOverride) : AndDerived<'a, 'a> =
        if not pass then
            Fail(t, None, methodNameOverride).Throw("{subject}")

        AndDerived(t, t.Subject)


    [<Extension>]
    static member Test(t: Testable<'a>, pass, ?methodNameOverride) : And<'a> =
        if not pass then
            Fail(t, None, methodNameOverride).Throw("{subject}")

        And(t)


    [<Extension>]
    static member PassDerived(t: Testable<'a>, ?methodNameOverride) : AndDerived<'a, 'a> =
        t.TestDerived(true, defaultArg methodNameOverride (nameof Assertions.PassDerived))


    [<Extension>]
    static member Pass(t: Testable<'a>, ?methodNameOverride) : And<'a> =
        t.Test(true, defaultArg methodNameOverride (nameof Assertions.Pass))


    [<Extension>]
    static member FailDerived(t: Testable<'a>, ?methodNameOverride) : AndDerived<'a, 'a> =
        t.TestDerived(false, defaultArg methodNameOverride (nameof Assertions.FailDerived))


    [<Extension>]
    static member Fail(t: Testable<'a>, ?methodNameOverride) : And<'a> =
        t.Test(false, defaultArg methodNameOverride (nameof Assertions.Fail))
