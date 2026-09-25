namespace Faqt

open System
#if NET7_0_OR_GREATER
open System.Diagnostics.CodeAnalysis
#endif
open System.Globalization
open System.Runtime.CompilerServices
open System.Text
open System.Text.Encodings.Web
open System.Text.Json
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
        let subject = subject.Replace("\r\n", "\n")
        let pattern = pattern.Replace("\r\n", "\n")

        let parts =
            pattern.Split('*')
            |> Array.map (fun part -> Regex.Escape(part).Replace(@"\?", "."))

        let sb = StringBuilder(pattern.Length * 2 + 2)
        sb.Append('^').Append(parts[0]) |> ignore

        for i in 1 .. parts.Length - 1 do
            if i = parts.Length - 1 then
                // The final section must be free to match at the end of the subject.
                sb.Append(".*").Append(parts[i]) |> ignore
            else
                // Before another '*', the earliest match leaves the most room for the remaining sections.
                // Commit to it so a later mismatch cannot retry exponentially many combinations.
                sb.Append("(?>.*?").Append(parts[i]).Append(')') |> ignore

        let regexPattern = sb.Append(@"\z").ToString()

        Regex.IsMatch(
            subject,
            regexPattern,
            RegexOptions.IgnoreCase
            ||| RegexOptions.Singleline
            ||| RegexOptions.CultureInvariant
        )


