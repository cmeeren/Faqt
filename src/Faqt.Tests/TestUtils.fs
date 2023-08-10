[<AutoOpen>]
module TestUtils

open System
open System.Globalization
open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)

    Assert.Equal(
        "\n" + msg.ReplaceLineEndings("\n").Trim() + "\n" :> obj, // Cast to obj to force full output
        "\n" + ex.Message.ReplaceLineEndings("\n").Trim().Replace("\t", "    ") + "\n"
    )


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
            t.Fail("{subject}", None)

        AndDerived(t, t.Subject)


    [<Extension>]
    static member Test<'a>(t: Testable<'a>, pass) : And<'a> =
        use _ = t.Assert()

        if not pass then
            t.Fail("{subject}", None)

        And(t)


    [<Extension>]
    static member TestSatisfy(t: Testable<'a>, assertion) : And<'a> =
        use x = t.Assert(true)

        try
            assertion t.Subject |> ignore
            And(t)
        with :? AssertionFailedException as ex ->
            t.Fail("{subject}{0}", None, ex.Message)


    [<Extension>]
    static member TestSatisfyAny(t: Testable<'a>, assertions: seq<'a -> 'ignored>) : And<'a> =
        use _ = t.Assert(true)
        let assertions = assertions |> Seq.toArray

        let exceptions =
            assertions
            |> Array.choose (fun f ->
                try
                    f t.Subject |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some ex
            )

        if exceptions.Length = assertions.Length then
            let assertionFailuresString =
                exceptions |> Seq.map (fun ex -> ex.Message) |> String.concat ""

            t.Fail("{subject}{0}", None, assertionFailuresString)

        And(t)


    [<Extension>]
    static member TestAllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored) : And<_> =
        use _ = t.Assert(true, true)

        let exceptions =
            t.Subject
            |> Seq.choose (fun x ->
                try
                    use _ = t.AssertItem()
                    assertion x |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some ex
            )
            |> Seq.toArray

        if exceptions.Length > 0 then
            let assertionFailuresString =
                exceptions |> Seq.map (fun ex -> ex.Message) |> String.concat ""

            t.Fail("{subject}{0}", None, assertionFailuresString)

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
