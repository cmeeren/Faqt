namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers


[<Extension>]
type ComparisonAssertions =


    /// Asserts that the subject is at most the specified tolerance greater or smaller than the specified value.
    [<Extension>]
    static member inline BeCloseTo(t: Testable<'a>, target: 'b, tolerance: 'c, ?because) : And<'a> =
        use _ = t.Assert()

        // This implementation requires comparison as well as op_Subtraction both ways. Changing this may break clients.
        // Alternative implementations could require op_Subtraction only one way, but additionally require Abs or ~-
        // (negation).
        if
            isNull (box t.Subject)
            || t.Subject - target > tolerance
            || target - t.Subject > tolerance
        then
            t
                .With("Target", target)
                .With("With tolerance", tolerance)
                .With("But was", t.Subject)
                .Fail(because)

        And(t)


    /// Asserts that the subject is more than the specified tolerance greater or smaller than the specified value.
    [<Extension>]
    static member inline NotBeCloseTo(t: Testable<'a>, target: 'b, tolerance: 'c, ?because) : And<'a> =
        use _ = t.Assert()

        // This implementation requires comparison as well as op_Subtraction both ways. Changing this may break clients.
        // Alternative implementations could require op_Subtraction only one way, but additionally require Abs or ~-
        // (negation).
        if
            isNull (box t.Subject)
            || not (t.Subject - target > tolerance || target - t.Subject > tolerance)
        then
            t
                .With("Target", target)
                .With("With tolerance", tolerance)
                .With("But was", t.Subject)
                .Fail(because)

        And(t)


    [<Extension>]
    static member inline private Compare(t: Testable<'a>, op, other, because) =
        if isNull (box t.Subject) || not (op t.Subject other) then
            t.With("Other", other).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is greater than the specified value.
    [<Extension>]
    static member inline BeGreaterThan(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((>), other, because)


    /// Asserts that the subject is greater than or equal to the specified value.
    [<Extension>]
    static member inline BeGreaterThanOrEqualTo(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((>=), other, because)


    /// Asserts that the subject is less than the specified value.
    [<Extension>]
    static member inline BeLessThan(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((<), other, because)


    /// Asserts that the subject is less than or equal to the specified value.
    [<Extension>]
    static member inline BeLessThanOrEqualTo(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((<=), other, because)


    /// Asserts that the subject is greater than zero.
    [<Extension>]
    static member inline BePositive(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject <= LanguagePrimitives.GenericZero then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is less than zero.
    [<Extension>]
    static member inline BeNegative(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject >= LanguagePrimitives.GenericZero then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is greater than or equal to zero.
    [<Extension>]
    static member inline BeNonNegative(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject < LanguagePrimitives.GenericZero then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is less than or equal to zero.
    [<Extension>]
    static member inline BeNonPositive(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject > LanguagePrimitives.GenericZero then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is the specified inclusive range.
    [<Extension>]
    static member inline BeInRange(t: Testable<'a>, lower: 'a, upper: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject < lower || t.Subject > upper then
            t
                .With("Lower", lower)
                .With("Upper", upper)
                .With("But was", t.Subject)
                .Fail(because)

        And(t)
