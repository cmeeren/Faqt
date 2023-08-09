module ``Subject name``

open Faqt
open Faqt.Operators
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
let ``Assertions with explicit type parameters`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Test<int>(false)
    |> assertExnMsg "thisIsAVariableName"


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
        thisIsAVariableName.Should().FailDerived().Whose.Length.Should(()).Pass()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``Whose, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().PassDerived().Whose.Length.Should(()).Fail()
    |> assertExnMsg "thisIsAVariableName...Length"


[<Fact>]
let ``Multiple Whose`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .Whose.Length.Should(())
            .PassDerived()
            .Whose.ToString()
            .GetType()
            .Should(())
            .Fail()
    |> assertExnMsg "thisIsAVariableName...Length...ToString().GetType()"


[<Fact>]
let ``That, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().That.Length.Should(()).Pass()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``That, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().PassDerived().That.Length.Should(()).Fail()
    |> assertExnMsg "thisIsAVariableName...Length"


[<Fact>]
let ``Multiple That`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .That.Length.Should(())
            .PassDerived()
            .That.ToString()
            .GetType()
            .Should(())
            .Fail()
    |> assertExnMsg "thisIsAVariableName...Length...ToString().GetType()"


[<Fact>]
let ``WhoseValue, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().WhoseValue.Length.Should(()).Pass()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``WhoseValue, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .WhoseValue.Length.GetType()
            .Should(())
            .Fail()
    |> assertExnMsg "thisIsAVariableName...Length.GetType()"


[<Fact>]
let ``Multiple WhoseValue`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .WhoseValue.Length.Should(())
            .PassDerived()
            .WhoseValue.ToString()
            .GetType()
            .Should(())
            .Fail()
    |> assertExnMsg "thisIsAVariableName...Length...ToString().GetType()"


[<Fact>]
let ``Whose, same child assertion, first fails`` () =
    fun () ->
        let thisIsAVariableName = ""
        thisIsAVariableName.Should().FailDerived().Whose.Length.Should(()).FailDerived()
    |> assertExnMsg "thisIsAVariableName"


[<Fact>]
let ``Whose, same child assertion, second fails`` () =
    fun () ->
        let x = ""
        x.Should().TestDerived(true).Whose.Length.Should(()).TestDerived(false)
    |> assertExnMsg "x...Length"


[<Fact>]
let ``Testable.Subject`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Subject.Length.Should(()).Fail()
    |> assertExnMsg "thisIsAVariableName.Length"


[<Fact>]
let ``Testable.Whose`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Whose.Length.Should(()).Fail()
    |> assertExnMsg "thisIsAVariableName.Length"


[<Fact>]
let ``And.Whose: Picks correct assertion among multiple with matching name`` () =
    fun () ->
        let thisIsAVariableName = ""

        thisIsAVariableName
            .Should()
            .TestDerived(true)
            .Whose.Length.Should(())
            .TestDerived(false)
            .And.Whose.ToString()
            .Should(())
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
                    .Should(())
                    .TestDerived(true)
                    .Whose.Length.Should(())
                    .TestDerived(false)
                    .And.Whose.ToString()
                    .Should(())
                    .TestDerived(true)
            )
    |> assertExnMsg
        """
thisIsAVariableName
x...Length
"""


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
let ``AllSatisfy, multiple failures, single-line`` () =
    fun () -> [ 1; 2; 3 ].Should().TestAllSatisfy(fun s -> s.Should().Fail())
    |> assertExnMsg
        """
[ 1; 2; 3 ]
s
s
s
"""


[<Fact>]
let ``AllSatisfy, multiple failures, multi-line`` () =
    fun () ->
        [ 1; 2 ]
            .Should()
            // Comment to force break
            .TestAllSatisfy(fun s -> s.Should().Fail())
    |> assertExnMsg
        """
[ 1; 2 ]
s
s
"""


