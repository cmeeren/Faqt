namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


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

        let subjectLength = Seq.stringOptimizedLength t.Subject

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

        let subjectLength = Seq.stringOptimizedLength t.Subject
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


    /// Asserts that the subject has the specified length.
    [<Extension>]
    static member HaveLength(t: Testable<#seq<'a>>, expected: int, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.Fail("{subject}\n\tshould have length\n{0}\n\t{because}but was\n{actual}", because, format expected)
        else
            let subjectLength = Seq.stringOptimizedLength t.Subject

            if subjectLength <> expected then
                t.Fail(
                    "{subject}\n\tshould have length\n{0}\n\t{because}but length was\n{1}\n\n{actual}",
                    because,
                    format expected,
                    format subjectLength
                )

            And(t)


    /// Asserts that the subject is empty. Equivalent to HaveLength(0) (but with a different error message and without
    /// full enumeration).
    [<Extension>]
    static member BeEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) || not (Seq.stringOptimizedIsEmpty t.Subject) then
            t.Fail("{subject}\n\tshould be empty{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is not empty.
    [<Extension>]
    static member NotBeEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.Fail("{subject}\n\tshould not be empty{because}, but was\n{actual}", because)
        elif Seq.stringOptimizedIsEmpty t.Subject then
            t.Fail("{subject}\n\tshould not be empty{because}, but was empty.", because)

        And(t)


    /// Asserts that the subject is null or empty.
    [<Extension>]
    static member BeNullOrEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull t.Subject || Seq.stringOptimizedIsEmpty t.Subject) then
            t.Fail("{subject}\n\tshould be null or empty{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject contains the specified item, as determined using the default equality comparison (=).
    [<Extension>]
    static member Contain(t: Testable<#seq<'a>>, expected: 'a, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || not (Seq.contains expected t.Subject) then
            t.Fail("{subject}\n\tshould contain\n{0}\n\t{because}but was\n{actual}", because, format expected)

        AndDerived(t, expected)


    /// Asserts that the subject does not contain the specified item, as determined using the default equality
    /// comparison (=). Passes if the subject is null.
    [<Extension>]
    static member NotContain(t: Testable<#seq<'a>>, expected: 'a, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) && Seq.contains expected t.Subject then
            t.Fail("{subject}\n\tshould not contain\n{0}\n\t{because}but was\n{actual}", because, format expected)

        And(t)
