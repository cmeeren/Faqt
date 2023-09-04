module GuidAssertions

open System
open Faqt
open Xunit


module Be =


    [<Fact>]
    let ``Can be chained with And`` () =
        let guid = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
        guid.Should().Be(guid.ToString()).Id<And<Guid>>().And.Be(guid)


    [<Theory>]
    [<InlineData("65006ace-504c-4ddb-8e4f-567c834d8125", "65006ace504c4ddb8e4f567c834d8125")>]
    [<InlineData("65006ACE-504C-4DDB-8E4F-567C834D8125", "{65006ace-504c-4ddb-8e4f-567c834d8125}")>]
    [<InlineData("65006ace504c4ddb8e4f567c834d8125", "65006ace504c4ddb8e4f567c834d8125")>]
    [<InlineData("{65006ace-504c-4ddb-8e4f-567c834d8125}", "65006ACE-504C-4DDB-8E4F-567C834D8125")>]
    [<InlineData("(65006ace-504c-4ddb-8e4f-567c834d8125)", "(65006ace-504c-4ddb-8e4f-567c834d8125)")>]
    let ``Passes if equal`` (subject: string) (expected: string) =
        Guid.Parse(subject).Should().Be(expected)


    [<Theory>]
    [<InlineData("65006ace-504c-4ddb-8e4f-567c834d8125", "00000000-0000-0000-0000-000000000000")>]
    [<InlineData("65006ace-504c-4ddb-8e4f-567c834d8125", "de949ab3-d592-4af9-bb7f-a8eaef986e17")>]
    let ``Fails if not equal`` (subject: string) (expected: string) =
        assertFails (fun () -> Guid.Parse(subject).Should().Be(expected))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
            x.Should().Be("de949ab3-d592-4af9-bb7f-a8eaef986e17")
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: de949ab3-d592-4af9-bb7f-a8eaef986e17
But was: 65006ace-504c-4ddb-8e4f-567c834d8125
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
            x.Should().Be("de949ab3-d592-4af9-bb7f-a8eaef986e17", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be
Expected: de949ab3-d592-4af9-bb7f-a8eaef986e17
But was: 65006ace-504c-4ddb-8e4f-567c834d8125
"""


module NotBe =


    [<Fact>]
    let ``Can be chained with And`` () =
        let guid = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")

        guid
            .Should()
            .NotBe("de949ab3-d592-4af9-bb7f-a8eaef986e17")
            .Id<And<Guid>>()
            .And.Be(guid)


    [<Theory>]
    [<InlineData("65006ace-504c-4ddb-8e4f-567c834d8125", "00000000-0000-0000-0000-000000000000")>]
    [<InlineData("65006ace-504c-4ddb-8e4f-567c834d8125", "de949ab3-d592-4af9-bb7f-a8eaef986e17")>]
    let ``Passes if not equal`` (subject: string) (other: string) =
        Guid.Parse(subject).Should().NotBe(other)


    [<Theory>]
    [<InlineData("65006ace-504c-4ddb-8e4f-567c834d8125", "65006ace504c4ddb8e4f567c834d8125")>]
    [<InlineData("65006ACE-504C-4DDB-8E4F-567C834D8125", "{65006ace-504c-4ddb-8e4f-567c834d8125}")>]
    [<InlineData("65006ace504c4ddb8e4f567c834d8125", "65006ace504c4ddb8e4f567c834d8125")>]
    [<InlineData("{65006ace-504c-4ddb-8e4f-567c834d8125}", "65006ACE-504C-4DDB-8E4F-567C834D8125")>]
    [<InlineData("(65006ace-504c-4ddb-8e4f-567c834d8125)", "(65006ace-504c-4ddb-8e4f-567c834d8125)")>]
    let ``Fails if equal`` (subject: string) (other: string) =
        assertFails (fun () -> Guid.Parse(subject).Should().NotBe(other))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
            x.Should().NotBe("65006ace-504c-4ddb-8e4f-567c834d8125")
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other: 65006ace-504c-4ddb-8e4f-567c834d8125
But was: 65006ace-504c-4ddb-8e4f-567c834d8125
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
            x.Should().NotBe("65006ace-504c-4ddb-8e4f-567c834d8125", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBe
Other: 65006ace-504c-4ddb-8e4f-567c834d8125
But was: 65006ace-504c-4ddb-8e4f-567c834d8125
"""


module BeEmpty =


    [<Fact>]
    let ``Passes if empty and can be chained with And`` () =
        Guid.Empty.Should().BeEmpty().Id<And<Guid>>().And.Be(Guid.Empty)


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
            x.Should().BeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: BeEmpty
But was: 65006ace-504c-4ddb-8e4f-567c834d8125
"""


    [<Fact>]
    let ``Fails with expected message with because if not empty`` () =
        fun () ->
            let x = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
            x.Should().BeEmpty("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeEmpty
But was: 65006ace-504c-4ddb-8e4f-567c834d8125
"""


module NotBeEmpty =


    [<Fact>]
    let ``Passes if empty and can be chained with And`` () =
        let guid = Guid.Parse("65006ace-504c-4ddb-8e4f-567c834d8125")
        guid.Should().NotBeEmpty().Id<And<Guid>>().And.Be(guid)


    [<Fact>]
    let ``Fails with expected message if empty`` () =
        fun () ->
            let x = Guid.Empty
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: NotBeEmpty
But was: 00000000-0000-0000-0000-000000000000
"""


    [<Fact>]
    let ``Fails with expected message with because if not empty`` () =
        fun () ->
            let x = Guid.Empty
            x.Should().NotBeEmpty("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBeEmpty
But was: 00000000-0000-0000-0000-000000000000
"""
