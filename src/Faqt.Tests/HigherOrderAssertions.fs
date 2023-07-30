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
"asd".Length
    should satisfy the supplied assertion, but the assertion failed with the following message:

x.ToString().Length
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () -> "asd".Should().Satisfy((fun x -> x.Length.Should().Fail()), "some reason")
        |> assertExnMsg
            """
"asd"
    should satisfy the supplied assertion because some reason, but the assertion failed with the following message:

x.Length
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
"asd"
    should not satisfy the supplied assertion, but the assertion succeeded.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () -> "asd".Should().NotSatisfy((fun x -> x.Length.Should().Pass()), "some reason")
        |> assertExnMsg
            """
"asd"
    should not satisfy the supplied assertion because some reason, but the assertion succeeded.
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
"asd"
    should satisfy all of the 3 supplied assertions, but 2 failed.

[Assertion 1/3]

s1.Length

[Assertion 3/3]

s3.Length
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
                    "some reason"
                )
        |> assertExnMsg
            """
"asd"
    should satisfy all of the 3 supplied assertions because some reason, but 2 failed.

[Assertion 1/3]

s1.Length

[Assertion 3/3]

s3.Length
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
                .SatisfyAny([ (fun s1 -> s1.Length.Should().Fail()); (fun s2 -> s2.Length.Should().Fail()) ])
        |> assertExnMsg
            """
"asd"
    should satisfy at least one of the 2 supplied assertions, but none were satisfied.

[Assertion 1/2]

s1.Length

[Assertion 2/2]

s2.Length
"""
