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
"""
