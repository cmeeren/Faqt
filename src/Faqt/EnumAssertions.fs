namespace Faqt

open System
open System.Runtime.CompilerServices
open AssertionHelpers


[<Extension>]
type EnumAssertions =


    /// Asserts that the subject enum has the specified flag.
    [<Extension>]
    static member HaveFlag<'a when 'a :> Enum>(t: Testable<'a>, flag: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if not (t.Subject.HasFlag(flag)) then
            t.With("Flag", flag).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject enum does not have the specified flag.
    [<Extension>]
    static member NotHaveFlag<'a when 'a :> Enum>(t: Testable<'a>, flag: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject.HasFlag(flag) then
            t.With("Flag", flag).With("But was", t.Subject).Fail(because)

        And(t)
