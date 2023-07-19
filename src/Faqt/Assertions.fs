namespace Faqt

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open Faqt.AssertionHelpers
open Formatting


[<Extension>]
type Assertions =


    /// Asserts that the subject satisfies the specified assertion. Often you can just use And for making multiple
    /// assertions, but Satisfy (combined with And) can be useful if you want to fluently perform multiple assertion
    /// chains, for example if asserting on different parts of a value.
    [<Extension>]
    static member Satisfy
        (
            t: Testable<'a>,
            assertion: 'a -> 'ignored,
            [<Optional; DefaultParameterValue("")>] because: string
        ) : And<'a> =
        use x = t.Assert(true)

        try
            assertion t.Subject |> ignore
            And(t)
        with :? AssertionFailedException as ex ->
            t.Fail(
                "{subject}\n\tshould satisfy the supplied assertion{because}, but the assertion failed with the following message:\n{0}",
                because,
                ex.Message
            )


    /// Asserts that the subject satisfies at least one of the specified assertions.
    [<Extension>]
    static member SatisfyAny
        (
            t: Testable<'a>,
            assertions: seq<'a -> 'ignored>,
            [<Optional; DefaultParameterValue("")>] because: string
        ) : And<'a> =
        use _ = t.Assert(true)
        let assertions = assertions |> Seq.toArray

        let exceptions =
            assertions
            |> Array.choose (fun f ->
                try
                    f t.Subject |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some ex
            )

        if exceptions.Length = assertions.Length then
            let assertionFailuresString =
                exceptions
                |> Seq.mapi (fun i ex -> $"\n\n[Assertion %i{i + 1}/%i{assertions.Length}]\n%s{ex.Message}")
                |> String.concat ""

            t.Fail(
                "{subject}\n\tshould satisfy at least one of the {0} supplied assertions{because}, but none were satisfied.{1}",
                because,
                string assertions.Length,
                assertionFailuresString
            )

        And(t)


    /// Asserts that the subject is the specified value, using the default equality comparison (=).
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, [<Optional; DefaultParameterValue("")>] because: string) : And<'a> =
        use _ = t.Assert()

        if t.Subject <> expected then
            t.Fail("{subject}\n\tshould be\n{0}\n\t{because}but was\n{actual}", because, format expected)

        And(t)


    /// Asserts that the subject is not the specified value, using default equality comparison (=).
    [<Extension>]
    static member NotBe
        (
            t: Testable<'a>,
            expected: 'a,
            [<Optional; DefaultParameterValue("")>] because: string
        ) : And<'a> =
        use _ = t.Assert()

        if t.Subject = expected then
            t.Fail("{subject}\n\tshould not be\n{0}\n\t{because}but the values were equal.", because, format expected)

        And(t)


    /// Asserts that the subject is null.
    [<Extension>]
    static member BeNull(t: Testable<'a>, [<Optional; DefaultParameterValue("")>] because: string) : And<'a> =
        use _ = t.Assert()

        if not (isNull t.Subject) then
            t.Fail("{subject}\n\tshould be null{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is not null.
    [<Extension>]
    static member NotBeNull(t: Testable<'a>, [<Optional; DefaultParameterValue("")>] because: string) : And<'a> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.Fail("{subject}\n\tshould not be null{because}, but was null.", because)

        And(t)


    /// Asserts that the subject is of the specified case, and allows continuing to assert on the value of that case.
    /// Note that it is not necessary to use explicit quotations; call it as `.BeOfCase(MyDuCase)`.
    [<Extension>]
    static member BeOfCase
        (
            t: Testable<'a>,
            [<ReflectedDefinition>] caseConstructor: Quotations.Expr<'b -> 'a>,
            [<Optional; DefaultParameterValue("")>] because: string
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
    /// Note that it is not necessary to use explicit quotations; call it as `.BeOfCase(MyDuCase)`.
    [<Extension>]
    static member BeOfCase
        (
            t: Testable<'a>,
            [<ReflectedDefinition>] caseConstructor: Quotations.Expr<'a>,
            [<Optional; DefaultParameterValue("")>] because: string
        ) : And<'a> =
        use _ = t.Assert()

        match caseConstructor with
        | NewUnionCase(caseInfo, _) when caseInfo.Tag <> FSharpValue.PreComputeUnionTagReaderCached typeof<'a> t.Subject ->
            t.Fail("{subject}\n\tshould be of case\n{0}\n\t{because}but was\n{actual}", because, caseInfo.Name)
        | NewUnionCase _ -> And(t)
        | _ -> invalidOp $"The specified expression is not a case constructor for %s{typeof<'a>.FullName}"


    /// Asserts that the subject is Some, and allows continuing to assert on the inner value. Alias of BeOfCase(Some).
    [<Extension>]
    static member BeSome
        (
            t: Testable<'a option>,
            [<Optional; DefaultParameterValue("")>] because: string
        ) : AndDerived<'a option, 'a> =
        use _ = t.Assert()
        t.BeOfCase(Some, because)


    /// Asserts that the subject is None. Alias of BeOfCase(None), and equivalent to Be(None) (but with a different
    /// error message).
    [<Extension>]
    static member BeNone
        (
            t: Testable<'a option>,
            [<Optional; DefaultParameterValue("")>] because: string
        ) : And<'a option> =
        use _ = t.Assert()
        t.BeOfCase(None, because)
