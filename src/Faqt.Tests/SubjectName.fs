module ``Subject name``

open Faqt
open Xunit


[<Fact>]
let ``Literal boolean`` () =
    fun () -> true.Should().Fail()
    |> assertExnMsg "true"


[<Fact>]
let ``Literal string`` () =
    fun () -> "asd".Should().Fail()
    |> assertExnMsg "\"asd\""


[<Fact>]
let ``Single variable name`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Fail()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``Dot chain`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Length.GetType().Should().Fail()
    |> assertExnMsg "thisIsAVariableName.Length.GetType()"


[<Fact>]
let ``Dot chain with line breaks`` () =
    fun () ->
        let thisIsAVariableNameThatIsVeryLongAndShouldForceNextMethodCallOnNextLineWithoutUsingComments_______________ =
            "1"

        thisIsAVariableNameThatIsVeryLongAndShouldForceNextMethodCallOnNextLineWithoutUsingComments_______________
            .Length
            .Should()
            .Fail()
    |> assertExnMsg
        "thisIsAVariableNameThatIsVeryLongAndShouldForceNextMethodCallOnNextLineWithoutUsingComments_______________.Length"


[<Fact>]
let ``Simple parenthesized expression`` () =
    fun () -> (1 + 2).Should().Fail()
    |> assertExnMsg "(1 + 2)"


[<Fact>]
let ``Comments`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName // Inline comment here
            // Line comment here
            .Length // Another comment here
            .Should()
            .Fail()
    |> assertExnMsg "thisIsAVariableName.Length"


[<Fact>]
let ``And-chained assertions, same name, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Fail().And.Fail()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``And-chained assertions, same name, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Test(true).And.Test(false)
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``And-chained assertions, different names, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().FailDerived().And.Fail()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``And-chained assertions, different names, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Pass().And.Fail()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``Whose, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().Whose.Length.Should().Pass()
    |> assertExnMsg "thisIsAVariableName"


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
    |> assertExnMsg "thisIsAVariableName...Length.GetType()"


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
    |> assertExnMsg "thisIsAVariableName...Length...ToString().GetType()"


[<Fact>]
let ``Which, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().Which.Length.Should().Pass()
    |> assertExnMsg "thisIsAVariableName"


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
    |> assertExnMsg "thisIsAVariableName...Length.GetType()"


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
    |> assertExnMsg "thisIsAVariableName...Length...ToString().GetType()"


[<Fact>]
let ``Whose, same child assertion, first fails`` () =
    fun () ->
        let thisIsAVariableName = ""
        thisIsAVariableName.Should().FailDerived().Which.Should().FailDerived()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``Testable.Subject`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Subject.Length.Should().Fail()
    |> assertExnMsg "thisIsAVariableName.Length"


