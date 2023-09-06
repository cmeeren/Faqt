namespace Faqt

open System
open System.Runtime.CompilerServices
open Faqt.AssertionHelpers


[<Extension>]
type FunctionAssertions =


    /// Asserts that the subject throws the specified exception type or a subtype.
    ///
    /// Note that due to F# limitations, two type parameters are required: Throw<MyException, _>()
    [<Extension>]
    [<RequiresExplicitTypeArguments>]
    static member Throw<'exn, 'ignored when 'exn :> Exception>
        (
            t: Testable<unit -> 'ignored>,
            ?because
        ) : AndDerived<unit -> 'ignored, 'exn> =
        use _ = t.Assert()

        let retVal =
            try
                t.Subject() |> ignore
                ValueNone
            with
            | :? 'exn as ex -> ValueSome(AndDerived(t, ex))
            | ex -> t.With("Exception", typeof<'exn>).With("But threw", ex).Fail(because)

        retVal
        |> ValueOption.defaultWith (fun () ->
            t.With("Exception", typeof<'exn>).With("But succeeded", true).Fail(because)
        )


    /// Asserts that the subject throws the specified exception type or a subtype, or throws an exception that has such
    /// an inner exception at any level.
    ///
    /// Note that due to F# limitations, two type parameters are required: ThrowInner<MyException, _>()
    [<Extension>]
    [<RequiresExplicitTypeArguments>]
    static member ThrowInner<'exn, 'ignored when 'exn :> Exception>
        (
            t: Testable<unit -> 'ignored>,
            ?because
        ) : AndDerived<unit -> 'ignored, 'exn> =
        use _ = t.Assert()

        let rec getMatchingException (ex: Exception) =
            match ex with
            | :? 'exn as ex -> Some ex
            | :? AggregateException as ex -> ex.InnerExceptions |> Seq.tryPick getMatchingException
            | ex ->
                match ex.InnerException with
                | null -> None
                | innerEx -> getMatchingException innerEx

        let retVal =
            try
                t.Subject() |> ignore
                ValueNone
            with ex ->
                match getMatchingException ex with
                | Some ex -> ValueSome(AndDerived(t, ex))
                | None -> t.With("Exception", typeof<'exn>).With("But threw", ex).Fail(because)

        retVal
        |> ValueOption.defaultWith (fun () ->
            t.With("Exception", typeof<'exn>).With("But succeeded", true).Fail(because)
        )


    /// Asserts that the subject throws the specified exception type (not a subtype).
    ///
    /// Note that due to F# limitations, two type parameters are required: ThrowInner<MyException, _>()
    [<Extension>]
    [<RequiresExplicitTypeArguments>]
    static member ThrowExactly<'exn, 'ignored when 'exn :> Exception>
        (
            t: Testable<unit -> 'ignored>,
            ?because
        ) : AndDerived<unit -> 'ignored, 'exn> =
        use _ = t.Assert()

        let retVal =
            try
                t.Subject() |> ignore
                ValueNone
            with
            | :? 'exn as ex when ex.GetType() = typeof<'exn> -> ValueSome(AndDerived(t, ex))
            | ex -> t.With("Exception", typeof<'exn>).With("But threw", ex).Fail(because)

        retVal
        |> ValueOption.defaultWith (fun () ->
            t.With("Exception", typeof<'exn>).With("But succeeded", true).Fail(because)
        )


    /// Asserts that the subject does not throw.
    [<Extension>]
    static member NotThrow(t: Testable<unit -> 'ignored>, ?because) : And<unit -> 'ignored> =
        use _ = t.Assert()

        try
            t.Subject() |> ignore
            And(t)
        with ex ->
            t.With("But threw", ex).Fail(because)
