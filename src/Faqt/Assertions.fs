namespace Faqt

open System
open System.Runtime.CompilerServices
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open AssertionHelpers


[<Extension; ContainsFaqtAssertions>]
type Assertions =


    /// Asserts that the subject satisfies the specified assertion. Often you can just use And for making multiple
    /// assertions, but Satisfy (combined with And) can be useful if you want to fluently perform multiple assertion
    /// chains, for example if asserting on different parts of a value.
    [<Extension>]
    static member Satisfy(t: Testable<'a>, assertion: 'a -> 'ignored, ?because) : And<'a> =
        try
            assertion t.Subject |> ignore
            And(t)
        with :? AssertionFailedException as ex ->
            fail
                $"{sub ()}\n\tshould satisfy the supplied assertion{sbc because}, but the assertion failed with the following message:\n{ex.Message}"


    [<Extension>]
    static member private SatisfyAny(t: Testable<'a>, because, assertions: ('a -> 'ignored)[]) : And<'a> =
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

            fail
                $"{sub ()}\n\tshould satisfy at least one of the %i{assertions.Length} supplied assertions{sbc because}, but none were satisfied.{assertionFailuresString}"

        And(t)

    /// Asserts that the subject satisfies at least one of the specified assertions.
    [<Extension>]
    static member SatisfyAny(t: Testable<'a>, [<ParamArray>] assertions: ('a -> 'ignored)[]) : And<'a> =
        t.SatisfyAny(None, assertions)


    /// Asserts that the subject satisfies at least one of the specified assertions.
    [<Extension>]
    static member SatisfyAnyBecause
        (
            t: Testable<'a>,
            because: string,
            [<ParamArray>] assertions: ('a -> 'ignored)[]
        ) : And<'a> =
        t.SatisfyAny(Some because, Seq.toArray assertions)


    /// Asserts that the subject is the specified value, using the default equality comparison (=).
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        if t.Subject <> expected then
            fail $"{sub ()}\n\tshould be\n{fmt expected}\n\t{bcc because}but was\n{fmt t.Subject}"

        And(t)


    /// Asserts that the subject is not the specified value, using default equality comparison (=).
    [<Extension>]
    static member NotBe(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        if t.Subject = expected then
            fail $"{sub ()}\n\tshould not be\n{fmt expected}\n\t{bcc because}but the values were equal."

        And(t)


    /// Asserts that the subject is null.
    [<Extension>]
    static member BeNull(t: Testable<'a>, ?because) : And<'a> =
        if not (isNull t.Subject) then
            fail $"{sub ()}\n\tshould be null{sbc because}, but was\n{fmt t.Subject}"

        And(t)


    /// Asserts that the subject is not null.
    [<Extension>]
    static member NotBeNull(t: Testable<'a>, ?because) : And<'a> =
        if isNull t.Subject then
            fail $"{sub ()}\n\tshould not be null{sbc because}, but was null."

        And(t)


    /// Asserts that the subject is of the specified case, and allows continuing to assert on the value of that case.
    /// Note that it is not necessary to use explicit quotations; call it as `.BeOfCase(MyDuCase)`.
    [<Extension>]
    static member BeOfCase
        (
            t: Testable<'a>,
            [<ReflectedDefinition>] caseConstructor: Quotations.Expr<'b -> 'a>,
            ?because
        ) : AndDerived<'a, 'b> =
        let rec inner (expr: Expr) =
            match expr with
            | Lambda(_, expr) -> inner expr
            | Let(_, _, expr) -> inner expr
            | NewUnionCase(caseInfo, _) when
                caseInfo.Tag <> FSharpValue.PreComputeUnionTagReaderCached typeof<'a> t.Subject
                ->
                fail $"{sub ()}\n\tshould be of case\n{caseInfo.Name}\n\t{bcc because}but was\n{fmt t.Subject}"
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
            ?because
        ) : And<'a> =
        match caseConstructor with
        | NewUnionCase(caseInfo, _) when caseInfo.Tag <> FSharpValue.PreComputeUnionTagReaderCached typeof<'a> t.Subject ->
            fail $"{sub ()}\n\tshould be of case\n{caseInfo.Name}\n\t{bcc because}but was\n{fmt t.Subject}"
        | NewUnionCase _ -> And(t)
        | _ -> invalidOp $"The specified expression is not a case constructor for %s{typeof<'a>.FullName}"


    /// Asserts that the subject is Some, and allows continuing to assert on the inner value. Alias of BeOfCase(Some).
    [<Extension>]
    static member BeSome(t: Testable<'a option>, ?because) : AndDerived<'a option, 'a> =
        t.BeOfCase(Some, ?because = because)


    /// Asserts that the subject is None. Alias of BeOfCase(None), and equivalent to Be(None) (but with a different
    /// error message).
    [<Extension>]
    static member BeNone(t: Testable<'a option>, ?because) : And<'a option> = t.BeOfCase(None, ?because = because)
