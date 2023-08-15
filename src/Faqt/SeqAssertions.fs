namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<Extension>]
type SeqAssertions =


    /// Asserts that all items in the collection satisfy the supplied assertion.
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


    /// Asserts that the subject contains the same number of items as the assertion collection, and that each subject
    /// item satisfies the corresponding assertion in the assertion collection.
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


    /// Asserts that the subject contains the same items in the same order as the specified sequence, as determined
    /// using the default equality comparison (=). Passes if both sequences are null.
    [<Extension>]
    static member SequenceEqual(t: Testable<#seq<'a>>, expected: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) && not (isNull expected) then
            t.Fail(
                "{subject}\n\tshould be sequence equal to\n{0}\n\t{because}but was\n{actual}",
                because,
                format expected
            )
        elif not (isNull (box t.Subject)) then
            let subjectLength = Seq.length t.Subject
            let expectedLength = Seq.length expected

            if subjectLength <> expectedLength then
                t.Fail(
                    "{subject}\n\tshould be sequence equal to\n{0}\n\t{because}but expected length\n{1}\n\tis different from actual length\n{2}\n\n{actual}",
                    because,
                    format expected,
                    expectedLength.ToString(),
                    subjectLength.ToString()
                )
            else
                for i, (actualItem, expectedItem) in Seq.zip t.Subject expected |> Seq.indexed do
                    if actualItem <> expectedItem then
                        t.Fail(
                            "{subject}\n\tshould be sequence equal to\n{0}\n\t{because}but actual item at index {1}\n{2}\n\tis not equal to expected item\n{3}\n\tFull sequence:\n{actual}",
                            because,
                            format expected,
                            i.ToString(),
                            format actualItem,
                            format expectedItem
                        )

        And(t)


    /// Asserts that the subject contains exactly one item.
    [<Extension>]
    static member ContainExactlyOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.Fail("{subject}\n\tshould contain exactly one item{because}, but was\n{actual}", because)
        else
            let subjectLength = Seq.length t.Subject

            if subjectLength <> 1 then
                t.Fail(
                    "{subject}\n\tshould contain exactly one item{because}, but actual length was\n{0}\n\n{actual}",
                    because,
                    subjectLength.ToString()
                )

        AndDerived(t, Seq.head t.Subject)


    /// Asserts that the subject contains exactly one item matching the predicate.
    [<Extension>]
    static member ContainExactlyOneItemMatching
        (
            t: Testable<#seq<'a>>,
            predicate: 'a -> bool,
            ?because
        ) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.Fail(
                "{subject}\n\tshould contain exactly one item matching the specified predicate{because}, but was\n{actual}",
                because
            )
        else
            let matchingItems = t.Subject |> Seq.filter predicate
            let matchingLength = Seq.length matchingItems

            if matchingLength <> 1 then
                t.Fail(
                    "{subject}\n\tshould contain exactly one item matching the specified predicate{because}, but found\n{0}\n\titems matching the predicate:\n{1}\n\tFull sequence:\n{actual}",
                    because,
                    matchingLength.ToString(),
                    format matchingItems
                )

            AndDerived(t, Seq.head matchingItems)


    /// Asserts that the subject contains at least one item. Equivalent to NotBeEmpty, but with a different error
    /// message and allows continuing to assert on the item.
    [<Extension>]
    static member ContainAtLeastOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.Fail("{subject}\n\tshould contain at least one item{because}, but was\n{actual}", because)
        else if Seq.isEmpty t.Subject then
            t.Fail("{subject}\n\tshould contain at least one item{because}, but was empty.", because)

        AndDerived(t, Seq.head t.Subject)
