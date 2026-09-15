namespace Faqt

open System
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type EnumAssertions =


    /// Asserts that the subject enum has the specified flag.
    ///
    /// Follows Enum.HasFlag semantics: a zero flag always passes. To assert that no flags are set, use Be with the
    /// enum's zero value instead.
    [<Extension>]
    static member HaveFlag<'a when 'a :> Enum>(t: Testable<'a>, flag: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if not (t.Subject.HasFlag(flag)) then
            t.With("Flag", flag).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject enum does not have the specified flag.
    ///
    /// Follows Enum.HasFlag semantics: a zero flag always fails. To assert that at least one flag is set, use NotBe
    /// with the enum's zero value instead.
    [<Extension>]
    static member NotHaveFlag<'a when 'a :> Enum>(t: Testable<'a>, flag: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject.HasFlag(flag) then
            t.With("Flag", flag).With("But was", t.Subject).Fail(because)

        And(t)
