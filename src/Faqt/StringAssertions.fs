namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<Extension>]
type StringAssertions =


    // TODO: Remove this after the corresponding IEnumerable method has been implemented?
    /// Asserts that the subject has the specified length.
    [<Extension>]
    static member HaveLength(t: Testable<string>, expected: int, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.Fail("{subject}\n\tshould have length\n{0}\n\t{because}but was\n{actual}", because, format expected)
        elif t.Subject.Length <> expected then
            t.Fail(
                "{subject}\n\tshould have length\n{0}\n\t{because}but was\n{actual}\n\twith length\n{1}",
                because,
                format expected,
                format t.Subject.Length
            )

        And(t)


    // TODO: Remove this after the corresponding IEnumerable method has been implemented?
    /// Asserts that the subject is empty. Equivalent to HaveLength(0) (but with a different error message).
    [<Extension>]
    static member BeEmpty(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject || t.Subject.Length <> 0 then
            t.Fail("{subject}\n\tshould be empty{because}, but was\n{actual}", because)

        And(t)


    // TODO: Remove this after the corresponding IEnumerable method has been implemented?
    /// Asserts that the subject is not empty.
    [<Extension>]
    static member NotBeEmpty(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.Fail("{subject}\n\tshould not be empty{because}, but was\n{actual}", because)
        elif t.Subject.Length = 0 then
            t.Fail("{subject}\n\tshould not be empty{because}, but was empty.", because)

        And(t)
