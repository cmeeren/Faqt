module Testable

open System
open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
open Xunit


module Subject =


    [<Fact>]
    let ``Returns the inner value`` () =
        Assert.Equal("asd", "asd".Should().Subject)


    [<Fact>]
    let ``Realistic example usage`` () =
        "asd".Should().Be("asd").And.Subject.Length.Should(()).Be(3)


module Whose =


    [<Fact>]
    let ``Returns the inner value`` () =
        Assert.Equal("asd", "asd".Should().Whose)


    [<Fact>]
    let ``Realistic example usage`` () =
        (Some "asd")
            .Should()
            .BeSome()
            .That.Should(())
            .NotBe("a")
            .And.Whose.Length.Should(())
            .Be(3)


module ``With and Fail`` =


    [<Extension>]
    type private Assertions =


        [<Extension>]
        static member Do(t: Testable<'a>, f: Testable<'a> -> unit) =
            use _ = t.Assert()
            f t


    [<Fact>]
    let ``Fail throws expected exception without because`` () =
        fun () -> "a".Should().Do(fun t -> t.Fail(None))
        |> assertExnMsg
            """
Subject: '"a"'
Should: Do
"""


    [<Fact>]
    let ``Fail throws expected exception with because`` () =
        fun () -> "a".Should().Do(fun t -> t.Fail(Some "Some reason"))
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: Do
"""


    [<Fact>]
    let ``With + Fail throws expected exception without because`` () =
        fun () -> "a".Should().Do(fun t -> t.With("A", 1).Fail(None))
        |> assertExnMsg
            """
Subject: '"a"'
Should: Do
A: 1
"""


    [<Fact>]
    let ``With + Fail throws expected exception with because`` () =
        fun () -> "a".Should().Do(fun t -> t.With("A", 1).Fail(Some "Some reason"))
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: Do
A: 1
"""


    [<Theory>]
    [<InlineData("Subject")>]
    [<InlineData("Because")>]
    [<InlineData("Should")>]
    let ``With throws expected exception for reserved keywords`` key =
        let ex =
            Assert.Throws<ArgumentException>(fun () -> "a".Should().Do(fun t -> t.With(key, 1) |> ignore))

        Assert.Equal($"The key name %s{key} is reserved by Faqt (Parameter 'key')", ex.Message)


    [<Fact>]
    let ``With(true) + Fail throws expected exception without because`` () =
        fun () -> "a".Should().Do(fun t -> t.With(true, "A", 1).Fail(None))
        |> assertExnMsg
            """
Subject: '"a"'
Should: Do
A: 1
"""


    [<Fact>]
    let ``With(true) + Fail throws expected exception with because`` () =
        fun () -> "a".Should().Do(fun t -> t.With(true, "A", 1).Fail(Some "Some reason"))
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: Do
A: 1
"""


    [<Fact>]
    let ``With(false) + Fail throws expected exception without because`` () =
        fun () -> "a".Should().Do(fun t -> t.With(false, "A", 1).Fail(None))
        |> assertExnMsg
            """
Subject: '"a"'
Should: Do
"""


    [<Fact>]
    let ``With(false) + Fail throws expected exception with because`` () =
        fun () -> "a".Should().Do(fun t -> t.With(false, "A", 1).Fail(Some "Some reason"))
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: Do
"""


    [<Theory>]
    [<InlineData("Subject")>]
    [<InlineData("Because")>]
    [<InlineData("Should")>]
    let ``With(bool) throws expected exception for reserved keywords`` key =
        let ex =
            Assert.Throws<ArgumentException>(fun () -> "a".Should().Do(fun t -> t.With(false, key, 1) |> ignore))

        Assert.Equal($"The key name %s{key} is reserved by Faqt (Parameter 'key')", ex.Message)


    [<Fact>]
    let ``With + With + Fail throws expected exception without because`` () =
        fun () -> "a".Should().Do(fun t -> t.With("A", 1).With("B", 2).Fail(None))
        |> assertExnMsg
            """
Subject: '"a"'
Should: Do
A: 1
B: 2
"""


    [<Fact>]
    let ``With + With + Fail throws expected exception with because`` () =
        fun () -> "a".Should().Do(fun t -> t.With("A", 1).With("B", 2).Fail(Some "Some reason"))
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: Do
A: 1
B: 2
"""


    [<Theory>]
    [<InlineData("Subject")>]
    [<InlineData("Because")>]
    [<InlineData("Should")>]
    let ``With + With throws expected exception for reserved keywords`` key =
        let ex =
            Assert.Throws<ArgumentException>(fun () -> "a".Should().Do(fun t -> t.With("A", 1).With(key, 1) |> ignore))

        Assert.Equal($"The key name %s{key} is reserved by Faqt (Parameter 'key')", ex.Message)


    [<Fact>]
    let ``With + With(true) + Fail throws expected exception without because`` () =
        fun () -> "a".Should().Do(fun t -> t.With("A", 1).With(true, "B", 2).Fail(None))
        |> assertExnMsg
            """
Subject: '"a"'
Should: Do
A: 1
B: 2
"""


    [<Fact>]
    let ``With + With(true) + Fail throws expected exception with because`` () =
        fun () ->
            "a"
                .Should()
                .Do(fun t -> t.With("A", 1).With(true, "B", 2).Fail(Some "Some reason"))
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: Do
A: 1
B: 2
"""


    [<Fact>]
    let ``With + With(false) + Fail throws expected exception without because`` () =
        fun () -> "a".Should().Do(fun t -> t.With("A", 1).With(false, "B", 2).Fail(None))
        |> assertExnMsg
            """
Subject: '"a"'
Should: Do
A: 1
"""


    [<Fact>]
    let ``With + With(false) + Fail throws expected exception with because`` () =
        fun () ->
            "a"
                .Should()
                .Do(fun t -> t.With("A", 1).With(false, "B", 2).Fail(Some "Some reason"))
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: Do
A: 1
"""


    [<Theory>]
    [<InlineData("Subject")>]
    [<InlineData("Because")>]
    [<InlineData("Should")>]
    let ``With + With(bool) throws expected exception for reserved keywords`` key =
        let ex =
            Assert.Throws<ArgumentException>(fun () ->
                "a".Should().Do(fun t -> t.With("A", 1).With(false, key, 1) |> ignore)
            )

        Assert.Equal($"The key name %s{key} is reserved by Faqt (Parameter 'key')", ex.Message)
