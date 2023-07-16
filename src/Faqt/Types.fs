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
    static val mutable private assertions: Dictionary<Assembly * string * int, string list>

    [<ThreadStatic; DefaultValue>]
    static val mutable private isAsserting: Dictionary<Assembly * string * int, bool>

    // TODO: simplify logic using Stack?

    static let addAssertion key assertion =
        if isNull Testable.assertions then
            Testable.assertions <- Dictionary()

        match Testable.assertions.TryGetValue key with
        | false, _ -> Testable.assertions[key] <- [ assertion ]
        | true, assertions -> Testable.assertions[key] <- assertions @ [ assertion ]


    static let setAssertions key assertions =
        if isNull Testable.assertions then
            Testable.assertions <- Dictionary()

        Testable.assertions[key] <- assertions


    static let setIsAsserting key value =
        if isNull Testable.isAsserting then
            Testable.isAsserting <- Dictionary()

        Testable.isAsserting[key] <- value


    static let getIsAsserting key =
        if isNull Testable.isAsserting then
            Testable.isAsserting <- Dictionary()

        match Testable.isAsserting.TryGetValue key with
        | true, x -> x
        | false, _ -> false


    // TODO: Can we remove this?
    internal new(subject: 'a, continueFrom: Testable<'a>) =
        Testable(subject, continueFrom.CallerFilePath, continueFrom.CallerLineNo, continueFrom.CallerAssembly)


    /// Call this at the start of your assertions, and make sure to dispose the returned value at the end. This is
    /// needed to track important state necessary for subject names to work. If your assertion calls user code that is
    /// expected to call their own assertions (like `Satisfy`), call `t.Assert(true)` instead, and make sure it's
    /// disposed before you call `Fail`.
    member this.Assert
        (
            [<Optional; DefaultParameterValue(false)>] trackSubAssertions,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] assertion: string
        ) =
        let key = callerAssembly, callerFilePath, callerLineNo

        if getIsAsserting key then
            IDisposable.noOp
        else
            if not trackSubAssertions then
                setIsAsserting key true

            addAssertion key assertion
            let currentAssertions = this.Assertions

            { new IDisposable with
                member _.Dispose() =
                    if not trackSubAssertions then
                        setIsAsserting key false

                    if trackSubAssertions then
                        setAssertions key currentAssertions
            }

    /// Returns the subject being tested. Aliases: Whose, Which.
    member _.Subject: 'a = subject

    /// Returns the subject being tested. Aliases: Subject, Which.
    member _.Whose: 'a = subject

    member internal _.CallerFilePath = callerFilePath

    member internal _.CallerLineNo = callerLineNo

    member internal _.CallerAssembly = callerAssembly

    member internal _.Assertions =
        let key = callerAssembly, callerFilePath, callerLineNo

        match Testable.assertions.TryGetValue key with
        | true, xs -> xs
        | false, _ -> []


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
