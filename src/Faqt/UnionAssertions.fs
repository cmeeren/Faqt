namespace Faqt

open System.Runtime.CompilerServices
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open Faqt.AssertionHelpers


[<Extension>]
type UnionAssertions =


    /// Asserts that the subject is of the specified case, and allows continuing to assert on the value of that case.
    /// Note that it is not necessary to use explicit quotations; call it as `.BeOfCase(MyDuCase)`.
    [<Extension>]
    static member BeOfCase
        (t: Testable<'a>, [<ReflectedDefinition>] caseConstructor: Quotations.Expr<'b -> 'a>, ?because)
        : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let rec inner (expr: Expr) =
            match expr with
            | Lambda(_, expr) -> inner expr
            | Let(_, _, expr) -> inner expr
            | NewUnionCase(caseInfo, _) when
                caseInfo.Tag <> FSharpValue.PreComputeUnionTagReaderCached typeof<'a> t.Subject
                ->
                t.With("Expected", caseInfo.Name).With("But was", t.Subject).Fail(because)

            | NewUnionCase(caseInfo, _) ->
                let fieldValues = FSharpValue.PreComputeUnionReaderCached caseInfo t.Subject

                let caseData =
                    match fieldValues with
                    | [| x |] -> x :?> 'b
                    | _ ->
                        let tupleType = FSharpType.MakeCaseTupleTypeCached caseInfo
                        FSharpValue.MakeTuple(fieldValues, tupleType) :?> 'b

                AndDerived(t, caseData)
            | _ -> invalidOp $"The specified expression is not a case constructor for %s{typeof<'a>.FullName}"

        inner caseConstructor


    /// Asserts that the subject is Some, and allows continuing to assert on the inner value. Equivalent to
    /// BeOfCase(Some) (but with a different error message).
    [<Extension>]
    static member BeSome(t: Testable<'a option>, ?because) : AndDerived<'a option, 'a> =
        use _ = t.Assert()

        match t.Subject with
        | Some x -> AndDerived(t, x)
        | None -> t.With("But was", t.Subject).Fail(because)


    /// Asserts that the subject is None. Equivalent to BeOfCase(None) and Be(None) (but with a different error
    /// message).
    [<Extension>]
    static member BeNone(t: Testable<'a option>, ?because) : And<'a option> =
        use _ = t.Assert()

        match t.Subject with
        | None -> And(t)
        | Some _ -> t.With("But was", t.Subject).Fail(because)


    /// Asserts that the subject is Ok, and allows continuing to assert on the inner value. Alias of Equivalent to
    /// BeOfCase(Ok) (but with a different error message).
    [<Extension>]
    static member BeOk(t: Testable<Result<'a, 'b>>, ?because) : AndDerived<Result<'a, 'b>, 'a> =
        use _ = t.Assert()

        match t.Subject with
        | Ok x -> AndDerived(t, x)
        | Error _ -> t.With("But was", t.Subject).Fail(because)


    /// Asserts that the subject is Error, and allows continuing to assert on the inner value. Equivalent to
    /// BeOfCase(Error) (but with a different error message).
    [<Extension>]
    static member BeError(t: Testable<Result<'a, 'b>>, ?because) : AndDerived<Result<'a, 'b>, 'b> =
        use _ = t.Assert()

        match t.Subject with
        | Error x -> AndDerived(t, x)
        | Ok _ -> t.With("But was", t.Subject).Fail(because)
