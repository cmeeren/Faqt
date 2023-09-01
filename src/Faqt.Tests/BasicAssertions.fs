module BasicAssertions

open System
open Faqt
open Xunit


module Be =


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().Be(1).Id<And<int>>().And.Be(1)


    [<Theory>]
    [<InlineData("a", "a")>]
    [<InlineData(null, null)>]
    let ``Passes if equal`` (a: string, b: string) = a.Should().Be(b)


    [<Theory>]
    [<InlineData("a", "b")>]
    [<InlineData("a", null)>]
    [<InlineData(null, "a")>]
    let ``Fails if not equal`` (a: string, b: string) =
        Assert.Throws<AssertionFailedException>(fun () -> a.Should().Be(b) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().Be(2)
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: 2
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().Be(2, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be
Expected: 2
But was: 1
"""


module ``Be with custom comparer`` =


    [<Fact>]
    let ``Can be called with different types and chained with AndDerived with expected value`` () =
        (1)
            .Should()
            .Be("asd", (fun (_: int) (_: string) -> true))
            .Id<AndDerived<int, string>>()
            .WhoseValue.Should(())
            .Be("asd")


    [<Theory>]
    [<InlineData("a", "a")>]
    [<InlineData(null, null)>]
    [<InlineData("a", "b")>]
    [<InlineData("a", null)>]
    [<InlineData(null, "a")>]
    let ``Passes if and only if comparer returns true`` (a: string, b: string) =
        // Pass
        a.Should().Be(b, (fun _ _ -> true)) |> ignore

        // Fail
        Assert.Throws<AssertionFailedException>(fun () -> a.Should().Be(b, (fun _ _ -> false)) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().Be(1, (fun _ _ -> false))
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: 1
But was: 1
WithCustomEquality: true
"""

    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().Be(1, (fun _ _ -> false), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be
Expected: 1
But was: 1
WithCustomEquality: true
"""


module NotBe =


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().NotBe(2).Id<And<int>>().And.Be(1)


    [<Theory>]
    [<InlineData("a", "b")>]
    [<InlineData("a", null)>]
    [<InlineData(null, "a")>]
    let ``Passes if not equal`` (a: string, b: string) = a.Should().NotBe(b)


    [<Theory>]
    [<InlineData("a", "a")>]
    [<InlineData(null, null)>]
    let ``Fails if equal`` (a: string, b: string) =
        Assert.Throws<AssertionFailedException>(fun () -> a.Should().NotBe(b) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(1)
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other: 1
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBe
Other: 1
But was: 1
"""


module ``NotBe with custom comparer`` =


    [<Fact>]
    let ``Can be called with different types and chained with And`` () =
        (1)
            .Should()
            .NotBe("asd", (fun (_: int) (_: string) -> false))
            .Id<And<int>>()
            .And.Be(1)


    [<Theory>]
    [<InlineData("a", "a")>]
    [<InlineData(null, null)>]
    [<InlineData("a", "b")>]
    [<InlineData("a", null)>]
    [<InlineData(null, "a")>]
    let ``Passes if and only if comparer returns false`` (a: string, b: string) =
        // Pass
        a.Should().NotBe(b, (fun _ _ -> false)) |> ignore

        // Fail
        Assert.Throws<AssertionFailedException>(fun () -> a.Should().NotBe(b, (fun _ _ -> true)) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        let isEqual _ _ = true

        fun () ->
            let x = 1
            x.Should().NotBe(2, isEqual)
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other: 2
But was: 1
WithCustomEquality: true
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let isEqual _ _ = true

        fun () ->
            let x = 1
            x.Should().NotBe(1, isEqual, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBe
Other: 1
But was: 1
WithCustomEquality: true
"""


module BeSameAs =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().BeSameAs("a").Id<And<string>>().And.Be("a")


    let passData = [
        [| box TestRefEqualityType.Instance; TestRefEqualityType.Instance |]
        [| null; null |]
        // Interned strings (.NET implementation detail, not a requirement, just documenting behavior; remove test case
        // if it causes trouble)
        [| "a"; "a" |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if reference equal`` (subject: obj) (expected: obj) = subject.Should().BeSameAs(expected)


    let failData = [
        [| box (TestRefEqualityType()); TestRefEqualityType() |]
        [| TestRefEqualityType(); null |]
        [| null; TestRefEqualityType() |]
        [| "a" + "b"; "ab" |] // Non-interned strings
        [| {| A = 1 |}; {| A = 1 |} |] // Structurally equal values
        [| box 1; box 1 |] // Separately boxed (but otherwise equal) value types
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not reference equal`` (subject: obj) (expected: obj) =
        Assert.Throws<AssertionFailedException>(fun () -> subject.Should().BeSameAs(expected) |> ignore)


    [<Fact>]
    let ``Fails with expected message if only subject is null`` () =
        let x: string = null
        let y = "asd"

        fun () -> x.Should().BeSameAs(y)
        |> assertExnMsg
            $"""
Subject: x
Should: BeSameAs
Expected:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash y}
  Type: System.String
  Value: asd
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if only expected is null`` () =
        let x = "a"
        let y: string = null

        fun () -> x.Should().BeSameAs(y)
        |> assertExnMsg
            $"""
Subject: x
Should: BeSameAs
Expected: null
But was:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash x}
  Type: System.String
  Value: a
"""


    [<Fact>]
    let ``Fails with expected message if not reference equal`` () =
        let x = box "a"
        let y = box 1

        fun () -> x.Should().BeSameAs(y)
        |> assertExnMsg
            $"""
Subject: x
Should: BeSameAs
Expected:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash y}
  Type: System.Int32
  Value: 1
But was:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash x}
  Type: System.String
  Value: a
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let x = box "a"
        let y = box 1

        fun () -> x.Should().BeSameAs(y, "Some reason")
        |> assertExnMsg
            $"""
Subject: x
Because: Some reason
Should: BeSameAs
Expected:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash y}
  Type: System.Int32
  Value: 1
But was:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash x}
  Type: System.String
  Value: a
"""


module NotBeSameAs =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().NotBeSameAs("b").Id<And<string>>().And.Be("a")


    let passData = [
        [| box (TestRefEqualityType()); TestRefEqualityType() |]
        [| TestRefEqualityType(); null |]
        [| null; TestRefEqualityType() |]
        [| "a" + "b"; "ab" |] // Non-interned strings
        [| {| A = 1 |}; {| A = 1 |} |] // Structurally equal values
        [| box 1; box 1 |] // Separately boxed (but otherwise equal) value types
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if not reference equal`` (subject: obj) (expected: obj) = subject.Should().NotBeSameAs(expected)


    let failData = [
        [| box TestRefEqualityType.Instance; TestRefEqualityType.Instance |]
        [| null; null |]
        // Interned strings (.NET implementation detail, not a requirement, just documenting behavior; remove test case
        // if it causes trouble)
        [| "a"; "a" |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if reference equal`` (subject: obj) (expected: obj) =
        Assert.Throws<AssertionFailedException>(fun () -> subject.Should().NotBeSameAs(expected) |> ignore)


    [<Fact>]
    let ``Fails with expected message if both are null`` () =
        let x: obj = null
        let y: obj = null

        fun () -> x.Should().NotBeSameAs(y)
        |> assertExnMsg
            """
Subject: x
Should: NotBeSameAs
Other: null
"""


    [<Fact>]
    let ``Fails with expected message if not reference equal`` () =
        let x = "asd"
        let y = x

        fun () -> x.Should().NotBeSameAs(y)
        |> assertExnMsg
            """
Subject: x
Should: NotBeSameAs
Other: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let x = "asd"
        let y = x

        fun () -> x.Should().NotBeSameAs(y, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBeSameAs
Other: asd
"""


module BeNull =


    [<Fact>]
    let ``Can be chained with And`` () =
        (null: string).Should().BeNull().Id<And<string>>().And.BeNull()


    [<Fact>]
    let ``Passes if null`` () = (null: obj).Should().BeNull()


    [<Fact>]
    let ``Fails if not null`` () =
        Assert.Throws<AssertionFailedException>(fun () -> obj().Should().BeNull() |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull()
        |> assertExnMsg
            """
Subject: x
Should: BeNull
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNull
But was: asd
"""


module NotBeNull =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().NotBeNull().Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Passes if not null`` () = obj().Should().NotBeNull()


    [<Fact>]
    let ``Fails if null`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (null: obj).Should().NotBeNull() |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x: obj = null
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
            let x: obj = null
            x.Should().NotBeNull("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBeNull
But was: null
"""


module ``BeOfType non-generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with And`` () =
        "asd".Should().BeOfType(typeof<string>).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with And`` () =
        (box "asd").Should().BeOfType(typeof<string>).Id<And<obj>>().And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeOfType(typeof<string>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for different types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType(typeof<int>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.Int32
But was: System.String
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message if non-generic interface, even if type implements it`` () =
        fun () ->
            let x = TestSubType()
            x :> TestInterface |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<TestInterface>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: TestUtils+TestInterface
But was: TestUtils+TestSubType
Subject value: {}
"""


    [<Fact>]
    let ``Fails with expected message if generic interface, even if type implements it`` () =
        fun () ->
            let x = TestSubType<string, int>()
            x :> TestInterface<string, int> |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<TestInterface<string, int>>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: TestUtils+TestInterface<System.String, System.Int32>
But was: TestUtils+TestSubType<System.String, System.Int32>
Subject value: {}
"""


    [<Fact>]
    let ``Fails with expected message if sub-type`` () =
        fun () ->
            let x = TestSubType()
            x :> TestBaseType |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<TestBaseType>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: TestUtils+TestBaseType
But was: TestUtils+TestSubType
Subject value: {}
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType(typeof<int>, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeOfType
Expected: System.Int32
But was: System.String
Subject value: asd
"""


module ``BeOfType generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with AndDerived with cast value`` () =
        "asd"
            .Should()
            .BeOfType<string>()
            .Id<AndDerived<string, string>>()
            .WhoseValue.Should(())
            .Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with AndDerived with cast value`` () =
        (box "asd")
            .Should()
            .BeOfType<string>()
            .Id<AndDerived<obj, string>>()
            .WhoseValue.Should(())
            .Be("asd")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeOfType<string>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for different types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType<int>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.Int32
But was: System.String
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message if non-generic interface, even if type implements it`` () =
        fun () ->
            let x = TestSubType()
            x :> TestInterface |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<TestInterface>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: TestUtils+TestInterface
But was: TestUtils+TestSubType
Subject value: {}
"""


    [<Fact>]
    let ``Fails with expected message if generic interface, even if type implements it`` () =
        fun () ->
            let x = TestSubType<string, int>()
            x :> TestInterface<string, int> |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<TestInterface<string, int>>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: TestUtils+TestInterface<System.String, System.Int32>
But was: TestUtils+TestSubType<System.String, System.Int32>
Subject value: {}
"""


    [<Fact>]
    let ``Fails with expected message if sub-type`` () =
        fun () ->
            let x = TestSubType()
            x :> TestBaseType |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<TestBaseType>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: TestUtils+TestBaseType
But was: TestUtils+TestSubType
Subject value: {}
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType<int>("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeOfType
Expected: System.Int32
But was: System.String
Subject value: asd
"""


module ``BeAssignableTo non-generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with And`` () =
        "asd".Should().BeAssignableTo(typeof<string>).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with And`` () =
        (box "asd").Should().BeAssignableTo(typeof<string>).Id<And<obj>>().And.Be("asd")


    [<Fact>]
    let ``Passes for instance of type that implements specified interface`` () =
        let x = TestSubType()
        x.Should().BeAssignableTo(typeof<TestInterface>)


    [<Fact>]
    let ``Passes for boxed instance of type that implements specified interface`` () =
        let x = TestSubType()
        (box x).Should().BeAssignableTo(typeof<TestInterface>)


    [<Fact>]
    let ``Passes for instance of subtype of specified type`` () =
        let x = TestSubType()
        x.Should().BeAssignableTo(typeof<TestBaseType>)


    [<Fact>]
    let ``Passes for boxed instance of subtype of specified type`` () =
        let x = TestSubType()
        (box x).Should().BeAssignableTo(typeof<TestBaseType>)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeAssignableTo(typeof<string>)
        |> assertExnMsg
            """
Subject: x
Should: BeAssignableTo
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for incompatible types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo(typeof<int>)
        |> assertExnMsg
            """
Subject: x
Should: BeAssignableTo
Expected: System.Int32
But was: System.String
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo(typeof<int>, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAssignableTo
Expected: System.Int32
But was: System.String
Subject value: asd
"""


module ``BeAssignableTo generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with AndDerived with cast value`` () =
        "asd"
            .Should()
            .BeAssignableTo<string>()
            .Id<AndDerived<string, string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with AndDerived with cast value`` () =
        (box "asd")
            .Should()
            .BeAssignableTo<string>()
            .Id<AndDerived<obj, string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes for instance of type that implements specified interface`` () =
        let x = TestSubType()
        x.Should().BeAssignableTo<TestInterface>()


    [<Fact>]
    let ``Passes for boxed instance of type that implements specified interface`` () =
        let x = TestSubType()
        (box x).Should().BeAssignableTo<TestInterface>()


    [<Fact>]
    let ``Passes for instance of subtype of specified type`` () =
        let x = TestSubType()
        x.Should().BeAssignableTo<TestBaseType>()


    [<Fact>]
    let ``Passes for boxed instance of subtype of specified type`` () =
        let x = TestSubType()
        (box x).Should().BeAssignableTo<TestBaseType>()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeAssignableTo<string>()
        |> assertExnMsg
            """
Subject: x
Should: BeAssignableTo
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for incompatible types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo<int>()
        |> assertExnMsg
            """
Subject: x
Should: BeAssignableTo
Expected: System.Int32
But was: System.String
Subject value: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo<int>("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAssignableTo
Expected: System.Int32
But was: System.String
Subject value: asd
"""


module Transform =


    [<Fact>]
    let ``Passes when parsing string integer using int and can be chained with AndDerived with transformed value`` () =
        "1"
            .Should()
            .Transform(int)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when function throws`` () =
        fun () -> "a".Should().Transform(fun _ -> failwith "foo")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Should: Transform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () -> "a".Should().Transform((fun _ -> failwith "foo"), "Some reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: Some reason
Should: Transform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


module ``TryTransform option`` =


    [<Fact>]
    let ``Passes when function returns Some`` () =
        "1"
            .Should()
            .TryTransform(int >> Some)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when function returns None`` () =
        fun () -> "a".Should().TryTransform(fun _ -> Option<string>.None)
        |> assertExnMsg
            """
Subject: '"a"'
Should: TryTransform
But got: null
For value: a
"""


    [<Fact>]
    let ``Fails with expected message when function throws`` () =
        fun () -> "a".Should().TryTransform(fun _ -> failwith<string option> "foo")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when returning None`` () =
        fun () -> "a".Should().TryTransform((fun _ -> Option<string>.None), "Some reason")
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But got: null
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function throws`` () =
        fun () ->
            "a"
                .Should()
                .TryTransform((fun _ -> failwith<string option> "foo"), "Some reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


module ``TryTransform voption`` =


    [<Fact>]
    let ``Passes when function returns ValueSome`` () =
        "1"
            .Should()
            .TryTransform(int >> ValueSome)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when function returns ValueNone`` () =
        fun () -> "a".Should().TryTransform(fun _ -> ValueOption<string>.ValueNone)
        |> assertExnMsg
            """
Subject: '"a"'
Should: TryTransform
But got: ValueNone
For value: a
"""


    [<Fact>]
    let ``Fails with expected message when function throws`` () =
        fun () -> "a".Should().TryTransform(fun _ -> failwith<string voption> "foo")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when returning ValueNone`` () =
        fun () ->
            "a"
                .Should()
                .TryTransform((fun _ -> ValueOption<string>.ValueNone), "ValueSome reason")
        |> assertExnMsg
            """
Subject: '"a"'
Because: ValueSome reason
Should: TryTransform
But got: ValueNone
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function throws`` () =
        fun () ->
            "a"
                .Should()
                .TryTransform((fun _ -> failwith<string voption> "foo"), "ValueSome reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: ValueSome reason
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


module ``TryTransform Result`` =


    [<Fact>]
    let ``Passes when function returns Ok`` () =
        "1"
            .Should()
            .TryTransform(int >> Ok)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when function returns Error`` () =
        fun () -> "a".Should().TryTransform(fun _ -> Result<string, _>.Error "foo")
        |> assertExnMsg
            """
Subject: '"a"'
Should: TryTransform
But got:
  Error: foo
For value: a
"""


    [<Fact>]
    let ``Fails with expected message when function throws`` () =
        fun () -> "a".Should().TryTransform(fun _ -> failwith<Result<string, string>> "foo")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when returning Error`` () =
        fun () ->
            "a"
                .Should()
                .TryTransform((fun _ -> Result<string, _>.Error "foo"), "Some reason")
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But got:
  Error: foo
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function throws`` () =
        fun () ->
            "a"
                .Should()
                .TryTransform((fun _ -> failwith<Result<string, string>> "foo"), "Some reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


module ``TryTransform parse`` =


    [<Fact>]
    let ``Passes when function returns true`` () =
        "1"
            .Should()
            .TryTransform(fun s -> Int32.TryParse s)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when function returns false`` () =
        fun () -> "a".Should().TryTransform(fun s -> Int32.TryParse s)
        |> assertExnMsg
            """
Subject: '"a"'
Should: TryTransform
But got: false
For value: a
"""


    [<Fact>]
    let ``Fails with expected message when function throws`` () =
        fun () -> "a".Should().TryTransform(fun _ -> failwith<bool * int> "foo")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when returning false`` () =
        fun () -> "a".Should().TryTransform((fun s -> Int32.TryParse s), "Some reason")
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But got: false
For value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function throws`` () =
        fun () -> "a".Should().TryTransform((fun _ -> failwith<bool * int> "foo"), "Some reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
For value: a
"""
