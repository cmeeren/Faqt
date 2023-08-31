namespace Faqt

open System.Collections.Generic
open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<AutoOpen>]
module private DictionaryAssertionsHelpers =


    [<Struct>]
    type SatisfyReportItem<'key> = { Key: 'key; Failure: FailureData }


    [<Struct>]
    type ExpectedActualReportItem<'a> = { Expected: 'a; Actual: 'a }


[<Extension>]
type DictionaryAssertions =


    /// Asserts that all items in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy
        (
            t: Testable<#IDictionary<'key, 'value>>,
            assertion: KeyValuePair<'key, 'value> -> 'ignored,
            ?because
        ) : And<_> =
        use _ = t.Assert(true, true)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let failures =
            t.Subject
            |> Seq.choose (fun kvp ->
                try
                    use _ = t.AssertItem()
                    assertion kvp |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some {
                        Key = TryFormat kvp.Key
                        Failure = ex.FailureData
                    }
            )
            |> Seq.toArray

        if failures.Length > 0 then
            t.With("Failures", failures).With("Value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the same number of items as the assertion collection, and that each subject
    /// item satisfies the corresponding assertion in the assertion collection.
    [<Extension>]
    static member SatisfyRespectively
        (
            t: Testable<#IDictionary<'key, 'value>>,
            assertions: seq<KeyValuePair<'key, 'value> -> 'ignored>,
            ?because
        ) : And<_> =
        use _ = t.Assert(true)

        if isNull assertions then
            nullArg (nameof assertions)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let subjectCount = t.Subject.Count
        let assertionsCount = Seq.length assertions

        if subjectCount <> assertionsCount then
            t
                .With("Expected count", assertionsCount)
                .With("Actual count", subjectCount)
                .With("Value", t.Subject)
                .Fail(because)

        let failures =
            Seq.zip t.Subject assertions
            |> Seq.choose (fun (kvp, assertion) ->
                try
                    assertion kvp |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some {
                        Key = TryFormat kvp.Key
                        Failure = ex.FailureData
                    }
            )
            |> Seq.toArray

        if failures.Length > 0 then
            t.With("Failures", failures).With("Value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the specified key-value pair, as determined using the default equality
    /// comparison (=).
    [<Extension>]
    static member Contain
        (
            t: Testable<#IDictionary<'key, 'value>>,
            key: 'key,
            value: 'value,
            ?because
        ) : AndDerived<_, KeyValuePair<'key, 'value>> =
        use _ = t.Assert()
        let kvp = KeyValuePair(key, value)

        if isNull (box t.Subject) || not (t.Subject.Contains(kvp)) then
            t.With("Item", kvp).With("But was", t.Subject).Fail(because)

        AndDerived(t, kvp)


    /// Asserts that the subject does not contain the specified key-value pair, as determined using the default equality
    /// comparison (=). Passes if the subject is null.
    [<Extension>]
    static member NotContain(t: Testable<#IDictionary<'key, 'value>>, key: 'key, value: 'value, ?because) : And<_> =
        use _ = t.Assert()
        let kvp = KeyValuePair(key, value)

        if not (isNull (box t.Subject)) && t.Subject.Contains(kvp) then
            t.With("Item", kvp).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject contains the same items as the specified dictionary, as determined using the default
    /// equality comparison (=). Passes if both dictionaries are null.
    [<Extension>]
    static member HaveSameItemsAs
        (
            t: Testable<#IDictionary<'key, 'value>>,
            expected: IDictionary<'key, 'value>,
            ?because
        ) : And<_> =
        use _ = t.Assert()

        if isNull (box t.Subject) && not (isNull expected) then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)
        elif not (isNull (box t.Subject)) then
            let differentValues = Dictionary()
            let extraKeys = ResizeArray()
            let missingKeys = ResizeArray()

            for kvp in t.Subject do
                match expected.TryGetValue kvp.Key with
                | true, expectedItem when expectedItem = kvp.Value -> ()
                | true, expectedItem ->
                    differentValues.Add(
                        TryFormat(Key kvp.Key),
                        {
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
        (
            t: Testable<#IDictionary<'key, 'value>>,
            key: 'key,
            ?because
        ) : AndDerived<_, KeyValuePair<'key, 'value>> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("Key", key).With("But was", t.Subject).Fail(because)

        match t.Subject.TryGetValue key with
        | false, _ -> t.With("Key", key).With("But was", t.Subject).Fail(because)
        | true, value -> AndDerived(t, KeyValuePair(key, value))


    /// Asserts that the subject does not contain the specified key. Passes if the subject is null.
    [<Extension>]
    static member NotContainKey(t: Testable<#IDictionary<'key, 'value>>, key: 'key, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) && t.Subject.ContainsKey(key) then
            t
                .With("Key", key)
                .With("But found value", t.Subject[key])
                .With("Value", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject contains the specified key.
    [<Extension>]
    static member ContainValue
        (
            t: Testable<#IDictionary<'key, 'value>>,
            value: 'value,
            ?because
        ) : AndDerived<_, KeyValuePair<'key, 'value>> =
        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.With("Value", value).With("But was", t.Subject).Fail(because)

        match t.Subject |> Seq.tryFind (fun kvp -> kvp.Value = value) with
        | None -> t.With("Value", value).With("But was", t.Subject).Fail(because)
        | Some kvp -> AndDerived(t, kvp)


    /// Asserts that the subject does not contain the specified key. Passes if the subject is null.
    [<Extension>]
    static member NotContainValue(t: Testable<#IDictionary<'key, 'value>>, value: 'value, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) then
            let xs = t.Subject |> Seq.filter (fun kvp -> kvp.Value = value)

            if not (Seq.isEmpty xs) then
                t
                    .With("Value", value)
                    .With("But found value for keys", xs |> Seq.map (fun kvp -> kvp.Key))
                    .With("Dictionary", t.Subject)
                    .Fail(because)

        And(t)
