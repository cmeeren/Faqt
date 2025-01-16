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
    let ``Overload resolution for case without data when type is unknown`` () =
        ignore (fun x -> x.Should().BeOfCase(NoFields))


    [<Fact>]
    let ``Overload resolution for case with data when type is unknown`` () =
        ignore (fun x -> x.Should().BeOfCase(SingleFieldInt))


    [<Fact>]
    let ``NoFields passes and can be chained with And`` () =
        NoFields.Should().BeOfCase(NoFields).Id<And<MyDu>>().And.Be(NoFields)


    [<Fact>]
    let ``SingleFieldInt passes and can be chained with AndDerived with inner value`` () =
        (SingleFieldInt 1).Should().BeOfCase(SingleFieldInt).Id<AndDerived<MyDu, int>>().WhoseValue.Should(()).Be(1)


    [<Fact>]
    let ``SingleFieldRecord passes and can be chained with AndDerived with inner value`` () =
        (SingleFieldRecord { A = 1; B = "a" })
            .Should()
            .BeOfCase(SingleFieldRecord)
            .Id<AndDerived<MyDu, RecordFieldData>>()
            .WhoseValue.Should(())
            .Be({ A = 1; B = "a" })


    [<Fact>]
    let ``SingleFieldAnonymousRecord passes and can be chained with AndDerived with inner value`` () =
        (SingleFieldAnonymousRecord {| X = "a"; Y = 1 |})
            .Should()
            .BeOfCase(SingleFieldAnonymousRecord)
            .Id<AndDerived<MyDu, {| X: string; Y: int |}>>()
            .WhoseValue.Should(())
            .Be({| X = "a"; Y = 1 |})


    [<Fact>]
    let ``MultipleAnonymousFields passes and can be chained with AndDerived with inner value`` () =
        MultipleAnonymousFields(1, "a")
            .Should()
            .BeOfCase(MultipleAnonymousFields)
            .Id<AndDerived<MyDu, int * string>>()
            .WhoseValue.Should(())
            .Be((1, "a"))


    [<Fact>]
    let ``MultipleNamedFields passes and can be chained with AndDerived with inner value`` () =
        MultipleNamedFields(1, "a")
            .Should()
            .BeOfCase(MultipleNamedFields)
            .Id<AndDerived<MyDu, int * string>>()
            .WhoseValue.Should(())
            .Be((1, "a"))


    [<Fact>]
    let ``SingleFieldInt fails when actual value is a different case`` () =
        fun () ->
            let x = NoFields
            x.Should().BeOfCase(SingleFieldInt)
        |> assertExnMsg
            """
Subject: x
Should: BeOfCase
Expected: SingleFieldInt
But was: NoFields
"""


    [<Fact>]
    let ``NoFields fails when actual value is a different case`` () =
        fun () ->
            let x = SingleFieldInt 1
            x.Should().BeOfCase(NoFields)
        |> assertExnMsg
            """
Subject: x
Should: BeOfCase
Expected: NoFields
But was:
  SingleFieldInt: 1
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
    let ``Passes for Some and can be chained with AndDerived with inner value`` () =
        (Some 1).Should().BeSome().Id<AndDerived<int option, int>>().WhoseValue.Should(()).Be(1)


    [<Fact>]
    let ``Fails with expected message if None`` () =
        fun () ->
            let x = Option<int>.None
            x.Should().BeSome()
        |> assertExnMsg
            """
Subject: x
Should: BeSome
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if None`` () =
        fun () ->
            let x = Option<int>.None
            x.Should().BeSome("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeSome
But was: null
"""


module BeNone =


    [<Fact>]
    let ``Passes for None and can be chained with And`` () =
        Option<int>.None.Should().BeNone().Id<And<int option>>().And.Be(None)


    [<Fact>]
    let ``Fails with expected message if Some`` () =
        fun () ->
            let x = Some 1
            x.Should().BeNone()
        |> assertExnMsg
            """
Subject: x
Should: BeNone
But was:
  Some: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if Some`` () =
        fun () ->
            let x = Some 1
            x.Should().BeNone("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNone
But was:
  Some: 1
"""


module BeOk =


    [<Fact>]
    let ``Passes for Ok and can be chained with AndDerived with inner value`` () =
        (Ok 1).Should().BeOk().Id<AndDerived<Result<int, _>, int>>().WhoseValue.Should(()).Be(1)


    [<Fact>]
    let ``Fails with expected message if Error`` () =
        fun () ->
            let x = Error "asd"
            x.Should().BeOk()
        |> assertExnMsg
            """
Subject: x
Should: BeOk
But was:
  Error: asd
"""


    [<Fact>]
    let ``Fails with expected message with because if Error`` () =
        fun () ->
            let x = Error "asd"
            x.Should().BeOk("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeOk
But was:
  Error: asd
"""


module BeError =


    [<Fact>]
    let ``Passes for Error and can be chained with AndDerived with inner value`` () =
        (Error 1).Should().BeError().Id<AndDerived<Result<_, int>, int>>().WhoseValue.Should(()).Be(1)


    [<Fact>]
    let ``Fails with expected message if Ok`` () =
        fun () ->
            let x = Ok "asd"
            x.Should().BeError()
        |> assertExnMsg
            """
Subject: x
Should: BeError
But was:
  Ok: asd
"""


    [<Fact>]
    let ``Fails with expected message with because if Ok`` () =
        fun () ->
            let x = Ok "asd"
            x.Should().BeError("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeError
But was:
  Ok: asd
"""
