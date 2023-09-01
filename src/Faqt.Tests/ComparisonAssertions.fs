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
        (Comparison 2).Should().BeGreaterThan(Comparison 1)


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BeGreaterThan(0).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > other`` () = (1).Should().BeGreaterThan(0)


    [<Fact>]
    let ``Fails if subject = other`` () =
        assertFails (fun () -> (0).Should().BeGreaterThan(0))


    [<Fact>]
    let ``Fails if subject < other`` () =
        assertFails (fun () -> (-1).Should().BeGreaterThan(0))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<Comparison>
            x.Should().BeGreaterThan(Comparison 1)
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThan
Other: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for ints`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThan(0)
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThan
Other: 0
But was: -1
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 4, 0)
            x.Should().BeGreaterThan(TimeSpan(0, 5, 0))
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThan
Other: 00:05:00
But was: 00:04:00
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThan(0, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeGreaterThan
Other: 0
But was: -1
"""


module BeGreaterThanOrEqualTo =


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 2).Should().BeGreaterThanOrEqualTo(Comparison 1)


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BeGreaterThanOrEqualTo(0).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > other`` () = (1).Should().BeGreaterThanOrEqualTo(0)


    [<Fact>]
    let ``Passes if subject = other`` () = (0).Should().BeGreaterThanOrEqualTo(0)


    [<Fact>]
    let ``Fails if subject < other`` () =
        assertFails (fun () -> (-1).Should().BeGreaterThanOrEqualTo(0))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<Comparison>
            x.Should().BeGreaterThanOrEqualTo(Comparison 1)
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThanOrEqualTo
Other: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for ints`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThanOrEqualTo(0)
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThanOrEqualTo
Other: 0
But was: -1
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 4, 0)
            x.Should().BeGreaterThanOrEqualTo(TimeSpan(0, 5, 0))
        |> assertExnMsg
            """
Subject: x
Should: BeGreaterThanOrEqualTo
Other: 00:05:00
But was: 00:04:00
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThanOrEqualTo(0, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeGreaterThanOrEqualTo
Other: 0
But was: -1
"""


module BeLessThan =


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 1).Should().BeLessThan(Comparison 2)


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeLessThan(0).Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < other`` () = (-1).Should().BeLessThan(0)


    [<Fact>]
    let ``Fails if subject = other`` () =
        assertFails (fun () -> (0).Should().BeLessThan(0))


    [<Fact>]
    let ``Fails if subject > other`` () =
        assertFails (fun () -> (1).Should().BeLessThan(0))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<Comparison>
            x.Should().BeLessThan(Comparison 1)
        |> assertExnMsg
            """
Subject: x
Should: BeLessThan
Other: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for ints`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThan(0)
        |> assertExnMsg
            """
Subject: x
Should: BeLessThan
Other: 0
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 5, 0)
            x.Should().BeLessThan(TimeSpan(0, 4, 0))
        |> assertExnMsg
            """
Subject: x
Should: BeLessThan
Other: 00:04:00
But was: 00:05:00
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThan(0, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLessThan
Other: 0
But was: 1
"""


module BeLessThanOrEqualTo =


    [<Fact>]
    let ``Can be called with any type that has comparison`` () =
        (Comparison 1).Should().BeLessThanOrEqualTo(Comparison 2)


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeLessThanOrEqualTo(0).Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < other`` () = (-1).Should().BeLessThanOrEqualTo(0)


    [<Fact>]
    let ``Passes if subject = other`` () = (0).Should().BeLessThanOrEqualTo(0)


    [<Fact>]
    let ``Fails if subject > other`` () =
        assertFails (fun () -> (1).Should().BeLessThanOrEqualTo(0))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<Comparison>
            x.Should().BeLessThanOrEqualTo(Comparison 1)
        |> assertExnMsg
            """
Subject: x
Should: BeLessThanOrEqualTo
Other: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message for ints`` () =
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
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 5, 0)
            x.Should().BeLessThanOrEqualTo(TimeSpan(0, 4, 0))
        |> assertExnMsg
            """
