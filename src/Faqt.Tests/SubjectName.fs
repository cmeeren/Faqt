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
        thisIsAVariableName.Should().Fail().And.Fail()
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
        thisIsAVariableName.Should().FailDerived().And.Fail()
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


// TODO: Can we work around this now?
[<Fact>]
let ``Known limitation: And.Whose: Only first assertion with the matching name is considered`` () =
    fun () ->
        let thisIsAVariableName = ""

        thisIsAVariableName
            .Should()
            .TestDerived(true)
            .Whose.Length.Should()
            .TestDerived(false)
    // Subject name should ideally be "thisIsAVariableName...Length". Update if this is ever supported.
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``Known limitation: And.Which: Only first assertion with the matching name is considered`` () =
    fun () ->
        let thisIsAVariableName = ""

        thisIsAVariableName
            .Should()
            .TestDerived(true)
            .Which.Length.Should()
            .TestDerived(false)
    // Subject name should ideally be "thisIsAVariableName...Length". Update if this is ever supported.
    |> assertExnMsgSubjectName "thisIsAVariableName"


[<Fact>]
let ``Literal URLs are supported`` () =
    fun () -> "http://test.example.com".Should().Fail()
    |> assertExnMsgSubjectName "\"http://test.example.com\""


[<Fact>]
let ``Known limitation: Contents of strings after // are removed (except ://)`` () =
    fun () -> "this is// a test".Should().Fail()
    // Subject name should ideally be "http://test.example.com". Update if this is ever supported.
    |> assertExnMsgSubjectName "\"this is"


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly 1`` () =
    fun () ->
        "this
is a test"
            .Should()
            .Fail()
    |> assertExnMsgSubjectName "\"this"


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly`` () =
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
    |> assertExnMsgSubjectName "\"string\".Split("
