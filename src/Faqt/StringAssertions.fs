namespace Faqt

open System
open System.Globalization
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open AssertionHelpers
open Formatting


[<AutoOpen>]
module private Helpers =

    let getStringComparisonStr (comparisonType: StringComparison) =
        let cultureSuffix =
            match comparisonType with
            | StringComparison.CurrentCulture
            | StringComparison.CurrentCultureIgnoreCase ->
                if CultureInfo.CurrentCulture.Name = "" then
                    " (invariant culture)"
                else
                    $" (culture {CultureInfo.CurrentCulture.Name})"
            | _ -> ""

        "StringComparison." + comparisonType.ToString() + cultureSuffix


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


    /// Asserts that the subject is lower-case (i.e., that it is unchanged when calling ToLower with the specified
    /// culture).
    [<Extension>]
    static member BeLowerCase(t: Testable<string>, culture: CultureInfo, ?because) : And<string> =
        use _ = t.Assert()

        let cultureStr =
            if culture.Name = "" then
                "the invariant culture"
            else
                "culture " + culture.Name

        if isNull t.Subject || t.Subject <> t.Subject.ToLower(culture) then
            t.Fail(
                "{subject}\n\tshould be lower-case according to {0}{because}, but was\n{actual}",
                because,
                cultureStr
            )

        And(t)


    /// Asserts that the subject is lower-case according to the invariant culture (i.e., that it is unchanged when
    /// calling ToLowerInvariant).
    [<Extension>]
    static member BeLowerCase(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()
        t.BeLowerCase(CultureInfo.InvariantCulture, ?because = because)


    /// Asserts that the subject contains the specified string using the specified string comparison type.
    [<Extension>]
    static member Contain
        (
            t: Testable<string>,
            substring: string,
            comparisonType: StringComparison,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if isNull substring then
            nullArg (nameof substring)

        if isNull t.Subject || not (t.Subject.Contains(substring, comparisonType)) then
            t.Fail(
                "{subject}\n\tshould contain\n{0}\n\tusing {1}{because}, but was\n{actual}",
                because,
                format substring,
                getStringComparisonStr comparisonType
            )

        And(t)


    /// Asserts that the subject contains the specified string using ordinal string comparison.
    [<Extension>]
    static member Contain(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.Contain(substring, StringComparison.Ordinal, ?because = because)


    // Asserts that the subject does not contain the specified string using the specified string comparison type. Passes
    // if the subject is null.
    [<Extension>]
    static member NotContain
        (
            t: Testable<string>,
            substring: string,
            comparisonType: StringComparison,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if isNull substring then
            nullArg (nameof substring)

        if not (isNull t.Subject) && t.Subject.Contains(substring, comparisonType) then
            t.Fail(
                "{subject}\n\tshould not contain\n{0}\n\tusing {1}{because}, but was\n{actual}",
                because,
                format substring,
                getStringComparisonStr comparisonType
            )

        And(t)


    // Asserts that the subject does not contain the specified string using ordinal string comparison. Passes if the
    // subject is null.
    [<Extension>]
    static member NotContain(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.NotContain(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject starts with the specified string using the specified string comparison type.
    [<Extension>]
    static member StartWith
        (
            t: Testable<string>,
            substring: string,
            comparisonType: StringComparison,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if isNull substring then
            nullArg (nameof substring)

        if isNull t.Subject || not (t.Subject.StartsWith(substring, comparisonType)) then
            t.Fail(
                "{subject}\n\tshould start with\n{0}\n\tusing {1}{because}, but was\n{actual}",
                because,
                format substring,
                getStringComparisonStr comparisonType
            )

        And(t)


    /// Asserts that the subject starts with the specified string using ordinal string comparison.
    [<Extension>]
    static member StartWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.StartWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject does not start with the specified string using the specified string comparison type.
    /// Passes if the subject is null.
    [<Extension>]
    static member NotStartWith
        (
            t: Testable<string>,
            substring: string,
            comparisonType: StringComparison,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if isNull substring then
            nullArg (nameof substring)

        if not (isNull t.Subject) && t.Subject.StartsWith(substring, comparisonType) then
            t.Fail(
                "{subject}\n\tshould not start with\n{0}\n\tusing {1}{because}, but was\n{actual}",
                because,
                format substring,
                getStringComparisonStr comparisonType
            )

        And(t)


    /// Asserts that the subject does not start with the specified string using ordinal string comparison. Passes if the
    /// subject is null.
    [<Extension>]
    static member NotStartWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.NotStartWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject ends with the specified string using the specified string comparison type.
    [<Extension>]
    static member EndWith
        (
            t: Testable<string>,
            substring: string,
            comparisonType: StringComparison,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if isNull substring then
            nullArg (nameof substring)

        if isNull t.Subject || not (t.Subject.EndsWith(substring, comparisonType)) then
            t.Fail(
                "{subject}\n\tshould end with\n{0}\n\tusing {1}{because}, but was\n{actual}",
                because,
                format substring,
                getStringComparisonStr comparisonType
            )

        And(t)


    /// Asserts that the subject ends with the specified string using ordinal string comparison.
    [<Extension>]
    static member EndWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.EndWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject does not end with the specified string using the specified string comparison type.
    /// Passes if the subject is null.
    [<Extension>]
    static member NotEndWith
        (
            t: Testable<string>,
            substring: string,
            comparisonType: StringComparison,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if isNull substring then
            nullArg (nameof substring)

        if not (isNull t.Subject) && t.Subject.EndsWith(substring, comparisonType) then
            t.Fail(
                "{subject}\n\tshould not end with\n{0}\n\tusing {1}{because}, but was\n{actual}",
                because,
                format substring,
                getStringComparisonStr comparisonType
            )

        And(t)


    /// Asserts that the subject does not end with the specified string using ordinal string comparison. Passes if the
    /// subject is null.
    [<Extension>]
    static member NotEndWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.NotEndWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject matches the specified regex.
    [<Extension>]
    static member MatchRegex(t: Testable<string>, regex: Regex, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject || not (regex.IsMatch(t.Subject)) then
            t.Fail(
                "{subject}\n\tshould match the regex\n{0}\n\tusing RegexOptions.{1}{because}, but was\n{actual}",
                because,
                regex.ToString(),
                regex.Options.ToString()
            )

        And(t)
