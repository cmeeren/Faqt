module ComparisonAssertions

open System
open Faqt
open Xunit

type Comparison = Comparison of int

type ComparisonZero =
    | ComparisonZero of int

    static member Zero = ComparisonZero 0


[<NoEquality; NoComparison>]
type NumberWithoutOps = NumberWithoutOps of int


[<NoEquality; NoComparison>]
type NumberWithSubtraction =
    | NumberWithSubtraction of int

    static member op_Subtraction(NumberWithoutOps a, NumberWithSubtraction b) = Comparison(a - b)
    static member op_Subtraction(NumberWithSubtraction a, NumberWithoutOps b) = Comparison(a - b)


[<Struct>]
type OffsetNumber =
    val Raw: int
    new(value: int) = { Raw = value - 100 }
    member this.Value = this.Raw + 100

    static member (-)(a: OffsetNumber, b: OffsetNumber) = OffsetNumber(a.Value - b.Value)


module ToleranceValidation =


    let inline private negativeCases name zero negative = [
        [|
            box (name + ".BeCloseTo")
            box (fun () -> zero.Should().BeCloseTo(zero, negative) |> ignore)
        |]
        [|
            box (name + ".NotBeCloseTo")
            box (fun () -> zero.Should().NotBeCloseTo(zero, negative) |> ignore)
        |]
    ]


    let negativeToleranceData = [
        yield! negativeCases "sbyte" 0y -1y
        yield! negativeCases "int16" 0s -1s
        yield! negativeCases "int" 0 -1
        yield! negativeCases "int64" 0L -1L
        yield! negativeCases "nativeint" 0n -1n
        yield! negativeCases "Half" Half.Zero -Half.One
        yield! negativeCases "single" 0.0f -1.0f
        yield! negativeCases "double" 0.0 -1.0
        yield! negativeCases "decimal" 0m -1m
        yield! negativeCases "bigint" 0I -1I
        yield! negativeCases "TimeSpan" TimeSpan.Zero (TimeSpan.FromTicks(-1L))
        yield! negativeCases "Int128" Int128.Zero -Int128.One
    ]


    [<Theory>]
    [<MemberData(nameof negativeToleranceData)>]
    let ``Rejects negative known tolerances`` (_name: string) (assertion: unit -> unit) =
        Assert.Throws<ArgumentException>(fun () -> assertion ()) |> ignore


