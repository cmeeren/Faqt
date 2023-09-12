module And

open Faqt
open Xunit


module And =


    [<Fact>]
    let ``Allows chaining assertions - pass`` () = "asd".Should().Pass().And.Pass()


    [<Fact>]
    let ``Allows chaining assertions - fail first`` () =
        fun () -> "asd".Should().Fail().And.Pass()
        |> assertExnMsg
            """
Subject: '"asd"'
Should: Fail
"""


    [<Fact>]
    let ``Allows chaining assertions - fail second`` () =
        fun () -> "asd".Should().Pass().And.Fail()
        |> assertExnMsg
            """
Subject: '"asd"'
Should: Fail
"""


    [<Fact>]
    let ``Realistic example usage`` () =
        "asd".Should().NotBeNull().And.HaveLength(3)


module Subject =


    [<Fact>]
    let ``Returns the original value`` () =
        Assert.Equal("asd", "asd".Should().Pass().Subject)


    [<Fact>]
    let ``Realistic example usage`` () =
        let innerValue =
            (Some "asd").Should().BeSome().WhoseValue.Should().HaveLength(3).Subject

        ignore<string> innerValue
