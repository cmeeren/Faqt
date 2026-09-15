namespace Faqt

open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type SetAssertions =


    /// Asserts that the subject contains the specified item.
    [<Extension>]
    static member Contain(t: Testable<Set<'a>>, item: 'a, ?because) : AndDerived<_, 'a> =
        use _ = t.Assert()

        match t.Subject |> Seq.tryFind (fun actualItem -> compare actualItem item = 0) with
        | Some actualItem -> AndDerived(t, actualItem)
        | None -> t.With("Item", item).With("But was", t.Subject).Fail(because)


    /// Asserts that the subject does not contain the specified item.
    [<Extension>]
    static member NotContain(t: Testable<Set<'a>>, item: 'a, ?because) : And<_> =
        use _ = t.Assert()

        if Set.contains item t.Subject then
            t.With("Item", item).With("But was", t.Subject).Fail(because)

        And(t)
