namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers


[<Extension>]
type SeqAssertions =


    /// Asserts that all elements in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored, ?because) : And<_> =
        use _ = t.Assert(true, true)

        if isNull (box t.Subject) then
            t.Fail(
                "{subject}\n\tshould only contain items satisfying the supplied assertion{because}, but was\n{actual}",
                because
            )

        let subjectLength = Seq.length t.Subject

        let exceptions =
            t.Subject
            |> Seq.indexed
            |> Seq.choose (fun (i, x) ->
                try
                    use _ = t.AssertItem()
                    assertion x |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some(i, ex)
            )
            |> Seq.toArray

        if exceptions.Length > 0 then
            let assertionFailuresString =
                exceptions
                |> Seq.map (fun (i, ex) -> $"\n\n[Item %i{i + 1}/%i{subjectLength}]\n%s{ex.Message}")
                |> String.concat ""

            t.Fail(
                "{subject}\n\tshould only contain items satisfying the supplied assertion{because}, but {0} of {1} items failed.{2}",
                because,
                string exceptions.Length,
                string subjectLength,
                assertionFailuresString
            )

        And(t)


    /// Asserts that the subject contains the same number of elements as the assertion collection, and that each
    /// subject element satisfies the corresponding assertion in the assertion collection.
    [<Extension>]
    static member SatisfyRespectively(t: Testable<#seq<'a>>, assertions: seq<'a -> 'ignored>, ?because) : And<_> =
        use _ = t.Assert(true)

        if isNull (box t.Subject) then
            t.Fail(
                "{subject}\n\tshould contain items respectively satisfying the specified assertions{because}, but was\n{actual}",
                because
            )

        let subjectLength = Seq.length t.Subject
        let assertionsLength = Seq.length assertions

        if subjectLength <> assertionsLength then
            t.Fail(
                "{subject}\n\tshould contain items respectively satisfying the\n{0}\n\tspecified assertions{because}, but actual length was\n{1}\n\n{actual}",
                because,
                string assertionsLength,
                string subjectLength
            )

        let exceptions =
            Seq.zip t.Subject assertions
            |> Seq.indexed
            |> Seq.choose (fun (i, (x, assertion)) ->
                try
                    assertion x |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some(i, ex)
            )
            |> Seq.toArray

        if exceptions.Length > 0 then
            let assertionFailuresString =
                exceptions
                |> Seq.map (fun (i, ex) -> $"\n\n[Item %i{i + 1}/%i{subjectLength}]\n%s{ex.Message}")
                |> String.concat ""

            t.Fail(
                "{subject}\n\tshould contain items respectively satisfying the specified assertions{because}, but {0} of {1} items failed.{2}",
                because,
                string exceptions.Length,
                string subjectLength,
                assertionFailuresString
            )

        And(t)
