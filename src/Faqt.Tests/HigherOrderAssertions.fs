module HigherOrderAssertions

open System
open Faqt
open Faqt.AssertionHelpers
open Xunit


module EvaluationErrors =


    let private customAssertion (t: Testable<'a>) assertion because =
        use _ = t.Assert(true)

        try
            assertion t.Subject
        with
        | :? AssertionFailedException -> reraise ()
        | ex -> t.With("Operation", "custom operation").RaiseError(ex, because)


    let private nestedError depth custom =
        let original =
            InvalidOperationException("external error", Exception("root-error-marker"))

        let rec run remaining =
            if remaining = 0 then
                raise original
            elif custom then
                customAssertion (remaining.Should()) (fun _ -> run (remaining - 1)) (Some $"context-%i{remaining}|")
            elif remaining % 2 = 0 then
                remaining.Should().Satisfy((fun _ -> run (remaining - 1)), $"context-%i{remaining}|")
                |> ignore
            else
                "subject".Should().Satisfy((fun _ -> run (remaining - 1)), $"context-%i{remaining}|")
                |> ignore

        original, Assert.Throws<Exception>(fun () -> run depth)


    [<Theory>]
    [<InlineData(1, false)>]
    [<InlineData(2, false)>]
    [<InlineData(8, false)>]
    [<InlineData(1, true)>]
    [<InlineData(2, true)>]
    [<InlineData(8, true)>]
    let ``Nested evaluation errors preserve the external exception chain`` depth custom =
        let original, error = nestedError depth custom
        Assert.Same(original, error.InnerException)


    [<Theory>]
    [<InlineData(1, false)>]
    [<InlineData(2, false)>]
    [<InlineData(8, false)>]
    [<InlineData(1, true)>]
    [<InlineData(2, true)>]
    [<InlineData(8, true)>]
    let ``Nested evaluation errors retain context from every level`` depth custom =
        let _, error = nestedError depth custom

        for level in 1..depth do
            Assert.Contains($"context-%i{level}|", error.Message)


    [<Theory>]
    [<InlineData(1, false)>]
    [<InlineData(2, false)>]
    [<InlineData(8, false)>]
    [<InlineData(1, true)>]
    [<InlineData(2, true)>]
    [<InlineData(8, true)>]
    let ``Nested evaluation errors do not multiply the original diagnostic exponentially`` depth custom =
        let _, error = nestedError depth custom
        let occurrences = error.ToString().Split("root-error-marker").Length - 1
        Assert.InRange(occurrences, 1, depth + 1)


    [<Theory>]
    [<InlineData("root-error-marker")>]
    [<InlineData("Operation: custom operation")>]
    let ``Custom evaluation errors include both the exception and supplied context`` expected =
        let _, error = nestedError 1 true
        Assert.Contains(expected, error.Message)


    let cases =
        evaluationErrorCases [ "Satisfy"; "NotSatisfy"; "SatisfyAll"; "SatisfyAny"; "Custom" ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Callback errors cannot become successful negation or alternatives`` assertion composition cancellation =
        assertEvaluationError
            composition
            cancellation
            (fun error ->
                let callback () : unit = raise error

                match assertion with
                | "Satisfy" -> ().Should().Satisfy(callback) |> ignore
                | "NotSatisfy" -> ().Should().NotSatisfy(callback) |> ignore
                | "SatisfyAll" -> ().Should().SatisfyAll([ callback ]) |> ignore
                | "SatisfyAny" -> ().Should().SatisfyAny([ callback; ignore ]) |> ignore
                | "Custom" -> customAssertion (().Should()) callback None
                | _ -> failwith "Unknown assertion"
            )


    [<Theory>]
    [<InlineData("SatisfyAll")>]
    [<InlineData("SatisfyAny")>]
    let ``Aggregators stop at errors after earlier assertion failures`` assertion =
        assertEvaluationError
            "Direct"
            false
            (fun error ->
                let callbacks = [
                    (fun () -> ().Should().Fail() |> ignore)
                    (fun () -> raise error)
                    (fun () -> failwith "This later callback must not run")
                ]

                match assertion with
                | "SatisfyAll" -> ().Should().SatisfyAll(callbacks) |> ignore
                | "SatisfyAny" -> ().Should().SatisfyAny(callbacks) |> ignore
                | _ -> failwith "Unknown assertion"
            )


    [<Theory>]
    [<InlineData("SatisfyAll")>]
    [<InlineData("SatisfyAny")>]
    let ``Ordinary assertion failures are still aggregated`` assertion =
        let callbacks = [
            (fun () -> (1).Should().Be(2) |> ignore)
            (fun () -> (3).Should().Be(4) |> ignore)
        ]

        let error =
            assertFails (fun () ->
                match assertion with
                | "SatisfyAll" -> ().Should().SatisfyAll(callbacks) |> ignore
                | "SatisfyAny" -> ().Should().SatisfyAny(callbacks) |> ignore
                | _ -> failwith "Unknown assertion"
            )

        Assert.Contains("Expected: 2", error.Message)
        Assert.Contains("Expected: 4", error.Message)


    [<Fact>]
    let ``Successful alternative skips later unexpected errors`` () =
        ().Should().SatisfyAny([ ignore; (fun () -> failwith "This later callback must not run") ])


    [<Fact>]
    let ``Explicit NotThrow assertion failure can still be negated`` () =
        (fun () -> failwith<int> "expected exception").Should().NotSatisfy(fun callback -> callback.Should().NotThrow())


module Satisfy =


    [<Fact>]
    let ``Passes if the inner assertion passes and can be chained with And`` () =
        "asd".Should().Satisfy(fun x -> x.Should().Pass()).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if the inner assertion fails`` () =
        fun () -> "asd".Length.Should().Satisfy(fun x -> x.ToString().Length.Should().Fail())
        |> assertExnMsg
            """
Subject: '"asd".Length'
Should: Satisfy
Failure:
  Subject: x.ToString().Length
  Should: Fail
Subject value: 3
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () -> "asd".Should().Satisfy((fun x -> x.Length.Should().Fail()), "Some reason")
        |> assertExnMsg
            """
Subject: '"asd"'
Because: Some reason
Should: Satisfy
Failure:
  Subject: x.Length
  Should: Fail
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message if the inner assertion throws`` () =
        fun () -> "asd".Length.Should().Satisfy(fun _ -> failwith "foo")
        |> assertErrorMsgWildcard
            """
Subject: '"asd".Length'
Should: Satisfy
But threw: |-
  System.Exception: foo
*
Subject value: 3
"""


    [<Fact>]
    let ``Fails with expected message if the inner assertion throws with because`` () =
        fun () -> "asd".Length.Should().Satisfy((fun _ -> failwith "foo"), "Some reason")
        |> assertErrorMsgWildcard
            """
Subject: '"asd".Length'
Because: Some reason
Should: Satisfy
But threw: |-
  System.Exception: foo
*
Subject value: 3
"""


module NotSatisfy =


    [<Fact>]
    let ``Passes if the inner assertion fails and can be chained with And`` () =
        "asd".Should().NotSatisfy(fun x -> x.Should().Fail()).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if the inner assertion passes`` () =
        fun () -> "asd".Should().NotSatisfy(fun x -> x.Should().Pass())
        |> assertExnMsg
            """
Subject: '"asd"'
Should: NotSatisfy
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () -> "asd".Should().NotSatisfy((fun x -> x.Length.Should().Pass()), "Some reason")
        |> assertExnMsg
            """
Subject: '"asd"'
Because: Some reason
Should: NotSatisfy
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message if the inner assertion throws`` () =
        fun () -> "asd".Should().NotSatisfy(fun _ -> failwith "foo")
        |> assertErrorMsgWildcard
            """
Subject: '"asd"'
Should: NotSatisfy
But threw: |-
  System.Exception: foo
*
Subject value: asd
"""


module SatisfyAll =


    [<Fact>]
    let ``Passes if assertion list is empty and can be chained with And`` () =
        "asd".Should().SatisfyAll([]).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if all of the inner assertions passes`` () =
        "asd"
            .Should()
            .SatisfyAll(
                [
                    (fun s1 -> s1.Length.Should().Pass())
                    (fun s2 -> s2.Length.Should().Pass())
                    (fun s3 -> s3.Length.Should().Pass())
                ]
            )
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if at least one of the inner assertions fails or throws`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAll(
                    [
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun s2 -> s2.Length.Should().Pass())
                        (fun _ -> failwith "foo")
                    ]
                )
        |> assertErrorMsgWildcard
            """
Subject: '"asd"'
Should: SatisfyAll
Failures:
- Index: 0
  Failure:
    Subject: s1.Length
    Should: Fail
- Index: 2
  Exception: |-
    System.Exception: foo
*
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAll(
                    [
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun s2 -> s2.Length.Should().Pass())
                        (fun _ -> failwith "foo")
                    ],
                    "Some reason"
                )
        |> assertErrorMsgWildcard
            """
Subject: '"asd"'
Because: Some reason
Should: SatisfyAll
Failures:
- Index: 0
  Failure:
    Subject: s1.Length
    Should: Fail
- Index: 2
  Exception: |-
    System.Exception: foo
*
Subject value: asd
"""


module SatisfyAny =


    [<Fact>]
    let ``Passes if assertion list is empty and can be chained with And`` () =
        "asd".Should().SatisfyAny([]).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if all of the inner assertions passes`` () =
        "asd".Should().SatisfyAny([ (fun s1 -> s1.Length.Should().Pass()); (fun s2 -> s2.Length.Should().Pass()) ])


    [<Fact>]
    let ``Passes if only one of the inner assertions passes 1`` () =
        "asd".Should().SatisfyAny([ (fun s1 -> s1.Length.Should().Fail()); (fun s2 -> s2.Length.Should().Pass()) ])


    [<Fact>]
    let ``Passes if only one of the inner assertions passes 2`` () =
        "asd".Should().SatisfyAny([ (fun s1 -> s1.Length.Should().Pass()); (fun s2 -> s2.Length.Should().Fail()) ])


    [<Fact>]
    let ``Fails with expected message if all of the inner assertions fail or throws`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAny(
                    [
                        // Comment to force break
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun _ -> failwith "foo")
                    ]
                )
        |> assertErrorMsgWildcard
            """
Subject: '"asd"'
Should: SatisfyAny
Failures:
- Subject: s1.Length
  Should: Fail
- Exception: |-
    System.Exception: foo
*
Subject value: asd
"""

    [<Fact>]
    let ``Fails with expected message if all of the inner assertions fail or throws with because`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAny(
                    [
                        // Comment to force break
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun _ -> failwith "foo")
                    ],
                    "Some reason"
                )
        |> assertErrorMsgWildcard
            """
Subject: '"asd"'
Because: Some reason
Should: SatisfyAny
Failures:
- Subject: s1.Length
  Should: Fail
- Exception: |-
    System.Exception: foo
*
Subject value: asd
"""
