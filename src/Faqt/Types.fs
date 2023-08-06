namespace Faqt

open System
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


/// The exception raised for all Faqt assertion failures.
type AssertionFailedException(message: string) =
    inherit Exception(message)


[<Struct>]
type Testable<'a> internal (subject: 'a, origin: CallChainOrigin) =

    /// Call this at the start of your assertions, and make sure to dispose the returned value at the end. This is
    /// needed to track important state necessary for subject names to work. If your assertion calls user code that is
    /// expected to call their own assertions (like `Satisfy`), call `t.Assert(true)` instead. In that case, do not call
    /// other assertions directly in the implementation; the next assertion is assumed to be called by the user. If
    /// additionally you invoke user assertions for each item in a sequence, tall `t.Assert(true, true)` instead.
    member _.Assert
        (
            [<Optional; DefaultParameterValue(false)>] supportsChildAssertions,
            [<Optional; DefaultParameterValue(false)>] isSeqAssertion,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] assertionMethod: string
        ) =
        CallChain.Assert(origin, assertionMethod, supportsChildAssertions, isSeqAssertion)

    /// If your assertion invokes user-supplied assertions for each item in a sequence, call this before the assertion
    /// invocation for each item, and make sure to dispose the returned value after the assertion invocation. This is
    /// needed to track important state necessary for subject names to work.
    member _.AssertItem() = CallChain.AssertItem(origin)

    /// Returns the subject being tested. Alias of Whose.
    member _.Subject: 'a = subject

    /// Returns the subject being tested. Alias of Subject.
    member _.Whose: 'a = subject

    member internal _.CallChainOrigin = origin

    member internal _.CallChainAssertionHistory = CallChain.AssertionHistory origin


/// A type which allows chaining assertions.
[<Struct>]
type And<'a>(testable: Testable<'a>) =

    /// Continues asserting on the value that was previously asserted.
    member _.And: Testable<'a> = testable

    /// Returns the subject being tested.
    member _.Subject: 'a = testable.Subject


/// A type which allows chaining assertions or continue asserting on a derived value.
[<Struct>]
type AndDerived<'a, 'b>(testable: Testable<'a>, derived: 'b) =

    /// Continues asserting on the value that was previously asserted.
    member _.And: Testable<'a> = testable

    /// Returns the subject being tested.
    member _.Subject: 'a = testable.Subject

    /// Returns the value derived if the previous assertion succeeds. Aliases: WhoseValue, That.
    member _.Whose: 'b = derived

    /// Returns the value derived if the previous assertion succeeds. This fits well if wanting to continue asserting on
    /// the DU case data returned by e.g. BeOfCase or BeSome. Aliases: Whose, That.
    member _.WhoseValue: 'b = derived

    /// Returns the value derived if the previous assertion succeeds. Aliases: Whose, WhoseValue.
    member _.That: 'b = derived


module AssertionHelpers =


    [<Extension>]
    type TestableExtensions =


        /// Use this overload when calling Should() in custom assertions.
        [<Extension>]
        static member Should(this: 'a, continueFrom: Testable<'b>) : Testable<'a> =
            Testable(this, continueFrom.CallChainOrigin)


        /// Fail the assertion with the given template and args. The template may contain the tokens "{subject}",
        /// "{actual}", and "{because}". {subject} will be replaced with the subject name. {value} will be replaced with the
        /// (formatted) value being tested. {actual} will be replaced with an empty string if an empty "because" was passed
        /// to the Fail constructor. Otherwise, it will be replaced with the because message, prefixed with "because " if
        /// not already part of the message. It will be further prefixed with a single space if the template does not
        /// contain a whitespace character immediately preceding the token. Finally, it will be suffixed with ", " if the
        /// template does not contain ", " immediately following the token.
        [<Extension>]
        static member Fail
            (
                this: Testable<'a>,
                template: string,
                because: string option,
                [<ParamArray>] formattedValues: string[]
            ) =

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


            let subjectName =
                SubjectName.get this.CallChainOrigin this.CallChainAssertionHistory

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
            |> String.replace actualPlaceholder (Formatting.format this.Subject)
            |> String.regexReplace $"(?<!\s){becausePlaceholder}(?!, )" (bc because true true)
            |> String.regexReplace $"(?<!\s){becausePlaceholder}(?=, )" (bc because true false)
            |> String.regexReplace $"(?<=\s){becausePlaceholder}(?!, )" (bc because false true)
            |> String.regexReplace $"(?<=\s){becausePlaceholder}(?=, )" (bc because false false)
            |> AssertionFailedException
            |> raise


