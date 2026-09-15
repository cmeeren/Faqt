module EnumAssertions

open System
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


    [<Theory>]
    [<InlineData(RegexOptions.None, false)>]
    [<InlineData(RegexOptions.Compiled, false)>]
    [<InlineData(RegexOptions.None, true)>]
    [<InlineData(RegexOptions.Compiled, true)>]
    let ``Passes for a zero flag regardless of subject`` (subject: RegexOptions) asEnum =
        if asEnum then
            (subject :> Enum).Should().HaveFlag(RegexOptions.None :> Enum) |> ignore
        else
            subject.Should().HaveFlag(RegexOptions.None) |> ignore


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


    [<Theory>]
    [<InlineData(RegexOptions.None, false)>]
    [<InlineData(RegexOptions.Compiled, false)>]
    [<InlineData(RegexOptions.None, true)>]
    [<InlineData(RegexOptions.Compiled, true)>]
    let ``Fails for a zero flag regardless of subject`` (subject: RegexOptions) asEnum =
        assertFails (fun () ->
            if asEnum then
                (subject :> Enum).Should().NotHaveFlag(RegexOptions.None :> Enum) |> ignore
            else
                subject.Should().NotHaveFlag(RegexOptions.None) |> ignore
        )


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
