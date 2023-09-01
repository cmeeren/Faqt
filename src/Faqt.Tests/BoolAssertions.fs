module BoolAssertions

open Faqt
open Xunit


module BeTrue =


    [<Fact>]
    let ``Passes if true and can be chained with And`` () =
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
    let ``Fails with expected message with because if false`` () =
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
    let ``Passes if false and can be chained with And`` () =
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
    let ``Fails with expected message with because if true`` () =
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
    let ``Can be chained with And`` () =
        true.Should().Imply(true).Id<And<bool>>().And.Be(true)


    [<Theory>]
    [<InlineData(false, false)>]
    [<InlineData(false, true)>]
    [<InlineData(true, true)>]
    let ``Passes if subject is false or both are true`` (subject: bool) (other: bool) = subject.Should().Imply(other)


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
    let ``Fails with expected message with because if subject is true and other is false`` () =
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
    let ``Can be chained with And`` () =
        true.Should().BeImpliedBy(true).Id<And<bool>>().And.Be(true)


    [<Theory>]
    [<InlineData(false, false)>]
    [<InlineData(true, false)>]
    [<InlineData(true, true)>]
    let ``Passes if subject is true or both are true`` (subject: bool) (other: bool) =
        subject.Should().BeImpliedBy(other)


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
    let ``Fails with expected message with because if subject is false and other is true`` () =
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
