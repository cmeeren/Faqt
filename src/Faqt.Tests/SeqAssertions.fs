module SeqAssertions

open Faqt
open Xunit


module SatisfyAll =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        [ "asd"; "123" ]
            .Should()
            .AllSatisfy(fun x -> x.Length.Should().Pass())
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
