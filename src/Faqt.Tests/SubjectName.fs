module ``Subject name``

open System
open System.Linq
open Faqt
open Faqt.Operators
open Xunit


[<Fact>]
let ``Literal boolean`` () =
    fun () -> true.Should().Fail()
    |> assertExnMsg
        """
Subject: 'true'
Should: Fail
"""


[<Fact>]
let ``Literal string`` () =
    fun () -> "asd".Should().Fail()
    |> assertExnMsg
        """
Subject: '"asd"'
Should: Fail
"""


[<Fact>]
let ``Single variable name`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: Fail
"""


[<Fact>]
let ``Dot chain`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Length.GetType().Should().Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName.Length.GetType()
Should: Fail
"""


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
        """
Subject: thisIsAVariableNameThatIsVeryLongAndShouldForceNextMethodCallOnNextLineWithoutUsingComments_______________.Length
Should: Fail
"""


[<Fact>]
let ``Simple parenthesized expression`` () =
    fun () -> (1 + 2).Should().Fail()
    |> assertExnMsg
        """
Subject: 1 + 2
Should: Fail
"""


[<Fact>]
let ``Assertions with explicit type parameters`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Test<int>(false)
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: Test
"""


[<Fact>]
let ``Comments`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName // Inline comment here
            // Line comment here
            .Length // Another comment here
            .Should()
            .Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName.Length
Should: Fail
"""


[<Fact>]
let ``And-chained assertions, same name, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Fail().And.Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: Fail
"""


[<Fact>]
let ``And-chained assertions, same name, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Test(true).And.Test(false)
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: Test
"""


[<Fact>]
let ``And-chained assertions, different names, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().FailDerived().And.Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: FailDerived
"""


[<Fact>]
let ``And-chained assertions, different names, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = 1
        thisIsAVariableName.Should().Pass().And.Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: Fail
"""


[<Fact>]
let ``Whose, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().Whose.Length.Should(()).Pass()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: FailDerived
"""


[<Fact>]
let ``Whose, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().PassDerived().Whose.Length.Should(()).Fail()
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
Should: Fail
"""


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
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
- ToString().GetType()
Should: Fail
"""


[<Fact>]
let ``That, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().That.Length.Should(()).Pass()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: FailDerived
"""


[<Fact>]
let ``That, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().PassDerived().That.Length.Should(()).Fail()
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
Should: Fail
"""


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
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
- ToString().GetType()
Should: Fail
"""


[<Fact>]
let ``WhoseValue, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().WhoseValue.Length.Should(()).Pass()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: FailDerived
"""


[<Fact>]
let ``WhoseValue, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().PassDerived().WhoseValue.Length.Should(()).Fail()
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
Should: Fail
"""


[<Fact>]
let ``WhoseValue.Should(), second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().PassDerived().WhoseValue.Should(()).Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: Fail
"""


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
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
- ToString().GetType()
Should: Fail
"""


[<Fact>]
let ``Derived, single line, first fails`` () =
    fun () ->
        let thisIsAVariableName = "asd"
        thisIsAVariableName.Should().FailDerived().Derived.Length.Should(()).Pass()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: FailDerived
"""


[<Fact>]
let ``Derived, single line, second fails`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .Derived.Length.GetType()
            .Should(())
            .Fail()
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length.GetType()
Should: Fail
"""


[<Fact>]
let ``Multiple Derived`` () =
    fun () ->
        let thisIsAVariableName = "1"

        thisIsAVariableName
            .Should()
            .PassDerived()
            .Derived.Length.Should(())
            .PassDerived()
            .Derived.ToString()
            .GetType()
            .Should(())
            .Fail()
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
- ToString().GetType()
Should: Fail
"""


[<Fact>]
let ``Whose, same child assertion, first fails`` () =
    fun () ->
        let thisIsAVariableName = ""
        thisIsAVariableName.Should().FailDerived().Whose.Length.Should(()).FailDerived()
    |> assertExnMsg
        """
Subject: thisIsAVariableName
Should: FailDerived
"""


[<Fact>]
let ``Whose, same child assertion, second fails`` () =
    fun () ->
        let x = ""
        x.Should().TestDerived(true).Whose.Length.Should(()).TestDerived(false)
    |> assertExnMsg
        """
Subject:
- x
- Length
Should: TestDerived
"""


