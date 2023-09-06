namespace Faqt

open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type BoolAssertions =


    /// Asserts that the subject is true. Equivalent to Be(true) (but with a different error message).
    [<Extension>]
    static member BeTrue(t: Testable<bool>, ?because) : And<bool> =
        use _ = t.Assert()

        if t.Subject = false then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is false. Equivalent to Be(false) (but with a different error message).
    [<Extension>]
    static member BeFalse(t: Testable<bool>, ?because) : And<bool> =
        use _ = t.Assert()

        if t.Subject = true then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that if the subject is true, other is also true.
    [<Extension>]
    static member Imply(t: Testable<bool>, other: bool, ?because) : And<bool> =
        use _ = t.Assert()

        if t.Subject && not other then
            t.With("But was", t.Subject).With("With other", other).Fail(because)

        And(t)


    /// Asserts that if other is true, subject is also true.
    [<Extension>]
    static member BeImpliedBy(t: Testable<bool>, other: bool, ?because) : And<bool> =
        use _ = t.Assert()

        if other && not t.Subject then
            t.With("Other", other).With("But was", t.Subject).Fail(because)

        And(t)
