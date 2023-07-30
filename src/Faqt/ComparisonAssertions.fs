namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<Extension>]
type ComparisonAssertions =


    /// Asserts that the subject is at most the specified tolerance greater or smaller than the specified value.
    [<Extension>]
    static member inline BeCloseTo(t: Testable<'a>, expected: 'b, tolerance: 'c, ?because) : And<'a> =
        use _ = t.Assert()

        // This implementation requires comparison as well as op_Subtraction both ways. Changing this may break clients.
        // Alternative implementations could require op_Subtraction only one way, but additionally require Abs or ~-
        // (negation).
        if
            isNull (box t.Subject)
            || t.Subject - expected > tolerance
            || expected - t.Subject > tolerance
        then
            t.Fail(
                "{subject}\n\tshould be\n{0} ± {1}\n\t{because}but was\n{actual}",
                because,
                format expected,
                format tolerance
            )

        And(t)


    /// Asserts that the subject is more than the specified tolerance greater or smaller than the specified value.
    [<Extension>]
    static member inline NotBeCloseTo(t: Testable<'a>, expected: 'b, tolerance: 'c, ?because) : And<'a> =
        use _ = t.Assert()

        // This implementation requires comparison as well as op_Subtraction both ways. Changing this may break clients.
        // Alternative implementations could require op_Subtraction only one way, but additionally require Abs or ~-
        // (negation).
        if
            isNull (box t.Subject)
            || not (t.Subject - expected > tolerance || expected - t.Subject > tolerance)
        then
            t.Fail(
                "{subject}\n\tshould not be\n{0} ± {1}\n\t{because}but was\n{actual}",
                because,
                format expected,
                format tolerance
            )

        And(t)


    [<Extension>]
    static member inline private Compare(t: Testable<'a>, op, opText, expected, because) =
        if isNull (box t.Subject) || not (op t.Subject expected) then
            t.Fail("{subject}\n\tshould be {0}\n{1}\n\t{because}but was\n{actual}", because, opText, format expected)

        And(t)


    /// Asserts that the subject is greater than the specified value.
    [<Extension>]
    static member inline BeGreaterThan(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((>), "greater than", other, because)


    /// Asserts that the subject is greater than or equal to the specified value.
    [<Extension>]
    static member inline BeGreaterThanOrEqualTo(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((>=), "greater than or equal to", other, because)


    /// Asserts that the subject is less than the specified value.
    [<Extension>]
    static member inline BeLessThan(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((<), "less than", other, because)


    /// Asserts that the subject is less than or equal to the specified value.
    [<Extension>]
    static member inline BeLessThanOrEqualTo(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((<=), "less than or equal to", other, because)


    /// Asserts that the subject is positive.
    [<Extension>]
    static member inline BePositive(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject <= LanguagePrimitives.GenericZero then
            t.Fail("{subject}\n\tshould be positive{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is negative.
    [<Extension>]
    static member inline BeNegative(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject >= LanguagePrimitives.GenericZero then
            t.Fail("{subject}\n\tshould be negative{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is non-negative (i.e., is zero or positive).
    [<Extension>]
    static member inline BeNonNegative(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject < LanguagePrimitives.GenericZero then
            t.Fail("{subject}\n\tshould be non-negative{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is non-positive (i.e., is zero or negative).
    [<Extension>]
    static member inline BeNonPositive(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject > LanguagePrimitives.GenericZero then
            t.Fail("{subject}\n\tshould be non-positive{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is the specified inclusive range.
    [<Extension>]
    static member inline BeInRange(t: Testable<'a>, lower: 'a, upper: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || t.Subject < lower || t.Subject > upper then
            t.Fail(
                "{subject}\n\tshould be in the range\n[{0}, {1}]\n\t{because}but was\n{actual}",
                because,
                format lower,
                format upper
            )

        And(t)
