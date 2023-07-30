module ComparisonAssertions

open System
open Faqt
open Xunit


module BeCloseTo =


    [<Fact>]
    let ``Passes for equal integers with zero tolerance and can be chained with And`` () =
        (1).Should().BeCloseTo(1, 0).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes above lower bound`` () = (-0.9).Should().BeCloseTo(0., 1.)


    [<Fact>]
    let ``Passes at lower bound`` () = (-1.0).Should().BeCloseTo(0., 1.)


    [<Fact>]
    let ``Fails below lower bound`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (-1.1).Should().BeCloseTo(0., 1.) |> ignore)


    [<Fact>]
    let ``Passes below upper bound`` () = (0.9).Should().BeCloseTo(0., 1.)


    [<Fact>]
    let ``Passes at upper bound`` () = (1.0).Should().BeCloseTo(0., 1.)


    [<Fact>]
    let ``Throws above upper bound`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (1.1).Should().BeCloseTo(0., 1.) |> ignore)


    [<Fact>]
    let ``Fails with expected message for floats`` () =
        fun () ->
            let x = 1.09
            x.Should().BeCloseTo(1.0, 0.05)
        |> assertExnMsg
            """
x
    should be
1.0 ± 0.05
    but was
1.09
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 5, 2)
            x.Should().BeCloseTo(TimeSpan(0, 5, 0), TimeSpan(0, 0, 1))
        |> assertExnMsg
            """
x
    should be
00:05:00 ± 00:00:01
    but was
00:05:02
"""


    [<Fact>]
    let ``Can be called with different types`` () =
        // TODO: Ideally, we should find an example with three different types instead of just two. Currently the subject/expected types would need to support subtraction both ways.
        DateTime(2000, 1, 2, 3, 4, 5)
            .Should()
            .BeCloseTo(DateTime(2000, 1, 2, 3, 4, 6), TimeSpan(0, 0, 1))


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1.09
            x.Should().BeCloseTo(1.0, 0.05, "some reason")
        |> assertExnMsg
            """
x
    should be
1.0 ± 0.05
    because some reason, but was
1.09
"""


module NotBeCloseTo =


    [<Fact>]
    let ``Fails for non-equal integers with zero tolerance and can be chained with And`` () =
        (2).Should().NotBeCloseTo(1, 0).Id<And<int>>().And.Be(2)


    [<Fact>]
    let ``Fails above lower bound`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (-0.9).Should().NotBeCloseTo(0., 1.) |> ignore)


    [<Fact>]
    let ``Fails at lower bound`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (-1.0).Should().NotBeCloseTo(0., 1.) |> ignore)


    [<Fact>]
    let ``Passes below lower bound`` () = (-1.1).Should().NotBeCloseTo(0., 1.)


    [<Fact>]
    let ``Fails below upper bound`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (0.9).Should().NotBeCloseTo(0., 1.) |> ignore)


    [<Fact>]
    let ``Fails at upper bound`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (1.0).Should().NotBeCloseTo(0., 1.) |> ignore)


    [<Fact>]
    let ``Passes above upper bound`` () = (1.1).Should().NotBeCloseTo(0., 1.)


    [<Fact>]
    let ``Fails with expected message for floats`` () =
        fun () ->
            let x = 1.02
            x.Should().NotBeCloseTo(1.0, 0.05)
        |> assertExnMsg
            """
x
    should not be
1.0 ± 0.05
    but was
1.02
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 5, 1)
            x.Should().NotBeCloseTo(TimeSpan(0, 5, 0), TimeSpan(0, 0, 2))
        |> assertExnMsg
            """
x
    should not be
00:05:00 ± 00:00:02
    but was
00:05:01
"""


    [<Fact>]
    let ``Can be called with different types`` () =
        // TODO: Ideally, we should find an example with three different types instead of just two. Currently the subject/expected types would need to support subtraction both ways.
        DateTime(2000, 1, 2, 3, 4, 5)
            .Should()
            .NotBeCloseTo(DateTime(2000, 1, 2, 3, 4, 7), TimeSpan(0, 0, 1))


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1.02
            x.Should().NotBeCloseTo(1.0, 0.05, "some reason")
        |> assertExnMsg
            """
x
    should not be
1.0 ± 0.05
    because some reason, but was
1.02
"""


module BeGreaterThan =


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BeGreaterThan(0).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > other`` () = (1).Should().BeGreaterThan(0)


    [<Fact>]
    let ``Fails if subject = other`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (0).Should().BeGreaterThan(0) |> ignore)


    [<Fact>]
    let ``Fails if subject < other`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (-1).Should().BeGreaterThan(0) |> ignore)


    [<Fact>]
    let ``Fails with expected message for ints`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThan(0)
        |> assertExnMsg
            """
x
    should be greater than
0
    but was
-1
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 4, 0)
            x.Should().BeGreaterThan(TimeSpan(0, 5, 0))
        |> assertExnMsg
            """
x
    should be greater than
00:05:00
    but was
00:04:00
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThan(0, "some reason")
        |> assertExnMsg
            """
x
    should be greater than
0
    because some reason, but was
-1
"""


module BeGreaterThanOrEqualTo =


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BeGreaterThanOrEqualTo(0).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > other`` () = (1).Should().BeGreaterThanOrEqualTo(0)


    [<Fact>]
    let ``Passes if subject = other`` () = (0).Should().BeGreaterThanOrEqualTo(0)


    [<Fact>]
    let ``Fails if subject < other`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (-1).Should().BeGreaterThanOrEqualTo(0) |> ignore)


    [<Fact>]
    let ``Fails with expected message for ints`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThanOrEqualTo(0)
        |> assertExnMsg
            """
x
    should be greater than or equal to
0
    but was
-1
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 4, 0)
            x.Should().BeGreaterThanOrEqualTo(TimeSpan(0, 5, 0))
        |> assertExnMsg
            """
x
    should be greater than or equal to
00:05:00
    but was
00:04:00
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = -1
            x.Should().BeGreaterThanOrEqualTo(0, "some reason")
        |> assertExnMsg
            """
x
    should be greater than or equal to
0
    because some reason, but was
-1
"""


module BeLessThan =


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeLessThan(0).Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < other`` () = (-1).Should().BeLessThan(0)


    [<Fact>]
    let ``Fails if subject = other`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (0).Should().BeLessThan(0) |> ignore)


    [<Fact>]
    let ``Fails if subject > other`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (1).Should().BeLessThan(0) |> ignore)


    [<Fact>]
    let ``Fails with expected message for ints`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThan(0)
        |> assertExnMsg
            """
x
    should be less than
0
    but was
1
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 5, 0)
            x.Should().BeLessThan(TimeSpan(0, 4, 0))
        |> assertExnMsg
            """
