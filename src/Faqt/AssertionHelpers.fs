namespace Faqt.AssertionHelpers

open System.Runtime.CompilerServices
open Faqt


[<Extension>]
type TestableExtensions =


    /// Use this overload when calling Should() in custom assertions.
    [<Extension>]
    static member Should(this: 'a, continueFrom: Testable<'b>) : Testable<'a> =
        Testable(this, continueFrom.CallChainOrigin)


    /// Raises an AssertionFailedException with the specified message.
    [<Extension>]
    static member Fail(this: Testable<'a>, because: string option) =
        { Testable = this; Data = [] }.Fail(because)


    /// Adds the specified key/value to the failure message if condition is true.
    [<Extension>]
    static member With(this: Testable<'a>, condition: bool, key: string, value: 'b) =
        { Testable = this; Data = [] }.With(condition, key, value)


    /// Adds the specified key/value to the failure message.
    [<Extension>]
    static member With(this: Testable<'a>, key: string, value: 'b) =
        { Testable = this; Data = [] }.With(key, value)
