﻿namespace Faqt

open System.Runtime.CompilerServices
open Faqt.AssertionHelpers
open Faqt.Formatting


[<AutoOpen>]
module private HigherOrderAssertionsHelpers =


    type SatisfyAllReportFailureItem = { Index: int; Failure: FailureData }


    type SatisfyAllReportExceptionItem = { Index: int; Exception: TryFormat }


[<Extension>]
type HigherOrderAssertions =


    /// Asserts that the subject satisfies the supplied assertion. Often you can just use And for making multiple
    /// assertions, but Satisfy (combined with And) can be useful if you want to fluently perform multiple assertion
    /// chains, for example if asserting on different parts of a value.
    [<Extension>]
    static member Satisfy(t: Testable<'a>, assertion: 'a -> 'ignored, ?because) : And<'a> =
        use x = t.Assert(true)

        try
            assertion t.Subject |> ignore
            And(t)
        with
        | :? AssertionFailedException as ex ->
            t.With("Failure", ex.FailureData).With("Subject value", t.Subject).Fail(because)
        | ex -> t.With("But threw", ex).With("Subject value", t.Subject).Fail(because)


    /// Asserts that the subject does not satisfy the supplied assertion. If using this in performance critical
    /// scenarios, note that in general, assertions are optimized for success, not failure.
    [<Extension>]
    static member NotSatisfy(t: Testable<'a>, assertion: 'a -> 'ignored, ?because) : And<'a> =
        use x = t.Assert(true)

        let succeeded =
            try
                assertion t.Subject |> ignore
                true
            with :? AssertionFailedException ->
                false

        if succeeded then
            t.With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject satisfies at least one of the supplied assertions. If using this in performance
    /// critical scenarios, note that in general, assertions are optimized for success, not failure.
    [<Extension>]
    static member SatisfyAny(t: Testable<'a>, assertions: seq<'a -> 'ignored>, ?because) : And<'a> =
        use _ = t.Assert(true)
        let assertions = assertions |> Seq.toArray

        if assertions.Length > 0 then
            let failures = ResizeArray()
            let mutable succeeded = false

            for f in assertions do
                if not succeeded then
                    try
                        f t.Subject |> ignore
                        succeeded <- true
                    with
                    | :? AssertionFailedException as ex -> failures.Add(box ex.FailureData)
                    | ex -> failures.Add(box {| Exception = ex |})

            if not succeeded then
                t.With("Failures", failures).With("Subject value", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject satisfies all of the supplied assertions.
    [<Extension>]
    static member SatisfyAll(t: Testable<'a>, assertions: seq<'a -> 'ignored>, ?because) : And<'a> =
        use _ = t.Assert(true)
        let assertions = assertions |> Seq.toArray

        let exceptions =
            assertions
            |> Seq.indexed
            |> Seq.choose (fun (i, f) ->
                try
                    f t.Subject |> ignore
                    None
                with ex ->
                    Some(i, ex)
            )
            |> Seq.toArray

        if exceptions.Length > 0 then
            t
                .With(
                    "Failures",
                    exceptions
                    |> Array.map (fun (i, ex) ->
                        match ex with
                        | :? AssertionFailedException as ex -> box { Index = i; Failure = ex.FailureData }
                        | ex -> box { Index = i; Exception = TryFormat ex }
                    )
                )
                .With("Subject value", t.Subject)
                .Fail(because)

        And(t)
