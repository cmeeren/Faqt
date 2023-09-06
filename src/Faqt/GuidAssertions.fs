namespace Faqt

open System
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type GuidAssertions =


    /// Asserts that the subject is equal to the specified value.
    [<Extension>]
    static member Be(t: Testable<Guid>, expected: string, ?because) : And<Guid> =
        use _ = t.Assert()

        if not (t.Subject = Guid.Parse(expected)) then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not equal to the specified value.
    [<Extension>]
    static member NotBe(t: Testable<Guid>, other: string, ?because) : And<Guid> =
        use _ = t.Assert()

        if t.Subject = Guid.Parse(other) then
            t.With("Other", other).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is Guid.Empty.
    [<Extension>]
    static member BeEmpty(t: Testable<Guid>, ?because) : And<Guid> =
        use _ = t.Assert()

        if not (t.Subject = Guid.Empty) then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not Guid.Empty.
    [<Extension>]
    static member NotBeEmpty(t: Testable<Guid>, ?because) : And<Guid> =
        use _ = t.Assert()

        if t.Subject = Guid.Empty then
            t.With("But was", t.Subject).Fail(because)

        And(t)
