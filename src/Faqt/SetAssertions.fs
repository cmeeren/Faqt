namespace Faqt

open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type SetAssertions =


    /// Asserts that the subject contains the specified item.
    [<Extension>]
    static member Contain(t: Testable<Set<'a>>, item: 'a, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        if isNull (box t.Subject) || not (Set.contains item t.Subject) then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        AndDerived(t, item)


    /// Asserts that the subject does not contain the specified item. Passes if the subject is null.
    [<Extension>]
    static member NotContain(t: Testable<Set<'a>>, item: 'a, ?because) : And<_> =
        use _ = t.Assert()

        if not (isNull (box t.Subject)) && Set.contains item t.Subject then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        And(t)
