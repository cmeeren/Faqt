module StringAssertions

open Faqt
open Xunit


module HaveLength =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().HaveLength(1).Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Passes if length = expected`` () = "a".Should().HaveLength(1)


    [<Fact>]
    let ``Fails if length < expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> "".Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails if length > expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> "as".Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = ""
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but was
""
    with length
0
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
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
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = ""
            x.Should().HaveLength(1, "some reason")
        |> assertExnMsg
            """
x
    should have length
1
    because some reason, but was
""
    with length
0
"""


module BeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        "".Should().BeEmpty().Id<And<string>>().And.Be("")


    [<Fact>]
    let ``Passes if string is empty`` () = "".Should().BeEmpty()


    [<Fact>]
    let ``Fails if string is not empty`` () =
        Assert.Throws<AssertionFailedException>(fun () -> "a".Should().BeEmpty() |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "a"
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
"a"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "a"
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
"a"
"""
