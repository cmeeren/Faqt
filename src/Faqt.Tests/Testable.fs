module Testable

open Faqt
open Xunit


module Subject =


    [<Fact>]
    let ``Returns the inner value`` () =
        Assert.Equal("asd", "asd".Should().Subject)


    [<Fact>]
    let ``Realistic example usage`` () =
        "asd".Should().Be("asd").And.Subject.Length.Should().Be(3)


module Whose =


    [<Fact>]
    let ``Returns the inner value`` () =
        Assert.Equal("asd", "asd".Should().Whose)


    [<Fact>]
    let ``Realistic example usage`` () =
        (Some "asd")
            .Should()
            .BeSome()
            .That.Should()
            .Be("asd")
            .And.Whose.Length.Should()
            .Be(3)
