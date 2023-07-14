[<AutoOpen>]
module TestUtils

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Faqt
open AssertionHelpers
open type AssertionHelpers
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)
    Assert.Equal(msg.ReplaceLineEndings("\n").Trim(), ex.Message.ReplaceLineEndings("\n").Trim().Replace("\t", "    "))


[<Extension>]
type Assertions =


    [<Extension>]
    static member TestDerived
        (
            t: Testable<'a>,
            pass,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] fn,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lno,
            ?methodNameOverride
        ) : AndDerived<'a, 'a> =
        if not pass then
            fail (sub (fn, lno, methodNameOverride))

        AndDerived(t, t.Subject)


    [<Extension>]
    static member Test
        (
            t: Testable<'a>,
            pass,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] fn,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lno,
            ?methodNameOverride
        ) : And<'a> =
        if not pass then
            fail (sub (fn, lno, methodNameOverride))

        And(t)


    [<Extension>]
    static member PassDerived
        (
            t: Testable<'a>,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] fn,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lno,
            ?methodNameOverride
        ) : AndDerived<'a, 'a> =
        t.TestDerived(true, fn, lno, defaultArg methodNameOverride (nameof Assertions.PassDerived))


    [<Extension>]
    static member Pass
        (
            t: Testable<'a>,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] fn,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lno,
            ?methodNameOverride
        ) : And<'a> =
        t.Test(true, fn, lno, defaultArg methodNameOverride (nameof Assertions.Pass))


    [<Extension>]
    static member FailDerived
        (
            t: Testable<'a>,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] fn,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lno,
            ?methodNameOverride
        ) : AndDerived<'a, 'a> =
        t.TestDerived(false, fn, lno, defaultArg methodNameOverride (nameof Assertions.FailDerived))


    [<Extension>]
    static member Fail
        (
            t: Testable<'a>,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] fn,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lno,
            ?methodNameOverride
        ) : And<'a> =
        t.Test(false, fn, lno, defaultArg methodNameOverride (nameof Assertions.Fail))