module UnsignedTolerance =


    let inline private casesFor name zero one maximum =
        [
            "equal", maximum, maximum, zero, true
            "adjacent", maximum - one, maximum, one, true
            "full range", zero, maximum, maximum, true
            "outside tolerance", zero, maximum, zero, false
        ]
        |> List.collect (fun (scenario, subject, target, tolerance, expectedClose) ->
            [ subject, target; target, subject ]
            |> List.mapi (fun direction (subject, target) -> [|
                box $"{name}: {scenario}, direction {direction}"
                box (fun () ->
                    if expectedClose then
                        subject.Should().BeCloseTo(target, tolerance) |> ignore

                        assertFails (fun () -> subject.Should().NotBeCloseTo(target, tolerance))
                        |> ignore
                    else
                        assertFails (fun () -> subject.Should().BeCloseTo(target, tolerance)) |> ignore
                        subject.Should().NotBeCloseTo(target, tolerance) |> ignore
                )
            |])
        )


    let cases = [
        yield! casesFor "byte" 0uy 1uy Byte.MaxValue
        yield! casesFor "uint16" 0us 1us UInt16.MaxValue
        yield! casesFor "uint32" 0u 1u UInt32.MaxValue
        yield! casesFor "uint64" 0UL 1UL UInt64.MaxValue
        yield! casesFor "unativeint" 0un 1un UIntPtr.MaxValue
        yield! casesFor "UInt128" UInt128.Zero UInt128.One UInt128.MaxValue
    ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Honors unsigned tolerance in both directions`` (_name: string) (run: unit -> unit) = run ()


module SignedTolerance =


    let inline private casesFor name minimum maximum zero one =
        [
            "equal minimum", minimum, minimum, zero, true
            "equal maximum", maximum, maximum, zero, true
            "minimum and zero", minimum, zero, zero, false
            "minimum exceeds largest tolerance", minimum, zero, maximum, false
            "full range", minimum, maximum, maximum, false
            "adjacent minimum", minimum, minimum + one, one, true
            "adjacent maximum", maximum - one, maximum, one, true
            "negative exact boundary", minimum, -one, maximum, true
            "positive exact boundary", zero, maximum, maximum, true
            "crossing zero exact boundary", -one, maximum - one, maximum, true
            "crossing zero outside boundary", -one, maximum, maximum, false
            "small crossing zero", -one, one, one + one, true
        ]
        |> List.collect (fun (scenario, subject, target, tolerance, expectedClose) ->
            [ subject, target; target, subject ]
            |> List.mapi (fun direction (subject, target) -> [|
                box $"{name}: {scenario}, direction {direction}"
                box (fun () ->
                    if expectedClose then
                        subject.Should().BeCloseTo(target, tolerance) |> ignore

                        assertFails (fun () -> subject.Should().NotBeCloseTo(target, tolerance))
                        |> ignore
                    else
                        assertFails (fun () -> subject.Should().BeCloseTo(target, tolerance)) |> ignore
                        subject.Should().NotBeCloseTo(target, tolerance) |> ignore
                )
            |])
        )


    let cases = [
        yield! casesFor "sbyte" SByte.MinValue SByte.MaxValue 0y 1y
        yield! casesFor "int16" Int16.MinValue Int16.MaxValue 0s 1s
        yield! casesFor "int" Int32.MinValue Int32.MaxValue 0 1
        yield! casesFor "int64" Int64.MinValue Int64.MaxValue 0L 1L
        yield! casesFor "nativeint" IntPtr.MinValue IntPtr.MaxValue 0n 1n
        yield! casesFor "Int128" Int128.MinValue Int128.MaxValue Int128.Zero Int128.One
        yield! casesFor "TimeSpan" TimeSpan.MinValue TimeSpan.MaxValue TimeSpan.Zero (TimeSpan.FromTicks(1L))
    ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Honors signed tolerance without overflow`` (_name: string) (run: unit -> unit) = run ()


module DecimalTolerance =


    let cases =
        [
            "equal minimum", Decimal.MinValue, Decimal.MinValue, 0m, true
            "minimum to zero", Decimal.MinValue, 0m, Decimal.MaxValue, true
            "full range", Decimal.MinValue, Decimal.MaxValue, Decimal.MaxValue, false
            "outside zero tolerance", Decimal.MinValue, Decimal.MaxValue, 0m, false
            "adjacent minimum", Decimal.MinValue, Decimal.MinValue + 1m, 1m, true
            "adjacent maximum", Decimal.MaxValue - 1m, Decimal.MaxValue, 1m, true
            "negative span", Decimal.MinValue, -1m, Decimal.MaxValue - 1m, true
            "crossing zero exact boundary", -1m, Decimal.MaxValue - 1m, Decimal.MaxValue, true
            "crossing zero outside boundary", -1m, Decimal.MaxValue, Decimal.MaxValue, false
            "fractional boundary", -0.125m, 0.375m, 0.5m, true
            "outside fractional boundary", -0.125m, 0.375m, 0.499m, false
            "fraction beyond maximum distance", -0.1m, Decimal.MaxValue, Decimal.MaxValue, false
            "rounded distance below lower endpoint", 0.9m, Decimal.MaxValue, Decimal.MaxValue - 1m, false
            "exact lower endpoint", 1m, Decimal.MaxValue, Decimal.MaxValue - 1m, true
            "inside lower endpoint", 1.1m, Decimal.MaxValue, Decimal.MaxValue - 1m, true
            "tiny fraction beyond ordinary tolerance", -0.0000000000000000000000000001m, 10m, 10m, false
            "equivalent values with different scales", 1.00m, 1m, 0m, true
            "negative scaled zero", Decimal(0, 0, 0, true, 28uy), 0m, 0m, true
            "adjacent integers across 32 bits", 4294967295m, 4294967296m, 1m, true
            "outside tolerance across 32 bits", 4294967295m, 4294967296m, 0.9m, false
            "adjacent integers across 64 bits", 18446744073709551615m, 18446744073709551616m, 1m, true
            "outside tolerance across 64 bits", 18446744073709551615m, 18446744073709551616m, 0.9m, false

            // target - tolerance is exactly zero at every scale, without computing a rounded distance.
            for scale in 0..28 do
                let target = Decimal(-1, -1, -1, false, byte scale)
                let epsilon = 0.0000000000000000000000000001m
                $"outside zero endpoint at scale %i{scale}", -epsilon, target, target, false
                $"on zero endpoint at scale %i{scale}", 0m, target, target, true
                $"inside zero endpoint at scale %i{scale}", epsilon, target, target, true
        ]
        |> List.collect (fun (name, subject, target, tolerance, expectedClose) ->
            [ subject, target; target, subject; -subject, -target; -target, -subject ]
            |> List.mapi (fun direction (subject, target) -> [|
                box $"%s{name}, direction %i{direction}"
                box subject
                box target
                box tolerance
                box expectedClose
            |])
        )


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Honors exact decimal tolerance without rounding or overflow``
        (_name: string)
        (subject: decimal)
        (target: decimal)
        (tolerance: decimal)
        expectedClose
        =
        if expectedClose then
            subject.Should().BeCloseTo(target, tolerance) |> ignore

            assertFails (fun () -> subject.Should().NotBeCloseTo(target, tolerance))
            |> ignore
        else
            assertFails (fun () -> subject.Should().BeCloseTo(target, tolerance)) |> ignore
            subject.Should().NotBeCloseTo(target, tolerance) |> ignore


module RelationalNaN =


    let inline private casesFor name nan one =
        [ nan, one; one, nan; nan, nan ]
        |> List.mapi (fun position (subject, other) ->
            [
                "greater", (fun () -> subject.Should().BeGreaterThan(other) |> ignore)
                "greater or equal", (fun () -> subject.Should().BeGreaterThanOrEqualTo(other) |> ignore)
                "less", (fun () -> subject.Should().BeLessThan(other) |> ignore)
                "less or equal", (fun () -> subject.Should().BeLessThanOrEqualTo(other) |> ignore)
            ]
            |> List.map (fun (assertion, run) -> [| box $"%s{name}: %s{assertion}, operands %i{position}"; box run |])
        )
        |> List.concat


    let cases = [
        yield! casesFor "Half" Half.NaN Half.One
        yield! casesFor "single" Single.NaN 1.0f
        yield! casesFor "double" Double.NaN 1.0
        yield! casesFor "interface Half" (Half.NaN :> IComparable) (Half.One :> IComparable)
        yield! casesFor "interface single" (Single.NaN :> IComparable) (1.0f :> IComparable)
        yield! casesFor "interface double" (Double.NaN :> IComparable) (1.0 :> IComparable)
    ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Rejects NaN relational operands`` (_name: string) (run: unit -> unit) = assertFails run |> ignore


module HalfComparisons =


    let nanCases =
        [
            "close subject", (fun () -> Half.NaN.Should().BeCloseTo(Half.One, Half.Zero) |> ignore)
            "close target", (fun () -> Half.One.Should().BeCloseTo(Half.NaN, Half.Zero) |> ignore)
            "close tolerance", (fun () -> Half.One.Should().BeCloseTo(Half.One, Half.NaN) |> ignore)
            "not close subject", (fun () -> Half.NaN.Should().NotBeCloseTo(Half.One, Half.Zero) |> ignore)
            "not close target", (fun () -> Half.One.Should().NotBeCloseTo(Half.NaN, Half.Zero) |> ignore)
            "not close tolerance", (fun () -> Half.One.Should().NotBeCloseTo(Half.One, Half.NaN) |> ignore)
            "positive", (fun () -> Half.NaN.Should().BePositive() |> ignore)
            "negative", (fun () -> Half.NaN.Should().BeNegative() |> ignore)
            "nonnegative", (fun () -> Half.NaN.Should().BeNonNegative() |> ignore)
            "nonpositive", (fun () -> Half.NaN.Should().BeNonPositive() |> ignore)
            "range subject", (fun () -> Half.NaN.Should().BeInRange(Half.Zero, Half.One) |> ignore)
            "range lower", (fun () -> Half.Zero.Should().BeInRange(Half.NaN, Half.One) |> ignore)
            "range upper", (fun () -> Half.Zero.Should().BeInRange(Half.Zero, Half.NaN) |> ignore)
            "interface range", (fun () -> (Half.Zero :> IComparable).Should().BeInRange(Half.NaN, Half.One) |> ignore)
            "NaN before negative tolerance", (fun () -> Half.NaN.Should().BeCloseTo(Half.One, -Half.One) |> ignore)
        ]
        |> List.map (fun (name, run) -> [| box name; box run |])


    [<Theory>]
    [<MemberData(nameof nanCases)>]
    let ``Rejects Half NaN`` (_name: string) (run: unit -> unit) = assertFails run |> ignore


    [<Fact>]
    let ``Accepts finite Half values`` () =
        Half.One.Should().BeGreaterThan(Half.Zero).And.BeGreaterThanOrEqualTo(Half.One)
        |> ignore

        Half.Zero.Should().BeLessThan(Half.One).And.BeLessThanOrEqualTo(Half.Zero)
        |> ignore

        Half.One.Should().BeCloseTo(Half.Zero, Half.One) |> ignore
        Half.One.Should().NotBeCloseTo(Half.Zero, Half.Zero) |> ignore
        Half.One.Should().BePositive() |> ignore
        (-Half.One).Should().BeNegative() |> ignore
        Half.Zero.Should().BeNonNegative().And.BeNonPositive() |> ignore
        Half.One.Should().BeInRange(Half.Zero, Half.One) |> ignore


module BeCloseTo =


    [<Fact>]
    let ``Can be called with any set of 3 types where subject or target can be subtracted both ways and tolerance has comparison``
        ()
        =
        (NumberWithoutOps 0).Should().BeCloseTo(NumberWithSubtraction 0, Comparison 0)
        |> ignore

        (NumberWithSubtraction 0).Should().BeCloseTo(NumberWithoutOps 0, Comparison 0)
        |> ignore


    [<Fact>]
    let ``Can be called common combinations of types`` () =
        DateTime.MinValue.Should().BeCloseTo(DateTime.MinValue, TimeSpan.Zero) |> ignore
        (0).Should().BeCloseTo(0, 0) |> ignore
        (0.).Should().BeCloseTo(0., 0.) |> ignore
        0m.Should().BeCloseTo(0m, 0m) |> ignore


    [<Fact>]
    let ``Can be chained with And`` () =
        (0).Should().BeCloseTo(0, 0).Id<And<int>>().And.Be(0)


    [<Fact>]
    let ``Passes for unsigned integers within tolerance`` () = 0u.Should().BeCloseTo(1u, 1u)


    [<Theory>]
    [<InlineData(10, 10, 0)>]
    [<InlineData(10, 11, 1)>]
    let ``Supports custom numbers whose default is not zero`` subject target tolerance =
        OffsetNumber(subject).Should().BeCloseTo(OffsetNumber(target), OffsetNumber(tolerance))


    [<Fact>]
    let ``Fails outside custom tolerance when the default is not zero`` () =
        assertFails (fun () -> OffsetNumber(10).Should().BeCloseTo(OffsetNumber(12), OffsetNumber(1)))


    [<Theory>]
    [<InlineData(1., 1., 0.)>] // Equal with zero tolerance
    [<InlineData(1., 1., 1.)>] // Equal with non-zero tolerance
    [<InlineData(0.5, 1., 1.)>] // Inside interval
    [<InlineData(0.5, 1., 0.5)>] // At lower bound
    [<InlineData(1.5, 1., 0.5)>] // At upper bound
    let ``Passes if within tolerance`` (subject: float) (target: float) (tolerance: float) =
        subject.Should().BeCloseTo(target, tolerance)


    [<Theory>]
    [<InlineData(0.4, 1., 0.5)>] // Below lower bound
    [<InlineData(1.6, 1., 0.5)>] // Above upper bound
    let ``Fails if outside tolerance`` (subject: float) (target: float) (tolerance: float) =
        assertFails (fun () -> subject.Should().BeCloseTo(target, tolerance))


    [<Fact>]
    let ``Fails if any argument is NaN`` () =
        assertFails (fun () -> Double.NaN.Should().BeCloseTo(0.0, 0.0)) |> ignore
        assertFails (fun () -> 0.0.Should().BeCloseTo(Double.NaN, 0.0)) |> ignore
        assertFails (fun () -> 0.0.Should().BeCloseTo(0.0, Double.NaN)) |> ignore


    [<Fact>]
    let ``Throws ArgumentException if tolerance is negative`` () =
        Assert.Throws<ArgumentException>(fun () -> 1.0.Should().BeCloseTo(1.0, -0.1) |> ignore)
        |> ignore


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1.09
            x.Should().BeCloseTo(1.0, 0.05)
        |> assertExnMsg
            """
Subject: x
Should: BeCloseTo
Target: 1
With tolerance: 0.05
But was: 1.09
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1.09
            x.Should().BeCloseTo(1.0, 0.05, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeCloseTo
Target: 1
With tolerance: 0.05
But was: 1.09
"""


module NotBeCloseTo =


    [<Fact>]
    let ``Can be called with any set of 3 types where subject or target can be subtracted both ways and tolerance has comparison``
        ()
        =
        (NumberWithoutOps 0).Should().NotBeCloseTo(NumberWithSubtraction 1, Comparison 0)
        |> ignore

        (NumberWithSubtraction 0).Should().NotBeCloseTo(NumberWithoutOps 1, Comparison 0)
        |> ignore


    [<Fact>]
    let ``Can be called common combinations of types`` () =
        DateTime.MinValue.Should().NotBeCloseTo(DateTime.MaxValue, TimeSpan.Zero)
        |> ignore

        (0).Should().NotBeCloseTo(1, 0) |> ignore
        (0.).Should().NotBeCloseTo(1., 0.) |> ignore
        0m.Should().NotBeCloseTo(1m, 0m) |> ignore


    [<Fact>]
    let ``Can be chained with And`` () =
        (0).Should().NotBeCloseTo(1, 0).Id<And<int>>().And.Be(0)


    [<Fact>]
    let ``Fails for unsigned integers within tolerance`` () =
        assertFails (fun () -> 0u.Should().NotBeCloseTo(1u, 1u))


    [<Fact>]
    let ``Supports custom numbers whose default is not zero`` () =
        OffsetNumber(10).Should().NotBeCloseTo(OffsetNumber(12), OffsetNumber(1))


    [<Theory>]
    [<InlineData(10, 10, 0)>]
    [<InlineData(10, 11, 1)>]
    let ``Fails inside custom tolerance when the default is not zero`` subject target tolerance =
        assertFails (fun () ->
            OffsetNumber(subject).Should().NotBeCloseTo(OffsetNumber(target), OffsetNumber(tolerance))
        )


    [<Theory>]
    [<InlineData(0.4, 1., 0.5)>] // Below lower bound
    [<InlineData(1.6, 1., 0.5)>] // Above upper bound
    let ``Passes if outside tolerance`` (subject: float) (target: float) (tolerance: float) =
        subject.Should().NotBeCloseTo(target, tolerance)


    [<Theory>]
    [<InlineData(1., 1., 0.)>] // Equal with zero tolerance
    [<InlineData(1., 1., 1.)>] // Equal with non-zero tolerance
    [<InlineData(0.5, 1., 1.)>] // Inside interval
    [<InlineData(0.5, 1., 0.5)>] // At lower bound
    [<InlineData(1.5, 1., 0.5)>] // At upper bound
    let ``Fails if within tolerance`` (subject: float) (target: float) (tolerance: float) =
        assertFails (fun () -> subject.Should().NotBeCloseTo(target, tolerance))


    [<Fact>]
    let ``Fails if any argument is NaN`` () =
        assertFails (fun () -> Double.NaN.Should().NotBeCloseTo(0.0, 0.0)) |> ignore
        assertFails (fun () -> 0.0.Should().NotBeCloseTo(Double.NaN, 0.0)) |> ignore
        assertFails (fun () -> 0.0.Should().NotBeCloseTo(0.0, Double.NaN)) |> ignore


    [<Fact>]
    let ``Throws ArgumentException if tolerance is negative`` () =
        Assert.Throws<ArgumentException>(fun () -> 1.0.Should().NotBeCloseTo(1.0, -0.1) |> ignore)
        |> ignore


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1.02
            x.Should().NotBeCloseTo(1.0, 0.05)
        |> assertExnMsg
            """
Subject: x
Should: NotBeCloseTo
Target: 1
With tolerance: 0.05
But was: 1.02
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1.02
            x.Should().NotBeCloseTo(1.0, 0.05, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBeCloseTo
Target: 1
With tolerance: 0.05
But was: 1.02
"""


module BeGreaterThan =


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 1).Should().BeGreaterThan(Comparison 0)


    [<Fact>]
    let ``Passes if subject > other and can be chained with And`` () =
        (1).Should().BeGreaterThan(0).Id<And<int>>().And.Be(1)


    [<Theory>]
    [<InlineData(0, 0)>] // subject = other
    [<InlineData(0, 1)>] // subject < other
    let ``Fails if subject <= other`` (subject: int) (other: int) =
        assertFails (fun () -> subject.Should().BeGreaterThan(other))


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<Comparison>.Should().BeGreaterThan(Comparison 0))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 0
            x.Should().BeGreaterThan(0)
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThan
Other: 0
But was: 0
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 0
            x.Should().BeGreaterThan(0, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeGreaterThan
Other: 0
But was: 0
"""


module BeGreaterThanOrEqualTo =


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 0).Should().BeGreaterThanOrEqualTo(Comparison 0)


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BeGreaterThanOrEqualTo(0).Id<And<int>>().And.Be(1)


    [<Theory>]
    [<InlineData(1, 0)>] // subject > other
    [<InlineData(0, 0)>] // subject = other
    let ``Passes if subject >= other`` (subject: int) (other: int) =
        subject.Should().BeGreaterThanOrEqualTo(other)


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<Comparison>.Should().BeGreaterThanOrEqualTo(Comparison 0))


    [<Fact>]
    let ``Fails with expected message if subject < other`` () =
        fun () ->
            let x = 0
            x.Should().BeGreaterThanOrEqualTo(1)
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThanOrEqualTo
Other: 1
But was: 0
"""


    [<Fact>]
    let ``Fails with expected message with because if subject < other`` () =
        fun () ->
            let x = 0
            x.Should().BeGreaterThanOrEqualTo(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeGreaterThanOrEqualTo
Other: 1
But was: 0
"""


module BeLessThan =


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 0).Should().BeLessThan(Comparison 1)


    [<Fact>]
    let ``Passes if subject < other and can be chained with And`` () =
        (0).Should().BeLessThan(1).Id<And<int>>().And.Be(0)


    [<Theory>]
    [<InlineData(0, 0)>] // subject = other
    [<InlineData(1, 0)>] // subject > other
    let ``Fails if subject >= other`` (subject: int) (other: int) =
        assertFails (fun () -> subject.Should().BeLessThan(other))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 0
            x.Should().BeLessThan(0)
        |> assertExnMsg
            """
Subject: x
Should: BeLessThan
Other: 0
But was: 0
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 0
            x.Should().BeLessThan(0, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLessThan
Other: 0
But was: 0
"""


module BeLessThanOrEqualTo =


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 0).Should().BeLessThanOrEqualTo(Comparison 0)


    [<Fact>]
    let ``Can be chained with And`` () =
        (0).Should().BeLessThanOrEqualTo(1).Id<And<int>>().And.Be(0)


    [<Theory>]
    [<InlineData(0, 1)>] // subject < other
    [<InlineData(0, 0)>] // subject = other
    let ``Passes if subject <= other`` (subject: int) (other: int) =
        subject.Should().BeLessThanOrEqualTo(other)


    [<Fact>]
    let ``Fails with expected message if subject > other`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThanOrEqualTo(0)
        |> assertExnMsg
            """
Subject: x
Should: BeLessThanOrEqualTo
Other: 0
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if subject > other`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThanOrEqualTo(0, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLessThanOrEqualTo
Other: 0
But was: 1
"""


module BePositive =


    [<Fact>]
    let ``Can be called with any type that has comparison and Zero`` () =
        (ComparisonZero 1).Should().BePositive()


    [<Fact>]
    let ``Passes if subject is positive and can be chained with And`` () =
        (1).Should().BePositive().Id<And<int>>().And.Be(1)


    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(-1)>]
    let ``Fails if subject is zero or negative`` (subject: int) =
        assertFails (fun () -> subject.Should().BePositive())


    [<Fact>]
    let ``Fails if subject is NaN`` () =
        assertFails (fun () -> Double.NaN.Should().BePositive())


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<ComparisonZero>.Should().BePositive())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 0
            x.Should().BePositive()
        |> assertExnMsg
            """
Subject: x
Should: BePositive
But was: 0
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 0
            x.Should().BePositive("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BePositive
But was: 0
"""


module BeNegative =


    [<Fact>]
    let ``Can be called with any type that has comparison and Zero`` () =
        (ComparisonZero -1).Should().BeNegative()


    [<Fact>]
    let ``Passes if subject is negative and can be chained with And`` () =
        (-1).Should().BeNegative().Id<And<int>>().And.Be(-1)


    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(1)>]
    let ``Fails if subject is zero or positive`` (subject: int) =
        assertFails (fun () -> subject.Should().BeNegative())


    [<Fact>]
    let ``Fails if subject is NaN`` () =
        assertFails (fun () -> Double.NaN.Should().BeNegative())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 0
            x.Should().BeNegative()
        |> assertExnMsg
            """
Subject: x
Should: BeNegative
But was: 0
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 0
            x.Should().BeNegative("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNegative
But was: 0
"""


module BeNonNegative =


    [<Fact>]
    let ``Can be called with any type that has comparison and Zero`` () =
        (ComparisonZero 0).Should().BeNonNegative()


    [<Fact>]
    let ``Can be chained with And`` () =
        (0).Should().BeNonNegative().Id<And<int>>().And.Be(0)


    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(1)>]
    let ``Passes if subject is zero or positive`` (subject: int) = subject.Should().BeNonNegative()


    [<Fact>]
    let ``Fails if subject is NaN`` () =
        assertFails (fun () -> Double.NaN.Should().BeNonNegative())


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<ComparisonZero>.Should().BeNonNegative())


    [<Fact>]
    let ``Fails with expected message if subject is negative`` () =
        fun () ->
            let x = -1
            x.Should().BeNonNegative()
        |> assertExnMsg
            """
Subject: x
Should: BeNonNegative
But was: -1
"""


    [<Fact>]
    let ``Fails with expected message with because if subject is negative`` () =
        fun () ->
            let x = -1
            x.Should().BeNonNegative("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNonNegative
But was: -1
"""


module BeNonPositive =


    [<Fact>]
    let ``Can be called with any type that has comparison and Zero`` () =
        (ComparisonZero 0).Should().BeNonPositive()


    [<Fact>]
    let ``Can be chained with And`` () =
        (0).Should().BeNonPositive().Id<And<int>>().And.Be(0)


    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(-1)>]
    let ``Passes if subject is zero or negative`` (subject: int) = subject.Should().BeNonPositive()


    [<Fact>]
    let ``Fails if subject is NaN`` () =
        assertFails (fun () -> Double.NaN.Should().BeNonPositive())


    [<Fact>]
    let ``Fails with expected message if subject is positive`` () =
        fun () ->
            let x = 1
            x.Should().BeNonPositive()
        |> assertExnMsg
            """
Subject: x
Should: BeNonPositive
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if subject is positive`` () =
        fun () ->
            let x = 1
            x.Should().BeNonPositive("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNonPositive
But was: 1
"""


module BeInRange =


    [<Theory>]
    [<InlineData("Direct")>]
    [<InlineData("NotSatisfy")>]
    [<InlineData("SatisfyAny")>]
    let ``Rejects inverted bounds as invalid input`` composition =
        for subject in [ Int32.MinValue; 1; Int32.MaxValue ] do
            assertInvalidArgumentRejected<ArgumentException>
                "upper"
                composition
                (fun () -> subject.Should().BeInRange(2, 0) |> ignore)


    [<Fact>]
    let ``Rejects inverted bounds for custom comparable types`` () =
        assertInvalidArgumentRejected<ArgumentException>
            "upper"
            "Direct"
            (fun () -> (Comparison 1).Should().BeInRange(Comparison 2, Comparison 0) |> ignore)


    [<Fact>]
    let ``NaN failure takes precedence over inverted bounds`` () =
        assertFails (fun () -> Double.NaN.Should().BeInRange(1.0, 0.0)) |> ignore
        assertFails (fun () -> Single.NaN.Should().BeInRange(1.0f, 0.0f)) |> ignore


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 0).Should().BeInRange(Comparison 0, Comparison 0)


    [<Fact>]
    let ``Can be chained with And`` () =
        (0).Should().BeInRange(0, 0).Id<And<int>>().And.Be(0)


    [<Theory>]
    [<InlineData(0, 0, 0)>] // Zero range
    [<InlineData(1, 0, 2)>] // Inside range
    [<InlineData(0, 0, 1)>] // At lower bound
    [<InlineData(1, 0, 1)>] // At upper bound
    let ``Passes if within range`` (subject: int) (lower: int) (upper: int) =
        subject.Should().BeInRange(lower, upper)


    [<Theory>]
    [<InlineData(0, 1, 2)>] // Below lower bound
    [<InlineData(2, 0, 1)>] // Above upper bound
    let ``Fails if outside range`` (subject: int) (lower: int) (upper: int) =
        assertFails (fun () -> subject.Should().BeInRange(lower, upper))


    [<Fact>]
    let ``Fails if any argument is NaN`` () =
        assertFails (fun () -> Double.NaN.Should().BeInRange(0.0, 1.0)) |> ignore
        assertFails (fun () -> 0.0.Should().BeInRange(Double.NaN, 1.0)) |> ignore
        assertFails (fun () -> 0.0.Should().BeInRange(0.0, Double.NaN)) |> ignore


    [<Fact>]
    let ``Rejects NaN in interface typed range arguments`` () =
        assertFails (fun () -> (0.0 :> IComparable).Should().BeInRange(Double.NaN, 1.0))
        |> ignore

        assertFails (fun () -> (0.0f :> IComparable).Should().BeInRange(Single.NaN, 1.0f))
        |> ignore


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<Comparison>.Should().BeInRange(Comparison 0, Comparison 0))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 0
            x.Should().BeInRange(1, 2)
        |> assertExnMsg
            """
Subject: x
Should: BeInRange
Lower: 1
Upper: 2
But was: 0
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 0
            x.Should().BeInRange(1, 2, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeInRange
Lower: 1
Upper: 2
But was: 0
"""
