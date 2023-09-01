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
        assertFails (fun () -> a.Should().Be(b))


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
        assertFails (fun () -> a.Should().Be(b, (fun _ _ -> false)))


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
        assertFails (fun () -> a.Should().NotBe(b))


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
        assertFails (fun () -> a.Should().NotBe(b, (fun _ _ -> true)))


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
        assertFails (fun () -> subject.Should().BeSameAs(expected))


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
        assertFails (fun () -> subject.Should().NotBeSameAs(expected))


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
    let ``Passes if null and can be chained with And`` () =
        (null: string).Should().BeNull().Id<And<string>>().And.BeNull()


    [<Fact>]
    let ``Fails with expected message if not null`` () =
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
    let ``Fails with expected message with because if not null`` () =
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
    let ``Passes if not null and can be chained with And`` () =
        "a".Should().NotBeNull().Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Fails with expected message if null`` () =
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
    let ``Fails with expected message with because if null`` () =
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
    let ``Can be chained with And`` () =
        "asd".Should().BeOfType(typeof<string>).Id<And<string>>().And.Be("asd")


    let passData = [
        [| box "a"; typeof<string> |]
        [| 1; typeof<int> |]
        [| TestSubType(); typeof<TestSubType> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes for instances of the specified type`` (subject: obj) (expected: Type) =
        subject.Should().BeOfType(expected)


    let failData = [
        [| box (null: string); typeof<string> |]
        [| 1; typeof<string> |]
        // Cast as sanity check to avoid false negatives
        [| TestSubType() :> TestBaseType :> obj; typeof<TestBaseType> |]
        [| TestSubType() :> TestInterface :> obj; typeof<TestInterface> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails for null or instances of other types than the specified type`` (subject: obj) (expected: Type) =
        assertFails (fun () -> subject.Should().BeOfType(expected))


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
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeOfType(typeof<string>, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeOfType
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for different generic types`` () =
        fun () ->
            let x = TestSubType<string, int>()
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
    let ``Can be chained with AndDerived with cast value`` () =
        let x = TestSubType()

        (x :> TestBaseType)
            .Should()
            .BeOfType<TestSubType>()
            .Id<AndDerived<TestBaseType, TestSubType>>()
            .WhoseValue.Should(())
            .Be(x)


    let beOfType<'a> (t: Testable<obj>) = t.BeOfType<'a>() |> ignore


    let passData = [
        [| box "a"; beOfType<string> |]
        [| 1; beOfType<int> |]
        [| TestSubType(); beOfType<TestSubType> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes for instances of the specified type`` (subject: obj) run = run (subject.Should())


    let failData = [
        [| box (null: string); beOfType<string> |]
        [| 1; beOfType<string> |]
        // Cast as sanity check to avoid false negatives
        [| TestSubType() :> TestBaseType :> obj; beOfType<TestBaseType> |]
        [| TestSubType() :> TestInterface :> obj; beOfType<TestInterface> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails for null or instances of other types than the specified type`` (subject: obj) run =
        assertFails (fun () -> run (subject.Should()))


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
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeOfType<string>("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeOfType
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for different generic types`` () =
        fun () ->
            let x = TestSubType<string, int>()
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
    let ``Can be chained with And`` () =
        "asd".Should().BeAssignableTo(typeof<string>).Id<And<string>>().And.Be("asd")


    let passData = [
        [| box "a"; typeof<string> |]
        [| 1; typeof<int> |]
        [| TestSubType(); typeof<TestSubType> |]
        [| TestSubType(); typeof<TestBaseType> |]
        [| TestSubType(); typeof<TestInterface> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes for instances of a compatible type`` (subject: obj) (expected: Type) =
        subject.Should().BeAssignableTo(expected)


    let failData = [
        [| box (null: string); typeof<string> |]
        [| 1; typeof<string> |]
        [| TestBaseType(); typeof<TestSubType> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails for null or instances of incompatible types`` (subject: obj) (expected: Type) =
        assertFails (fun () -> subject.Should().BeAssignableTo(expected))


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
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeAssignableTo(typeof<string>, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAssignableTo
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for incompatible generic types`` () =
        fun () ->
            let x = TestBaseType<string, int>()
            x.Should().BeAssignableTo(typeof<TestSubType<string, int>>)
        |> assertExnMsg
            """
Subject: x
Should: BeAssignableTo
Expected: TestUtils+TestSubType<System.String, System.Int32>
But was: TestUtils+TestBaseType<System.String, System.Int32>
Subject value: {}
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
    let ``Can be chained with AndDerived with cast value`` () =
        let x = TestSubType()

        (x :> TestBaseType)
            .Should()
            .BeAssignableTo<TestSubType>()
            .Id<AndDerived<TestBaseType, TestSubType>>()
            .WhoseValue.Should(())
            .Be(x)


    let beAssignableTo<'a> (t: Testable<obj>) = t.BeAssignableTo<'a>() |> ignore


    let passData = [
        [| box "a"; beAssignableTo<string> |]
        [| 1; beAssignableTo<int> |]
        [| TestSubType(); beAssignableTo<TestSubType> |]
        [| TestSubType(); beAssignableTo<TestBaseType> |]
        [| TestSubType(); beAssignableTo<TestInterface> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes for instances of a compatible type`` (subject: obj) run = run (subject.Should())


    let failData = [
        [| box (null: string); beAssignableTo<string> |]
        [| 1; beAssignableTo<string> |]
        [| TestBaseType(); beAssignableTo<TestSubType> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails for null or instances of incompatible types`` (subject: obj) run =
        assertFails (fun () -> run (subject.Should()))


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
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeAssignableTo<string>("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAssignableTo
Expected: System.String
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for incompatible generic types`` () =
        fun () ->
            let x = TestBaseType<string, int>()
            x.Should().BeAssignableTo<TestSubType<string, int>>()
        |> assertExnMsg
            """
Subject: x
Should: BeAssignableTo
Expected: TestUtils+TestSubType<System.String, System.Int32>
But was: TestUtils+TestBaseType<System.String, System.Int32>
Subject value: {}
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
    let ``Passes when the function does not throw and can be chained with AndDerived with transformed value`` () =
        "a"
            .Should()
            .Transform(fun s -> s.Length)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when the function throws`` () =
        fun () -> "a".Should().Transform(fun _ -> failwith "foo")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Should: Transform
But threw: |-
  System.Exception: foo
     at *
Subject value: a
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
Subject value: a
"""


module ``TryTransform option`` =


    [<Fact>]
    let ``Passes when function returns Some and can be chained with AndDerived with transformed value`` () =
        "a"
            .Should()
            .TryTransform(fun s -> Some s.Length)
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
Subject value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function returns None`` () =
        fun () -> "a".Should().TryTransform((fun _ -> Option<string>.None), "Some reason")
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But got: null
Subject value: a
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
Subject value: a
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
Subject value: a
"""


module ``TryTransform voption`` =


    [<Fact>]
    let ``Passes when function returns ValueSome and can be chained with AndDerived with transformed value`` () =
        "a"
            .Should()
            .TryTransform(fun s -> ValueSome s.Length)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when function returns None`` () =
        fun () -> "a".Should().TryTransform(fun _ -> ValueOption<string>.ValueNone)
        |> assertExnMsg
            """
Subject: '"a"'
Should: TryTransform
But got: ValueNone
Subject value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function returns None`` () =
        fun () ->
            "a"
                .Should()
                .TryTransform((fun _ -> ValueOption<string>.ValueNone), "Some reason")
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But got: ValueNone
Subject value: a
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
Subject value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function throws`` () =
        fun () ->
            "a"
                .Should()
                .TryTransform((fun _ -> failwith<string voption> "foo"), "Some reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
Subject value: a
"""


module ``TryTransform Result`` =


    [<Fact>]
    let ``Passes when function returns Ok and can be chained with AndDerived with transformed value`` () =
        "a"
            .Should()
            .TryTransform(fun s -> Ok s.Length)
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
Subject value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function returns Error`` () =
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
Subject value: a
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
Subject value: a
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
Subject value: a
"""


module ``TryTransform parse`` =


    [<Fact>]
    let ``Can be called with functions with signatures like Int32.TryParse`` () =
        "1".Should().TryTransform(fun s -> Int32.TryParse s)


    [<Fact>]
    let ``Passes when function returns true and can be chained with AndDerived with transformed value`` () =
        "a"
            .Should()
            .TryTransform(fun s -> true, s.Length)
            .Id<AndDerived<string, int>>()
            .WhoseValue.Should(())
            .Be(1)


    [<Fact>]
    let ``Fails with expected message when function returns false`` () =
        fun () -> "a".Should().TryTransform(fun _ -> false, ())
        |> assertExnMsg
            """
Subject: '"a"'
Should: TryTransform
But got: false
Subject value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when returning false`` () =
        fun () -> "a".Should().TryTransform((fun _ -> false, ()), "Some reason")
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But got: false
Subject value: a
"""


    [<Fact>]
    let ``Fails with expected message when function throws`` () =
        fun () -> "a".Should().TryTransform(fun _ -> failwith<bool * unit> "foo")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
Subject value: a
"""


    [<Fact>]
    let ``Fails with expected message with because when function throws`` () =
        fun () -> "a".Should().TryTransform((fun _ -> failwith<bool * unit> "foo"), "Some reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: Some reason
Should: TryTransform
But threw: |-
  System.Exception: foo
     at *
Subject value: a
"""
