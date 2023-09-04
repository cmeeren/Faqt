module EnumAssertions

open System.Text.RegularExpressions
open Faqt
open Xunit


module HaveFlag =


    [<Fact>]
    let ``Can be chained with And`` () =
        RegexOptions.Compiled
            .Should()
            .HaveFlag(RegexOptions.Compiled)
            .Id<And<RegexOptions>>()
            .And.Be(RegexOptions.Compiled)


    [<Theory>]
    [<InlineData(RegexOptions.Compiled, RegexOptions.Compiled)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.Multiline)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.Compiled ||| RegexOptions.Multiline)>]
    let ``Passes if has flag`` (subject: RegexOptions) (expected: RegexOptions) = subject.Should().HaveFlag(expected)


    [<Theory>]
    [<InlineData(RegexOptions.Compiled, RegexOptions.Multiline)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.IgnoreCase)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.Multiline ||| RegexOptions.IgnoreCase)>]
    let ``Fails if not has flag`` (subject: RegexOptions) (expected: RegexOptions) =
        assertFails (fun () -> subject.Should().HaveFlag(expected))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = RegexOptions.Compiled ||| RegexOptions.Multiline
            x.Should().HaveFlag(RegexOptions.IgnoreCase)
        |> assertExnMsg
            """
Subject: x
Should: HaveFlag
Flag: IgnoreCase
But was: Multiline, Compiled
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = RegexOptions.Compiled
            x.Should().HaveFlag(RegexOptions.IgnoreCase, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveFlag
Flag: IgnoreCase
But was: Compiled
"""


module NotHaveFlag =


    [<Fact>]
    let ``Can be chained with And`` () =
        RegexOptions.Compiled
            .Should()
            .NotHaveFlag(RegexOptions.IgnoreCase)
            .Id<And<RegexOptions>>()
            .And.Be(RegexOptions.Compiled)


    [<Theory>]
    [<InlineData(RegexOptions.Compiled, RegexOptions.Multiline)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.IgnoreCase)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.Multiline ||| RegexOptions.IgnoreCase)>]
    let ``Passes if not has flag`` (subject: RegexOptions) (expected: RegexOptions) =
        subject.Should().NotHaveFlag(expected)


    [<Theory>]
    [<InlineData(RegexOptions.Compiled, RegexOptions.Compiled)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.Multiline)>]
    [<InlineData(RegexOptions.Compiled ||| RegexOptions.Multiline, RegexOptions.Compiled ||| RegexOptions.Multiline)>]
    let ``Fails if has flag`` (subject: RegexOptions) (expected: RegexOptions) =
        assertFails (fun () -> subject.Should().NotHaveFlag(expected))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = RegexOptions.Compiled ||| RegexOptions.Multiline
            x.Should().NotHaveFlag(RegexOptions.Compiled)
        |> assertExnMsg
            """
Subject: x
Should: NotHaveFlag
Flag: Compiled
But was: Multiline, Compiled
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = RegexOptions.Compiled
            x.Should().NotHaveFlag(RegexOptions.Compiled, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotHaveFlag
Flag: Compiled
But was: Compiled
"""
