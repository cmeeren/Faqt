module UnionAssertions

open System
open Faqt
open Xunit


module BeOfCase =


    type RecordFieldData = { A: int; B: string }


    type MyDu =
        | NoFields
        | SingleFieldInt of int
        | SingleFieldRecord of RecordFieldData
        | SingleFieldAnonymousRecord of {| X: string; Y: int |}
        | MultipleAnonymousFields of int * string
        | MultipleNamedFields of a: int * b: string


    [<Fact>]
    let ``NoFields passes can be chained with And`` () =
        NoFields.Should().BeOfCase(NoFields) |> ignore<And<MyDu>>


    [<Fact>]
    let ``SingleFieldInt passes and allows asserting on inner value`` () =
        (SingleFieldInt 1).Should().BeOfCase(SingleFieldInt).WhoseValue.Should().Be(1)


    [<Fact>]
    let ``SingleFieldRecord passes and allows asserting on inner value`` () =
        (SingleFieldRecord { A = 1; B = "a" })
            .Should()
            .BeOfCase(SingleFieldRecord)
            .WhoseValue.Should()
            .Be({ A = 1; B = "a" })


    [<Fact>]
    let ``SingleFieldAnonymousRecord passes and allows asserting on inner value`` () =
        (SingleFieldAnonymousRecord {| X = "a"; Y = 1 |})
            .Should()
            .BeOfCase(SingleFieldAnonymousRecord)
            .WhoseValue.Should()
            .Be({| X = "a"; Y = 1 |})


    [<Fact>]
    let ``MultipleAnonymousFields passes and allows asserting on inner value`` () =
        MultipleAnonymousFields(1, "a")
            .Should()
            .BeOfCase(MultipleAnonymousFields)
            .WhoseValue.Should()
            .Be((1, "a"))


    [<Fact>]
    let ``MultipleNamedFields passes and allows asserting on inner value`` () =
        MultipleNamedFields(1, "a")
            .Should()
            .BeOfCase(MultipleNamedFields)
            .WhoseValue.Should()
            .Be((1, "a"))


    [<Fact>]
    let ``SingleFieldInt fails when actual value is a different case`` () =
        fun () ->
            let x = NoFields
            x.Should().BeOfCase(SingleFieldInt)
        |> assertExnMsg
            """
x
    should be of case
SingleFieldInt
    but was
NoFields
"""


    [<Fact>]
    let ``NoFields fails when actual value is a different case`` () =
        fun () ->
            let x = SingleFieldInt 1
            x.Should().BeOfCase(NoFields)
        |> assertExnMsg
            """
x
    should be of case
NoFields
    but was
SingleFieldInt 1
"""


    [<Fact>]
    let ``Throws InvalidOperationException for case without data when parameter is not union case`` () =
        let NoFields' = NoFields

        let ex =
            Assert.Throws<InvalidOperationException>(fun () -> NoFields.Should().BeOfCase(NoFields') |> ignore)

        Assert.Equal("The specified expression is not a case constructor for UnionAssertions+BeOfCase+MyDu", ex.Message)


    [<Fact>]
    let ``Throws InvalidOperationException for case with data when parameter is not union case`` () =
        let SingleFieldInt' = SingleFieldInt

        let ex =
            Assert.Throws<InvalidOperationException>(fun () ->
                (SingleFieldInt 1).Should().BeOfCase(SingleFieldInt') |> ignore
            )

        Assert.Equal("The specified expression is not a case constructor for UnionAssertions+BeOfCase+MyDu", ex.Message)


module BeSome =


    [<Fact>]
    let ``Passes for Some and allows asserting on inner value`` () =
        (Some 1).Should().BeSome().WhoseValue.Should().Be(1)


    [<Fact>]
    let ``Fails with expected message for None`` () =
        fun () ->
            let x = Option<int>.None
            x.Should().BeSome()
        |> assertExnMsg
            """
x
    should be of case
Some
    but was
None
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Option<int>.None
            x.Should().BeSome("some reason")
        |> assertExnMsg
            """
x
    should be of case
Some
    because some reason, but was
None
"""


module BeNone =


    [<Fact>]
    let ``Passes for None and can be chained with And`` () =
        Option<int>.None.Should().BeNone() |> ignore<And<int option>>


    [<Fact>]
    let ``Fails with expected message for Some`` () =
        fun () ->
            let x = Some 1
            x.Should().BeNone()
        |> assertExnMsg
            """
x
    should be of case
None
    but was
Some 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = Some 1
            x.Should().BeNone("some reason")
        |> assertExnMsg
            """
x
    should be of case
None
    because some reason, but was
Some 1
"""
