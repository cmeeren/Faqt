namespace Faqt

open System.Runtime.CompilerServices
open Faqt.AssertionHelpers
open Faqt.Formatting


[<Extension>]
type BasicAssertions =


    /// Asserts that the subject is equal to the specified value using the specified equality comparison.
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'b, isEqual: 'a -> 'b -> bool, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let areEqual =
            try
                isEqual t.Subject expected
            with
            | :? AssertionFailedException -> reraise ()
            | ex ->
                t
                    .With("Expected", expected)
                    .With("But threw", ex)
                    .With("Subject value", t.Subject)
                    .With("WithCustomEquality", true)
                    .RaiseErrorWithEmbeddedException(ex, because)

        if not areEqual then
            t.With("Expected", expected).With("But was", t.Subject).With("WithCustomEquality", true).Fail(because)

        AndDerived(t, expected)


    /// Asserts that the subject is equal to the specified value.
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject <> expected then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not equal to the specified value using the specified equality comparison.
    [<Extension>]
    static member NotBe(t: Testable<'a>, other: 'b, isEqual: 'a -> 'b -> bool, ?because) : And<'a> =
        use _ = t.Assert()

        let areEqual =
            try
                isEqual t.Subject other
            with
            | :? AssertionFailedException -> reraise ()
            | ex ->
                t
                    .With("Other", other)
                    .With("But threw", ex)
                    .With("Subject value", t.Subject)
                    .With("WithCustomEquality", true)
                    .RaiseErrorWithEmbeddedException(ex, because)

        if areEqual then
            t.With("Other", other).With("But was", t.Subject).With("WithCustomEquality", true).Fail(because)

        And(t)


    /// Asserts that the subject is not equal to the specified value.
    [<Extension>]
    static member NotBe(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject = other then
            t.With("Other", other).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is equal to one of the specified values.
    [<Extension>]
    static member BeOneOf(t: Testable<'a>, candidates: seq<'a>, ?because) : AndDerived<'a, 'a> =
        use _ = t.Assert()

        let checkedCandidates = ResizeArray<_>()

        match
            candidates
            |> Seq.tryFind (fun candidate ->
                checkedCandidates.Add(candidate)
                t.Subject = candidate
            )
        with
        | None -> t.With("Candidates", List.ofSeq checkedCandidates).With("But was", t.Subject).Fail(because)
        | Some x -> AndDerived(t, x)


    /// Asserts that the subject is equal to one of the specified first values, and returns the corresponding second
    /// value as the derived value.
    [<Extension>]
    static member BeOneOf(t: Testable<'a>, candidateMapping: seq<'a * 'b>, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let checkedCandidates = ResizeArray<_>()

        match
            candidateMapping
            |> Seq.tryFind (fun (candidate, _) ->
                checkedCandidates.Add(candidate)
                t.Subject = candidate
            )
        with
        | None -> t.With("Candidates", List.ofSeq checkedCandidates).With("But was", t.Subject).Fail(because)
        | Some(_, b) -> AndDerived(t, b)


    /// Asserts that the subject is not equal to one of the specified values. Passes if the candidate list is empty.
    /// Stops at the first match and reports the matching candidate without enumerating the remaining candidates.
    [<Extension>]
    static member NotBeOneOf(t: Testable<'a>, candidates: seq<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        match candidates |> Seq.tryFind ((=) t.Subject) with
        | Some candidate -> t.With("Matching candidate", candidate).With("But was", t.Subject).Fail(because)
        | None -> ()

        And(t)


    /// Asserts that the subject is reference equal to the specified value. Passes if both values are null.
    [<Extension>]
    static member BeSameAs(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        let getData x =
            if isNull (box x) then
                null
            else
                box {|
                    PhysicalHash = LanguagePrimitives.PhysicalHash x
                    Type = x.GetType()
                    Value = TryFormat x
                |}

        if not (LanguagePrimitives.PhysicalEquality t.Subject expected) then
            t.With("Expected", getData expected).With("But was", getData t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not reference equal to the specified value. Fails if both values are null.
    [<Extension>]
    static member NotBeSameAs(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if LanguagePrimitives.PhysicalEquality t.Subject other then
            t.With("Other", other).Fail(because)

        And(t)


    /// Asserts that the subject is null.
    [<Extension>]
    static member BeNull(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if not (isNull t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not null.
    [<Extension>]
    static member NotBeNull(t: Testable<'a | null>, ?because) : And<'a> =
        use _ = t.Assert()

        match t.Subject with
        | null -> t.With("But was", null).Fail(because)
        | s -> And(Testable(s, t.CallChainOrigin))


    // Transform/TryTransform intentionally treat cancellation from the tested operation as an assertion failure.
    // Rationale: DOCUMENTATION.md, "Assertion failures and unexpected exceptions".


    /// Asserts that the subject can be transformed using the specified function (i.e., that the function does not
    /// throw).
    [<Extension>]
    static member Transform(t: Testable<'a>, f: 'a -> 'b, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        if isNull (box f) then
            nullArg (nameof f)

        try
            AndDerived(t, f t.Subject)
        with ex ->
            t.With("But threw", ex).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns None or
    /// throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> 'b option, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        if isNull (box f) then
            nullArg (nameof f)

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("Subject value", t.Subject).Fail(because)

        match result with
        | Some x -> AndDerived(t, x)
        | None -> t.With("But got", None).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns
    /// ValueNone or throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> 'b voption, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        if isNull (box f) then
            nullArg (nameof f)

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("Subject value", t.Subject).Fail(because)

        match result with
        | ValueSome x -> AndDerived(t, x)
        | ValueNone -> t.With("But got", ValueNone).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns Error or
    /// throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> Result<'b, 'c>, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        if isNull (box f) then
            nullArg (nameof f)

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("Subject value", t.Subject).Fail(because)

        match result with
        | Ok x -> AndDerived(t, x)
        | Error err -> t.With("But got", Error err).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns false or
    /// throws). This overload is suitable for functions like Int32.TryParse.
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> bool * 'b, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        if isNull (box f) then
            nullArg (nameof f)

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("Subject value", t.Subject).Fail(because)

        match result with
        | true, x -> AndDerived(t, x)
        | false, _ -> t.With("But got", false).With("Subject value", t.Subject).Fail(because)
