namespace Faqt

open System
open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<Extension>]
type BasicAssertions =


    /// Asserts that the subject is the specified value, using the default equality comparison (=).
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject <> expected then
            t.Fail("{subject}\n\tshould be\n{0}\n\t{because}but was\n{actual}", because, format expected)

        And(t)


    /// Asserts that the subject is not the specified value, using default equality comparison (=).
    [<Extension>]
    static member NotBe(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject = expected then
            t.Fail("{subject}\n\tshould not be\n{0}\n\t{because}but the values were equal.", because, format expected)

        And(t)


    /// Asserts that the subject is reference equal to the specified value (which must not be null).
    [<Extension>]
    static member BeSameAs(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        if isNull (box expected) then
            raise <| ArgumentNullException(nameof expected)

        use _ = t.Assert()

        if isNull (box t.Subject) then
            t.Fail(
                "{subject}\n\tshould be reference equal to\n{0} {1}\n{2}\n\t{because}but was\nnull",
                because,
                (LanguagePrimitives.PhysicalHash expected).ToString(),
                typeof<'a>.AssertionName,
                format expected
            )
        elif not (LanguagePrimitives.PhysicalEquality t.Subject expected) then
            t.Fail(
                "{subject}\n\tshould be reference equal to\n{0} {1}\n{2}\n\t{because}but was\n{3} {4}\n{actual}",
                because,
                (LanguagePrimitives.PhysicalHash expected).ToString(),
                typeof<'a>.AssertionName,
                format expected,
                (LanguagePrimitives.PhysicalHash t.Subject).ToString(),
                t.Subject.GetType().AssertionName
            )

        And(t)


    /// Asserts that the subject is not reference equal to the specified value (which must not be null).
    [<Extension>]
    static member NotBeSameAs(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        if isNull (box expected) then
            raise <| ArgumentNullException(nameof expected)

        use _ = t.Assert()

        if
            not (isNull (box t.Subject))
            && LanguagePrimitives.PhysicalEquality t.Subject expected
        then
            t.Fail(
                "{subject}\n\tshould not be reference equal to\n{0} {1}\n{2}\n\t{because}but was the same reference.",
                because,
                (LanguagePrimitives.PhysicalHash expected).ToString(),
                typeof<'a>.AssertionName,
                format expected
            )

        And(t)


    /// Asserts that the subject is null.
    [<Extension>]
    static member BeNull(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if not (isNull t.Subject) then
            t.Fail("{subject}\n\tshould be null{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is not null.
    [<Extension>]
    static member NotBeNull(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.Fail("{subject}\n\tshould not be null{because}, but was null.", because)

        And(t)
