module BoolAssertions

open Faqt
open Xunit


module BeTrue =


    [<Fact>]
    let ``Passes for true values can be chained with And`` () =
        true.Should().BeTrue().Id<And<bool>>().And.Be(true)


    [<Fact>]
    let ``Fails with expected message for false values`` () =
        fun () ->
            let x = false
            x.Should().BeTrue()
        |> assertExnMsg
            """
x
    should be
true
    but was
false
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = false
            x.Should().BeTrue("some reason")
        |> assertExnMsg
            """
x
    should be
true
    because some reason, but was
false
"""


module BeFalse =


    [<Fact>]
    let ``Passes for false values can be chained with And`` () =
        false.Should().BeFalse().Id<And<bool>>().And.Be(false)


    [<Fact>]
    let ``Fails with expected message for true values`` () =
        fun () ->
            let x = true
            x.Should().BeFalse()
        |> assertExnMsg
            """
x
    should be
false
    but was
true
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = true
            x.Should().BeFalse("some reason")
        |> assertExnMsg
            """
x
    should be
false
    because some reason, but was
true
"""


module Imply =


    [<Fact>]
    let ``Passes if subject is false and other is false and can be chained with And`` () =
        false.Should().Imply(false).Id<And<bool>>().And.Be(false)


    [<Fact>]
    let ``Passes if subject is false and other is true`` () = false.Should().Imply(true)


    [<Fact>]
    let ``Passes if subject is true and other is true`` () = true.Should().Imply(true)


    [<Fact>]
    let ``Fails with expected message if subject is true and other is false`` () =
        fun () ->
            let x = true
            x.Should().Imply(false)
        |> assertExnMsg
            """
x
    should imply the specified value, but subject was
true
    and the specified value was
false
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = true
            x.Should().Imply(false, "some reason")
        |> assertExnMsg
            """
x
    should imply the specified value because some reason, but subject was
true
    and the specified value was
false
"""


module BeImpliedBy =


    [<Fact>]
    let ``Passes if subject is false and other is false and can be chained with And`` () =
        false.Should().BeImpliedBy(false).Id<And<bool>>().And.Be(false)


    [<Fact>]
    let ``Passes if subject is true and other is false`` () = true.Should().BeImpliedBy(false)


    [<Fact>]
    let ``Passes if subject is true and other is true`` () = true.Should().BeImpliedBy(true)


    [<Fact>]
    let ``Fails with expected message if subject is false and other is true`` () =
        fun () ->
            let x = false
            x.Should().BeImpliedBy(true)
        |> assertExnMsg
            """
x
    should be implied by the specified value, but the value was
true
    and the subject was
false
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = false
            x.Should().BeImpliedBy(true, "some reason")
        |> assertExnMsg
            """
x
    should be implied by the specified value because some reason, but the value was
true
    and the subject was
false
"""
