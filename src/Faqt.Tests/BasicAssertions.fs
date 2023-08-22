module BasicAssertions

open System
open System.Collections.Generic
open System.IO
open Faqt
open Xunit


module Be =


    [<Fact>]
    let ``Passes for equal integers and can be chained with And`` () =
        (1).Should().Be(1).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes for equal custom type and can be chained with And`` () =
        let x = {| A = 1; B = "foo" |}

        x
            .Should()
            .Be({| A = 1; B = "foo" |})
            .Id<And<{| A: int; B: string |}>>()
            .And.Be({| A = 1; B = "foo" |})


    [<Fact>]
    let ``Passes if both subject and expected is null`` () = (null: string).Should().Be(null)


    [<Fact>]
    let ``Fails with expected message for unequal integers`` () =
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
    let ``Fails with expected message for unequal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().Be({| A = 2; B = "bar" |})
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected:
  A: 2
  B: bar
But was:
  A: 1
  B: foo
"""


    [<Fact>]
    let ``Fails with expected message if only subject is null`` () =
        fun () ->
            let x: string = null
            x.Should().Be("")
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: ''
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if only expected is null`` () =
        fun () ->
            let x = ""
            x.Should().Be(null)
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: null
But was: ''
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
    let ``Passes if isEqual returns true and can be chained with AndDerived with expected value`` () =
        let isEqual _ _ = true

        (1)
            .Should()
            .Be("asd", isEqual)
            .Id<AndDerived<int, string>>()
            .WhoseValue.Should(())
            .Be("asd")


    [<Fact>]
    let ``Handles null subject`` () =
        let isEqual _ _ = true
        (null: String).Should().Be("asd", isEqual)


    [<Fact>]
    let ``Handles null expected`` () =
        let isEqual _ _ = true
        (1).Should().Be((null: string), isEqual)


    [<Fact>]
    let ``Fails with expected message if isEqual returns false`` () =
        let isEqual _ _ = false

        fun () ->
            let x = 1
            x.Should().Be(2, isEqual)
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: 2
But was: 1
WithCustomEquality: true
"""


    [<Fact>]
    let ``Fails with expected message if isEqual returns false even if values are equal using (=)`` () =
        let isEqual _ _ = false

        fun () ->
            let x = 1
            x.Should().Be(1, isEqual)
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
        let isEqual _ _ = false

        fun () ->
            let x = 1
            x.Should().Be(1, isEqual, "Some reason")
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
    let ``Passes for unequal integers and can be chained with And`` () =
        (1).Should().NotBe(2).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes for unequal custom type and can be chained with And`` () =
        let x = {| A = 1; B = "foo" |}

        x
            .Should()
            .NotBe({| A = 2; B = "bar" |})
            .Id<And<{| A: int; B: string |}>>()
            .And.Be({| A = 1; B = "foo" |})


    [<Fact>]
    let ``Passes if only subject is null`` () = (null: string).Should().NotBe("")


    [<Fact>]
    let ``Passes if only expected is null`` () = "".Should().NotBe(null)


    [<Fact>]
    let ``Fails with expected message for equal integers`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x)
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other: 1
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message for equal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().NotBe(x)
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other:
  A: 1
  B: foo
But was:
  A: 1
  B: foo
"""


    [<Fact>]
    let ``Fails with expected message if both subject and expected is null`` () =
        fun () ->
            let x: string = null
            x.Should().NotBe(null)
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other: null
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x, "Some reason")
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
    let ``Passes if isEqual returns false and can be chained with And`` () =
        let isEqual _ _ = false
        (1).Should().NotBe("asd", isEqual).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Handles null subject`` () =
        let isEqual _ _ = false
        (null: String).Should().NotBe("asd", isEqual)


    [<Fact>]
    let ``Handles null expected`` () =
        let isEqual _ _ = false
        (1).Should().NotBe((null: string), isEqual)


    [<Fact>]
    let ``Fails with expected message if isEqual returns true, even if the values are not equal using (=)`` () =
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
    let ``Passes for reference equal values and can be chained with And`` () =
        let x = "asd"
        let y = x
        x.Should().BeSameAs(y).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if both subject and expected are null references`` () =
        (null: string).Should().BeSameAs(null).Id<And<string>>().And.BeNull()


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
        let x = "asd"
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
  Value: asd
"""


    [<Fact>]
    let ``Fails with expected message for non-reference-equal values of generic type even if they are equal`` () =
        let x = Map.empty.Add("a", 1)
        let y = Map.empty.Add("a", 1)
        Assert.True((x = y)) // Sanity check

        fun () -> x.Should().BeSameAs(y)
        |> assertExnMsg
            $"""
Subject: x
Should: BeSameAs
Expected:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash y}
  Type: Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
  Value:
    a: 1
But was:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash x}
  Type: Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
  Value:
    a: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let x = "a"
        let y = "b"

        fun () -> x.Should().BeSameAs(y, "Some reason")
        |> assertExnMsg
            $"""
Subject: x
Because: Some reason
Should: BeSameAs
Expected:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash y}
  Type: System.String
  Value: b
But was:
  PhysicalHash: %i{LanguagePrimitives.PhysicalHash x}
  Type: System.String
  Value: a
"""


module NotBeSameAs =


    [<Fact>]
    let ``Passes for non-reference equal values and can be chained with And`` () =
        "asd".Should().NotBeSameAs("foo").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if only subject is null reference`` () = null.Should().NotBeSameAs("asd")


    [<Fact>]
    let ``Passes if only expected is null reference`` () = "asd".Should().NotBeSameAs(null)


    [<Fact>]
    let ``Fails with expected message if both subject and expected are null references`` () =
        fun () ->
            let x = null
            x.Should().NotBeSameAs(null)
        |> assertExnMsg
            """
Subject: x
Should: NotBeSameAs
Other: null
"""


    [<Fact>]
    let ``Fails with expected message for reference-equal values of generic type`` () =
        let x = Map.empty.Add("a", 1)
        let y = x

        fun () -> x.Should().NotBeSameAs(y)
        |> assertExnMsg
            """
Subject: x
Should: NotBeSameAs
Other:
  a: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let x = "a"
        let y = x

        fun () -> x.Should().NotBeSameAs(y, "Some reason")
        |> assertExnMsg
            $"""
Subject: x
Because: Some reason
Should: NotBeSameAs
Other: a
"""


module BeNull =


    [<Fact>]
    let ``Passes for null and can be chained with And`` () =
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
    let ``Passes for non-null and can be chained with And`` () =
        "asd".Should().NotBeNull().Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
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
            let (x: string) = null
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
But was:
  Type: System.String
  Value: asd
"""


    [<Fact>]
    let ``Fails with expected message if non-generic interface, even if type implements it`` () =
        fun () ->
            let x = new MemoryStream()
            x :> IDisposable |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<IDisposable>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.IDisposable
But was:
  Type: System.IO.MemoryStream
  Value: System.IO.MemoryStream
"""


    [<Fact>]
    let ``Fails with expected message if generic interface, even if type implements it`` () =
        fun () ->
            let x: Map<string, int> = Map.empty
            x :> IDictionary<string, int> |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<IDictionary<string, int>>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.Collections.Generic.IDictionary<System.String, System.Int32>
But was:
  Type: Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
  Value: {}
"""


    [<Fact>]
    let ``Fails with expected message if sub-type`` () =
        fun () ->
            let x = new MemoryStream()
            x :> Stream |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<Stream>)
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.IO.Stream
But was:
  Type: System.IO.MemoryStream
  Value: System.IO.MemoryStream
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
But was:
  Type: System.String
  Value: asd
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
But was:
  Type: System.String
  Value: asd
"""


    [<Fact>]
    let ``Fails with expected message if non-generic interface, even if type implements it`` () =
        fun () ->
            let x = new MemoryStream()
            x :> IDisposable |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<IDisposable>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.IDisposable
But was:
  Type: System.IO.MemoryStream
  Value: System.IO.MemoryStream
"""


    [<Fact>]
    let ``Fails with expected message if generic interface, even if type implements it`` () =
        fun () ->
            let x: Map<string, int> = Map.empty
            x :> IDictionary<string, int> |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<IDictionary<string, int>>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.Collections.Generic.IDictionary<System.String, System.Int32>
But was:
  Type: Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
  Value: {}
"""


    [<Fact>]
    let ``Fails with expected message if sub-type`` () =
        fun () ->
            let x = new MemoryStream()
            x :> Stream |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<Stream>()
        |> assertExnMsg
            """
Subject: x
Should: BeOfType
Expected: System.IO.Stream
But was:
  Type: System.IO.MemoryStream
  Value: System.IO.MemoryStream
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
But was:
  Type: System.String
  Value: asd
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
        let x = new MemoryStream()
        x.Should().BeAssignableTo(typeof<IDisposable>)


    [<Fact>]
    let ``Passes for boxed instance of type that implements specified interface`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo(typeof<IDisposable>)


    [<Fact>]
    let ``Passes for instance of subtype of specified type`` () =
        let x = new MemoryStream()
        x.Should().BeAssignableTo(typeof<Stream>)


    [<Fact>]
    let ``Passes for boxed instance of subtype of specified type`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo(typeof<Stream>)


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
But was:
  Type: System.String
  Value: asd
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
But was:
  Type: System.String
  Value: asd
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
        let x = new MemoryStream()
        x.Should().BeAssignableTo<IDisposable>()


    [<Fact>]
    let ``Passes for boxed instance of type that implements specified interface`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo<IDisposable>()


    [<Fact>]
    let ``Passes for instance of subtype of specified type`` () =
        let x = new MemoryStream()
        x.Should().BeAssignableTo<Stream>()


    [<Fact>]
    let ``Passes for boxed instance of subtype of specified type`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo<Stream>()


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
But was:
  Type: System.String
  Value: asd
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
But was:
  Type: System.String
  Value: asd
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
        fun () -> "a".Should().Transform(int, "Some reason")
        |> assertExnMsgWildcard
            """
Subject: '"a"'
Because: Some reason
Should: Transform
But threw: |-
  System.FormatException: The input string 'a' was not in a correct format.
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
