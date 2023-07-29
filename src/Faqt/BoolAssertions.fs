namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<Extension>]
type BoolAssertions =


    /// Asserts that the subject is true. Alias of Be(true).
    [<Extension>]
    static member BeTrue(t: Testable<bool>, ?because) : And<bool> =
        use _ = t.Assert()
        t.Be(true, ?because = because)


    /// Asserts that the subject is true. Alias of Be(false).
    [<Extension>]
    static member BeFalse(t: Testable<bool>, ?because) : And<bool> =
        use _ = t.Assert()
        t.Be(false, ?because = because)


    /// Asserts that if the subject is true, other is also true.
    [<Extension>]
    static member Imply(t: Testable<bool>, other: bool, ?because) : And<bool> =
        use _ = t.Assert()

        if t.Subject && not other then
            t.Fail(
                "{subject}\n\tshould imply the specified value{because}, but subject was\n{actual}\n\tand the specified value was\n{0}",
                because,
                format other
            )

        And(t)


    /// Asserts that if other is true, subject is also true.
    [<Extension>]
    static member BeImpliedBy(t: Testable<bool>, other: bool, ?because) : And<bool> =
        use _ = t.Assert()

        if other && not t.Subject then
            t.Fail(
                "{subject}\n\tshould be implied by the specified value{because}, but the value was\n{0}\n\tand the subject was\n{actual}",
                because,
                format other
            )

        And(t)
