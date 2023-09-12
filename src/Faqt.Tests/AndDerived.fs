module AndDerived

open System
open Faqt
open Xunit


module And =


    [<Fact>]
    let ``Allows chaining assertions - pass`` () = "asd".Should().PassDerived().And.Pass()


    [<Fact>]
    let ``Allows chaining assertions - fail first`` () =
        fun () -> "asd".Should().FailDerived().And.Pass()
        |> assertExnMsg
            """
Subject: '"asd"'
Should: FailDerived
"""


    [<Fact>]
    let ``Allows chaining assertions - fail second`` () =
        fun () -> "asd".Should().PassDerived().And.Fail()
        |> assertExnMsg
            """
Subject: '"asd"'
Should: Fail
"""


    [<Fact>]
    let ``Realistic example usage`` () =
        [ 1 ].Should().ContainAtLeastOneItem().And.ContainExactlyOneItemMatching((=) 1)


module Subject =


    [<Fact>]
    let ``Returns the original value`` () =
        Assert.Equal("asd", "asd".Should().PassDerived().Subject)


    [<Fact>]
    let ``Realistic example usage`` () =
        let innerValue =
            (Some [ 1 ])
                .Should()
                .BeSome()
                .WhoseValue.Should()
                .ContainExactlyOneItem()
                .Subject

        ignore<int list> innerValue


module Whose =


    [<Fact>]
    let ``Returns the derived value`` () =
        Assert.Equal("asd", "asd".Should().PassDerived().Whose)


    [<Fact>]
    let ``Realistic example usage`` () =
        (Some "asd").Should().BeSome().Whose.Length.Should(()).Be(3)


module That =


    [<Fact>]
    let ``Returns the derived value`` () =
        Assert.Equal("asd", "asd".Should().PassDerived().That)


    [<Fact>]
    let ``Realistic example usage`` () =
        Nullable(1).Should().HaveValue().That.Should(()).BeGreaterThan(0)


module WhoseValue =


    [<Fact>]
    let ``Returns the derived value`` () =
        Assert.Equal("asd", "asd".Should().PassDerived().WhoseValue)


    [<Fact>]
    let ``Realistic example usage`` () =
        (Some "asd").Should().BeSome().WhoseValue.Should(()).NotBeNull()


module Derived =


    [<Fact>]
    let ``Returns the derived value`` () =
        Assert.Equal("asd", "asd".Should().PassDerived().Derived)


    [<Fact>]
    let ``Realistic example usage`` () =
        let innerValue = (Some "asd").Should().BeSome().Derived
        ignore<string> innerValue
