namespace Faqt

open System
open System.Globalization
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
    /// Asserts that the subject is empty. Equivalent to HaveLength(0) and Be("") (but with a different error message).
    [<Extension>]
    static member BeEmpty(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject || t.Subject.Length <> 0 then
            t.Fail("{subject}\n\tshould be empty{because}, but was\n{actual}", because)

        And(t)


    // TODO: Remove this after the corresponding IEnumerable method has been implemented?
    /// Asserts that the subject is not empty. Equivalent to NotBe("") (but with a different error message).
    [<Extension>]
    static member NotBeEmpty(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.Fail("{subject}\n\tshould not be empty{because}, but was\n{actual}", because)
        elif t.Subject.Length = 0 then
            t.Fail("{subject}\n\tshould not be empty{because}, but was empty.", because)

        And(t)


    // TODO: Remove this after the corresponding IEnumerable method has been implemented?
    /// Asserts that the subject is null or empty.
    [<Extension>]
    static member BeNullOrEmpty(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()

        if not (String.IsNullOrEmpty t.Subject) then
            t.Fail("{subject}\n\tshould be null or empty{because}, but was\n{actual}", because)

        And(t)


    // TODO: Remove this after the corresponding IEnumerable method has been implemented?
    /// Asserts that the subject is not null or empty.
    [<Extension>]
    static member NotBeNullOrEmpty(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.Fail("{subject}\n\tshould not be null or empty{because}, but was\n{actual}", because)
        elif t.Subject.Length = 0 then
            t.Fail("{subject}\n\tshould not be null or empty{because}, but was empty.", because)

        And(t)


    /// Asserts that the subject is upper-case (i.e., that it is unchanged when calling ToUpper with the specified
    /// culture).
    [<Extension>]
    static member BeUpperCase(t: Testable<string>, culture: CultureInfo, ?because) : And<string> =
        use _ = t.Assert()

        let cultureStr =
            if culture.Name = "" then
                "the invariant culture"
            else
                "culture " + culture.Name

        if isNull t.Subject || t.Subject <> t.Subject.ToUpper(culture) then
            t.Fail(
                "{subject}\n\tshould be upper-case according to {0}{because}, but was\n{actual}",
                because,
                cultureStr
            )

        And(t)


    /// Asserts that the subject is upper-case according to the invariant culture (i.e., that it is unchanged when
    /// calling ToUpperInvariant).
    [<Extension>]
    static member BeUpperCase(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()
        t.BeUpperCase(CultureInfo.InvariantCulture, ?because = because)
