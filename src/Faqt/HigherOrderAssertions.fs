namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers


[<Extension>]
type HigherOrderAssertions =


    /// Asserts that the subject satisfies the supplied assertion. Often you can just use And for making multiple
    /// assertions, but Satisfy (combined with And) can be useful if you want to fluently perform multiple assertion
    /// chains, for example if asserting on different parts of a value.
    [<Extension>]
    static member Satisfy(t: Testable<'a>, assertion: 'a -> 'ignored, ?because) : And<'a> =
        use x = t.Assert(true)

        try
            assertion t.Subject |> ignore
            And(t)
        with :? AssertionFailedException as ex ->
            t.Fail(
                "{subject}\n\tshould satisfy the supplied assertion{because}, but the assertion failed with the following message:\n{0}",
                because,
                ex.Message
            )


    /// Asserts that the subject satisfies at least one of the supplied assertions.
    [<Extension>]
    static member SatisfyAny(t: Testable<'a>, assertions: seq<'a -> 'ignored>, ?because) : And<'a> =
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
                exceptions
                |> Seq.mapi (fun i ex -> $"\n\n[Assertion %i{i + 1}/%i{assertions.Length}]\n%s{ex.Message}")
                |> String.concat ""

            t.Fail(
                "{subject}\n\tshould satisfy at least one of the {0} supplied assertions{because}, but none were satisfied.{1}",
                because,
                string assertions.Length,
                assertionFailuresString
            )

        And(t)


    /// Asserts that the subject satisfies all of the supplied assertions.
    [<Extension>]
    static member SatisfyAll(t: Testable<'a>, assertions: seq<'a -> 'ignored>, ?because) : And<'a> =
        use _ = t.Assert(true)
        let assertions = assertions |> Seq.toArray

        let exceptions =
            assertions
            |> Seq.indexed
            |> Seq.choose (fun (i, f) ->
                try
                    f t.Subject |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some(i, ex)
            )
            |> Seq.toArray

        if exceptions.Length > 0 then
            let assertionFailuresString =
                exceptions
                |> Seq.map (fun (i, ex) -> $"\n\n[Assertion %i{i + 1}/%i{assertions.Length}]\n%s{ex.Message}")
                |> String.concat ""

            t.Fail(
                "{subject}\n\tshould satisfy all of the {0} supplied assertions{because}, but {1} failed.{2}",
                because,
                string assertions.Length,
                string exceptions.Length,
                assertionFailuresString
            )

        And(t)
