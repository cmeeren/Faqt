module HigherOrderAssertions

open Faqt
open Xunit


module Satisfy =


    [<Fact>]
    let ``Passes if the inner assertion passes and can be chained with And`` () =
        "asd"
            .Should()
            .Satisfy(fun x -> x.Should().Pass())
            .Id<And<string>>()
            .And.Be("asd")


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
"""


module NotSatisfy =


    [<Fact>]
    let ``Passes if the inner assertion fails and can be chained with And`` () =
        "asd"
            .Should()
            .NotSatisfy(fun x -> x.Should().Fail())
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if the inner assertion passes`` () =
        fun () -> "asd".Should().NotSatisfy(fun x -> x.Should().Pass())
        |> assertExnMsg
            """
Subject: '"asd"'
Should: NotSatisfy
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () -> "asd".Should().NotSatisfy((fun x -> x.Length.Should().Pass()), "Some reason")
        |> assertExnMsg
            """
Subject: '"asd"'
Because: Some reason
Should: NotSatisfy
"""


module SatisfyAll =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
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
    let ``Passes if assertion list is empty`` () = "asd".Should().SatisfyAll([])


    [<Fact>]
    let ``Fails with expected message if at least one of the inner assertions fail`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAll(
                    [
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun s2 -> s2.Length.Should().Pass())
                        (fun s3 -> s3.Length.Should().Fail())
                    ]
                )
        |> assertExnMsg
            """
Subject: '"asd"'
Should: SatisfyAll
Failures:
- Index: 0
  Failure:
    Subject: s1.Length
    Should: Fail
- Index: 2
  Failure:
    Subject: s3.Length
    Should: Fail
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
                        (fun s3 -> s3.Length.Should().Fail())
                    ],
                    "Some reason"
                )
        |> assertExnMsg
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
  Failure:
    Subject: s3.Length
    Should: Fail
"""


module SatisfyAny =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Length.Should().Pass()); (fun s2 -> s2.Length.Should().Pass()) ])
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if assertion list is empty`` () = "asd".Should().SatisfyAny([])


    [<Fact>]
    let ``Passes if only one of the inner assertions passes 1`` () =
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Length.Should().Fail()); (fun s2 -> s2.Length.Should().Pass()) ])


    [<Fact>]
    let ``Passes if only one of the inner assertions passes 2`` () =
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Length.Should().Pass()); (fun s2 -> s2.Length.Should().Fail()) ])


    [<Fact>]
    let ``Fails with expected message if all of the inner assertions fail`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAny(
                    [
                        // Comment to force break
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun s2 -> s2.Length.Should().Fail())
                    ]
                )
        |> assertExnMsg
            """
Subject: '"asd"'
Should: SatisfyAny
Failures:
- Subject: s1.Length
  Should: Fail
- Subject: s2.Length
  Should: Fail
"""
