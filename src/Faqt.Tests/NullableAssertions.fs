﻿module NullableAssertions

open System
open Faqt
open Xunit


module HaveValue =


    [<Fact>]
    let ``Passes for nullable values that have a value and can be chained with AndDerived with inner value`` () =
        Nullable(1)
            .Should()
            .HaveValue()
            .Id<AndDerived<Nullable<int>, int>>()
            .That.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message for nullable values without a value`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().HaveValue()
        |> assertExnMsg
            """
Subject: x
Should: HaveValue
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().HaveValue("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveValue
But was: null
"""


module NotHaveValue =


    [<Fact>]
    let ``Passes for nullable values that do not have a value and can be chained with And`` () =
        Nullable<int>()
            .Should()
            .NotHaveValue()
            .Id<And<Nullable<int>>>()
            .And.Be(Nullable())


    [<Fact>]
    let ``Fails with expected message for nullable values with a value`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().NotHaveValue()
        |> assertExnMsg
            """
Subject: x
Should: NotHaveValue
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().NotHaveValue("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotHaveValue
But was: 1
"""


module BeNull =


    [<Fact>]
    let ``Passes for nullable values that do not have a value and can be chained with And`` () =
        Nullable<int>().Should().BeNull().Id<And<Nullable<int>>>().And.Be(Nullable())


    [<Fact>]
    let ``Fails with expected message for nullable values with a value`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().BeNull()
        |> assertExnMsg
            """
Subject: x
Should: BeNull
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable(1)
            x.Should().BeNull("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNull
But was: 1
"""


module NotBeNull =


    [<Fact>]
    let ``Passes for nullable values that have a value and can be chained with AndDerived with inner value`` () =
        Nullable(1)
            .Should()
            .NotBeNull()
            .Id<AndDerived<Nullable<int>, int>>()
            .That.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message for nullable values without a value`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().NotBeNull()
        |> assertExnMsg
            """
Subject: x
Should: NotBeNull
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Nullable<int>()
            x.Should().NotBeNull("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBeNull
But was: null
"""
