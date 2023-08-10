module SeqAssertions

open Faqt
open Xunit


module SatisfyAll =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        [ "asd"; "123" ]
            .Should()
            .AllSatisfy(fun x -> x.Should().Pass())
            .Id<And<string list>>()
            .And.Be([ "asd"; "123" ])


    [<Fact>]
    let ``Passes if subject is empty`` () =
        List<int>.Empty.Should().AllSatisfy(fun x -> x.Should().Fail())


    [<Fact>]
    let ``Fails with expected message if at least one of the elements fail to satisfy the assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]
            x.Should().AllSatisfy(fun y -> y.Length.Should().Test(y.Length = 3))

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion, but 2 of 3 items failed.

[Item 2/3]

y.Length

[Item 3/3]

y.Length
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .AllSatisfy((fun y -> y.Length.Should().Test(y.Length = 3)), "some reason")

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion because some reason, but 2 of 3 items failed.

[Item 2/3]

y.Length

[Item 3/3]

y.Length
"""


module SatisfyRespectively =


    [<Fact>]
    let ``Passes if same length and all of the inner assertions passes and can be chained with And`` () =
        [ "asd"; "123" ]
            .Should()
            .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])
            .Id<And<string list>>()
            .And.Be([ "asd"; "123" ])


    [<Fact>]
    let ``Passes if subject and assertions are empty`` () =
        List<int>.Empty.Should().SatisfyRespectively([])


    [<Fact>]
    let ``Fails with expected message if subject does not contain one element per assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the
2
    specified assertions, but actual length was
3

["asd"; "test"; "1234"]
"""


    [<Fact>]
    let ``Fails with expected message if subject does not contain one element per assertion with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ], "some reason")

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the
2
    specified assertions because some reason, but actual length was
3

["asd"; "test"; "1234"]
"""


    [<Fact>]
    let ``Fails with expected message if at least one of the elements fail to satisfy the assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively(
                    [
                        (fun x1 -> x1.Should().Fail())
                        (fun x2 -> x2.Should().Pass())
                        (fun x3 -> x3.Should().Fail())
                    ]
                )

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions, but 2 of 3 items failed.

[Item 1/3]

x1

[Item 3/3]

x3
"""


    [<Fact>]
    let ``Fails with expected message if at least one of the elements fail to satisfy the assertion with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively(
                    [
                        (fun x1 -> x1.Should().Fail())
                        (fun x2 -> x2.Should().Pass())
                        (fun x3 -> x3.Should().Fail())
                    ],
                    "some reason"
                )

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions because some reason, but 2 of 3 items failed.

[Item 1/3]

x1

[Item 3/3]

x3
"""
