namespace Faqt

open System
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type NullableAssertions =


    /// Asserts that the subject has a value.
    [<Extension>]
    static member HaveValue(t: Testable<Nullable<'a>>, ?because) : AndDerived<Nullable<'a>, 'a> =
        use _ = t.Assert()

        if not t.Subject.HasValue then
            t.With("But was", t.Subject).Fail(because)

        AndDerived(t, t.Subject.Value)


    /// Asserts that the subject has a value.
    [<Extension>]
    static member NotHaveValue(t: Testable<Nullable<'a>>, ?because) : And<Nullable<'a>> =
        use _ = t.Assert()

        if t.Subject.HasValue then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is null (i.e., does not have a value). Equivalent to NotHaveValue (but with a different
    /// error message).
    [<Extension>]
    static member BeNull(t: Testable<Nullable<'a>>, ?because) : And<Nullable<'a>> =
        use _ = t.Assert()

        if t.Subject.HasValue then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not null (i.e., has a value). Equivalent to NotHaveValue (but with a different error
    /// message).
    [<Extension>]
    static member NotBeNull(t: Testable<Nullable<'a>>, ?because) : AndDerived<Nullable<'a>, 'a> =
        use _ = t.Assert()

        if not t.Subject.HasValue then
            t.With("But was", t.Subject).Fail(because)

        AndDerived(t, t.Subject.Value)
