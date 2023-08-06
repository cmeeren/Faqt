module AndDerived

open Faqt
open Xunit


module And =


    [<Fact>]
    let ``Allows chaining assertions - pass`` () = "asd".Should().PassDerived().And.Pass()


    [<Fact>]
    let ``Allows chaining assertions - fail first`` () =
        fun () -> "asd".Should().FailDerived().And.Pass()
        |> assertExnMsg "\"asd\""


    [<Fact>]
    let ``Allows chaining assertions - fail second`` () =
        fun () -> "asd".Should().PassDerived().And.Fail()
        |> assertExnMsg "\"asd\""


    [<Fact>]
    let ``Realistic example usage`` () =
        (Some "asd").Should().BeSome().And.Subject.Value.Should(()).Be("asd")


module Subject =


    [<Fact>]
    let ``Returns the original value`` () =
        Assert.Equal("asd", "asd".Should().PassDerived().Subject)


    [<Fact>]
    let ``Realistic example usage`` () =
        let innerValue = (Some "asd").Should().BeSome().Subject
        ignore<string option> innerValue


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
        (Some "asd").Should().BeSome().That.Should(()).NotBeNull()


module WhoseValue =


    [<Fact>]
    let ``Returns the derived value`` () =
        Assert.Equal("asd", "asd".Should().PassDerived().WhoseValue)


    [<Fact>]
    let ``Realistic example usage`` () =
        (Some "asd").Should().BeSome().WhoseValue.Should(()).NotBeNull()