[<Fact>]
let ``AllSatisfy single and then chain with same assertion`` () =
    fun () ->
        [ 1 ]
            .Should()
            .TestAllSatisfy(fun s -> s.ToString().Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
[ 1 ].Length
"""


[<Fact>]
let ``AllSatisfy multiple and then chain with same assertion`` () =
    fun () ->
        [ 1; 2; 3 ]
            .Should()
            .TestAllSatisfy(fun s -> s.ToString().Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
[ 1; 2; 3 ].Length
"""


[<Fact>]
let ``Assertions chained after higher-order assertions using the same assertion`` () =
    fun () ->
        "asd"
            .Should()
            .TestSatisfy(fun s1 -> s1.Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
"asd".Length
"""


[<Fact>]
let ``Multiple consecutive assertions on same thread, different callsites`` () =
    fun () ->
        let var1 = 1
        var1.Should().Pass().And.Subject.ToString().Length.Should(()).Pass() |> ignore

        let var2 = 1
        var2.Should().Pass().And.Subject.ToString().Length.Should(()).Fail()
    |> assertExnMsg "var2.ToString().Length"


[<Fact>]
let ``Multiple consecutive assertions on same thread, same callsite`` () =
    fun () ->
        let x = 1

        for i in [ 1..99 ] do
            x.Should().Pass().And.Subject.ToString().Length.Should(()).Test(i < 99)
            |> ignore
    |> assertExnMsg "x.ToString().Length"


[<Fact>]
let ``Multiline bracketed expressions`` () =
    fun () ->
        "string"
            .Split(
                // Comment to force break
                'a'
            )
            .Should()
            .Fail()
    |> assertExnMsg "\"string\".Split('a')"


[<Fact>]
let ``Literal URLs are supported`` () =
    fun () -> "http://test.example.com".Should().Fail()
    |> assertExnMsg "\"http://test.example.com\""


[<Fact>]
let ``Single-line strings with // are untouched`` () =
    fun () -> "this is// a test".Should().Fail()
    |> assertExnMsg "\"this is// a test\""


[<Fact>]
let ``Single-line triple-quoted strings with // are untouched`` () =
    fun () -> """this is// a test""".Should().Fail()
    |> assertExnMsg "\"\"\"this is// a test\"\"\""


[<Fact>]
let ``Quoted identifiers with // are untouched`` () =
    fun () ->
        let ``// some identifier`` = 1
        ``// some identifier``.Should().Fail()
    |> assertExnMsg "``// some identifier``"


[<Fact>]
let ``Ignore operator is stripped`` () =
    fun () -> %(1).Should().Fail()
    |> assertExnMsg "(1)"


[<Fact>]
let ``Known limitation: Lines of multi-line strings that start with // are removed`` () =
    fun () ->
        "this
    //is a test"
            .Should()
            .Fail()
    |> assertExnMsg "\"this"


[<Fact>]
let ``Known limitation: Literal multiline strings are concatenated to a single line 1`` () =
    fun () ->
        "this
is a test"
            .Should()
            .Fail()
    |> assertExnMsg "\"thisis a test\""


[<Fact>]
let ``Known limitation: Literal multiline strings are concatenated to a single line 2`` () =
    fun () ->
        "this
    .is a test"
            .Should()
            .Fail()
    |> assertExnMsg "\"this.is a test\""


[<Fact>]
let ``Known limitation: Nested multi-line Satisfy does not work correctly`` () =
    fun () ->
        "asd"
            .Should()
            .TestSatisfy(fun s1 ->
                // Comment to force break
                s1.Should().TestSatisfy(fun ss1 -> ss1.Should().Pass())
            )
            .And.TestSatisfy(fun s2 ->
                // Comment to force break
                s2.Should().TestSatisfy(fun ss2 -> ss2.Should().Fail())
            )
    |> assertExnMsg
        """
"asd".TestSatisfy(fun s2 ->s2
s2
ss2
"""


[<Fact>]
let ``Known limitation: AllSatisfy with empty sequence does not work correctly`` () =
    fun () ->
        List<int>.Empty
            .Should()
            .TestAllSatisfy(fun s -> s.ToString().Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
s.ToString()
"""


[<Fact>]
let ``Known limitation: Assertion chains must start on a new line or after lambda`` () =
    fun () -> ignore ("asd".Should().Fail())
    |> assertExnMsg "ignore (\"asd\""


[<Fact>]
let ``Known limitation: Single-line SatisfyAny, same assertion does not work correctly`` () =
    fun () ->
        "asd"
            .Should()
            .TestSatisfyAny([ (fun s1 -> s1.Should().Fail()); (fun s2 -> s2.Should().Fail()) ])
    |> assertExnMsg
        """
"asd"
s1
s1
"""
