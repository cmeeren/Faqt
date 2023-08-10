namespace Faqt

open System
open System.Diagnostics.CodeAnalysis
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


    let getRegexOptionsStr (options: RegexOptions) =
        "using RegexOptions." + options.ToString() + ", "


[<Extension>]
type StringAssertions =


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
            if regex.Options = RegexOptions.None then
                t.Fail(
                    "{subject}\n\tshould match the regex\n{0}\n\t{because}but was\n{actual}",
                    because,
                    regex.ToString()
                )
            else
                t.Fail(
                    "{subject}\n\tshould match the regex\n{0}\n\t{1}{because}but was\n{actual}",
                    because,
                    regex.ToString(),
                    getRegexOptionsStr regex.Options
                )

        And(t)


    /// Asserts that the subject matches the specified regex pattern using the specified options.
    [<Extension>]
    static member MatchRegex
        (
            t: Testable<string>,
#if NET7_0_OR_GREATER
            [<StringSyntax(StringSyntaxAttribute.Regex)>]
#endif
            pattern: string,
            options: RegexOptions,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject || not (Regex.IsMatch(t.Subject, pattern, options)) then
            if options = RegexOptions.None then
                t.Fail("{subject}\n\tshould match the regex\n{0}\n\t{because}but was\n{actual}", because, pattern)
            else
                t.Fail(
                    "{subject}\n\tshould match the regex\n{0}\n\t{1}{because}but was\n{actual}",
                    because,
                    pattern,
                    getRegexOptionsStr options
                )

        And(t)


    /// Asserts that the subject matches the specified regex pattern.
    [<Extension>]
    static member MatchRegex
        (
            t: Testable<string>,
#if NET7_0_OR_GREATER
            [<StringSyntax(StringSyntaxAttribute.Regex)>]
#endif
            pattern: string,
            ?because
        ) : And<string> =
        use _ = t.Assert()
        t.MatchRegex(pattern, RegexOptions.None, ?because = because)


    /// Asserts that the subject does not match the specified regex. Passes if the subject is null.
    [<Extension>]
    static member NotMatchRegex(t: Testable<string>, regex: Regex, ?because) : And<string> =
        use _ = t.Assert()

        if not (isNull t.Subject) && regex.IsMatch(t.Subject) then
            if regex.Options = RegexOptions.None then
                t.Fail(
                    "{subject}\n\tshould not match the regex\n{0}\n\t{because}but was\n{actual}",
                    because,
                    regex.ToString()
                )
            else
                t.Fail(
                    "{subject}\n\tshould not match the regex\n{0}\n\t{1}{because}but was\n{actual}",
                    because,
                    regex.ToString(),
                    getRegexOptionsStr regex.Options
                )

        And(t)


    /// Asserts that the subject does not match the specified regex pattern using the specified options. Passes if the
    /// subject is null.
    [<Extension>]
    static member NotMatchRegex
        (
            t: Testable<string>,
#if NET7_0_OR_GREATER
            [<StringSyntax(StringSyntaxAttribute.Regex)>]
#endif
            pattern: string,
            options: RegexOptions,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        if not (isNull t.Subject) && Regex.IsMatch(t.Subject, pattern, options) then
            if options = RegexOptions.None then
                t.Fail("{subject}\n\tshould not match the regex\n{0}\n\t{because}but was\n{actual}", because, pattern)
            else
                t.Fail(
                    "{subject}\n\tshould not match the regex\n{0}\n\t{1}{because}but was\n{actual}",
                    because,
                    pattern,
                    getRegexOptionsStr options
                )

        And(t)


    /// Asserts that the subject does not match the specified regex pattern. Passes if the subject is null.
    [<Extension>]
    static member NotMatchRegex
        (
            t: Testable<string>,
#if NET7_0_OR_GREATER
            [<StringSyntax(StringSyntaxAttribute.Regex)>]
#endif
            pattern: string,
            ?because
        ) : And<string> =
        use _ = t.Assert()
        t.NotMatchRegex(pattern, RegexOptions.None, ?because = because)
