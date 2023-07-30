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
