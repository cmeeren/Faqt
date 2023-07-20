module Assertions

open System
open Faqt
open Xunit


module Satisfy =


    [<Fact>]
    let ``Passes if the inner assertion passes and can be chained with And`` () =
        "asd".Length.Should().Satisfy(fun x -> x.ToString().Length.Should().Pass())
        |> ignore<And<int>>


    [<Fact>]
    let ``Fails with expected message if the inner assertion fails`` () =
        fun () -> "asd".Length.Should().Satisfy(fun x -> x.ToString().Length.Should().Fail())
        |> assertExnMsg
            """
"asd".Length
    should satisfy the supplied assertion, but the assertion failed with the following message:

x.ToString().Length
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () -> "asd".Should().Satisfy((fun x -> x.Length.Should().Fail()), "some reason")
        |> assertExnMsg
            """
"asd"
    should satisfy the supplied assertion because some reason, but the assertion failed with the following message:

x.Length
"""


module SatisfyAll =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        "asd"
            .Should()
            .SatisfyAll(
                [
                    (fun s1 -> s1.Length.Should().Pass())
                    (fun s2 -> s2.Length.Should().Pass())
                    (fun s3 -> s3.Length.Should().Pass())
                ]
            )
        |> ignore<And<string>>


    [<Fact>]
    let ``Fails with expected message if at least one of the inner assertions fail`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAll(
                    [
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun s2 -> s2.Length.Should().Pass())
                        (fun s3 -> s3.Length.Should().Fail())
                    ]
                )
        |> assertExnMsg
            """
"asd"
    should satisfy all of the 3 supplied assertions, but 2 failed.

[Assertion 1/3]

s1.Length

[Assertion 3/3]

s3.Length
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAll(
                    [
                        (fun s1 -> s1.Length.Should().Fail())
                        (fun s2 -> s2.Length.Should().Pass())
                        (fun s3 -> s3.Length.Should().Fail())
                    ],
                    "some reason"
                )
        |> assertExnMsg
            """
"asd"
    should satisfy all of the 3 supplied assertions because some reason, but 2 failed.

[Assertion 1/3]

s1.Length

[Assertion 3/3]

s3.Length
"""


module SatisfyAny =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Length.Should().Pass()); (fun s2 -> s2.Length.Should().Pass()) ])
        |> ignore<And<string>>


    [<Fact>]
    let ``Passes if only one of the inner assertions passes 1`` () =
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Length.Should().Fail()); (fun s2 -> s2.Length.Should().Pass()) ])


    [<Fact>]
    let ``Passes if only one of the inner assertions passes 2`` () =
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Length.Should().Pass()); (fun s2 -> s2.Length.Should().Fail()) ])


    [<Fact>]
    let ``Fails with expected message if all of the inner assertions fail`` () =
        fun () ->
            "asd"
                .Should()
                .SatisfyAny([ (fun s1 -> s1.Length.Should().Fail()); (fun s2 -> s2.Length.Should().Fail()) ])
        |> assertExnMsg
            """
"asd"
    should satisfy at least one of the 2 supplied assertions, but none were satisfied.

[Assertion 1/2]

s1.Length

[Assertion 2/2]

s2.Length
"""


module Be =


    [<Fact>]
    let ``Passes for equal integers and can be chained with And`` () =
        let x = 1
        x.Should().Be(1) |> ignore<And<int>>


    [<Fact>]
    let ``Passes for equal custom type`` () =
        let x = {| A = 1; B = "foo" |}
        x.Should().Be({| A = 1; B = "foo" |})


    [<Fact>]
    let ``Fails with expected message for unequal integers`` () =
        fun () ->
            let x = 1
            x.Should().Be(2)
        |> assertExnMsg
            """
x
    should be
2
    but was
1
"""


    [<Fact>]
    let ``Fails with expected message for unequal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().Be({| A = 2; B = "bar" |})
        |> assertExnMsg
            """
x
    should be
{ A = 2
  B = "bar" }
    but was
{ A = 1
  B = "foo" }
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().Be(2, "some reason")
        |> assertExnMsg
            """
