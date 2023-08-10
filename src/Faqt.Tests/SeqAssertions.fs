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
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<string> = null
            x.Should().AllSatisfy(fun y -> y.Length.Should().Test(y.Length = 3))

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<string> = null

            x
                .Should()
                .AllSatisfy((fun y -> y.Length.Should().Test(y.Length = 3)), "some reason")

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion because some reason, but was
<null>
"""


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
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<string> = null

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()) ])

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<string> = null

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()) ], "some reason")

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions because some reason, but was
<null>
"""


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


module HaveLength =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().HaveLength(0).Id<And<int list>>().And.Be([])


    [<Fact>]
    let ``Passes if length = expected`` () = [ 1 ].Should().HaveLength(1)


    [<Fact>]
    let ``Fails if length < expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> List<int>.Empty.Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails if length > expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> [ 1; 2 ].Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().HaveLength(1, "some reason")
        |> assertExnMsg
            """
x
    should have length
1
    because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if length does not match`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but length was
0

[]
"""


    [<Fact>]
    let ``Fails with expected message  if length does not match with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().HaveLength(1, "some reason")
        |> assertExnMsg
            """
x
    should have length
1
    because some reason, but length was
0

[]
"""


module BeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        List<int>.Empty.Should().BeEmpty().Id<And<int list>>().And.Be([])


    [<Fact>]
    let ``Passes if subject is empty`` () = List<int>.Empty.Should().BeEmpty()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = [ 1 ]
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
[1]
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = [ 1 ]
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
[1]
"""


module NotBeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().NotBeEmpty().Id<And<int list>>().And.Be([ 1 ])


    [<Fact>]
    let ``Passes if subject is not empty`` () = [ 1 ].Should().NotBeEmpty()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
x
    should not be empty, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().NotBeEmpty("some reason")
        |> assertExnMsg
            """
x
    should not be empty because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if empty`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
x
    should not be empty, but was empty.
"""


    [<Fact>]
    let ``Fails with expected message if empty with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().NotBeEmpty("some reason")
        |> assertExnMsg
            """
x
    should not be empty because some reason, but was empty.
"""


module BeNullOrEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        Seq.empty<int>.Should().BeNullOrEmpty().Id<And<seq<int>>>().And.Be(Seq.empty)


    [<Fact>]
    let ``Passes if subject is empty`` () = Seq.empty<int>.Should().BeNullOrEmpty()


    [<Fact>]
    let ``Passes if subject is null`` () =
        (null: seq<int>).Should().BeNullOrEmpty()


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = seq { 1 }
            x.Should().BeNullOrEmpty()
        |> assertExnMsg
            """
x
    should be null or empty, but was
seq [1]
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = seq { 1 }
            x.Should().BeNullOrEmpty("some reason")
        |> assertExnMsg
            """
x
    should be null or empty because some reason, but was
seq [1]
"""
