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
