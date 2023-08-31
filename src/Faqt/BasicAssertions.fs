namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<Extension>]
type BasicAssertions =


    /// Asserts that the subject is equal to the specified value using the specified equality comparison.
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'b, isEqual: 'a -> 'b -> bool, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        if not (isEqual t.Subject expected) then
            t
                .With("Expected", expected)
                .With("But was", t.Subject)
                .With("WithCustomEquality", true)
                .Fail(because)

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

        if isEqual t.Subject other then
            t
                .With("Other", other)
                .With("But was", t.Subject)
                .With("WithCustomEquality", true)
                .Fail(because)

        And(t)


    /// Asserts that the subject is not equal to the specified value.
    [<Extension>]
    static member NotBe(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject = other then
            t.With("Other", other).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is reference equal to the specified value (which must not be null).
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
            t
                .With("Expected", getData expected)
                .With("But was", getData t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject is not reference equal to the specified value (which must not be null).
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
    static member NotBeNull(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject can be transformed using the specified function (i.e., that the function does not
    /// throw).
    [<Extension>]
    static member Transform(t: Testable<'a>, f: 'a -> 'b, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        try
            AndDerived(t, f t.Subject)
        with ex ->
            t.With("But threw", ex).With("For value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns None or
    /// throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> 'b option, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("For value", t.Subject).Fail(because)

        match result with
        | Some x -> AndDerived(t, x)
        | None -> t.With("But got", None).With("For value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns
    /// ValueNone or throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> 'b voption, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("For value", t.Subject).Fail(because)

        match result with
        | ValueSome x -> AndDerived(t, x)
        | ValueNone -> t.With("But got", ValueNone).With("For value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns Error or
    /// throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> Result<'b, 'c>, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("For value", t.Subject).Fail(because)

        match result with
        | Ok x -> AndDerived(t, x)
        | Error err -> t.With("But got", Error err).With("For value", t.Subject).Fail(because)


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns false or
    /// throws). This overload is suitable for functions like Int32.TryParse.
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> bool * 'b, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.With("But threw", ex).With("For value", t.Subject).Fail(because)

        match result with
        | true, x -> AndDerived(t, x)
        | false, _ -> t.With("But got", false).With("For value", t.Subject).Fail(because)
