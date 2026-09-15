namespace Faqt

open System
open System.Collections.Generic
open System.Globalization
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers
open Faqt.Formatting


[<AutoOpen>]
module private SeqAssertionsHelpers =


    [<Struct>]
    type SatisfyReportFailureItem = { Index: int; Failure: FailureData }


    [<Struct>]
    type SatisfyReportExceptionItem = { Index: int; Exception: TryFormat }


    type ExpectedActualReportItem<'a> = { Index: int; Expected: 'a; Actual: 'a }


    type BeDistinctByReportItem<'a, 'b> = {
        Count: int
        Projected: 'b
        Items: 'a list
    }


    [<Struct>]
    type ZeroOneMany<'a> =
        | Zero
        | One of item: 'a
        | Many of count: int


    [<Struct>]
    type ZeroOneManyMatches<'a> =
        | ZeroMatches
        | OneMatch of item: 'a
        | ManyMatches of count: int * matchingItems: 'a list


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
        let subset = Seq.toArray subset
        subset |> Seq.iter (decrement >> ignore)

        let containedItemNotInSubset = freqMap |> Seq.exists (fun kvp -> kvp.Value > 0)

        let extraItemsInSubset = ResizeArray()

        for item in subset do
            if increment item < 1 then
                extraItemsInSubset.Add item

        extraItemsInSubset, containedItemNotInSubset


    let tryGetFirstItem (source: seq<'a>) =
        use enumerator = source.GetEnumerator()

        if enumerator.MoveNext() then
            ValueSome enumerator.Current
        else
            ValueNone


    let tryGetFirstMatchingItem predicate (source: seq<'a>) =
        use enumerator = source.GetEnumerator()

        let mutable matchingItem = ValueNone

        while ValueOption.isNone matchingItem && enumerator.MoveNext() do
            let item = enumerator.Current

            if predicate item then
                matchingItem <- ValueSome item

        matchingItem


    let getZeroOneManyItems (source: seq<'a>) =
        use enumerator = source.GetEnumerator()

        if not (enumerator.MoveNext()) then
            Zero
        else
            let firstItem = enumerator.Current

            if not (enumerator.MoveNext()) then
                One firstItem
            else
                let mutable count = 2

                while enumerator.MoveNext() do
                    count <- count + 1

                Many count


    let getZeroOneManyMatchingItems predicate (source: seq<'a>) =
        use enumerator = source.GetEnumerator()

        let mutable firstMatchingItem = Unchecked.defaultof<'a>
        let mutable count = 0
        let mutable matchingItems = Unchecked.defaultof<ResizeArray<'a>>

        while enumerator.MoveNext() do
            let item = enumerator.Current

            if predicate item then
                count <- count + 1

                match count with
                | 1 -> firstMatchingItem <- item
                | 2 ->
                    let items = ResizeArray()
                    items.Add(firstMatchingItem)
                    items.Add(item)
                    matchingItems <- items
                | _ -> matchingItems.Add(item)

        match count with
        | 0 -> ZeroMatches
        | 1 -> OneMatch firstMatchingItem
        | _ -> ManyMatches(count, List.ofSeq matchingItems)


    let countRemainingItems processedCount hasCurrent (enumerator: IEnumerator<'a>) =
        let mutable count = processedCount

        if hasCurrent then
            count <- count + 1

            while enumerator.MoveNext() do
                count <- count + 1

        count


[<Extension>]
type SeqAssertions =


    /// Asserts that all items in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored, ?because) : And<_> =
        use _ = t.Assert(true, true)

        let failures =
            t.Subject
            |> Seq.indexed
            |> Seq.choose (fun (i, x) ->
                try
                    use _ = t.AssertItem()
                    assertion x |> ignore
                    None
                with
                | :? AssertionFailedException as ex -> { Index = i; Failure = ex.FailureData } |> box |> Some
                | ex -> { Index = i; Exception = TryFormat ex } |> box |> Some
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

        use subjectEnumerator = t.Subject.GetEnumerator()
        use assertionsEnumerator = assertions.GetEnumerator()

        let mutable index = 0
        let mutable subjectHasNext = subjectEnumerator.MoveNext()
        let mutable assertionsHasNext = assertionsEnumerator.MoveNext()
        let mutable failures = Unchecked.defaultof<ResizeArray<obj>>

        let addFailure failure =
            if isNull failures then
                failures <- ResizeArray()

            failures.Add failure

        while subjectHasNext && assertionsHasNext do
            try
                assertionsEnumerator.Current subjectEnumerator.Current |> ignore
            with
            | :? AssertionFailedException as ex ->
                {
                    Index = index
                    Failure = ex.FailureData
                }
                |> box
                |> addFailure
            | ex ->
                {
                    Index = index
                    Exception = TryFormat ex
                }
                |> box
                |> addFailure

            index <- index + 1
            subjectHasNext <- subjectEnumerator.MoveNext()
            assertionsHasNext <- assertionsEnumerator.MoveNext()

        if subjectHasNext <> assertionsHasNext then
            let subjectLength = countRemainingItems index subjectHasNext subjectEnumerator

            let assertionsLength =
                countRemainingItems index assertionsHasNext assertionsEnumerator

            t
                .With("Expected length", assertionsLength)
                .With("Actual length", subjectLength)
                .With("Subject value", t.Subject)
                .Fail(because)

        if not (isNull failures) then
            t.With("Failures", failures.ToArray()).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject has the specified length.
    [<Extension>]
    static member HaveLength(t: Testable<#seq<'a>>, expected: int, ?because) : And<_> =
        use _ = t.Assert()

        if expected < 0 then
            invalidArg (nameof expected) "The expected length must be non-negative"

        let subjectLength = Seq.stringOptimizedLength t.Subject

        if subjectLength <> expected then
            t.With("Expected", expected).With("But was", subjectLength).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is empty. Equivalent to HaveLength(0) (but with a different error message and without
    /// full enumeration). If null should be allowed, see BeNullOrEmpty.
    [<Extension>]
    static member BeEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if not (Seq.stringOptimizedIsEmpty t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not empty.
    [<Extension>]
    static member NotBeEmpty(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        if Seq.stringOptimizedIsEmpty t.Subject then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is null or empty.
    [<Extension>]
    static member BeNullOrEmpty(t: Testable<#seq<'a> | null>, ?because) : And<_> =
        use _ = t.Assert()

        match t.Subject with
        | null -> ()
        | sub when Seq.stringOptimizedIsEmpty sub -> ()
        | sub -> t.With("But was", sub).Fail(because)

        And(t)


    /// Asserts that the subject contains the specified item.
    [<Extension>]
    static member Contain(t: Testable<#seq<'a>>, item: 'a, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        match tryGetFirstMatchingItem ((=) item) t.Subject with
        | ValueSome actualItem -> AndDerived(t, actualItem)
        | ValueNone -> t.With("Item", item).With("But was", t.Subject).Fail(because)


    /// Asserts that the subject does not contain the specified item.
    [<Extension>]
    static member NotContain(t: Testable<#seq<'a>>, item: 'a, ?because) : And<_> =
        use _ = t.Assert()

        if Seq.contains item t.Subject then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that all items in the subject are equal to the specified value.
    [<Extension>]
    static member AllBe(t: Testable<#seq<'a>>, expected: 'a, ?because) : And<_> =
        use _ = t.Assert()

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
            t.With("Expected", expected).With("Failures", differentItems).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that all items in the subject, when transformed using the specified projection, are equal to the
    /// specified value.
    [<Extension>]
    static member AllBeMappedTo(t: Testable<#seq<'a>>, expected: 'b, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

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
            t.With("Expected", expected).With("Failures", differentItems).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that all items in the subject are equal.
    [<Extension>]
    static member AllBeEqual(t: Testable<#seq<'a>>, ?because) : And<_> =
        use _ = t.Assert()

        use enumerator = t.Subject.GetEnumerator()

        if enumerator.MoveNext() then
            let first = enumerator.Current
            let mutable index = 1
            let mutable hasNext = enumerator.MoveNext()

            while hasNext do
                let item = enumerator.Current

                if item <> first then
                    t
                        .With("But found", [ {| Index = 0; Value = first |}; {| Index = index; Value = item |} ])
                        .With("Subject value", t.Subject)
                        .Fail(because)

                index <- index + 1
                hasNext <- enumerator.MoveNext()

        And(t)


    /// Asserts that all items in the subject are equal by the specified projection.
    [<Extension>]
    static member AllBeEqualBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        use enumerator = t.Subject.GetEnumerator()

        if enumerator.MoveNext() then
            let first = enumerator.Current
            let firstProjected = projection first
            let mutable index = 1
            let mutable hasNext = enumerator.MoveNext()

            while hasNext do
                let item = enumerator.Current
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
                                    Index = index
                                    Projected = projected
                                    Value = item
                                |}
                            ]
                        )
                        .With("Subject value", t.Subject)
                        .Fail(because)

                index <- index + 1
                hasNext <- enumerator.MoveNext()

        And(t)


    /// Asserts that the subject contains the same items in the same order as the specified sequence.
    [<Extension>]
    static member SequenceEqual(t: Testable<#seq<'a>>, expected: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        use subjectEnumerator = t.Subject.GetEnumerator()
        use expectedEnumerator = expected.GetEnumerator()

        let mutable index = 0
        let mutable subjectHasNext = subjectEnumerator.MoveNext()
        let mutable expectedHasNext = expectedEnumerator.MoveNext()

        let mutable failures =
            Unchecked.defaultof<ResizeArray<ExpectedActualReportItem<TryFormat>>>

        while subjectHasNext && expectedHasNext do
            let actualItem = subjectEnumerator.Current
            let expectedItem = expectedEnumerator.Current

            if actualItem <> expectedItem then
                if isNull failures then
                    failures <- ResizeArray()

                failures.Add(
                    {
                        Index = index
                        Expected = TryFormat expectedItem
                        Actual = TryFormat actualItem
                    }
                )

            index <- index + 1
            subjectHasNext <- subjectEnumerator.MoveNext()
            expectedHasNext <- expectedEnumerator.MoveNext()

        if subjectHasNext <> expectedHasNext then
            let subjectLength = countRemainingItems index subjectHasNext subjectEnumerator
            let expectedLength = countRemainingItems index expectedHasNext expectedEnumerator

            t
                .With("Expected length", expectedLength)
                .With("Actual length", subjectLength)
                .With("Expected", expected)
                .With("Actual", t.Subject)
                .Fail(because)
        elif not (isNull failures) then
            t.With("Failures", failures).With("Expected", expected).With("Actual", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the same items (ignoring order) as the specified sequence.
    [<Extension>]
    static member HaveSameItemsAs(t: Testable<#seq<'a>>, expected: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        let subjectItems = Seq.toArray t.Subject
        let freqMap = Dictionary()
        let additionalSubjectItems = ResizeArray<_>()
        let missingSubjectItems = ResizeArray<_>()

        for x in subjectItems do
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

        for x in subjectItems do
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
                .With("Actual", subjectItems)
                .Fail(because)

        And(t)


    /// Asserts that the subject contains exactly one item.
    [<Extension>]
    static member ContainExactlyOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        match getZeroOneManyItems t.Subject with
        | One item -> AndDerived(t, item)
        | Zero -> t.With("But length was", 0).With("Subject value", t.Subject).Fail(because)
        | Many count -> t.With("But length was", count).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject contains exactly one item matching the predicate.
    [<Extension>]
    static member ContainExactlyOneItemMatching
        (t: Testable<#seq<'a>>, predicate: 'a -> bool, ?because)
        : AndDerived<_, 'a> =
        use _ = t.Assert()

        match getZeroOneManyMatchingItems predicate t.Subject with
        | OneMatch item -> AndDerived(t, item)
        | ZeroMatches ->
            t.With("But found", 0).With("Matching items", []).With("Subject value", t.Subject).Fail(because)
        | ManyMatches(count, matchingItems) ->
            t
                .With("But found", count)
                .With("Matching items", matchingItems)
                .With("Subject value", t.Subject)
                .Fail(because)


    /// Asserts that the subject contains at least one item. Equivalent to NotBeEmpty, but with a different error
    /// message and allows continuing to assert on the first matching item.
    [<Extension>]
    static member ContainAtLeastOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        match tryGetFirstItem t.Subject with
        | ValueSome item -> AndDerived(t, item)
        | ValueNone -> t.With("But was", t.Subject).Fail(because)


    /// Asserts that the subject contains at least one item matching the predicate. Similar to ContainItemsMatching, but
    /// allows continuing to assert on the first matching item instead of all matching items.
    [<Extension>]
    static member ContainAtLeastOneItemMatching
        (t: Testable<#seq<'a>>, predicate: 'a -> bool, ?because)
        : AndDerived<_, 'a> =
        use _ = t.Assert()

        match tryGetFirstMatchingItem predicate t.Subject with
        | ValueSome item -> AndDerived(t, item)
        | ValueNone -> t.With("But found", 0).With("Matching items", []).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject contains at most one item.
    [<Extension>]
    static member ContainAtMostOneItem(t: Testable<#seq<'a>>, ?because) : AndDerived<_, 'a option> =
        use _ = t.Assert()

        match getZeroOneManyItems t.Subject with
        | Zero -> AndDerived(t, None)
        | One item -> AndDerived(t, Some item)
        | Many count -> t.With("But length was", count).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject contains at most one item matching the predicate.
    [<Extension>]
    static member ContainAtMostOneItemMatching
        (t: Testable<#seq<'a>>, predicate: 'a -> bool, ?because)
        : AndDerived<_, 'a option> =
        use _ = t.Assert()

        match getZeroOneManyMatchingItems predicate t.Subject with
        | ZeroMatches -> AndDerived(t, None)
        | OneMatch item -> AndDerived(t, Some item)
        | ManyMatches(count, matchingItems) ->
            t
                .With("But found", count)
                .With("Matching items", matchingItems)
                .With("Subject value", t.Subject)
                .Fail(because)


    /// Asserts that the subject contains at least one item matching the predicate. Similar to
    /// ContainAtLeastOneItemMatching, but allows continuing to assert on all the matching items instead of just the
    /// first.
    ///
    /// Stops at the first match. Enumerating the derived matching items re-enumerates the source and re-evaluates
    /// the predicate. Materialize single-pass sources before calling this assertion if using the derived items.
    [<Extension>]
    static member ContainItemsMatching
        (t: Testable<#seq<'a>>, predicate: 'a -> bool, ?because)
        : AndDerived<_, seq<'a>> =
        use _ = t.Assert()

        let matchingItems = t.Subject |> Seq.filter predicate

        if Seq.stringOptimizedIsEmpty matchingItems then
            t.With("But found", 0).With("Subject value", t.Subject).Fail(because)

        AndDerived(t, matchingItems)


    /// Asserts that the subject does not contain items matching the predicate.
    [<Extension>]
    static member NotContainItemsMatching(t: Testable<#seq<'a>>, predicate: 'a -> bool, ?because) : And<_> =
        use _ = t.Assert()

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

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if ComparisonAssertions.IsNaN(a) || ComparisonAssertions.IsNaN(b) || a > b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order using the specified comparison type.
    [<Extension>]
    static member BeAscending(t: Testable<#seq<string>>, comparisonType: StringComparison, ?because) : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if String.Compare(a, b, comparisonType) > 0 then
                t
                    .With("Using StringComparison", comparisonType)
                    .With(
                        comparisonType = StringComparison.CurrentCulture
                        || comparisonType = StringComparison.CurrentCultureIgnoreCase,
                        "CurrentCulture",
                        CultureInfo.CurrentCulture
                    )
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order using the specified culture and compare options.
    [<Extension>]
    static member BeAscending
        (t: Testable<#seq<string>>, culture: CultureInfo, compareOptions: CompareOptions, ?because)
        : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if String.Compare(a, b, culture, compareOptions) > 0 then
                t
                    .With("In culture", culture)
                    .With("With CompareOptions", compareOptions)
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order by the specified projection.
    [<Extension>]
    static member BeAscendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if ComparisonAssertions.IsNaN(a') || ComparisonAssertions.IsNaN(b') || a' > b' then
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


    /// Asserts that the subject is in ascending order by the specified projection using the specified comparison type.
    [<Extension>]
    static member BeAscendingBy
        (t: Testable<#seq<'a>>, projection: 'a -> string, comparisonType: StringComparison, ?because)
        : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if String.Compare(a', b', comparisonType) > 0 then
                t
                    .With("Using StringComparison", comparisonType)
                    .With(
                        comparisonType = StringComparison.CurrentCulture
                        || comparisonType = StringComparison.CurrentCultureIgnoreCase,
                        "CurrentCulture",
                        CultureInfo.CurrentCulture
                    )
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


    /// Asserts that the subject is in ascending order by the specified projection using the specified culture and
    /// compare options.
    [<Extension>]
    static member BeAscendingBy
        (t: Testable<#seq<'a>>, projection: 'a -> string, culture: CultureInfo, compareOptions: CompareOptions, ?because) : And<
                                                                                                                                _
                                                                                                                             >
        =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if String.Compare(a', b', culture, compareOptions) > 0 then
                t
                    .With("In culture", culture)
                    .With("With CompareOptions", compareOptions)
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

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if ComparisonAssertions.IsNaN(a) || ComparisonAssertions.IsNaN(b) || a < b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in descending order using the specified comparison type.
    [<Extension>]
    static member BeDescending(t: Testable<#seq<string>>, comparisonType: StringComparison, ?because) : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if String.Compare(a, b, comparisonType) < 0 then
                t
                    .With("Using StringComparison", comparisonType)
                    .With(
                        comparisonType = StringComparison.CurrentCulture
                        || comparisonType = StringComparison.CurrentCultureIgnoreCase,
                        "CurrentCulture",
                        CultureInfo.CurrentCulture
                    )
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in descending order using the specified culture and compare options.
    [<Extension>]
    static member BeDescending
        (t: Testable<#seq<string>>, culture: CultureInfo, compareOptions: CompareOptions, ?because)
        : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if String.Compare(a, b, culture, compareOptions) < 0 then
                t
                    .With("In culture", culture)
                    .With("With CompareOptions", compareOptions)
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in descending order by the specified projection.
    [<Extension>]
    static member BeDescendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if ComparisonAssertions.IsNaN(a') || ComparisonAssertions.IsNaN(b') || a' < b' then
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


    /// Asserts that the subject is in descending order by the specified projection using the specified comparison type.
    [<Extension>]
    static member BeDescendingBy
        (t: Testable<#seq<'a>>, projection: 'a -> string, comparisonType: StringComparison, ?because)
        : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if String.Compare(a', b', comparisonType) < 0 then
                t
                    .With("Using StringComparison", comparisonType)
                    .With(
                        comparisonType = StringComparison.CurrentCulture
                        || comparisonType = StringComparison.CurrentCultureIgnoreCase,
                        "CurrentCulture",
                        CultureInfo.CurrentCulture
                    )
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


    /// Asserts that the subject is in descending order by the specified projection using the specified culture and
    /// compare options.
    [<Extension>]
    static member BeDescendingBy
        (t: Testable<#seq<'a>>, projection: 'a -> string, culture: CultureInfo, compareOptions: CompareOptions, ?because) : And<
                                                                                                                                _
                                                                                                                             >
        =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if String.Compare(a', b', culture, compareOptions) < 0 then
                t
                    .With("In culture", culture)
                    .With("With CompareOptions", compareOptions)
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

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if ComparisonAssertions.IsNaN(a) || ComparisonAssertions.IsNaN(b) || a >= b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in ascending order (i.e., is ascending and distinct) by the specified projection.
    [<Extension>]
    static member BeStrictlyAscendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if ComparisonAssertions.IsNaN(a') || ComparisonAssertions.IsNaN(b') || a' >= b' then
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

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do

            if ComparisonAssertions.IsNaN(a) || ComparisonAssertions.IsNaN(b) || a <= b then
                t
                    .With("But found", [ {| Index = i; Item = TryFormat a |}; {| Index = i + 1; Item = TryFormat b |} ])
                    .With("Subject value", t.Subject)
                    .Fail(because)

        And(t)


    /// Asserts that the subject is in descending order (i.e., is descending and distinct) by the specified projection.
    [<Extension>]
    static member BeStrictlyDescendingBy(t: Testable<#seq<'a>>, projection: 'a -> 'b, ?because) : And<_> =
        use _ = t.Assert()

        for i, (a, b) in t.Subject |> Seq.pairwise |> Seq.indexed do
            let a' = projection a
            let b' = projection b

            if ComparisonAssertions.IsNaN(a') || ComparisonAssertions.IsNaN(b') || a' <= b' then
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


    /// Asserts that the subject contains all items in the specified sequence (including any duplicates).
    [<Extension>]
    static member BeSupersetOf(t: Testable<#seq<'a>>, subset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSupersetOf'(subset, false, ?because = because)


    /// Asserts that the subject contains all items in the specified sequence (including any duplicates) and at least
    /// one additional item.
    [<Extension>]
    static member BeProperSupersetOf(t: Testable<#seq<'a>>, subset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSupersetOf'(subset, true, ?because = because)


    [<Extension>]
    static member private BeSubsetOf'(t: Testable<#seq<'a>>, superset: seq<'a>, proper: bool, ?because) : And<_> =
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


    /// Asserts that the specified sequence contains all items in the subject (including any duplicates).
    [<Extension>]
    static member BeSubsetOf(t: Testable<#seq<'a>>, superset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSubsetOf'(superset, false, ?because = because)


    /// Asserts that the specified sequence contains all items in the subject (including any duplicates) and at least
    /// one additional item.
    [<Extension>]
    static member BeProperSubsetOf(t: Testable<#seq<'a>>, superset: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()
        t.BeSubsetOf'(superset, true, ?because = because)


    /// Asserts that the subject has at least one item in common with the other. Fails if either sequence is empty.
    [<Extension>]
    static member IntersectWith(t: Testable<#seq<'a>>, other: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        let set = HashSet(t.Subject :> seq<'a>)

        if not (other |> Seq.exists set.Contains) then
            t.With("Other", other).With("But had no common items", []).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject has no items in common with the other sequence. Passes if either sequence is empty.
    [<Extension>]
    static member NotIntersectWith(t: Testable<#seq<'a>>, other: seq<'a>, ?because) : And<_> =
        use _ = t.Assert()

        let set = HashSet(t.Subject :> seq<'a>)
        set.IntersectWith(other)

        if set.Count > 0 then
            t.With("Other", other).With("But found common items", set).With("Subject value", t.Subject).Fail(because)

        And(t)
