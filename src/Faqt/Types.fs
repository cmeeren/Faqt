namespace Faqt

open System
open System.Collections.Generic
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


/// The exception raised for all Faqt assertion failures.
type AssertionFailedException(message: string) =
    inherit Exception(message)


type private AssertionChainCallsite = {
    Assembly: Assembly
    File: String
    Line: int
}


type private AssertionInfo = {
    Method: string
    SupportsChildAssertions: bool
}


// TODO: Move state to a different type, and simplify this?
// TODO: Test three chains with triple Satisfy with triple assertion with same name, fail the middle one
// TODO: Test Satisfy inside Satisfy (or a similar test fake)
// TODO: Replace some Satisfy calls with test fake
type Testable<'a> internal (subject: 'a, callerAssembly: Assembly, callerFilePath: string, callerLineNo: int) =

    do
        if isNull Testable.activeUserAssertions then
            Testable.activeUserAssertions <- Dictionary()

        if isNull Testable.topLevelAssertionHistory then
            Testable.topLevelAssertionHistory <- Dictionary()

    // TODO: Clean up items when empty
    [<ThreadStatic; DefaultValue>]
    static val mutable private activeUserAssertions: Dictionary<AssertionChainCallsite, AssertionInfo list>

    // TODO: Cleanup?
    [<ThreadStatic; DefaultValue>]
    static val mutable private topLevelAssertionHistory: Dictionary<AssertionChainCallsite, string list>

    static let pushAssertion callsite method supportsChildAssertions =

        let assertions =
            match Testable.activeUserAssertions.TryGetValue callsite with
            | false, _ -> []
            | true, xs -> xs

        Testable.activeUserAssertions[callsite] <-
            {
                Method = method
                SupportsChildAssertions = supportsChildAssertions
            }
            :: assertions

    static let tryPopAssertion callsite =
        match Testable.activeUserAssertions.TryGetValue callsite with
        | false, _ -> ()
        | true, [] -> ()
        | true, hd :: tl ->
            Testable.activeUserAssertions[callsite] <- tl

            match Testable.topLevelAssertionHistory.TryGetValue callsite with
            | false, _ -> Testable.topLevelAssertionHistory[callsite] <- [ hd.Method ]
            | true, xs -> Testable.topLevelAssertionHistory[callsite] <- hd.Method :: xs


    let canPushAssertion callsite =
        match Testable.activeUserAssertions with
        | null -> true
        | dict ->
            match dict.TryGetValue callsite with
            | false, _ -> true
            | true, [] -> true
            | true, hd :: _ -> hd.SupportsChildAssertions


    internal new(subject: 'a, continueFrom: Testable<'a>) =
        Testable(subject, continueFrom.CallerAssembly, continueFrom.CallerFilePath, continueFrom.CallerLineNo)


    member private _.Callsite = {
        Assembly = callerAssembly
        File = callerFilePath
        Line = callerLineNo
    }


    /// Call this at the start of your assertions, and make sure to dispose the returned value at the end. This is
    /// needed to track important state necessary for subject names to work. If your assertion calls user code that is
    /// expected to call their own assertions (like `Satisfy`), call `t.Assert(true)` instead. In that case, do not
    /// call other assertions directly in the implementation; the next assertion is assumed to be called by the user.
    member this.Assert
        (
            // TODO: supportsChildAssertions - rename? And/or perhaps make this member private and surface two distinct members?
            [<Optional; DefaultParameterValue(false)>] supportsChildAssertions,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] assertionMethod: string
        ) =

        if not (canPushAssertion this.Callsite) then
            IDisposable.noOp
        else
            pushAssertion this.Callsite assertionMethod supportsChildAssertions

            { new IDisposable with
                member _.Dispose() = tryPopAssertion this.Callsite
            }

    /// Returns the subject being tested. Aliases: Whose, Which.
    member _.Subject: 'a = subject

    /// Returns the subject being tested. Aliases: Subject, Which.
    member _.Whose: 'a = subject

    member internal _.CallerAssembly = callerAssembly

    member internal _.CallerFilePath = callerFilePath

    member internal _.CallerLineNo = callerLineNo

    member internal this.CurrentChainAssertionHistory =
        let topLevelAssertions =
            match Testable.topLevelAssertionHistory.TryGetValue this.Callsite with
            | true, xs -> xs |> List.rev
            | false, _ -> []

        let activeAssertions =
            match Testable.activeUserAssertions.TryGetValue this.Callsite with
            | true, xs -> xs |> List.map (fun x -> x.Method) |> List.rev
            | false, _ -> []

        topLevelAssertions @ activeAssertions


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
        Testable(this, Assembly.GetCallingAssembly(), fn, lno)
