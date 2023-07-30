namespace Faqt

open System.Runtime.CompilerServices
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open AssertionHelpers


[<Extension>]
type UnionAssertions =


    /// Asserts that the subject is of the specified case, and allows continuing to assert on the value of that case.
    /// Note that it is not necessary to use explicit quotations; call it as `.BeOfCase(MyDuCase)`.
    [<Extension>]
    static member BeOfCase
        (
            t: Testable<'a>,
            [<ReflectedDefinition>] caseConstructor: Quotations.Expr<'b -> 'a>,
            ?because
        ) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let rec inner (expr: Expr) =
            match expr with
            | Lambda(_, expr) -> inner expr
            | Let(_, _, expr) -> inner expr
            | NewUnionCase(caseInfo, _) when
                caseInfo.Tag <> FSharpValue.PreComputeUnionTagReaderCached typeof<'a> t.Subject
                ->
                t.Fail("{subject}\n\tshould be of case\n{0}\n\t{because}but was\n{actual}", because, caseInfo.Name)

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
            t.Fail("{subject}\n\tshould be of case\n{0}\n\t{because}but was\n{actual}", because, caseInfo.Name)
        | NewUnionCase _ -> And(t)
        | _ -> invalidOp $"The specified expression is not a case constructor for %s{typeof<'a>.FullName}"


    /// Asserts that the subject is Some, and allows continuing to assert on the inner value. Alias of BeOfCase(Some).
    [<Extension>]
    static member BeSome(t: Testable<'a option>, ?because) : AndDerived<'a option, 'a> =
        use _ = t.Assert()
        t.BeOfCase(Some, ?because = because)


    /// Asserts that the subject is None. Alias of BeOfCase(None), and equivalent to Be(None) (but with a different
    /// error message).
    [<Extension>]
    static member BeNone(t: Testable<'a option>, ?because) : And<'a option> =
        use _ = t.Assert()
        t.BeOfCase(None, ?because = because)


    /// Asserts that the subject is Ok, and allows continuing to assert on the inner value. Alias of BeOfCase(Ok).
    [<Extension>]
    static member BeOk(t: Testable<Result<'a, 'b>>, ?because) : AndDerived<Result<'a, 'b>, 'a> =
        use _ = t.Assert()
        t.BeOfCase(Ok, ?because = because)


    /// Asserts that the subject is Error, and allows continuing to assert on the inner value. Alias of BeOfCase(Error).
    [<Extension>]
    static member BeError(t: Testable<Result<'a, 'b>>, ?because) : AndDerived<Result<'a, 'b>, 'b> =
        use _ = t.Assert()
        t.BeOfCase(Error, ?because = because)
