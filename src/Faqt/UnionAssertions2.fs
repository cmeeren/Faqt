namespace Faqt

open System.Runtime.CompilerServices
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open Faqt.AssertionHelpers


[<Extension>]
type UnionAssertions2 =


    /// Asserts that the subject is of the specified case, and allows continuing to assert on the value of that case.
    /// Note that it is not necessary to use explicit quotations; call it as `.BeOfCase(MyDuCase)`. This overload (for
    /// data-less cases) is equivalent to Be(MyDuCase) (but with a different error message).
    [<Extension>]
    static member BeOfCase
        (
            t: Testable<'a>,
            [<ReflectedDefinition>] caseConstructor: Quotations.Expr<'a>,
            ?because
        ) : And<'a> =
        use _ = t.Assert()

        match caseConstructor with
        | NewUnionCase(caseInfo, _) when caseInfo.Tag <> FSharpValue.PreComputeUnionTagReaderCached typeof<'a> t.Subject ->
            t.With("Expected", caseInfo.Name).With("But was", t.Subject).Fail(because)
        | NewUnionCase _ -> And(t)
        | _ -> invalidOp $"The specified expression is not a case constructor for %s{typeof<'a>.FullName}"