x
    should be less than
00:04:00
    but was
00:05:00
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThan(0, "some reason")
        |> assertExnMsg
            """
x
    should be less than
0
    because some reason, but was
1
"""


module BeLessThanOrEqualTo =


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeLessThanOrEqualTo(0).Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < other`` () = (-1).Should().BeLessThanOrEqualTo(0)


    [<Fact>]
    let ``Passes if subject = other`` () = (0).Should().BeLessThanOrEqualTo(0)


    [<Fact>]
    let ``Fails if subject > other`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (1).Should().BeLessThanOrEqualTo(0) |> ignore)


    [<Fact>]
    let ``Fails with expected message for ints`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThanOrEqualTo(0)
        |> assertExnMsg
            """
x
    should be less than or equal to
0
    but was
1
"""


    [<Fact>]
    let ``Fails with expected message for TimeSpan`` () =
        fun () ->
            let x = TimeSpan(0, 5, 0)
            x.Should().BeLessThanOrEqualTo(TimeSpan(0, 4, 0))
        |> assertExnMsg
            """
x
    should be less than or equal to
00:04:00
    but was
00:05:00
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeLessThanOrEqualTo(0, "some reason")
        |> assertExnMsg
            """
x
    should be less than or equal to
0
    because some reason, but was
1
"""


module BePositive =


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BePositive().Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > 0`` () = (1).Should().BePositive()


    [<Fact>]
    let ``Fails if subject = 0`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (0).Should().BePositive() |> ignore)


    [<Fact>]
    let ``Fails if subject < 0`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (-1).Should().BePositive() |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = -1
            x.Should().BePositive()
        |> assertExnMsg
            """
x
    should be positive, but was
-1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = -1
            x.Should().BePositive("some reason")
        |> assertExnMsg
            """
x
    should be positive because some reason, but was
-1
"""


module BeNegative =


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeNegative().Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < 0`` () = (-1).Should().BeNegative()


    [<Fact>]
    let ``Fails if subject = 0`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (0).Should().BeNegative() |> ignore)


    [<Fact>]
    let ``Fails if subject > 0`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (1).Should().BeNegative() |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().BeNegative()
        |> assertExnMsg
            """
x
    should be negative, but was
1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeNegative("some reason")
        |> assertExnMsg
            """
x
    should be negative because some reason, but was
1
"""


module BeNonNegative =


    [<Fact>]
    let ``Can be chained with And`` () =
        (1).Should().BeNonNegative().Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes if subject > 0`` () = (1).Should().BeNonNegative()


    [<Fact>]
    let ``Passes if subject = 0`` () = (0).Should().BeNonNegative()


    [<Fact>]
    let ``Fails if subject < 0`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (-1).Should().BeNonNegative() |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = -1
            x.Should().BeNonNegative()
        |> assertExnMsg
            """
x
    should be non-negative, but was
-1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = -1
            x.Should().BeNonNegative("some reason")
        |> assertExnMsg
            """
x
    should be non-negative because some reason, but was
-1
"""


module BeNonPositive =


    [<Fact>]
    let ``Can be chained with And`` () =
        (-1).Should().BeNonPositive().Id<And<int>>().And.Be(-1)


    [<Fact>]
    let ``Passes if subject < 0`` () = (-1).Should().BeNonPositive()


    [<Fact>]
    let ``Passes if subject = 0`` () = (0).Should().BeNonPositive()


    [<Fact>]
    let ``Fails if subject > 0`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (1).Should().BeNonPositive() |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().BeNonPositive()
        |> assertExnMsg
            """
x
    should be non-positive, but was
1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeNonPositive("some reason")
        |> assertExnMsg
            """
x
    should be non-positive because some reason, but was
1
"""


module BeInRange =


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
        Assert.Throws<AssertionFailedException>(fun () -> (4).Should().BeInRange(1, 3) |> ignore)


    [<Fact>]
    let ``Fails if subject is below the lower bound`` () =
        Assert.Throws<AssertionFailedException>(fun () -> (0).Should().BeInRange(1, 3) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = 1
            x.Should().BeInRange(2, 4)
        |> assertExnMsg
            """
x
    should be in the range
[2, 4]
    but was
1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().BeInRange(2, 4, "some reason")
        |> assertExnMsg
            """
x
    should be in the range
[2, 4]
    because some reason, but was
1
"""
