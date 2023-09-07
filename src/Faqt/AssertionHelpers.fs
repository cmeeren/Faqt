namespace Faqt.AssertionHelpers

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Faqt


[<Extension>]
type TestableExtensions =


    /// Call this at the start of your assertions, and make sure to dispose the returned value at the end. This is
    /// needed to track important state necessary for subject names to work. If your assertion calls user code that is
    /// expected to call their own assertions (like `Satisfy`), call `t.Assert(true)` instead. In that case, do not call
    /// other assertions directly in the implementation; the next assertion is assumed to be called by the user. If
    /// additionally you invoke user assertions for each item in a sequence, tall `t.Assert(true, true)` instead.
    [<Extension>]
    static member Assert
        (
            this: Testable<'a>,
            [<Optional; DefaultParameterValue(false)>] supportsChildAssertions,
            [<Optional; DefaultParameterValue(false)>] isSeqAssertion,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] assertionMethod: string
        ) =
        this.Assert'(supportsChildAssertions, isSeqAssertion, assertionMethod)


    /// If your assertion invokes user-supplied assertions for each item in a sequence, call this before the assertion
    /// invocation for each item, and make sure to dispose the returned value after the assertion invocation. This is
    /// needed to track important state necessary for subject names to work.
    [<Extension>]
    static member AssertItem(this: Testable<'a>) = this.AssertItem'()


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
