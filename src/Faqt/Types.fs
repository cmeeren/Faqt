namespace Faqt

open System
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Faqt.Formatting


/// The exception raised for all Faqt assertion failures.
type AssertionFailedException internal (message: string, failureData: FailureData) =
    inherit Exception(message)

    /// The data for this assertion failure.
    member _.FailureData = failureData


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


/// A type to help build assertion failures.
type FailureBuilder<'a> = private {
    Testable: Testable<'a>
    Data: (string * obj) list
} with


    /// Adds the specified key/value to the failure message if condition is true.
    member this.With(condition: bool, key: string, value: 'b) =
        if key = "Subject" || key = "Because" || key = "Should" then
            invalidArg (nameof key) $"The key name %s{key} is reserved by Faqt"

        if not condition then
            this
        else
            {
                this with
                    Data = this.Data @ [ key, box value ]
            }


    /// Adds the specified key/value to the failure message.
    member this.With(key: string, value: 'b) = this.With(true, key, value)


    /// Raises an AssertionFailedException with the specified and previously added data.
    member this.Fail(because: string option) =
        let data = {
            Subject = SubjectName.get this.Testable.CallChainOrigin this.Testable.CallChainAssertionHistory
            Because = because
            Should =
                if List.isEmpty this.Testable.CallChainAssertionHistory then
                    invalidOp
                        "Call chain assertion history is empty. Testable.Assert must be called in all assertions before calling Fail."
                else
                    this.Testable.CallChainAssertionHistory |> List.last
            Extra = this.Data
        }

        AssertionFailedException(Formatter.Current data, data) |> raise


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


// Note: The type checking assertions below are implemented as intrinsic extension methods so we can get away with only
// one explicit method type parameter.
type Testable<'a> with


    /// Asserts that the runtime type of the subject is the exact specified type. See 'BeAssignableTo' for allowing
    /// subtypes and interface implementations.
    member this.BeOfType(expectedType: Type, ?because) : And<'a> =
        use _ = this.Assert()

        if isNull (box this.Subject) then
            { Testable = this; Data = [] }
                .With("Expected", expectedType)
                .With("But was", this.Subject)
                .Fail(because)
        else
            let actualType = this.Subject.GetType()

            if actualType = expectedType then
                And(this)
            else
                { Testable = this; Data = [] }
                    .With("Expected", expectedType)
                    .With("But was", actualType)
                    .With("Subject value", this.Subject)
                    .Fail(because)


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
            { Testable = this; Data = [] }
                .With("Expected", expectedType)
                .With("But was", this.Subject)
                .Fail(because)
        else
            let actualType = this.Subject.GetType()

            if actualType.IsAssignableTo(expectedType) then
                And(this)
            else
                { Testable = this; Data = [] }
                    .With("Expected", expectedType)
                    .With("But was", actualType)
                    .With("Subject value", this.Subject)
                    .Fail(because)


    /// Asserts that the runtime type of the subject is assignable to the specified type (i.e., it must either be the
    /// specified type, be a subtype of the specified type, or implement the specified interface). See 'BeOfType' for
    /// requiring an exact type.
    [<RequiresExplicitTypeArguments>]
    member this.BeAssignableTo<'b>(?because) : AndDerived<'a, 'b> =
        use _ = this.Assert()
        this.BeAssignableTo(typeof<'b>, ?because = because) |> ignore
        AndDerived(this, box this.Subject :?> 'b)
