module Faqt.AssertionHelpers

open System
open System.Runtime.CompilerServices
open Faqt


/// Helper type for formatting and throwing assertion failures.
type Fail<'a>(t: Testable<'a>, because: string option) =


    let bc (because: string option) prefixSpace suffixComma : string =
        because
        |> Option.map (
            String.removePrefix "because "
            >> String.trim
            >> (fun reason ->
                (if prefixSpace then " " else "")
                + "because "
                + reason
                + (if suffixComma then ", " else "")
            )
        )
        |> Option.defaultValue ""


    /// Fail the assertion with the given template and args. The template may contain the tokens "{subject}",
    /// "{actual}", and "{because}". {subject} will be replaced with the subject name. {value} will be replaced with the
    /// (formatted) value being tested. {actual} will be replaced with an empty string if an empty "because" was passed
    /// to the Fail constructor. Otherwise, it will be replaced with the because message, prefixed with "because " if
    /// not already part of the message. It will be further prefixed with a single space if the template does not
    /// contain a whitespace character immediately preceding the token. Finally, it will be suffixed with ", " if the
    /// template does not contain ", " immediately following the token.
    member _.Throw(template, [<ParamArray>] formattedValues: string[]) =
        let subjectName =
            SubjectName.get t.CallerAssembly.Location t.CallerFilePath t.Assertions t.CallerLineNo

        // We want to replace {subject}, {actual}, and {because} with values we have no control over, and which may
        // contain formatting tokens such as {0}, {1}, etc. We do not want those replaced in String.formatSimple; only
        // those originally in the template should be replaced. Similarly, the call to String.formatSimple can produce
        // the tokens {subject}, {actual}, and {because} which should not be replaced. To work around this, temporarily
        // replace these tokens with placeholder strings that will never occur in any formatted value, then call
        // String.formatSimple, and finally replace the placeholders with the target values.

        let subjectPlaceholder = Guid.NewGuid().ToString("N")
        let actualPlaceholder = Guid.NewGuid().ToString("N")
        let becausePlaceholder = Guid.NewGuid().ToString("N")

        "\n" + template
        |> String.replace "{subject}" subjectPlaceholder
        |> String.replace "{actual}" actualPlaceholder
        |> String.replace "{because}" becausePlaceholder
        |> String.formatSimple formattedValues
        |> String.replace subjectPlaceholder subjectName
        |> String.replace actualPlaceholder (Formatting.format t.Subject)
        |> String.regexReplace $"(?<!\s){becausePlaceholder}(?!, )" (bc because true true)
        |> String.regexReplace $"(?<!\s){becausePlaceholder}(?=, )" (bc because true false)
        |> String.regexReplace $"(?<=\s){becausePlaceholder}(?!, )" (bc because false true)
        |> String.regexReplace $"(?<=\s){becausePlaceholder}(?=, )" (bc because false false)
        |> AssertionFailedException
        |> raise


[<Extension>]
type TestableExtensions =


    /// Use this overload when calling Should() in custom assertions.
    [<Extension>]
    static member Should(this: 'a, continueFrom: Testable<'a>) : Testable<'a> = Testable(this, continueFrom)
