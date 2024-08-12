[<AutoOpen>]
module TestUtils

open System
open System.Globalization
open System.Runtime.CompilerServices
open Faqt
open Faqt.AssertionHelpers
open Xunit


let assertFails f =
    Assert.Throws<AssertionFailedException>(f >> ignore)


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)

    Assert.Equal(
        ("\n\n" + msg.ReplaceLineEndings("\n").Trim() + "\n") :> obj, // Cast to obj to force full output
        ("\n\n" + ex.Message.ReplaceLineEndings("\n").Trim() + "\n")
    )


let assertExnMsgAsync (msg: string) (f: unit -> Async<'a>) =
    task {
        let! ex = Assert.ThrowsAsync<AssertionFailedException>(f >> Async.StartImmediateAsTask >> (fun t -> upcast t))

        Assert.Equal(
            ("\n\n" + msg.ReplaceLineEndings("\n").Trim() + "\n") :> obj, // Cast to obj to force full output
            ("\n\n" + ex.Message.ReplaceLineEndings("\n").Trim() + "\n")
        )
    }


let assertExnMsgWildcard (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)

    match msg.Split('*') with
    | [| a; b |] ->
        let exnMsg = "\n\n" + ex.Message.ReplaceLineEndings("\n").Trim() + "\n"
        let a = "\n\n" + a.ReplaceLineEndings("\n").Trim()
        let b = b.ReplaceLineEndings("\n").Trim() + "\n"
        Assert.StartsWith(a, exnMsg)
        Assert.EndsWith(b, exnMsg)
    | _ -> failwith "Expected msg to contain a single *"


type TestInterface =
    interface
    end


type TestBaseType() =
    class
    end


type TestSubType() =
    inherit TestBaseType()
    interface TestInterface


type TestInterface<'a, 'b> =
    interface
    end


type TestBaseType<'a, 'b>() =
    class
    end


type TestSubType<'a, 'b>() =
    inherit TestBaseType<'a, 'b>()
    interface TestInterface<'a, 'b>


type TestUnserializableType() =
    member _.WillThrow = failwith<int> "Foo"


type TestRefEqualityType() =
    static member val Instance = TestRefEqualityType()


module CultureInfo =

    let withCurrentCulture (cultureName: string) =
        let ci = CultureInfo(cultureName)
        let oldCi = CultureInfo.CurrentCulture
        CultureInfo.CurrentCulture <- ci

        { new IDisposable with
            member _.Dispose() = CultureInfo.CurrentCulture <- oldCi
        }


[<Extension>]
type MiscExtensions =


    [<Extension; RequiresExplicitTypeArguments>]
    static member Id<'a>(x: 'a) : 'a = x


[<Extension>]
type Assertions =


    [<Extension>]
    static member TestDerived(t: Testable<'a>, pass) : AndDerived<'a, 'a> =
        use _ = t.Assert()

        if not pass then
            t.Fail(None)

        AndDerived(t, t.Subject)


    [<Extension>]
    static member Test<'a>(t: Testable<'a>, pass) : And<'a> =
        use _ = t.Assert()

        if not pass then
            t.Fail(None)

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


    [<Extension>]
    static member FailWith(t: Testable<'a>, key: string, value: 'b) : And<'a> =
        use _ = t.Assert()
        t.With(key, value).Fail(None)


    [<Extension>]
    static member FailWithBecause(t: Testable<'a>, because: string, key: string, value: 'b) : And<'a> =
        use _ = t.Assert()
        t.With(key, value).Fail(Some because)