[<Extension>]
type StringAssertions =


    /// Asserts that the subject is upper-case (i.e., that it is unchanged when calling ToUpper with the specified
    /// culture).
    [<Extension>]
    static member BeUpperCase(t: Testable<string>, culture: CultureInfo, ?because) : And<string> =
        use _ = t.Assert()

        if t.Subject <> t.Subject.ToUpper(culture) then
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

        if t.Subject <> t.Subject.ToLower(culture) then
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
        (t: Testable<string>, substring: string, comparisonType: StringComparison, ?because)
        : And<string> =
        use _ = t.Assert()

        if not (t.Subject.Contains(substring, comparisonType)) then
            comparisonFail t "Substring" substring comparisonType because

        And(t)


    /// Asserts that the subject contains the specified string using ordinal string comparison.
    [<Extension>]
    static member Contain(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.Contain(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject does not contain the specified string using the specified string comparison type.
    [<Extension>]
    static member NotContain
        (t: Testable<string>, substring: string, comparisonType: StringComparison, ?because)
        : And<string> =
        use _ = t.Assert()

        if t.Subject.Contains(substring, comparisonType) then
            comparisonFail t "Substring" substring comparisonType because

        And(t)


    /// Asserts that the subject does not contain the specified string using ordinal string comparison.
    [<Extension>]
    static member NotContain(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.NotContain(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject starts with the specified string using the specified string comparison type.
    [<Extension>]
    static member StartWith
        (t: Testable<string>, substring: string, comparisonType: StringComparison, ?because)
        : And<string> =
        use _ = t.Assert()

        if not (t.Subject.StartsWith(substring, comparisonType)) then
            comparisonFail t "Substring" substring comparisonType because

        And(t)


    /// Asserts that the subject starts with the specified string using ordinal string comparison.
    [<Extension>]
    static member StartWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.StartWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject does not start with the specified string using the specified string comparison type.
    [<Extension>]
    static member NotStartWith
        (t: Testable<string>, substring: string, comparisonType: StringComparison, ?because)
        : And<string> =
        use _ = t.Assert()

        if t.Subject.StartsWith(substring, comparisonType) then
            comparisonFail t "Substring" substring comparisonType because

        And(t)


    /// Asserts that the subject does not start with the specified string using ordinal string comparison.
    [<Extension>]
    static member NotStartWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.NotStartWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject ends with the specified string using the specified string comparison type.
    [<Extension>]
    static member EndWith
        (t: Testable<string>, substring: string, comparisonType: StringComparison, ?because)
        : And<string> =
        use _ = t.Assert()

        if not (t.Subject.EndsWith(substring, comparisonType)) then
            comparisonFail t "Substring" substring comparisonType because

        And(t)


    /// Asserts that the subject ends with the specified string using ordinal string comparison.
    [<Extension>]
    static member EndWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.EndWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject does not end with the specified string using the specified string comparison type.
    [<Extension>]
    static member NotEndWith
        (t: Testable<string>, substring: string, comparisonType: StringComparison, ?because)
        : And<string> =
        use _ = t.Assert()

        if t.Subject.EndsWith(substring, comparisonType) then
            comparisonFail t "Substring" substring comparisonType because

        And(t)


    /// Asserts that the subject does not end with the specified string using ordinal string comparison.
    [<Extension>]
    static member NotEndWith(t: Testable<string>, substring: string, ?because) : And<string> =
        use _ = t.Assert()
        t.NotEndWith(substring, StringComparison.Ordinal, ?because = because)


    /// Asserts that the subject matches the specified regex.
    [<Extension>]
    static member MatchRegex(t: Testable<string>, regex: Regex, ?because) : And<string> =
        use _ = t.Assert()

        if not (regex.IsMatch(t.Subject)) then
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

        if not (Regex.IsMatch(t.Subject, pattern, options)) then
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

        t.MatchRegex(pattern, RegexOptions.None, ?because = because)


    /// Asserts that the subject does not match the specified regex.
    [<Extension>]
    static member NotMatchRegex(t: Testable<string>, regex: Regex, ?because) : And<string> =
        use _ = t.Assert()

        if regex.IsMatch(t.Subject) then
            regexFail t (regex.ToString()) regex.Options because

        And(t)


    /// Asserts that the subject does not match the specified regex pattern using the specified options.
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

        if Regex.IsMatch(t.Subject, pattern, options) then
            regexFail t pattern options because

        And(t)


    /// Asserts that the subject does not match the specified regex pattern.
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


    /// Asserts that the entire subject matches the specified wildcard pattern, which is case insensitive using the invariant
    /// culture and may contain `*` (matches zero or more characters, including newlines) and `?` (matches a single
    /// character, including newlines).
    /// Newlines are normalized to \n before matching. For more complicated matching, use MatchRegex.
    [<Extension>]
    static member MatchWildcard(t: Testable<string>, pattern: string, ?because) : And<string> =
        use _ = t.Assert()

        if not (isWildcardMatch t.Subject pattern) then
            t.With("Pattern", pattern).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the entire subject does not match the specified wildcard pattern, which is case insensitive using the
    /// invariant culture and may contain `*` (matches zero or more characters, including newlines) and `?` (matches a
    /// single character, including newlines). Newlines are normalized to \n before matching. For more complicated
    /// matching, use MatchRegex.
    [<Extension>]
    static member NotMatchWildcard(t: Testable<string>, pattern: string, ?because) : And<string> =
        use _ = t.Assert()

        if isWildcardMatch t.Subject pattern then
            t.With("Pattern", pattern).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject represents a JSON structure equivalent to that represented by the specified string
    /// (ignoring key order and formatting such as indentation). The comparison is case-sensitive. Numbers are compared
    /// as written, so for example 180 and 180.0, or -0 and 0, are not equivalent.
    [<Extension>]
    static member BeJsonEquivalentTo
        (
            t: Testable<string>,
#if NET7_0_OR_GREATER
            [<StringSyntax(StringSyntaxAttribute.Json)>]
#endif
            expected: string,
            ?because
        ) : And<string> =
        use _ = t.Assert()

        let serializerOptions =
            JsonSerializerOptions(WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping)

        serializerOptions.Converters.Add(JsonElementSortedKeysConverter())

        let formatter =
            FracturedJson.Formatter(
                Options =
                    FracturedJson.FracturedJsonOptions(
                        OmitTrailingWhitespace = true,
                        NumberListAlignment = FracturedJson.NumberListAlignment.Left
                    )
            )

        let expectedDoc =
            try
                JsonDocument.Parse(expected)
            with :? JsonException ->
                invalidArg (nameof expected) "The value must be valid JSON"

        let expectedFormatted =
            formatter.Serialize(expectedDoc, 0, serializerOptions).Trim()

        let subjectDoc =
            try
                JsonDocument.Parse(t.Subject)
            with :? JsonException ->
                t.With("Expected", expectedFormatted).With("But was", t.Subject).Fail(because)

        let subjectFormatted = formatter.Serialize(subjectDoc, 0, serializerOptions).Trim()

        if subjectFormatted <> expectedFormatted then
            t.With("Expected", expectedFormatted).With("But was", subjectFormatted).Fail(because)

        And(t)


    // Deserialization exceptions, including cancellation, intentionally become assertion failures in all overloads below.
    // Rationale: DOCUMENTATION.md, "Assertion failures and unexpected exceptions".


    /// Asserts that the subject is deserializable to a non-null instance of the specified target type using the
    /// specified options.
    [<Extension>]
    static member DeserializeTo
        (t: Testable<string>, targetType: Type, options: JsonSerializerOptions | null, ?because)
        : AndDerived<string, obj> =
        use _ = t.Assert()

        if isNull (box targetType) then
            nullArg (nameof targetType)

        try
            match JsonSerializer.Deserialize(t.Subject, targetType, options) with
            | null ->
                t
                    .With("Target type", targetType)
                    .With("But deserialized to", null)
                    .With("Subject value", t.Subject)
                    .Fail(because)
            | x -> AndDerived(t, x)
        with
        | :? AssertionFailedException -> reraise ()
        | ex -> t.With("Target type", targetType).With("But threw", ex).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject is deserializable to a non-null instance of the specified target type.
    [<Extension>]
    static member DeserializeTo(t: Testable<string>, targetType: Type, ?because) : AndDerived<string, obj> =
        use _ = t.Assert()
        t.DeserializeTo(targetType, null, ?because = because)


    /// Asserts that the subject is deserializable to a non-null instance of the specified target type using the
    /// specified options.
    [<Extension>]
    [<RequiresExplicitTypeArguments>]
    static member DeserializeTo<'a>
        (t: Testable<string>, options: JsonSerializerOptions | null, ?because)
        : AndDerived<string, 'a> =
        use _ = t.Assert()

        try
            match JsonSerializer.Deserialize<'a>(t.Subject, options) with
            | null when usesNullAsTrueValue typeof<'a> -> AndDerived(t, Unchecked.defaultof<'a>)
            | null ->
                t
                    .With("Target type", typeof<'a>)
                    .With("But deserialized to", null)
                    .With("Subject value", t.Subject)
                    .Fail(because)
            | x -> AndDerived(t, x)

        with
        | :? AssertionFailedException -> reraise ()
        | ex -> t.With("Target type", typeof<'a>).With("But threw", ex).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject is deserializable to a non-null instance of the specified target type.
    [<Extension>]
    [<RequiresExplicitTypeArguments>]
    static member DeserializeTo<'a>(t: Testable<string>, ?because) : AndDerived<string, 'a> =
        use _ = t.Assert()
        t.DeserializeTo<'a>(null, ?because = because)


    /// Asserts that the subject is deserializable to a (possibly null) instance of the specified target type using the
    /// specified options.
    [<Extension>]
    static member DeserializeToNullable
        (t: Testable<string>, targetType: Type, options: JsonSerializerOptions | null, ?because)
        : AndDerived<string, obj | null> =
        use _ = t.Assert()

        if isNull (box targetType) then
            nullArg (nameof targetType)

        try
            AndDerived(t, JsonSerializer.Deserialize(t.Subject, targetType, options))
        with ex ->
            t.With("Target type", targetType).With("But threw", ex).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject is deserializable to a (possibly null) instance of the specified target type.
    [<Extension>]
    static member DeserializeToNullable
        (t: Testable<string>, targetType: Type, ?because)
        : AndDerived<string, obj | null> =
        use _ = t.Assert()
        t.DeserializeToNullable(targetType, null, ?because = because)


    /// Asserts that the subject is deserializable to a (possibly null) instance of the specified target type using the
    /// specified options.
    [<Extension>]
    [<RequiresExplicitTypeArguments>]
    static member DeserializeToNullable<'a when 'a: not null and 'a: not struct>
        (t: Testable<string>, options: JsonSerializerOptions | null, ?because)
        : AndDerived<string, 'a | null> =
        use _ = t.Assert()

        try
            AndDerived(t, JsonSerializer.Deserialize<'a>(t.Subject, options))

        with ex ->
            t.With("Target type", typeof<'a>).With("But threw", ex).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject is deserializable to a (possibly null) instance of the specified target type.
    [<Extension>]
    [<RequiresExplicitTypeArguments>]
    static member DeserializeToNullable<'a when 'a: not null and 'a: not struct>
        (t: Testable<string>, ?because)
        : AndDerived<string, 'a | null> =
        use _ = t.Assert()
        t.DeserializeToNullable<'a>(null, ?because = because)
