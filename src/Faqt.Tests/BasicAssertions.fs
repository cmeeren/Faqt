module BasicAssertions

open Faqt
open Xunit


module Be =


    [<Fact>]
    let ``Passes for equal integers and can be chained with And`` () =
        (1).Should().Be(1).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes for equal custom type and can be chained with And`` () =
        let x = {| A = 1; B = "foo" |}

        x
            .Should()
            .Be({| A = 1; B = "foo" |})
            .Id<And<{| A: int; B: string |}>>()
            .And.Be({| A = 1; B = "foo" |})


    [<Fact>]
    let ``Fails with expected message for unequal integers`` () =
        fun () ->
            let x = 1
            x.Should().Be(2)
        |> assertExnMsg
            """
x
    should be
2
    but was
1
"""


    [<Fact>]
    let ``Fails with expected message for unequal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().Be({| A = 2; B = "bar" |})
        |> assertExnMsg
            """
x
    should be
{ A = 2
  B = "bar" }
    but was
{ A = 1
  B = "foo" }
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().Be(2, "some reason")
        |> assertExnMsg
            """
x
    should be
2
    because some reason, but was
1
"""


module NotBe =


    [<Fact>]
    let ``Passes for unequal integers and can be chained with And`` () =
        (1).Should().NotBe(2).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes for unequal custom type and can be chained with And`` () =
        let x = {| A = 1; B = "foo" |}

        x
            .Should()
            .NotBe({| A = 2; B = "bar" |})
            .Id<And<{| A: int; B: string |}>>()
            .And.Be({| A = 1; B = "foo" |})


    [<Fact>]
    let ``Fails with expected message for equal integers`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x)
        |> assertExnMsg
            """
x
    should not be
1
    but the values were equal.
"""


    [<Fact>]
    let ``Fails with expected message for equal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().NotBe(x)
        |> assertExnMsg
            """
x
    should not be
{ A = 1
  B = "foo" }
    but the values were equal.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x, "some reason")
        |> assertExnMsg
            """
x
    should not be
1
    because some reason, but the values were equal.
"""


module BeNull =


    [<Fact>]
    let ``Passes for null and can be chained with And`` () =
        (null: string).Should().BeNull().Id<And<string>>().And.BeNull()


    [<Fact>]
    let ``Fails with expected message if not null`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull()
        |> assertExnMsg
            """
x
    should be null, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull("some reason")
        |> assertExnMsg
            """
x
    should be null because some reason, but was
"asd"
"""


module NotBeNull =


    [<Fact>]
    let ``Passes for non-null and can be chained with And`` () =
        "asd".Should().NotBeNull().Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().NotBeNull()
        |> assertExnMsg
            """
x
    should not be null, but was null.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let (x: string) = null
            x.Should().NotBeNull("some reason")
        |> assertExnMsg
            """
x
    should not be null because some reason, but was null.
"""