x
    should be
2
    because some reason, but was
1
"""


module NotBe =


    [<Fact>]
    let ``Passes for unequal integers and can be chained with And`` () =
        let x = 1
        x.Should().NotBe(2) |> ignore<And<int>>


    [<Fact>]
    let ``Passes for unequal custom type`` () =
        let x = {| A = 1; B = "foo" |}
        x.Should().NotBe({| A = 2; B = "bar" |})


    [<Fact>]
    let ``Fails with expected message for equal integers`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x)
        |> assertExnMsg
            """
x
    should not be
1
    but the values were equal.
"""


    [<Fact>]
    let ``Fails with expected message for equal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().NotBe(x)
        |> assertExnMsg
            """
x
    should not be
{ A = 1
  B = "foo" }
    but the values were equal.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x, "some reason")
        |> assertExnMsg
            """
x
    should not be
1
    because some reason, but the values were equal.
"""


module BeNull =


    [<Fact>]
    let ``Passes for null and can be chained with And`` () =
        let x: string = null
        x.Should().BeNull() |> ignore<And<string>>


    [<Fact>]
    let ``Fails with expected message if not null`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull()
        |> assertExnMsg
            """
x
    should be null, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull("some reason")
        |> assertExnMsg
            """
x
    should be null because some reason, but was
"asd"
"""


module NotBeNull =


    [<Fact>]
    let ``Passes for non-null and can be chained with And`` () =
        let x = "asd"
        x.Should().NotBeNull() |> ignore<And<string>>


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().NotBeNull()
        |> assertExnMsg
            """
x
    should not be null, but was null.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let (x: string) = null
            x.Should().NotBeNull("some reason")
        |> assertExnMsg
            """
x
    should not be null because some reason, but was null.
"""


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
        (SingleFieldInt 1).Should().BeOfCase(SingleFieldInt).That.Should().Be(1)


    [<Fact>]
    let ``SingleFieldRecord passes and allows asserting on inner value`` () =
        (SingleFieldRecord { A = 1; B = "a" })
            .Should()
            .BeOfCase(SingleFieldRecord)
            .That.Should()
            .Be({ A = 1; B = "a" })


    [<Fact>]
    let ``SingleFieldAnonymousRecord passes and allows asserting on inner value`` () =
        (SingleFieldAnonymousRecord {| X = "a"; Y = 1 |})
            .Should()
            .BeOfCase(SingleFieldAnonymousRecord)
            .That.Should()
            .Be({| X = "a"; Y = 1 |})


    [<Fact>]
    let ``MultipleAnonymousFields passes and allows asserting on inner value`` () =
        MultipleAnonymousFields(1, "a")
            .Should()
            .BeOfCase(MultipleAnonymousFields)
            .That.Should()
            .Be((1, "a"))


    [<Fact>]
    let ``MultipleNamedFields passes and allows asserting on inner value`` () =
        MultipleNamedFields(1, "a")
            .Should()
            .BeOfCase(MultipleNamedFields)
            .That.Should()
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

        Assert.Equal("The specified expression is not a case constructor for Assertions+BeOfCase+MyDu", ex.Message)


    [<Fact>]
    let ``Throws InvalidOperationException for case with data when parameter is not union case`` () =
        let SingleFieldInt' = SingleFieldInt

        let ex =
            Assert.Throws<InvalidOperationException>(fun () ->
                (SingleFieldInt 1).Should().BeOfCase(SingleFieldInt') |> ignore
            )

        Assert.Equal("The specified expression is not a case constructor for Assertions+BeOfCase+MyDu", ex.Message)


module BeSome =


    [<Fact>]
    let ``Passes for Some and allows asserting on inner value`` () =
        (Some 1).Should().BeSome().That.Should().Be(1)


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
