namespace Faqt

open System
open System.Collections.Generic
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


/// The exception raised for all Faqt assertion failures.
type AssertionFailedException(message: string) =
    inherit Exception(message)


type internal CallChainOrigin = {
    AssemblyPath: string
    SourceFilePath: string
    LineNumber: int
}


type private AssertionInfo = {
    Method: string
    SupportsChildAssertions: bool
}


type private CallChain() =

    [<ThreadStatic; DefaultValue>]
    static val mutable private activeUserAssertions: Dictionary<CallChainOrigin, AssertionInfo list>

    [<ThreadStatic; DefaultValue>]
    static val mutable private topLevelAssertionHistory: Dictionary<CallChainOrigin, string list>

    static let pushAssertion callsite method supportsChildAssertions =

        let assertions =
            match CallChain.activeUserAssertions.TryGetValue callsite with
            | false, _ -> []
            | true, xs -> xs

        CallChain.activeUserAssertions[callsite] <-
            {
                Method = method
                SupportsChildAssertions = supportsChildAssertions
            }
            :: assertions

    static let tryPopAssertion callsite =
        match CallChain.activeUserAssertions.TryGetValue callsite with
        | false, _ -> ()
        | true, [] -> ()
        | true, hd :: tl ->
            CallChain.activeUserAssertions[callsite] <- tl

            match CallChain.topLevelAssertionHistory.TryGetValue callsite with
            | false, _ -> CallChain.topLevelAssertionHistory[callsite] <- [ hd.Method ]
            | true, xs -> CallChain.topLevelAssertionHistory[callsite] <- hd.Method :: xs


    static let canPushAssertion callsite =
        match CallChain.activeUserAssertions with
        | null -> true
        | dict ->
            match dict.TryGetValue callsite with
            | false, _ -> true
            | true, [] -> true
            | true, hd :: _ -> hd.SupportsChildAssertions

    static member private EnsureInitialized() =
        if isNull CallChain.activeUserAssertions then
            CallChain.activeUserAssertions <- Dictionary()

        if isNull CallChain.topLevelAssertionHistory then
            CallChain.topLevelAssertionHistory <- Dictionary()


    static member Assert(callsite, assertionMethod, supportsChildAssertions) =
        CallChain.EnsureInitialized()

        if not (canPushAssertion callsite) then
            IDisposable.noOp
        else
            pushAssertion callsite assertionMethod supportsChildAssertions

            { new IDisposable with
                member _.Dispose() = tryPopAssertion callsite
            }

    static member AssertionHistory(callsite) =
        let topLevelAssertions =
            match CallChain.topLevelAssertionHistory.TryGetValue callsite with
            | true, xs -> xs |> List.rev
            | false, _ -> []

        let activeAssertions =
            match CallChain.activeUserAssertions.TryGetValue callsite with
            | true, xs -> xs |> List.map (fun x -> x.Method) |> List.rev
            | false, _ -> []

        topLevelAssertions @ activeAssertions


[<Struct>]
type Testable<'a> internal (subject: 'a, origin: CallChainOrigin) =

    /// Call this at the start of your assertions, and make sure to dispose the returned value at the end. This is
    /// needed to track important state necessary for subject names to work. If your assertion calls user code that is
    /// expected to call their own assertions (like `Satisfy`), call `t.Assert(true)` instead. In that case, do not
    /// call other assertions directly in the implementation; the next assertion is assumed to be called by the user.
    member _.Assert
        (
            [<Optional; DefaultParameterValue(false)>] supportsChildAssertions,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] assertionMethod: string
        ) =
        CallChain.Assert(origin, assertionMethod, supportsChildAssertions)

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
            [<CallerFilePath; Optional; DefaultParameterValue("")>] filePath: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] lineNumber: int
        ) : Testable<'a> =
        let origin = {
            AssemblyPath = Assembly.GetCallingAssembly().Location
            SourceFilePath = filePath
            LineNumber = lineNumber
        }

        Testable(this, origin)
