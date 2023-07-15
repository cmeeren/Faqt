namespace Faqt

open System
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


/// The exception raised for all Faqt assertion failures.
type AssertionFailedException(message: string) =
    inherit Exception(message)


[<Struct>]
type Testable<'a> internal (subject: 'a, callerFilePath: string, callerLineNo: int, callerAssembly: Assembly) =

    internal new(subject: 'a, continueFrom: Testable<'a>) =
        Testable(subject, continueFrom.CallerFilePath, continueFrom.CallerLineNo, continueFrom.CallerAssembly)

    /// Returns the subject being tested. Aliases: Whose, Which.
    member _.Subject: 'a = subject

    /// Returns the subject being tested. Aliases: Subject, Which.
    member _.Whose: 'a = subject

    member internal _.CallerFilePath = callerFilePath

    member internal _.CallerLineNo = callerLineNo

    member internal _.CallerAssembly = callerAssembly


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
