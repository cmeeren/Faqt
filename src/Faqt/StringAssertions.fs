namespace Faqt

open System
#if NET7_0_OR_GREATER
open System.Diagnostics.CodeAnalysis
#endif
open System.Globalization
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open Faqt.AssertionHelpers


[<AutoOpen>]
module private Helpers =


    let comparisonFail (t: Testable<string>) otherName other comparisonType because =
        t
            .With(otherName, other)
            .With("StringComparison", comparisonType)
            .With(
                comparisonType = StringComparison.CurrentCulture
                || comparisonType = StringComparison.CurrentCultureIgnoreCase,
                "CurrentCulture",
                CultureInfo.CurrentCulture
            )
            .With("But was", t.Subject)
            .Fail(because)


    let regexFail (t: Testable<string>) pattern options because =
        t
            .With("Pattern", pattern)
            .With(options <> RegexOptions.None, "RegexOptions", options)
            .With("But was", t.Subject)
            .Fail(because)


    let isWildcardMatch (subject: string) (pattern: string) =
        let asteriskPlaceholder = "40b37b46d8a74affbdf34544f5355b05"
        let questionMarkPlaceholder = "191590a562c744b499798af900dee217"

        let regexPattern =
            pattern
            |> String.replace "*" asteriskPlaceholder
            |> String.replace "?" questionMarkPlaceholder
            |> Regex.Escape
            |> String.replace asteriskPlaceholder ".*"
            |> String.replace questionMarkPlaceholder "."
            |> sprintf "^%s$"

        Regex.IsMatch(subject.Replace("\r\n", "\n"), regexPattern, RegexOptions.IgnoreCase ||| RegexOptions.Singleline)


[<Extension>]
type StringAssertions =


    /// Asserts that the subject is upper-case (i.e., that it is unchanged when calling ToUpper with the specified
    /// culture).
    [<Extension>]
    static member BeUpperCase(t: Testable<string>, culture: CultureInfo, ?because) : And<string> =
        use _ = t.Assert()

        if isNull t.Subject || t.Subject <> t.Subject.ToUpper(culture) then
            t.With("In culture", culture).With("But was", t.Subject).Fail(because)

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

        if isNull t.Subject || t.Subject <> t.Subject.ToLower(culture) then
            t.With("In culture", culture).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is lower-case according to the invariant culture (i.e., that it is unchanged when
    /// calling ToLowerInvariant).
    [<Extension>]
    static member BeLowerCase(t: Testable<string>, ?because) : And<string> =
        use _ = t.Assert()
        t.BeLowerCase(CultureInfo.InvariantCulture, ?because = because)


    /// Asserts that the subject is equal to the specified value using the specified string comparison type.
    [<Extension>]
    static member Be(t: Testable<string>, expected: string, comparisonType: StringComparison, ?because) : And<string> =
        use _ = t.Assert()

        if not (String.Equals(t.Subject, expected, comparisonType)) then
            comparisonFail t "Expected" expected comparisonType because

        And(t)


    /// Asserts that the subject is not equal to the specified value using the specified string comparison type.
    [<Extension>]
    static member NotBe(t: Testable<string>, other: string, comparisonType: StringComparison, ?because) : And<string> =
        use _ = t.Assert()

        if String.Equals(t.Subject, other, comparisonType) then
            comparisonFail t "Other" other comparisonType because

        And(t)


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
            comparisonFail t "Substring" substring comparisonType because

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
            comparisonFail t "Substring" substring comparisonType because

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
            comparisonFail t "Substring" substring comparisonType because

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
            comparisonFail t "Substring" substring comparisonType because

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
            comparisonFail t "Substring" substring comparisonType because

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
            comparisonFail t "Substring" substring comparisonType because

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

        if isNull regex then
            nullArg (nameof regex)

        if isNull t.Subject || not (regex.IsMatch(t.Subject)) then
            regexFail t (regex.ToString()) regex.Options because

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

        if isNull pattern then
            nullArg (nameof pattern)

        if isNull t.Subject || not (Regex.IsMatch(t.Subject, pattern, options)) then
            regexFail t pattern options because

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

        if isNull pattern then
            nullArg (nameof pattern)

        t.MatchRegex(pattern, RegexOptions.None, ?because = because)


    /// Asserts that the subject does not match the specified regex. Passes if the subject is null.
    [<Extension>]
    static member NotMatchRegex(t: Testable<string>, regex: Regex, ?because) : And<string> =
        use _ = t.Assert()

        if isNull regex then
            nullArg (nameof regex)

        if not (isNull t.Subject) && regex.IsMatch(t.Subject) then
            regexFail t (regex.ToString()) regex.Options because

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

        if isNull pattern then
            nullArg (nameof pattern)

        if not (isNull t.Subject) && Regex.IsMatch(t.Subject, pattern, options) then
            regexFail t pattern options because

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

        if isNull pattern then
            nullArg (nameof pattern)

        t.NotMatchRegex(pattern, RegexOptions.None, ?because = because)


    /// Asserts that the subject matches the specified wildcard pattern, which is case insensitive and may contain `*`
    /// (matches zero or more characters, including newlines) and `?` (matches a single character, including newlines).
    /// Newlines are normalized to \n before matching. For more complicated matching, use MatchRegex.
    [<Extension>]
    static member MatchWildcard(t: Testable<string>, pattern: string, ?because) : And<string> =
        use _ = t.Assert()

        if isNull pattern then
            nullArg (nameof pattern)

        if isNull t.Subject || not (isWildcardMatch t.Subject pattern) then
            t.With("Pattern", pattern).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject does not match the specified wildcard pattern, which is case insensitive and may
    /// contain `*` (matches zero or more characters, including newlines) and `?` (matches a single character, including
    /// newlines). Newlines are normalized to \n before matching. Passes if the subject is null. For more complicated
    /// matching, use MatchRegex.
    [<Extension>]
    static member NotMatchWildcard(t: Testable<string>, pattern: string, ?because) : And<string> =
        use _ = t.Assert()

        if isNull pattern then
            nullArg (nameof pattern)

        if not (isNull t.Subject) && isWildcardMatch t.Subject pattern then
            t.With("Pattern", pattern).With("But was", t.Subject).Fail(because)

        And(t)