[<Fact>]
let ``Testable.Subject`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Subject.Length.Should(()).Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName.Length
Should: Fail
"""


[<Fact>]
let ``Testable.Whose`` () =
    fun () ->
        let thisIsAVariableName = "1"
        thisIsAVariableName.Should().Pass().And.Whose.Length.Should(()).Fail()
    |> assertExnMsg
        """
Subject: thisIsAVariableName.Length
Should: Fail
"""


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
    |> assertExnMsg
        """
Subject:
- thisIsAVariableName
- Length
Should: TestDerived
"""


[<Fact>]
let ``And.Whose: Picks correct assertion among multiple with matching name in Satisfy`` () =
    fun () ->
        let thisIsAVariableName = ""

        thisIsAVariableName
            .Should()
            .Satisfy(fun x ->
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
Subject: thisIsAVariableName
Should: Satisfy
Failure:
  Subject: [x, Length]
  Should: TestDerived
"""


[<Fact>]
let ``Single-line Satisfy`` () =
    fun () -> "asd".Should().Satisfy(fun x -> x.Length.Should().Fail())
    |> assertExnMsg
        """
Subject: '"asd"'
Should: Satisfy
Failure:
  Subject: x.Length
  Should: Fail
"""


[<Fact>]
let ``Single-line Satisfy with shorthand lambda syntax`` () =
    fun () -> "asd".Should().Satisfy(_.Length.Should().Fail())
    |> assertExnMsg
        """
Subject: '"asd"'
Should: Satisfy
Failure:
  Subject: _.Length
  Should: Fail
"""


[<Fact>]
let ``Single-line Satisfy with shorthand lambda syntax without intermediate property`` () =
    fun () -> "asd".Should().Satisfy(_.Should().Fail())
    |> assertExnMsg
        """
Subject: '"asd"'
Should: Satisfy
Failure:
  Subject: _
  Should: Fail
"""


[<Fact>]
let ``Names containing _. outside of shorthand lambda syntax`` () =
    fun () ->
        let myVar123_ = ""
        myVar123_.Length.Should().Fail()
    |> assertExnMsg
        """
Subject: myVar123_.Length
Should: Fail
"""


[<Fact>]
let ``Names containing _ outside of shorthand lambda syntax`` () =
    fun () ->
        let myVar_123 = ""
        myVar_123.Length.Should().Fail()
    |> assertExnMsg
        """
Subject: myVar_123.Length
Should: Fail
"""


[<Fact>]
let ``Names ending with _ outside of shorthand lambda syntax`` () =
    fun () ->
        let myVar123_ = ""
        myVar123_.Should().Fail()
    |> assertExnMsg
        """
Subject: myVar123_
Should: Fail
"""


[<Fact>]
let ``Multi-line Satisfy`` () =
    fun () ->
        "asd"
            .Should()
            .Satisfy(fun x ->
                // Comment to force break
                x.Length.Should().Fail()
            )
    |> assertExnMsg
        """
Subject: '"asd"'
Should: Satisfy
Failure:
  Subject: x.Length
  Should: Fail
"""


[<Fact>]
let ``Multiple multi-line Satisfy, last fails`` () =
    fun () ->
        "asd"
            .Should()
            .Satisfy(fun x1 ->
                // Comment to force break
                x1.Length.Should().Pass()
            )
            .And.Satisfy(fun x2 ->
                // Comment to force break
                x2.Length.Should().Fail()
            )
    |> assertExnMsg
        """
Subject: '"asd"'
Should: Satisfy
Failure:
  Subject: x2.Length
  Should: Fail
"""


[<Fact>]
let ``Multi-line SatisfyAny, same assertion`` () =
    fun () ->
        "asd"
            .Should()
            .SatisfyAny(
                [
                    // Comment to force break
                    (fun s1 -> s1.Should().Fail())
                    // Comment to force break
                    (fun s2 -> s2.Should().Fail())
                ]
            )
    |> assertExnMsg
        """
Subject: '"asd"'
Should: SatisfyAny
Failures:
- Subject: s1
  Should: Fail
- Subject: s2
  Should: Fail
"""


[<Fact>]
let ``AllSatisfy, multiple failures, single-line`` () =
    fun () -> [ 1; 2; 3 ].Should().AllSatisfy(fun s -> s.Should().Fail())
    |> assertExnMsg
        """
Subject: '[ 1; 2; 3 ]'
Should: AllSatisfy
Failures:
- Index: 0
  Failure:
    Subject: s
    Should: Fail
- Index: 1
  Failure:
    Subject: s
    Should: Fail
- Index: 2
  Failure:
    Subject: s
    Should: Fail
Subject value: [1, 2, 3]
"""


[<Fact>]
let ``AllSatisfy, multiple failures, multi-line`` () =
    fun () ->
        [ 1; 2 ]
            .Should()
            // Comment to force break
            .AllSatisfy(fun s -> s.Should().Fail())
    |> assertExnMsg
        """
Subject: '[ 1; 2 ]'
Should: AllSatisfy
Failures:
- Index: 0
  Failure:
    Subject: s
    Should: Fail
- Index: 1
  Failure:
    Subject: s
    Should: Fail
Subject value: [1, 2]
"""


[<Fact>]
let ``AllSatisfy single and then chain with same assertion`` () =
    fun () ->
        [ 1 ]
            .Should()
            .AllSatisfy(fun s -> s.ToString().Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
Subject: '[ 1 ].Length'
Should: Test
"""


[<Fact>]
let ``AllSatisfy multiple and then chain with same assertion`` () =
    fun () ->
        [ 1; 2; 3 ]
            .Should()
            .AllSatisfy(fun s -> s.ToString().Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
Subject: '[ 1; 2; 3 ].Length'
Should: Test
"""


[<Fact>]
let ``Assertions chained after higher-order assertions using the same assertion`` () =
    fun () ->
        "asd"
            .Should()
            .Satisfy(fun s1 -> s1.Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
Subject: '"asd".Length'
Should: Test
"""


[<Fact>]
let ``Multiple consecutive assertions on same thread, different callsites`` () =
    fun () ->
        let var1 = 1
        var1.Should().Pass().And.Subject.ToString().Length.Should(()).Pass() |> ignore

        let var2 = 1
        var2.Should().Pass().And.Subject.ToString().Length.Should(()).Fail()
    |> assertExnMsg
        """
Subject: var2.ToString().Length
Should: Fail
"""


[<Fact>]
let ``Multiple consecutive assertions on same thread, same callsite`` () =
    fun () ->
        let x = 1

        for i in [ 1..99 ] do
            x.Should().Pass().And.Subject.ToString().Length.Should(()).Test(i < 99)
            |> ignore
    |> assertExnMsg
        """
Subject: x.ToString().Length
Should: Test
"""


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
    |> assertExnMsg
        """
Subject: "\"string\".Split('a')"
Should: Fail
"""


[<Fact>]
let ``Literal URLs are supported`` () =
    fun () -> "https://test.example.com".Should().Fail()
    |> assertExnMsg
        """
Subject: '"https://test.example.com"'
Should: Fail
"""


[<Fact>]
let ``Single-line strings with // are untouched`` () =
    fun () -> "this is// a test".Should().Fail()
    |> assertExnMsg
        """
Subject: '"this is// a test"'
Should: Fail
"""


[<Fact>]
let ``Single-line triple-quoted strings with // are untouched`` () =
    fun () -> """this is// a test""".Should().Fail()
    |> assertExnMsg
        @"
Subject: '""""""this is// a test""""""'
Should: Fail
"


[<Fact>]
let ``Quoted identifiers with // are untouched`` () =
    fun () ->
        let ``// some identifier`` = 1
        ``// some identifier``.Should().Fail()
    |> assertExnMsg
        """
Subject: '``// some identifier``'
Should: Fail
"""


[<Fact>]
let ``Ignore operator is stripped`` () =
    fun () -> %(1).Should().Fail()
    |> assertExnMsg
        """
Subject: '1'
Should: Fail
"""


[<Fact>]
let ``Balanced parentheses are stripped`` () =
    fun () ->
        (("1" + "2") |> Some)
            .Should()
            .BeSome()
            .Whose.Replace("a", ("b" + "c"))
            .Should()
            .Fail()
    |> assertExnMsg
        """
Subject:
- ("1" + "2") |> Some
- Replace("a", ("b" + "c"))
Should: Fail
"""


[<Fact>]
let ``Ignores leading let`` () =
    fun () ->
        let x = "a".Should().Fail().Subject
        ignore<string> x
    |> assertExnMsg
        """
Subject: '"a"'
Should: Fail
"""


[<Fact>]
let ``Ignores leading use`` () =
    fun () ->
        let s =
            { new IDisposable with
                member _.Dispose() = ()
            }

        use x = s.Should().Fail().Subject
        ignore<IDisposable> x
    |> assertExnMsg
        """
Subject: s
Should: Fail
"""


[<Fact>]
let ``Ignores leading do`` () =
    fun () ->
        // Comment to force break
        do %"a".Should().Fail()
    |> assertExnMsg
        """
Subject: '"a"'
Should: Fail
"""


[<Fact>]
let ``Known limitation: Lines of multi-line strings that start with // are removed`` () =
    fun () ->
        "this
    //is a test"
            .Should()
            .Fail()
    |> assertExnMsg
        """
Subject: '"this'
Should: Fail
"""


[<Fact>]
let ``Known limitation: Literal multiline strings are concatenated to a single line 1`` () =
    fun () ->
        "this
is a test"
            .Should()
            .Fail()
    |> assertExnMsg
        """
Subject: '"thisis a test"'
Should: Fail
"""


[<Fact>]
let ``Known limitation: Literal multiline strings are concatenated to a single line 2`` () =
    fun () ->
        "this
    .is a test"
            .Should()
            .Fail()
    |> assertExnMsg
        """
Subject: '"this.is a test"'
Should: Fail
"""


[<Fact>]
let ``Known limitation: Nested multi-line Satisfy does not work correctly`` () =
    fun () ->
        "asd"
            .Should()
            .Satisfy(fun s1 ->
                // Comment to force break
                s1.Should().Satisfy(fun ss1 -> ss1.Should().Pass())
            )
            .And.Satisfy(fun s2 ->
                // Comment to force break
                s2.Should().Satisfy(fun ss2 -> ss2.Should().Fail())
            )
    |> assertExnMsg
        """
Subject: '"asd".Satisfy(fun s2 ->s2'
Should: Satisfy
Failure:
  Subject: s2
  Should: Satisfy
  Failure:
    Subject: ss2
    Should: Fail
"""


[<Fact>]
let ``Known limitation: AllSatisfy with empty sequence does not work correctly`` () =
    fun () ->
        List<int>.Empty
            .Should()
            .AllSatisfy(fun s -> s.ToString().Should().Test(true))
            .And.Subject.Length.Should(())
            .Test(false)
    |> assertExnMsg
        """
Subject: s.ToString()
Should: Test
"""


[<Fact>]
let ``Known limitation: Assertion chains must start on a new line or after lambda`` () =
    fun () -> ignore ("asd".Should().Fail())
    |> assertExnMsg
        """
Subject: ignore ("asd"
Should: Fail
"""


[<Fact>]
let ``Known limitation: Single-line SatisfyAny, same assertion does not work correctly`` () =
    fun () ->
        "asd"
            .Should()
            .SatisfyAny([ (fun s1 -> s1.Should().Fail()); (fun s2 -> s2.Should().Fail()) ])
    |> assertExnMsg
        """
Subject: '"asd"'
Should: SatisfyAny
Failures:
- Subject: s1
  Should: Fail
- Subject: s1
  Should: Fail
"""


type private String with

    member this.Fail() = this


[<Fact>]
let ``Known limitation: Non-assertion method with same name as an assertion gives incorrect results`` () =
    fun () -> "asd".Fail().Length.Should().Fail()
    |> assertExnMsg
        """
Subject: '"asd"'
Should: Fail
"""


[<Fact>]
let ``Known limitation: Nested AllSatisfy does not work correctly`` () =
    fun () ->
        let x = [ [ 11; 12; 13 ]; [ 21; 22; 23 ]; [ 31; 32; 33 ] ]

        x
            .Should()
            .AllSatisfy(fun x1 -> x1.Should().AllSatisfy(fun x2 -> x2.Should().Fail()))
    |> assertExnMsg
        """
Subject: subject
Should: AllSatisfy
Failures:
- Index: 0
  Failure:
    Subject: ''
    Should: AllSatisfy
    Failures:
    - Index: 0
      Failure:
        Subject: x2
        Should: Fail
    - Index: 1
      Failure:
        Subject: x2
        Should: Fail
    - Index: 2
      Failure:
        Subject: x2
        Should: Fail
    Subject value: [11, 12, 13]
- Index: 1
  Failure:
    Subject: ''
    Should: AllSatisfy
    Failures:
    - Index: 0
      Failure:
        Subject: x2
        Should: Fail
    - Index: 1
      Failure:
        Subject: x2
        Should: Fail
    - Index: 2
      Failure:
        Subject: x2
        Should: Fail
    Subject value: [21, 22, 23]
- Index: 2
  Failure:
    Subject: ''
    Should: AllSatisfy
    Failures:
    - Index: 0
      Failure:
        Subject: x2
        Should: Fail
    - Index: 1
      Failure:
        Subject: x2
        Should: Fail
    - Index: 2
      Failure:
        Subject: x2
        Should: Fail
    Subject value: [31, 32, 33]
Subject value:
- [11, 12, 13]
- [21, 22, 23]
- [31, 32, 33]
"""


[<Fact>]
let ``Known limitation: Lambdas in subject does not work`` () =
    fun () ->
        // Comment to force break
        [ 1; 2; 3 ].Select(fun x -> x.ToString()).Should().Fail()
    |> assertExnMsg
        """
Subject: x.ToString())
Should: Fail
"""


[<Fact>]
let ``Known limitation: Shorthand lambdas in subject does not work`` () =
    fun () ->
        // Comment to force break
        [ 1; 2; 3 ].Select(_.ToString()).Should().Fail()
    |> assertExnMsg
        """
Subject: _.ToString())
Should: Fail
"""
