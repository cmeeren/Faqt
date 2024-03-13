namespace Faqt

open System.Collections.Generic
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers
open Faqt.Formatting


[<AutoOpen>]
module private SeqAssertionsHelpers =


    [<Struct>]
    type SatisfyReportItem = { Index: int; Failure: FailureData }


    type ExpectedActualReportItem<'a> = { Index: int; Expected: 'a; Actual: 'a }


    type BeDistinctByReportItem<'a, 'b> = {
        Count: int
        Projected: 'b
        Items: 'a list
    }


    let getMissingFromSupersetAndIsProperSuperset superset subset =
        let freqMap = Dictionary()

        let increment item =
            let key = Key item

            let newCount =
                match freqMap.TryGetValue(key) with
                | false, _ -> 1
                | true, count -> count + 1

            freqMap[key] <- newCount
            newCount

        let decrement item =
            let key = Key item

            let newCount =
                match freqMap.TryGetValue(key) with
                | false, _ -> -1
                | true, count -> count - 1

            freqMap[key] <- newCount
            newCount

        superset |> Seq.iter (increment >> ignore)
        subset |> Seq.iter (decrement >> ignore)

        let containedItemNotInSubset = freqMap |> Seq.exists (fun kvp -> kvp.Value > 0)

        let extraItemsInSubset = ResizeArray()

        for item in subset do
            if increment item < 1 then
                extraItemsInSubset.Add item

        extraItemsInSubset, containedItemNotInSubset