[<Fact>]
let ``Testable.Whose`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Whose.Length.Should().Fail()
    |> assertExnMsg "thisIsAVariableName.Length"


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
    |> assertExnMsg "thisIsAVariableName...Length"


[<Fact>]
let ``And.Whose: Picks correct assertion among multiple with matching name in Satisfy`` () =
    fun () ->
        let thisIsAVariableName = ""

        thisIsAVariableName
            .Should()
            .TestSatisfy(fun x ->
                x
                    .Should()
                    .TestDerived(true)
                    .Whose.Length.Should()
                    .TestDerived(false)
                    .And.Whose.ToString()
                    .Should()
                    .TestDerived(true)
            )
    |> assertExnMsg
        """
thisIsAVariableName
x...Length
"""


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
    |> assertExnMsg "thisIsAVariableName...Length"


[<Fact>]
let ``Single-line Satisfy`` () =
    fun () -> "asd".Should().TestSatisfy(fun x -> x.Length.Should().Fail())
    |> assertExnMsg
        """
"asd"
x.Length
"""


[<Fact>]
let ``Multi-line Satisfy`` () =
    fun () ->
        "asd"
            .Should()
            .TestSatisfy(fun x ->
                // Comment to force break
                x.Length.Should().Fail()
            )
    |> assertExnMsg
        """
"asd"
x.Length
"""


[<Fact>]
let ``Multiple multi-line Satisfy, last fails`` () =
    fun () ->
        "asd"
            .Should()
            .TestSatisfy(fun x1 ->
                // Comment to force break
                x1.Length.Should().Pass()
            )
            .And.TestSatisfy(fun x2 ->
                // Comment to force break
                x2.Length.Should().Fail()
            )
    |> assertExnMsg
        """
"asd"
x2.Length
"""


[<Fact>]
let ``Single-line SatisfyAny, same assertion`` () =
    fun () ->
        "asd"
            .Should()
            .TestSatisfyAny([ (fun s1 -> s1.Should().Fail()); (fun s2 -> s2.Should().Fail()) ])
    |> assertExnMsg
        """
"asd"
s1
s2
"""


[<Fact>]
let ``Multi-line SatisfyAny, same assertion`` () =
    fun () ->
        "asd"
            .Should()
            .TestSatisfyAny(
                [
                    // Comment to force break
                    (fun s1 -> s1.Should().Fail())
                    // Comment to force break
                    (fun s2 -> s2.Should().Fail())
                ]
            )
    |> assertExnMsg
        """
"asd"
s1
s2
"""


[<Fact>]
let ``Multiple consecutive assertions on same thread`` () =
    fun () ->
        let var1 = 1
        var1.Should().Pass().And.Subject.ToString().Length.Should().Pass() |> ignore

        let var2 = 1
        var2.Should().Pass().And.Subject.ToString().Length.Should().Fail()
    |> assertExnMsg "var2.ToString().Length"


[<Fact>]
let ``Triple chains with triple Satisfy with Satisfy with triple assertions with same name, middle fails`` () =
    fun () ->
        "asd1"
            .Should()
            .TestSatisfy(fun s1 ->
                s1
                    .Should()
                    .TestSatisfy(fun ss1 ->
                        ss1
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
            .And.TestSatisfy(fun s2 ->
                s2
                    .Should()
                    .TestSatisfy(fun ss2 ->
                        ss2
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
            .And.TestSatisfy(fun s3 ->
                s3
                    .Should()
                    .TestSatisfy(fun ss3 ->
                        ss3
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
        |> ignore

        "asd2"
            .Should()
            .TestSatisfy(fun s4 ->
                s4
                    .Should()
                    .TestSatisfy(fun ss4 ->
                        ss4
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
            .And.TestSatisfy(fun s5 ->
                s5
                    .Should()
                    .TestSatisfy(fun ss5 ->
                        ss5
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(false)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
            .And.TestSatisfy(fun s6 ->
                s6
                    .Should()
                    .TestSatisfy(fun ss6 ->
                        ss6
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
        |> ignore

        "asd3"
            .Should()
            .TestSatisfy(fun s7 ->
                s7
                    .Should()
                    .TestSatisfy(fun ss7 ->
                        ss7
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
            .And.TestSatisfy(fun s8 ->
                s8
                    .Should()
                    .TestSatisfy(fun ss8 ->
                        ss8
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
            .And.TestSatisfy(fun s9 ->
                s9
                    .Should()
                    .TestSatisfy(fun ss9 ->
                        ss9
                            .Should()
                            .TestDerived(true)
                            .Which.Length.Should()
                            .TestDerived(true)
                            .Which.ToString()
                            .Should()
                            .TestDerived(true)
                    )
            )
        |> ignore
    |> assertExnMsg
        """
"asd2"
s5
ss5...Length
"""


[<Fact>]
let ``Literal URLs are supported`` () =
    fun () -> "http://test.example.com".Should().Fail()
    |> assertExnMsg "\"http://test.example.com\""


[<Fact>]
let ``Known limitation: Contents of strings after // are removed (except ://)`` () =
    fun () -> "this is// a test".Should().Fail()
    // Subject name should ideally be "this is// a test". Update if this is ever supported.
    |> assertExnMsg "subject"


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly 1`` () =
    fun () ->
        "this
is a test"
            .Should()
            .Fail()
    |> assertExnMsg "subject"


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly 2`` () =
    fun () ->
        "this
    .is a test"
            .Should()
            .Fail()
    |> assertExnMsg "\"this.is a test\""


[<Fact>]
let ``Known limitation: Literal multiline strings are not handled correctly 3`` () =
    fun () ->
        "this
    //is a test"
            .Should()
            .Fail()
    |> assertExnMsg "\"this"


[<Fact>]
let ``Known limitation: Multiline bracketed expressions are not handled correctly`` () =
    fun () ->
        "string"
            .Split(
                // Comment to force break
                'a'
            )
            .Should()
            .Fail()
    |> assertExnMsg "subject"
