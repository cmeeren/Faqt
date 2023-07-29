module NullableAssertions

open System
open Faqt
open Xunit


module HaveValue =


    [<Fact>]
    let ``Passes for nullable values that have a value and can be chained with AndDerived with inner value`` () =
        Nullable(1)
            .Should()
            .HaveValue()
            .Id<AndDerived<Nullable<int>, int>>()
            .That.Should()
            .Be(1)


    [<Fact>]
    let ``Fails with expected message for nullable values without a value`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().HaveValue()
        |> assertExnMsg
            """
x
    should have a value, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().HaveValue("some reason")
        |> assertExnMsg
            """
x
    should have a value because some reason, but was
<null>
"""


module NotHaveValue =


    [<Fact>]
    let ``Passes for nullable values that do not have a value and can be chained with And`` () =
        Nullable<int>()
            .Should()
            .NotHaveValue()
            .Id<And<Nullable<int>>>()
            .And.Be(Nullable())


    [<Fact>]
    let ``Fails with expected message for nullable values with a value`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().NotHaveValue()
        |> assertExnMsg
            """
x
    should not have a value, but was
1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().NotHaveValue("some reason")
        |> assertExnMsg
            """
x
    should not have a value because some reason, but was
1
"""


module BeNull =


    [<Fact>]
    let ``Passes for nullable values that do not have a value and can be chained with And`` () =
        Nullable<int>().Should().BeNull().Id<And<Nullable<int>>>().And.Be(Nullable())


    [<Fact>]
    let ``Fails with expected message for nullable values with a value`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().BeNull()
        |> assertExnMsg
            """
x
    should be null, but was
1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().BeNull("some reason")
        |> assertExnMsg
            """
x
    should be null because some reason, but was
1
"""


module NotBeNull =


    [<Fact>]
    let ``Passes for nullable values that have a value and can be chained with AndDerived with inner value`` () =
        Nullable(1)
            .Should()
            .NotBeNull()
            .Id<AndDerived<Nullable<int>, int>>()
            .That.Should()
            .Be(1)


    [<Fact>]
    let ``Fails with expected message for nullable values without a value`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().NotBeNull()
        |> assertExnMsg
            """
x
    should not be null, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().NotBeNull("some reason")
        |> assertExnMsg
            """
x
    should not be null because some reason, but was
<null>
"""
