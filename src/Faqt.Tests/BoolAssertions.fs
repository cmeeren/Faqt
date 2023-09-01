module BoolAssertions

open Faqt
open Xunit


module BeTrue =


    [<Fact>]
    let ``Passes for true values can be chained with And`` () =
        true.Should().BeTrue().Id<And<bool>>().And.Be(true)


    [<Fact>]
    let ``Fails with expected message if false`` () =
        fun () ->
            let x = false
            x.Should().BeTrue()
        |> assertExnMsg
            """
Subject: x
Should: BeTrue
But was: false
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = false
            x.Should().BeTrue("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeTrue
But was: false
"""


module BeFalse =


    [<Fact>]
    let ``Passes for false values can be chained with And`` () =
        false.Should().BeFalse().Id<And<bool>>().And.Be(false)


    [<Fact>]
    let ``Fails with expected message if true`` () =
        fun () ->
            let x = true
            x.Should().BeFalse()
        |> assertExnMsg
            """
Subject: x
Should: BeFalse
But was: true
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = true
            x.Should().BeFalse("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeFalse
But was: true
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
Subject: x
Should: Imply
But was: true
With other: false
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = true
            x.Should().Imply(false, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Imply
But was: true
With other: false
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
Subject: x
Should: BeImpliedBy
Other: true
But was: false
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = false
            x.Should().BeImpliedBy(true, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeImpliedBy
Other: true
But was: false
"""
