module ``Subject name``

open Faqt
open Xunit


[<AutoOpen>]
module Helpers =


    let assertExnMsgSubjectName expected (f: unit -> 'a) =
        let ex = Assert.Throws<AssertionFailedException>(f >> ignore)
        let actual = ex.Message.Trim()
        Assert.Equal(expected, actual)


[<Fact>]
let ``Literal boolean`` () =
    fun () -> true.Should().Fail()
    |> assertExnMsgSubjectName "true"


[<Fact>]
let ``Literal string`` () =
    fun () -> "asd".Should().Fail()
    |> assertExnMsgSubjectName "\"asd\""


[<Fact>]
let ``Single variable name`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``Dot chain`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Length.GetType().Should().Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName.Length.GetType()"


[<Fact>]
let ``Dot chain with line breaks`` () =
    fun () ->
        let thisIsAVariableNameThatIsVeryLongAndShouldForceNextMethodCallOnNextLineWithoutUsingComments_______________ =
            "1"

        thisIsAVariableNameThatIsVeryLongAndShouldForceNextMethodCallOnNextLineWithoutUsingComments_______________
            .Length
            .Should()
            .Fail()
    |> assertExnMsgSubjectName
        "thisIsAVariableNameThatIsVeryLongAndShouldForceNextMethodCallOnNextLineWithoutUsingComments_______________.Length"


[<Fact>]
let ``Simple parenthesized expression`` () =
    fun () -> (1 + 2).Should().Fail()
    |> assertExnMsgSubjectName "(1 + 2)"


[<Fact>]
let ``Comments`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName // Inline comment here
            // Line comment here
            .Length // Another comment here
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName.Length"


[<Fact>]
let ``And-chained assertions, same name, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Fail().And.Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``And-chained assertions, same name, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Test(true).And.Test(false)
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``And-chained assertions, different names, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().FailDerived().And.Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``And-chained assertions, different names, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Pass().And.Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``Whose, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().Whose.Length.Should().Pass()
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``Whose, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .Whose.Length.GetType()
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName...Length.GetType()"


[<Fact>]
let ``Multiple Whose`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .Whose.Length.Should()
            .PassDerived()
            .Whose.ToString()
            .GetType()
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName...Length...ToString().GetType()"


[<Fact>]
let ``Which, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().Which.Length.Should().Pass()
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``Which, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .Which.Length.GetType()
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName...Length.GetType()"


[<Fact>]
let ``Multiple Which`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .Which.Length.Should()
            .PassDerived()
            .Which.ToString()
            .GetType()
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName...Length...ToString().GetType()"


[<Fact>]
let ``Whose, same child assertion, first fails`` () =
    fun () ->
        let thisIsAVariableName = ""
        thisIsAVariableName.Should().FailDerived().Which.Should().FailDerived()
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``Testable.Subject`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Subject.Length.Should().Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName.Length"


[<Fact>]
let ``Testable.Whose`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Whose.Length.Should().Fail()
    |> assertExnMsgSubjectName "thisIsAVariableName.Length"


[<Fact>]
let ``And.Whose: Picks correct assertion among multiple with matching name`` () =
    fun () ->
        let thisIsAVariableName = ""

        thisIsAVariableName
            .Should()
            .TestDerived(true)
            .Whose.Length.Should()
            .TestDerived(false)
            .And.Whose.ToString()
            .Should()
            .TestDerived(true)
    |> assertExnMsgSubjectName "thisIsAVariableName...Length"


[<Fact>]
let ``And.Which: Picks correct assertion among multiple with matching name`` () =
    fun () ->
        let thisIsAVariableName = ""

        thisIsAVariableName
            .Should()
            .TestDerived(true)
            .Which.Length.Should()
            .TestDerived(false)
            .And.Whose.ToString()
            .Should()
            .TestDerived(true)
    |> assertExnMsgSubjectName "thisIsAVariableName...Length"


[<Fact>]
let ``Single-line Satisfy`` () =
    fun () -> "asd".Should().Satisfy(fun x -> x.Length.Should().Be(2))
    // Subject name in inner failure should ideally be "x.Length". Update if this is ever supported.
    |> assertExnMsg
        """
"asd"
    should satisfy the supplied assertion, but the assertion failed with the following message:

x.Length
    should be
2
    but was
3
"""


[<Fact>]
let ``Single-line SatisfyAny, same assertion`` () =
    fun () ->
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Should().Be("a")); (fun s2 -> s2.Should().Be("b")) ])
    |> assertExnMsg
        """
"asd"
    should satisfy at least one of the 2 supplied assertions, but none were satisfied.

[Assertion 1/2]

s1
    should be
"a"
    but was
"asd"

[Assertion 2/2]

s2
    should be
"b"
    but was
"asd"
"""


[<Fact>]
let ``Multiple consecutive assertions on same thread`` () =
    fun () ->
        let var1 = 1
        var1.Should().Pass().And.Subject.ToString().Length.Should().Pass() |> ignore

        let var2 = 1
        var2.Should().Pass().And.Subject.ToString().Length.Should().Fail()
    |> assertExnMsgSubjectName "var2.ToString().Length"


[<Fact>]
let ``Literal URLs are supported`` () =
    fun () -> "http://test.example.com".Should().Fail()
    |> assertExnMsgSubjectName "\"http://test.example.com\""


[<Fact>]
let ``Known limitation: Contents of strings after // are removed (except ://)`` () =
    fun () -> "this is// a test".Should().Fail()
    // Subject name should ideally be "this is// a test". Update if this is ever supported.
    |> assertExnMsgSubjectName "subject"


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly 1`` () =
    fun () ->
        "this
is a test"
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "subject"


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly 2`` () =
    fun () ->
        "this
    .is a test"
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "\"this.is a test\""


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly 3`` () =
    fun () ->
        "this
    //is a test"
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "\"this"


[<Fact>]
let ``Known limitation: Multiline bracketed expressions are not handled correctly`` () =
    fun () ->
        "string"
            .Split(
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a',
                'a'
            )
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "subject"
