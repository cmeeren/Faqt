namespace Faqt

open System
open System.Collections.Generic
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


/// The exception raised for all Faqt assertion failures.
type AssertionFailedException(message: string) =
    inherit Exception(message)


type Testable<'a> internal (subject: 'a, callerFilePath: string, callerLineNo: int, callerAssembly: Assembly) =

    [<ThreadStatic; DefaultValue>]
    static val mutable private assertions: Dictionary<Assembly * string * int, ResizeArray<string>>

    [<ThreadStatic; DefaultValue>]
    static val mutable private isAsserting: Dictionary<Assembly * string * int, bool>


    static let addAssertion key assertion =
        if isNull Testable.assertions then
            Testable.assertions <- Dictionary()

        match Testable.assertions.TryGetValue key with
        | false, _ -> Testable.assertions[key] <- ResizeArray([ assertion ])
        | true, assertions -> assertions.Add(assertion)


    static let setIsAsserting key value =
        if isNull Testable.isAsserting then
            Testable.isAsserting <- Dictionary()

        Testable.isAsserting[key] <- value


    static let getIsAsserting key =
        if isNull Testable.isAsserting then
            Testable.isAsserting <- Dictionary()

        match Testable.isAsserting.TryGetValue key with
        | true, x -> x
        | false, _ -> false // TODO: Should this ever happen?


    // TODO: Can we remove this?
    internal new(subject: 'a, continueFrom: Testable<'a>) =
        Testable(subject, continueFrom.CallerFilePath, continueFrom.CallerLineNo, continueFrom.CallerAssembly)


    member _.Assert([<CallerMemberName; Optional; DefaultParameterValue("")>] assertion: string) =
        let key = callerAssembly, callerFilePath, callerLineNo

        if getIsAsserting key then
            IDisposable.noOp
        else
            setIsAsserting key true
            addAssertion key assertion

            { new IDisposable with
                member _.Dispose() = setIsAsserting key false
            }

    /// Returns the subject being tested. Aliases: Whose, Which.
    member _.Subject: 'a = subject

    /// Returns the subject being tested. Aliases: Subject, Which.
    member _.Whose: 'a = subject

    member internal _.CallerFilePath = callerFilePath

    member internal _.CallerLineNo = callerLineNo

    member internal _.CallerAssembly = callerAssembly

    member internal _.Assertions =
        Testable.assertions[(callerAssembly, callerFilePath, callerLineNo)]


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

    /// Returns the value derived if the previous assertion succeeds. Alias: Which.
    member _.Whose: 'b = derived

    /// Returns the value derived if the previous assertion succeeds. Alias: Whose.
    member _.Which: 'b = derived


[<Extension>]
type TestableExtensions =


    /// This is the entry point to performing assertions on this value.
    [<Extension>]
    static member Should
        (
            this: 'a,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] fn: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lno: int
        ) : Testable<'a> =
        Testable(this, fn, lno, Assembly.GetCallingAssembly())
