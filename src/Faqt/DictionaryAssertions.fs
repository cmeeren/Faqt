namespace Faqt

open System.Collections.Generic
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers
open Faqt.Formatting


[<AutoOpen>]
module private DictionaryAssertionsHelpers =


    [<Struct>]
    type SatisfyReportFailureItem<'key> = { Key: 'key; Failure: FailureData }


    [<Struct>]
    type SatisfyReportExceptionItem<'key> = { Key: 'key; Exception: TryFormat }


    [<Struct>]
    type ExpectedActualReportItem<'key, 'value> = {
        Key: 'key
        Expected: 'value
        Actual: 'value
    }


    let countRemainingItems processedCount hasCurrent (enumerator: IEnumerator<'a>) =
        let mutable count = processedCount

        if hasCurrent then
            count <- count + 1

            while enumerator.MoveNext() do
                count <- count + 1

        count


[<Extension>]
type DictionaryAssertions =


    /// Asserts that all items in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy
        (t: Testable<#IDictionary<'key, 'value>>, assertion: KeyValuePair<'key, 'value> -> 'ignored, ?because)
        : And<_> =
        use _ = t.Assert(true, true)

        let failures =
            t.Subject
            |> Seq.choose (fun kvp ->
                try
                    use _ = t.AssertItem()
                    assertion kvp |> ignore
                    None
                with
                | :? AssertionFailedException as ex ->
                    {
                        Key = TryFormat kvp.Key
                        Failure = ex.FailureData
                    }
                    |> box
                    |> Some
                | ex ->
                    {
                        Key = TryFormat kvp.Key
                        Exception = TryFormat(box ex)
                    }
                    |> box
                    |> Some
            )
            |> Seq.toArray

        if failures.Length > 0 then
            t.With("Failures", failures).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the same number of items as the assertion collection, and that each subject
    /// item satisfies the corresponding assertion in the assertion collection.
    [<Extension>]
    static member SatisfyRespectively
        (t: Testable<#IDictionary<'key, 'value>>, assertions: seq<KeyValuePair<'key, 'value> -> 'ignored>, ?because)
        : And<_> =
        use _ = t.Assert(true)

        let subjectCount = t.Subject.Count
        use subjectEnumerator = t.Subject.GetEnumerator()
        use assertionsEnumerator = assertions.GetEnumerator()

        let mutable processedCount = 0
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
                    Key = TryFormat subjectEnumerator.Current.Key
                    Failure = ex.FailureData
                }
                |> box
                |> addFailure
            | ex ->
                {
                    Key = TryFormat subjectEnumerator.Current.Key
                    Exception = TryFormat(box ex)
                }
                |> box
                |> addFailure

            processedCount <- processedCount + 1
            subjectHasNext <- subjectEnumerator.MoveNext()
            assertionsHasNext <- assertionsEnumerator.MoveNext()

        if subjectHasNext <> assertionsHasNext then
            let assertionsCount =
                countRemainingItems processedCount assertionsHasNext assertionsEnumerator

            t
                .With("Expected count", assertionsCount)
                .With("Actual count", subjectCount)
                .With("Subject value", t.Subject)
                .Fail(because)

        if not (isNull failures) then
            t.With("Failures", failures.ToArray()).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the specified key-value pair.
    [<Extension>]
    static member Contain
        (t: Testable<#IDictionary<'key, 'value>>, key: 'key, value: 'value, ?because)
        : AndDerived<_, KeyValuePair<'key, 'value>> =
        use _ = t.Assert()
        let expected = KeyValuePair(key, value)

        if not (t.Subject.Contains(expected)) then
            t.With("Item", expected).With("But was", t.Subject).Fail(because)

        AndDerived(t, KeyValuePair(key, t.Subject[key]))


    /// Asserts that the subject does not contain the specified key-value pair.
    [<Extension>]
    static member NotContain(t: Testable<#IDictionary<'key, 'value>>, key: 'key, value: 'value, ?because) : And<_> =
        use _ = t.Assert()
        let kvp = KeyValuePair(key, value)

        if t.Subject.Contains(kvp) then
            t.With("Item", kvp).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the same items as the specified dictionary.
    [<Extension>]
    static member HaveSameItemsAs
        (t: Testable<#IDictionary<'key, 'value>>, expected: IDictionary<'key, 'value>, ?because)
        : And<_> =
        use _ = t.Assert()

        if t.Subject.Count <> expected.Count then
            t
                .With("Expected count", expected.Count)
                .With("Actual count", t.Subject.Count)
                .With("Expected", expected)
                .With("Actual", t.Subject)
                .Fail(because)

        let differentValues = ResizeArray()
        let extraKeys = ResizeArray()
        let missingKeys = ResizeArray()

        for kvp in t.Subject do
            match expected.TryGetValue kvp.Key with
            | true, expectedItem when expectedItem = kvp.Value -> ()
            | true, expectedItem ->
                differentValues.Add(
                    {
                        Key = TryFormat kvp.Key
                        Expected = TryFormat expectedItem
                        Actual = TryFormat kvp.Value
                    }
                )
            | false, _ -> extraKeys.Add(TryFormat kvp.Key)

        for kvp in expected do
            if not (t.Subject.ContainsKey kvp.Key) then
                missingKeys.Add(TryFormat kvp.Key)

        if differentValues.Count > 0 || extraKeys.Count > 0 || missingKeys.Count > 0 then
            t
                .With("Missing keys", missingKeys)
                .With("Additional keys", extraKeys)
                .With("Different values", differentValues)
                .With("Expected", expected)
                .With("Actual", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject contains the specified key.
    [<Extension>]
    static member ContainKey
        (t: Testable<#IDictionary<'key, 'value>>, key: 'key, ?because)
        : AndDerived<_, KeyValuePair<'key, 'value>> =
        use _ = t.Assert()

        match t.Subject.TryGetValue key with
        | true, value -> AndDerived(t, KeyValuePair(key, value))
        | false, _ -> t.With("Key", key).With("But was", t.Subject).Fail(because)


    /// Asserts that the subject does not contain the specified key.
    [<Extension>]
    static member NotContainKey(t: Testable<#IDictionary<'key, 'value>>, key: 'key, ?because) : And<_> =
        use _ = t.Assert()

        if t.Subject.ContainsKey(key) then
            t.With("Key", key).With("But found value", t.Subject[key]).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains all the specified keys.
    [<Extension>]
    static member ContainKeys(t: Testable<#IDictionary<'key, 'value>>, keys: seq<'key>, ?because) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            nullArg "subject"

        let missingKeys = keys |> Seq.filter (not << t.Subject.ContainsKey)

        if not (Seq.isEmpty missingKeys) then
            t
                .With("Keys", keys)
                .With("But was missing", missingKeys |> Seq.distinct |> Seq.map (box >> TryFormat))
                .With("Subject value", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject contains the specified key.
    [<Extension>]
    static member ContainValue
        (t: Testable<#IDictionary<'key, 'value>>, value: 'value, ?because)
        : AndDerived<_, KeyValuePair<'key, 'value>> =
        use _ = t.Assert()

        match t.Subject |> Seq.tryFind (fun kvp -> kvp.Value = value) with
        | None -> t.With("Value", value).With("But was", t.Subject).Fail(because)
        | Some kvp -> AndDerived(t, kvp)


    /// Asserts that the subject does not contain the specified value.
    [<Extension>]
    static member NotContainValue(t: Testable<#IDictionary<'key, 'value>>, value: 'value, ?because) : And<_> =
        use _ = t.Assert()

        let xs = t.Subject |> Seq.filter (fun kvp -> kvp.Value = value)

        if not (Seq.isEmpty xs) then
            t
                .With("Value", value)
                .With("But found value for keys", xs |> Seq.map (fun kvp -> kvp.Key))
                .With("Subject value", t.Subject)
                .Fail(because)

        And(t)