[<Extension>]
type TestableExtensions =


    /// This is the entry point to performing assertions on this value. Note that for subsequent calls to Should inside
    /// an assertion chain, use the Should(()) overload instead. (This does not apply to the start of "child" expression
    /// chains inside higher-order assertions like Satisfy, which should still use Should() like the start of the
    /// top-level expression chain.)
    [<Extension>]
    static member Should
        (
            this: 'a,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] filePath: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lineNumber: int
        ) : Testable<'a> =
        let origin = {
            AssemblyPath = Assembly.GetCallingAssembly().Location
            SourceFilePath = filePath
            LineNumber = lineNumber
        }

        CallChain.Reset(origin)
        Testable(this, origin)


    /// Use this overload, i.e. .Should(()), for subsequent calls to Should in an assertion chain. (Use the normal
    /// Should() to start "child" expression chains in higher-order assertions like Satisfy.)
    [<Extension>]
    static member Should
        (
            this: 'a,
            _: unit,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] filePath: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lineNumber: int
        ) : Testable<'a> =
        let origin = {
            AssemblyPath = Assembly.GetCallingAssembly().Location
            SourceFilePath = filePath
            LineNumber = lineNumber
        }

        Testable(this, origin)


open AssertionHelpers


// Note: The type checking assertions below are implemented as intrinsic extension methods so we can get away with only
// one explicit method type parameter.
type Testable<'a> with


    /// Asserts that the runtime type of the subject is the exact specified type. See 'BeAssignableTo' for allowing
    /// subtypes and interface implementations.
    member this.BeOfType(expectedType: Type, ?because) : And<'a> =
        use _ = this.Assert()

        if isNull (box this.Subject) then
            this.Fail(
                "{subject}\n\tshould be of type\n{0}\n\t{because}but was\nnull",
                because,
                expectedType.AssertionName
            )
        else
            let actualType = this.Subject.GetType()

            if actualType = expectedType then
                And(this)
            else
                this.Fail(
                    "{subject}\n\tshould be of type\n{0}\n\t{because}but was\n{1}\n\twith data\n{2}",
                    because,
                    expectedType.AssertionName,
                    actualType.AssertionName,
                    Formatting.format this.Subject
                )


    /// Asserts that the runtime type of the subject is the exact specified type. See 'BeAssignableTo' for allowing
    /// subtypes and interface implementations.
    [<RequiresExplicitTypeArguments>]
    member this.BeOfType<'b>(?because) : AndDerived<'a, 'b> =
        use _ = this.Assert()
        this.BeOfType(typeof<'b>, ?because = because) |> ignore
        AndDerived(this, box this.Subject :?> 'b)


    /// Asserts that the runtime type of the subject is assignable to the specified type (i.e., it must either be the
    /// specified type, be a subtype of the specified type, or implement the specified interface). See 'BeOfType' for
    /// requiring an exact type.
    member this.BeAssignableTo(expectedType: Type, ?because) : And<'a> =
        use _ = this.Assert()

        if isNull (box this.Subject) then
            this.Fail(
                "{subject}\n\tshould be assignable to\n{0}\n\t{because}but was\nnull",
                because,
                expectedType.AssertionName
            )
        else
            let actualType = this.Subject.GetType()

            if actualType.IsAssignableTo(expectedType) then
                And(this)
            else
                this.Fail(
                    "{subject}\n\tshould be assignable to\n{0}\n\t{because}but was\n{1}\n\twith data\n{2}",
                    because,
                    expectedType.AssertionName,
                    actualType.AssertionName,
                    Formatting.format this.Subject
                )


    /// Asserts that the runtime type of the subject is assignable to the specified type (i.e., it must either be the
    /// specified type, be a subtype of the specified type, or implement the specified interface). See 'BeOfType' for
    /// requiring an exact type.
    [<RequiresExplicitTypeArguments>]
    member this.BeAssignableTo<'b>(?because) : AndDerived<'a, 'b> =
        use _ = this.Assert()
        this.BeAssignableTo(typeof<'b>, ?because = because) |> ignore
        AndDerived(this, box this.Subject :?> 'b)