[<Extension>]
type SeqAssertions =


    /// Asserts that all items in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored, ?because) : And<_> =
        use _ = t.Assert(true, true)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let failures =
            t.Subject
            |> Seq.indexed
            |> Seq.choose (fun (i, x) ->
                try
                    use _ = t.AssertItem()
                    assertion x |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some { Index = i; Failure = ex.FailureData }
            )
            |> Seq.toArray

        if failures.Length > 0 then
            t.With("Failures", failures).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the same number of items as the assertion collection, and that each subject
    /// item satisfies the corresponding assertion in the assertion collection.
    [<Extension>]
    static member SatisfyRespectively(t: Testable<#seq<'a>>, assertions: seq<'a -> 'ignored>, ?because) : And<_> =
        use _ = t.Assert(true)

        if isNull assertions then
            nullArg (nameof assertions)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let subjectLength = Seq.stringOptimizedLength t.Subject
        let assertionsLength = Seq.length assertions

        if subjectLength <> assertionsLength then
            t
                .With("Expected length", assertionsLength)
                .With("Actual length", subjectLength)
                .With("Subject value", t.Subject)
                .Fail(because)

        let failures =
            Seq.zip t.Subject assertions
            |> Seq.indexed
            |> Seq.choose (fun (i, (x, assertion)) ->
                try
                    assertion x |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some { Index = i; Failure = ex.FailureData }
            )
            |> Seq.toArray

        if failures.Length > 0 then
            t.With("Failures", failures).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject has the specified length.
    [<Extension>]
    static member HaveLength(t: Testable<#seq<'a>>, expected: int, ?because) : And<_> =
        use _ = t.Assert()

        if expected < 0 then
            invalidArg (nameof expected) "The expected length must be non-negative"

        if isNull (box t.Subject) then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)
        else
            let subjectLength = Seq.stringOptimizedLength t.Subject

            if subjectLength <> expected then
                t
                    .With("Expected", expected)
                    .With("But was", subjectLength)
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is empty. Equivalent to HaveLength(0) (but with a different error message and without
    /// full enumeration). If null should be allowed, see BeNullOrEmpty.
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


    /// Asserts that the subject contains the specified item.
    [<Extension>]
    static member Contain(t: Testable<#seq<'a>>, item: 'a, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || not (Seq.contains item t.Subject) then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        AndDerived(t, item)


    /// Asserts that the subject does not contain the specified item. Passes if the subject is null.
    [<Extension>]
    static member NotContain(t: Testable<#seq<'a>>, item: 'a, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) && Seq.contains item t.Subject then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that all items in the subject are equal to the specified value.
    [<Extension>]
    static member AllBe(t: Testable<#seq<'a>>, expected: 'a, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)
        else
            let differentItems =
                t.Subject
                |> Seq.indexed
                |> Seq.choose (fun (i, actualItem) ->
                    if actualItem <> expected then
                        Some {|
                            Index = i
                            Value = TryFormat actualItem
                        |}
                    else
                        None
                )

            if not (Seq.isEmpty differentItems) then
                t
                    .With("Expected", expected)
                    .With("Failures", differentItems)
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that all items in the subject, when transformed using the specified projection, are equal to the
    /// specified value.
    [<Extension>]
    static member AllBeMappedTo(t: Testable<#seq<'a>>, expected: 'b, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)
        else
            let differentItems =
                t.Subject
                |> Seq.indexed
                |> Seq.choose (fun (i, actualItem) ->
                    let projected = projection actualItem

                    if projected <> expected then
                        Some {|
                            Index = i
                            Projected = TryFormat projected
                            Value = TryFormat actualItem
                        |}
                    else
                        None
                )

            if not (Seq.isEmpty differentItems) then
                t
                    .With("Expected", expected)
                    .With("Failures", differentItems)
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that all items in the subject are equal.
    [<Extension>]
    static member AllBeEqual(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)
        elif not (Seq.stringOptimizedIsEmpty t.Subject) then
            let first = Seq.head t.Subject

            for i, item in Seq.indexed t.Subject do
                if item <> first then
                    t
                        .With("But found", [ {| Index = 0; Value = first |}; {| Index = i; Value = item |} ])
                        .With("Subject value", t.Subject)
                        .Fail(because)

        And(t)


    /// Asserts that all items in the subject are equal by the specified projection.
    [<Extension>]
    static member AllBeEqualBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)
        elif not (Seq.stringOptimizedIsEmpty t.Subject) then
            let first = Seq.head t.Subject
            let firstProjected = projection first

            for i, item in Seq.indexed t.Subject do
                let projected = projection item

                if projected <> firstProjected then
                    t
                        .With(
                            "But found",
                            [
                                {|
                                    Index = 0
                                    Projected = firstProjected
                                    Value = first
                                |}
                                {|
                                    Index = i
                                    Projected = projected
                                    Value = item
                                |}
                            ]
                        )
                        .With("Subject value", t.Subject)
                        .Fail(because)

        And(t)


    /// Asserts that the subject contains the same items in the same order as the specified sequence. Passes if both
    /// sequences are null.
    [<Extension>]
    static member SequenceEqual(t: Testable<#seq<'a>>, expected: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) <> isNull expected then
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
                                Expected = TryFormat expectedItem
                                Actual = TryFormat actualItem
                            }
                        else
                            None
                    )

                if not (Seq.isEmpty differentItems) then
                    t
                        .With("Failures", differentItems)
                        .With("Expected", expected)
                        .With("Actual", t.Subject)
                        .Fail(because)

        And(t)


    /// Asserts that the subject contains the same items (ignoring order) as the specified sequence. Passes if both
    /// sequences are null.
    [<Extension>]
    static member HaveSameItemsAs(t: Testable<#seq<'a>>, expected: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) <> isNull expected then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)
        elif not (isNull (box t.Subject)) then
            let freqMap = Dictionary()
            let additionalSubjectItems = ResizeArray<_>()
            let missingSubjectItems = ResizeArray<_>()

            for x in t.Subject do
                let key = Key x

                match freqMap.TryGetValue(key) with
                | true, count -> freqMap[key] <- count + 1
                | false, _ -> freqMap[key] <- 1

            for x in expected do
                let key = Key x

                match freqMap.TryGetValue(key) with
                | true, 1 -> freqMap.Remove(key) |> ignore
                | true, count -> freqMap[key] <- count - 1
                | false, _ -> missingSubjectItems.Add(x)

            for x in t.Subject do
                let key = Key x

                match freqMap.TryGetValue(key) with
                | true, 1 ->
                    additionalSubjectItems.Add(x)
                    freqMap.Remove(key) |> ignore
                | true, count ->
                    additionalSubjectItems.Add(x)
                    freqMap[key] <- count - 1
                | false, _ -> ()

            if missingSubjectItems.Count > 0 || additionalSubjectItems.Count > 0 then
                t
                    .With("Missing items", missingSubjectItems)
                    .With("Additional items", additionalSubjectItems)
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
                t
                    .With("But length was", subjectLength)
                    .With("Subject value", t.Subject)
                    .Fail(because)

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
                    .With("Subject value", t.Subject)
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
                    .With("Subject value", t.Subject)
                    .Fail(because)

            AndDerived(t, Seq.head matchingItems)


    /// Asserts that the subject contains at most one item.
    [<Extension>]
    static member ContainAtMostOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a option> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)
        else
            let subjectLength = Seq.stringOptimizedLength t.Subject

            if subjectLength > 1 then
                t
                    .With("But length was", subjectLength)
                    .With("Subject value", t.Subject)
                    .Fail(because)

        AndDerived(t, Seq.tryHead t.Subject)


    /// Asserts that the subject contains at most one item matching the predicate.
    [<Extension>]
    static member ContainAtMostOneItemMatching
        (
            t: Testable<#seq<'a>>,
            predicate: 'a -> bool,
            ?because
        ) : AndDerived<_, 'a option> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)
        else
            let matchingItems = t.Subject |> Seq.filter predicate
            let matchingLength = Seq.stringOptimizedLength matchingItems

            if matchingLength > 1 then
                t
                    .With("But found", matchingLength)
                    .With("Matching items", matchingItems)
                    .With("Subject value", t.Subject)
                    .Fail(because)

            AndDerived(t, Seq.tryHead matchingItems)


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
                t.With("But found", 0).With("Subject value", t.Subject).Fail(because)

            AndDerived(t, matchingItems)


    /// Asserts that the subject does not contains items matching the predicate. Passes if the subject is null.
    [<Extension>]
    static member NotContainItemsMatching(t: Testable<#seq<'a>>, predicate: 'a -> bool, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) then
            let matchingItems = t.Subject |> Seq.filter predicate
            let numMatching = Seq.stringOptimizedLength matchingItems

            if numMatching > 0 then
                t
                    .With("But found", numMatching)
                    .With("Matching items", matchingItems)
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is distinct.
    [<Extension>]
    static member BeDistinct(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let nonDistinctItemsWithCounts =
            t.Subject |> Seq.countBy id |> Seq.filter (fun (_, c) -> c > 1)

        if not (Seq.isEmpty nonDistinctItemsWithCounts) then
            let items =
                nonDistinctItemsWithCounts
                |> Seq.map (fun (x, c) -> {| Count = c; Item = TryFormat x |})
                |> Seq.toList

            t.With("Duplicates", items).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is distinct by the specified projection.
    [<Extension>]
    static member BeDistinctBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let duplicates =
            t.Subject
            |> Seq.groupBy projection
            |> Seq.choose (fun (p, xs) ->
                let xs = Seq.toList xs

                if xs.Length > 1 then
                    Some {
                        Count = xs.Length
                        Projected = TryFormat p
                        Items = xs |> List.map (box >> TryFormat)
                    }
                else
                    None
            )
            |> Seq.toList

        if not (Seq.isEmpty duplicates) then
            t.With("Duplicates", duplicates).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order.
    [<Extension>]
    static member BeAscending(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if a > b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order by the specified projection.
    [<Extension>]
    static member BeAscendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if a' > b' then
                t
                    .With(
                        "But found",
                        [
                            {|
                                Index = i
                                Item = TryFormat a
                                Projected = TryFormat a'
                            |}
                            {|
                                Index = i + 1
                                Item = TryFormat b
                                Projected = TryFormat b'
                            |}
                        ]
                    )
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in descending order.
    [<Extension>]
    static member BeDescending(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if a < b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in descending order by the specified projection.
    [<Extension>]
    static member BeDescendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if a' < b' then
                t
                    .With(
                        "But found",
                        [
                            {|
                                Index = i
                                Item = TryFormat a
                                Projected = TryFormat a'
                            |}
                            {|
                                Index = i + 1
                                Item = TryFormat b
                                Projected = TryFormat b'
                            |}
                        ]
                    )
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order (i.e., is ascending and distinct).
    [<Extension>]
    static member BeStrictlyAscending(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if a >= b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order (i.e., is ascending and distinct) by the specified projection.
    [<Extension>]
    static member BeStrictlyAscendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if a' >= b' then
                t
                    .With(
                        "But found",
                        [
                            {|
                                Index = i
                                Item = TryFormat a
                                Projected = TryFormat a'
                            |}
                            {|
                                Index = i + 1
                                Item = TryFormat b
                                Projected = TryFormat b'
                            |}
                        ]
                    )
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in strictly descending order (i.e., is descending and distinct).
    [<Extension>]
    static member BeStrictlyDescending(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if a <= b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in descending order (i.e., is descending and distinct) by the specified projection.
    [<Extension>]
    static member BeStrictlyDescendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if a' <= b' then
                t
                    .With(
                        "But found",
                        [
                            {|
                                Index = i
                                Item = TryFormat a
                                Projected = TryFormat a'
                            |}
                            {|
                                Index = i + 1
                                Item = TryFormat b
                                Projected = TryFormat b'
                            |}
                        ]
                    )
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    [<Extension>]
    static member private BeSupersetOf'(t: Testable<#seq<'a>>, subset: seq<'a>, proper: bool, ?because) : And<_> =
        if isNull subset then
            nullArg (nameof subset)

        if isNull (box t.Subject) then
            t.With("Subset", subset).With("But was", t.Subject).Fail(because)

        let extraItemsInSubset, containedItemNotInSubset =
            getMissingFromSupersetAndIsProperSuperset t.Subject subset

        if extraItemsInSubset.Count > 0 then
            t
                .With("Subset", subset)
                .With("But lacked", extraItemsInSubset)
                .With("Subject value", t.Subject)
                .Fail(because)
        elif proper && not containedItemNotInSubset then
            t
                .With("Subset", subset)
                .With("But had no additional items", [])
                .With("Subject value", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject contains all items in the specified non-null sequence (including any duplicates).
    [<Extension>]
    static member BeSupersetOf(t: Testable<#seq<'a>>, subset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSupersetOf'(subset, false, ?because = because)


    /// Asserts that the subject contains all items in the specified non-null sequence (including any duplicates) and at
    /// least one additional item.
    [<Extension>]
    static member BeProperSupersetOf(t: Testable<#seq<'a>>, subset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSupersetOf'(subset, true, ?because = because)


    [<Extension>]
    static member private BeSubsetOf'(t: Testable<#seq<'a>>, superset: seq<'a>, proper: bool, ?because) : And<_> =
        if isNull superset then
            nullArg (nameof superset)

        if isNull (box t.Subject) then
            t.With("Superset", superset).With("But was", t.Subject).Fail(because)

        let extraItemsInSubject, containedItemNotInSubset =
            getMissingFromSupersetAndIsProperSuperset superset t.Subject

        if extraItemsInSubject.Count > 0 then
            t
                .With("Superset", superset)
                .With("But had extra items", extraItemsInSubject)
                .With("Subject value", t.Subject)
                .Fail(because)
        elif proper && not containedItemNotInSubset then
            t
                .With("Superset", superset)
                .With("But superset had no additional items", [])
                .With("Subject value", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the specified non-null sequence contains all items in the subject (including any duplicates).
    [<Extension>]
    static member BeSubsetOf(t: Testable<#seq<'a>>, superset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSubsetOf'(superset, false, ?because = because)


    /// Asserts that the specified non-null sequence contains all items in the subject (including any duplicates) and at
    /// least one additional item.
    [<Extension>]
    static member BeProperSubsetOf(t: Testable<#seq<'a>>, superset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSubsetOf'(superset, true, ?because = because)


    /// Asserts that the subject has at least one item in common with the other non-null sequence. Fails if one or both
    /// sequences are empty.
    [<Extension>]
    static member IntersectWith(t: Testable<#seq<'a>>, other: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull other then
            nullArg (nameof other)

        if isNull (box t.Subject) then
            t.With("Other", other).With("But was", t.Subject).Fail(because)

        let set = HashSet(t.Subject :> seq<'a>)

        if not (other |> Seq.exists set.Contains) then
            t
                .With("Other", other)
                .With("But had no common items", [])
                .With("Subject value", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject has no items in common with the other non-null sequence. Passes if the subject is null
    /// or if either sequence is empty.
    [<Extension>]
    static member NotIntersectWith(t: Testable<#seq<'a>>, other: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull other then
            nullArg (nameof other)

        if not (isNull (box t.Subject)) then
            let set = HashSet(t.Subject :> seq<'a>)
            set.IntersectWith(other)

            if set.Count > 0 then
                t
                    .With("Other", other)
                    .With("But found common items", set)
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)