Subject: x
Should: BeLessThanOrEqualTo
Other: 00:04:00
But was: 00:05:00
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
    let ``Passes if subject > 0`` () = (1).Should().BePositive()


    [<Fact>]
    let ``Fails if subject = 0`` () =
        assertFails (fun () -> (0).Should().BePositive())


    [<Fact>]
    let ``Fails if subject < 0`` () =
        assertFails (fun () -> (-1).Should().BePositive())


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<ComparisonZero>
            x.Should().BePositive()
        |> assertExnMsg
            """
Subject: x
Should: BePositive
But was: null
"""


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = -1
            x.Should().BePositive()
        |> assertExnMsg
            """
Subject: x
Should: BePositive
But was: -1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = -1
            x.Should().BePositive("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BePositive
But was: -1
"""


module BeNegative =


    [<Fact>]
    let ``Can be called with any type that has comparison and Zero`` () =
        (ComparisonZero -1).Should().BeNegative()


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeNegative().Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < 0`` () = (-1).Should().BeNegative()


    [<Fact>]
    let ``Fails if subject = 0`` () =
        assertFails (fun () -> (0).Should().BeNegative())


    [<Fact>]
    let ``Fails if subject > 0`` () =
        assertFails (fun () -> (1).Should().BeNegative())


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<ComparisonZero>
            x.Should().BeNegative()
        |> assertExnMsg
            """
Subject: x
Should: BeNegative
But was: null
"""


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().BeNegative()
        |> assertExnMsg
            """
Subject: x
Should: BeNegative
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeNegative("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNegative
But was: 1
"""


module BeNonNegative =


    [<Fact>]
    let ``Can be called with any type that has comparison and Zero`` () =
        (ComparisonZero 1).Should().BeNonNegative()


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BeNonNegative().Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > 0`` () = (1).Should().BeNonNegative()


    [<Fact>]
    let ``Passes if subject = 0`` () = (0).Should().BeNonNegative()


    [<Fact>]
    let ``Fails if subject < 0`` () =
        assertFails (fun () -> (-1).Should().BeNonNegative())


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<ComparisonZero>
            x.Should().BeNonNegative()
        |> assertExnMsg
            """
Subject: x
Should: BeNonNegative
But was: null
"""


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
        (ComparisonZero -1).Should().BeNonPositive()


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeNonPositive().Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < 0`` () = (-1).Should().BeNonPositive()


    [<Fact>]
    let ``Passes if subject = 0`` () = (0).Should().BeNonPositive()


    [<Fact>]
    let ``Fails if subject > 0`` () =
        assertFails (fun () -> (1).Should().BeNonPositive())


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<ComparisonZero>
            x.Should().BeNonPositive()
        |> assertExnMsg
            """
Subject: x
Should: BeNonPositive
But was: null
"""


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
        (Comparison 1).Should().BeInRange(Comparison 0, Comparison 2)


    [<Fact>]
    let ``Can be chained with And`` () =
        (2).Should().BeInRange(1, 3).Id<And<int>>().And.Be(2)


    [<Fact>]
    let ``Passes if subject is inside the range`` () = (2).Should().BeInRange(1, 3)


    [<Fact>]
    let ``Passes if subject is at the lower bound`` () = (1).Should().BeInRange(1, 3)


    [<Fact>]
    let ``Passes if subject is at the upper bound`` () = (3).Should().BeInRange(1, 3)


    [<Fact>]
    let ``Fails if subject is above the upper bound`` () =
        assertFails (fun () -> (4).Should().BeInRange(1, 3))


    [<Fact>]
    let ``Fails if subject is below the lower bound`` () =
        assertFails (fun () -> (0).Should().BeInRange(1, 3))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x = Unchecked.defaultof<Comparison>
            x.Should().BeInRange(Comparison 0, Comparison 2)
        |> assertExnMsg
            """
Subject: x
Should: BeInRange
Lower: 0
Upper: 2
But was: null
"""


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().BeInRange(2, 4)
        |> assertExnMsg
            """
Subject: x
Should: BeInRange
Lower: 2
Upper: 4
But was: 1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeInRange(2, 4, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeInRange
Lower: 2
Upper: 4
But was: 1
"""
