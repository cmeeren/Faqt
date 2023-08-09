namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers


[<Extension>]
type SeqAssertions =


    /// Asserts that all elements in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored, ?because) : And<_> =
        use _ = t.Assert(true, true)

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
