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
        "asd".Should().NotBeNull().And.Subject.Length.Should(()).Be(3)


module Subject =


    [<Fact>]
    let ``Returns the original value`` () =
        Assert.Equal("asd", "asd".Should().Pass().Subject)


    [<Fact>]
    let ``Realistic example usage`` () =
        let lengthValue =
            (Some "asd").Should().BeSome().Whose.Length.Should(()).Be(3).Subject

        ignore<int> lengthValue
