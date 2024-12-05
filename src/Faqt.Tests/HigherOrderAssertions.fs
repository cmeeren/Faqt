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
        |> assertExnMsgWildcard
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
        |> assertExnMsgWildcard
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
        |> assertExnMsgWildcard
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
        |> assertExnMsgWildcard
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
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Length.Should().Pass()); (fun s2 -> s2.Length.Should().Pass()) ])


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
        |> assertExnMsgWildcard
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
        |> assertExnMsgWildcard
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
