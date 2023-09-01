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
    let ``Fails if null`` () =
        assertFails (fun () ->
            Unchecked.defaultof<NumberWithoutOps>
                .Should()
                .BeCloseTo(NumberWithSubtraction 1, Comparison 0)
        )


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
        (NumberWithoutOps 0)
            .Should()
            .NotBeCloseTo(NumberWithSubtraction 1, Comparison 0)
        |> ignore

        (NumberWithSubtraction 0)
            .Should()
            .NotBeCloseTo(NumberWithoutOps 1, Comparison 0)
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
    let ``Passes if null`` () =
        Unchecked.defaultof<NumberWithoutOps>
            .Should()
            .NotBeCloseTo(NumberWithSubtraction 1, Comparison 0)


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
    let ``Can be chained with And`` () =
        (1).Should().BeGreaterThan(0).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > other`` () = (1).Should().BeGreaterThan(0)


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
    let ``Fails if subject < other`` () =
        assertFails (fun () -> (0).Should().BeGreaterThanOrEqualTo(1))


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<Comparison>.Should().BeGreaterThanOrEqualTo(Comparison 0))


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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
    let ``Can be chained with And`` () =
        (0).Should().BeLessThan(1).Id<And<int>>().And.Be(0)


    [<Fact>]
    let ``Passes if subject < other`` () = (0).Should().BeLessThan(1)


    [<Theory>]
    [<InlineData(0, 0)>] // subject = other
    [<InlineData(1, 0)>] // subject > other
    let ``Fails if subject >= other`` (subject: int) (other: int) =
        assertFails (fun () -> subject.Should().BeLessThan(other))


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<Comparison>.Should().BeLessThan(Comparison 0))


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
    let ``Fails if subject > other`` () =
        assertFails (fun () -> (1).Should().BeLessThanOrEqualTo(0))


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<Comparison>.Should().BeLessThanOrEqualTo(Comparison 0))


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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
    let ``Can be chained with And`` () =
        (1).Should().BePositive().Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject is positive`` () = (1).Should().BePositive()


    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(-1)>]
    let ``Fails if subject is zero or negative`` (subject: int) =
        assertFails (fun () -> subject.Should().BePositive())


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
    let ``Can be chained with And`` () =
        (-1).Should().BeNegative().Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject is negative`` () = (-1).Should().BeNegative()


    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(1)>]
    let ``Fails if subject is zero or positive`` (subject: int) =
        assertFails (fun () -> subject.Should().BeNegative())


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<ComparisonZero>.Should().BeNegative())


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
    let ``passes if subject is zero or positive`` (subject: int) = subject.Should().BeNonNegative()


    [<Fact>]
    let ``Fails if subject is negative`` () =
        assertFails (fun () -> (-1).Should().BeNonNegative())


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<ComparisonZero>.Should().BeNonNegative())


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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
    let ``passes if subject is zero or negative`` (subject: int) = subject.Should().BeNonPositive()


    [<Fact>]
    let ``Fails if subject is positive`` () =
        assertFails (fun () -> (1).Should().BeNonPositive())


    [<Fact>]
    let ``Fails if null`` () =
        assertFails (fun () -> Unchecked.defaultof<ComparisonZero>.Should().BeNonPositive())


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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
