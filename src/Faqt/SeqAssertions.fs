namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


type private SatisfyReportItem = { Index: int; Failure: FailureData }


type private ExpectedActualReportItem<'a> = { Index: int; Expected: 'a; Actual: 'a }


type private BeDistinctByReportItem<'a, 'b> = {
    Count: int
    Projected: 'b
    Items: 'a list
}


[<Extension>]
type SeqAssertions =


    /// Asserts that all items in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored, ?because) : And<_> =
        use _ = t.Assert(true, true)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

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
            t
                .With("Failures", exceptions |> Array.map (fun (i, ex) -> { Index = i; Failure = ex.FailureData }))
                .With("Value", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject contains the same number of items as the assertion collection, and that each subject
    /// item satisfies the corresponding assertion in the assertion collection.
    [<Extension>]
    static member SatisfyRespectively(t: Testable<#seq<'a>>, assertions: seq<'a -> 'ignored>, ?because) : And<_> =
        use _ = t.Assert(true)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let subjectLength = Seq.stringOptimizedLength t.Subject
        let assertionsLength = Seq.stringOptimizedLength assertions

        if subjectLength <> assertionsLength then
            t
                .With("Expected length", assertionsLength)
                .With("Actual length", subjectLength)
                .With("Value", t.Subject)
                .Fail(because)

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
            t
                .With("Failures", exceptions |> Array.map (fun (i, ex) -> { Index = i; Failure = ex.FailureData }))
                .With("Value", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject has the specified length.
    [<Extension>]
    static member HaveLength(t: Testable<#seq<'a>>, expected: int, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)
        else
            let subjectLength = Seq.stringOptimizedLength t.Subject

            if subjectLength <> expected then
                t
                    .With("Expected", expected)
                    .With("But was", subjectLength)
                    .With("Value", t.Subject)
                    .Fail(because)

            And(t)


    /// Asserts that the subject is empty. Equivalent to HaveLength(0) (but with a different error message and without
    /// full enumeration).
    [<Extension>]
    static member BeEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) || not (Seq.stringOptimizedIsEmpty t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not empty.
    [<Extension>]
    static member NotBeEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) || Seq.stringOptimizedIsEmpty t.Subject then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is null or empty.
    [<Extension>]
    static member BeNullOrEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull t.Subject || Seq.stringOptimizedIsEmpty t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the specified item, as determined using the default equality comparison (=).
    [<Extension>]
    static member Contain(t: Testable<#seq<'a>>, item: 'a, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || not (Seq.contains item t.Subject) then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        AndDerived(t, item)


    /// Asserts that the subject does not contain the specified item, as determined using the default equality
    /// comparison (=). Passes if the subject is null.
    [<Extension>]
    static member NotContain(t: Testable<#seq<'a>>, item: 'a, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) && Seq.contains item t.Subject then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the same items in the same order as the specified sequence, as determined
    /// using the default equality comparison (=). Passes if both sequences are null.
    [<Extension>]
    static member SequenceEqual(t: Testable<#seq<'a>>, expected: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) && not (isNull expected) then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)
        elif not (isNull (box t.Subject)) then
            let subjectLength = Seq.stringOptimizedLength t.Subject
            let expectedLength = Seq.stringOptimizedLength expected

            if subjectLength <> expectedLength then
                t
                    .With("Expected length", expectedLength)
                    .With("Actual length", subjectLength)
                    .With("Expected", expected)
                    .With("Actual", t.Subject)
                    .Fail(because)
            else
                let differentItems =
                    Seq.zip t.Subject expected
                    |> Seq.indexed
                    |> Seq.choose (fun (i, (actualItem, expectedItem)) ->
                        if actualItem <> expectedItem then
                            Some {
                                Index = i
                                Expected = expectedItem
                                Actual = actualItem
                            }
                        else
                            None
                    )

                if not (Seq.stringOptimizedIsEmpty differentItems) then
                    t
                        .With("Failures", differentItems)
                        .With("Expected", expected)
                        .With("Actual", t.Subject)
                        .Fail(because)

        And(t)


    /// Asserts that the subject contains exactly one item.
    [<Extension>]
    static member ContainExactlyOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)
        else
            let subjectLength = Seq.stringOptimizedLength t.Subject

            if subjectLength <> 1 then
                t.With("But length was", subjectLength).With("Value", t.Subject).Fail(because)

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
            t.With("But was", t.Subject).Fail(because)
        else
            let matchingItems = t.Subject |> Seq.filter predicate
            let matchingLength = Seq.stringOptimizedLength matchingItems

            if matchingLength <> 1 then
                t
                    .With("But found", matchingLength)
                    .With("Matching items", matchingItems)
                    .With("Value", t.Subject)
                    .Fail(because)

            AndDerived(t, Seq.head matchingItems)


    /// Asserts that the subject contains at least one item. Equivalent to NotBeEmpty, but with a different error
    /// message and allows continuing to assert on the first matching item.
    [<Extension>]
    static member ContainAtLeastOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || Seq.stringOptimizedIsEmpty t.Subject then
            t.With("But was", t.Subject).Fail(because)

        AndDerived(t, Seq.head t.Subject)


    /// Asserts that the subject contains at least one item matching the predicate. Similar to ContainItemsMatching, but
    /// allows continuing to assert on the first matching item instead of all matching items.
    [<Extension>]
    static member ContainAtLeastOneItemMatching
        (
            t: Testable<#seq<'a>>,
            predicate: 'a -> bool,
            ?because
        ) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)
        else
            let matchingItems = t.Subject |> Seq.filter predicate

            if Seq.stringOptimizedIsEmpty matchingItems then
                t
                    .With("But found", 0)
                    .With("Matching items", matchingItems)
                    .With("Value", t.Subject)
                    .Fail(because)

            AndDerived(t, Seq.head matchingItems)


    /// Asserts that the subject contains at least one item matching the predicate. Similar to
    /// ContainAtLeastOneItemMatching, but allows continuing to assert on all the matching items instead of just the
    /// first.
    [<Extension>]
    static member ContainItemsMatching
        (
            t: Testable<#seq<'a>>,
            predicate: 'a -> bool,
            ?because
        ) : AndDerived<_, seq<'a>> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)
        else
            let matchingItems = t.Subject |> Seq.filter predicate

            if Seq.stringOptimizedIsEmpty matchingItems then
                t.With("But found", 0).With("Value", t.Subject).Fail(because)

            AndDerived(t, matchingItems)


    /// Asserts that the subject is distinct, as determined using default equality comparison (=). Passes if the subject
    /// is null.
    [<Extension>]
    static member BeDistinct(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) then
            let nonDistinctItemsWithCounts =
                t.Subject |> Seq.countBy id |> Seq.filter (fun (_, c) -> c > 1)

            if not (Seq.stringOptimizedIsEmpty nonDistinctItemsWithCounts) then
                let items =
                    nonDistinctItemsWithCounts
                    |> Seq.map (fun (x, c) -> {| Count = c; Item = TryFormat x |})
                    |> Seq.toList

                t.With("Duplicates", items).With("Value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is distinct by the specified projection, as determined using default equality
    /// comparison (=). Passes if the subject is null.
    [<Extension>]
    static member BeDistinctBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) then
            let duplicates =
                t.Subject
                |> Seq.groupBy projection
                |> Seq.choose (fun (p, xs) ->
                    let xs = Seq.toList xs

                    if xs.Length > 1 then
                        Some {
                            Count = xs.Length
                            Projected = p
                            Items = xs |> List.map (box >> TryFormat)
                        }
                    else
                        None
                )
                |> Seq.toList

            if not (Seq.stringOptimizedIsEmpty duplicates) then
                t.With("Duplicates", duplicates).With("Value", t.Subject).Fail(because)

        And(t)
