namespace Faqt

open System
open System.Runtime.CompilerServices


/// Place this attribute on classes containing assertion extensions as an alternative to applying FaqtAssertionAttribute
/// to individual methods/functions. See FaqtAssertionAttribute for details.
[<AttributeUsage(AttributeTargets.Class)>]
type ContainsFaqtAssertionsAttribute() =
    inherit Attribute()


/// Place this attribute on assertion methods as well as any intermediate helper functions/methods that call other
/// assertions. (Not needed if the type/module has ContainsFaqtAssertionsAttribute.) In short, from the top level
/// assertion (that a user calls) to the innermost assertion (that the top-level assertion delegates to), the methods in
/// all stack frames must contain an unbroken chain of this attribute.
[<AttributeUsage(AttributeTargets.Method
                 ||| AttributeTargets.Property
                 ||| AttributeTargets.Constructor)>]
type FaqtAssertionAttribute() =
    inherit Attribute()


/// The exception raised for all Faqt assertion failures.
type AssertionFailedException(message: string) =
    inherit Exception(message)


[<Struct>]
type Testable<'a>(subject: 'a) =

    /// Returns the subject being tested. Aliases: Whose, Which.
    member _.Subject: 'a = subject

    /// Returns the subject being tested. Aliases: Subject, Which.
    member _.Whose: 'a = subject


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
    static member Should(this: 'a) : Testable<'a> = Testable(this)
