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
        if t.Subject - expected > tolerance || expected - t.Subject > tolerance then
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
        if not (t.Subject - expected > tolerance || expected - t.Subject > tolerance) then
            t.Fail(
                "{subject}\n\tshould not be\n{0} ± {1}\n\t{because}but was\n{actual}",
                because,
                format expected,
                format tolerance
            )

        And(t)
