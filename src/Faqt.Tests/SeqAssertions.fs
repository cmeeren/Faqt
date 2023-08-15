module SeqAssertions

open Faqt
open Xunit


module SatisfyAll =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        [ "asd"; "123" ]
            .Should()
            .AllSatisfy(fun x -> x.Should().Pass())
            .Id<And<string list>>()
            .And.Be([ "asd"; "123" ])


    [<Fact>]
    let ``Passes if subject is empty`` () =
        List<int>.Empty.Should().AllSatisfy(fun x -> x.Should().Fail())


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<string> = null
            x.Should().AllSatisfy(fun y -> y.Length.Should().Test(y.Length = 3))

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<string> = null

            x
                .Should()
                .AllSatisfy((fun y -> y.Length.Should().Test(y.Length = 3)), "some reason")

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if at least one of the items fail to satisfy the assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]
            x.Should().AllSatisfy(fun y -> y.Length.Should().Test(y.Length = 3))

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion, but 2 of 3 items failed.

[Item 2/3]

y.Length

[Item 3/3]

y.Length
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .AllSatisfy((fun y -> y.Length.Should().Test(y.Length = 3)), "some reason")

        |> assertExnMsg
            """
x
    should only contain items satisfying the supplied assertion because some reason, but 2 of 3 items failed.

[Item 2/3]

y.Length

[Item 3/3]

y.Length
"""


module SatisfyRespectively =


    [<Fact>]
    let ``Passes if same length and all of the inner assertions passes and can be chained with And`` () =
        [ "asd"; "123" ]
            .Should()
            .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])
            .Id<And<string list>>()
            .And.Be([ "asd"; "123" ])


    [<Fact>]
    let ``Passes if subject and assertions are empty`` () =
        List<int>.Empty.Should().SatisfyRespectively([])


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<string> = null

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()) ])

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<string> = null

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()) ], "some reason")

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject does not contain one item per assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the
2
    specified assertions, but actual length was
3

["asd"; "test"; "1234"]
"""


    [<Fact>]
    let ``Fails with expected message if subject does not contain one item per assertion with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ], "some reason")

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the
2
    specified assertions because some reason, but actual length was
3

["asd"; "test"; "1234"]
"""


    [<Fact>]
    let ``Fails with expected message if at least one of the items fail to satisfy the assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively(
                    [
                        (fun x1 -> x1.Should().Fail())
                        (fun x2 -> x2.Should().Pass())
                        (fun x3 -> x3.Should().Fail())
                    ]
                )

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions, but 2 of 3 items failed.

[Item 1/3]

x1

[Item 3/3]

x3
"""


    [<Fact>]
    let ``Fails with expected message if at least one of the items fail to satisfy the assertion with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively(
                    [
                        (fun x1 -> x1.Should().Fail())
                        (fun x2 -> x2.Should().Pass())
                        (fun x3 -> x3.Should().Fail())
                    ],
                    "some reason"
                )

        |> assertExnMsg
            """
x
    should contain items respectively satisfying the specified assertions because some reason, but 2 of 3 items failed.

[Item 1/3]

x1

[Item 3/3]

x3
"""


module HaveLength =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().HaveLength(0).Id<And<int list>>().And.Be([])


    [<Fact>]
    let ``Passes if length = expected`` () = [ 1 ].Should().HaveLength(1)


    [<Fact>]
    let ``Fails if length < expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> List<int>.Empty.Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails if length > expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> [ 1; 2 ].Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().HaveLength(1, "some reason")
        |> assertExnMsg
            """
x
    should have length
1
    because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if length does not match`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but length was
0

[]
"""


    [<Fact>]
    let ``Fails with expected message if length does not match with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().HaveLength(1, "some reason")
        |> assertExnMsg
            """
x
    should have length
1
    because some reason, but length was
0

[]
"""


module BeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        List<int>.Empty.Should().BeEmpty().Id<And<int list>>().And.Be([])


    [<Fact>]
    let ``Passes if subject is empty`` () = List<int>.Empty.Should().BeEmpty()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = [ 1 ]
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
[1]
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = [ 1 ]
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
[1]
"""


module NotBeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().NotBeEmpty().Id<And<int list>>().And.Be([ 1 ])


    [<Fact>]
    let ``Passes if subject is not empty`` () = [ 1 ].Should().NotBeEmpty()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
x
    should not be empty, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().NotBeEmpty("some reason")
        |> assertExnMsg
            """
x
    should not be empty because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if empty`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
x
    should not be empty, but was empty.
"""


    [<Fact>]
    let ``Fails with expected message if empty with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().NotBeEmpty("some reason")
        |> assertExnMsg
            """
x
    should not be empty because some reason, but was empty.
"""


module BeNullOrEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        Seq.empty<int>.Should().BeNullOrEmpty().Id<And<seq<int>>>().And.Be(Seq.empty)


    [<Fact>]
    let ``Passes if subject is empty`` () = Seq.empty<int>.Should().BeNullOrEmpty()


    [<Fact>]
    let ``Passes if subject is null`` () =
        (null: seq<int>).Should().BeNullOrEmpty()


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = seq { 1 }
            x.Should().BeNullOrEmpty()
        |> assertExnMsg
            """
x
    should be null or empty, but was
seq [1]
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = seq { 1 }
            x.Should().BeNullOrEmpty("some reason")
        |> assertExnMsg
            """
x
    should be null or empty because some reason, but was
seq [1]
"""


module Contain =


    [<Fact>]
    let ``Can be chained with AndDerived with found value`` () =
        [ 1 ].Should().Contain(1).Id<AndDerived<int list, int>>().That.Should().Be(1)


    [<Fact>]
    let ``Passes if sequence contains value`` () = [ 1 ].Should().Contain(1)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().Contain(1)
        |> assertExnMsg
            """
x
    should contain
1
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().Contain(1, "some reason")
        |> assertExnMsg
            """
x
    should contain
1
    because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if not containing value`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().Contain(1)
        |> assertExnMsg
            """
x
    should contain
1
    but was
[]
"""


    [<Fact>]
    let ``Fails with expected message if not containing value with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().Contain(1, "some reason")
        |> assertExnMsg
            """
x
    should contain
1
    because some reason, but was
[]
"""


module NotContain =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().NotContain(2).Id<And<int list>>().And.Be([ 1 ])


    [<Fact>]
    let ``Passes if sequence does not contain the value`` () = [ 1 ].Should().NotContain(2)


    [<Fact>]
    let ``Passes if subject is null`` () = (null: seq<int>).Should().NotContain(2)


    [<Fact>]
    let ``Fails with expected message if not containing the value`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().NotContain(2)
        |> assertExnMsg
            """
x
    should not contain
2
    but was
[1; 2]
"""


    [<Fact>]
    let ``Fails with expected message if not containing the value with because`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().NotContain(2, "some reason")
        |> assertExnMsg
            """
x
    should not contain
2
    because some reason, but was
[1; 2]
"""


module SequenceEqual =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().SequenceEqual([ 1 ]).Id<And<int list>>().And.Be([ 1 ])


    [<Fact>]
    let ``Passes if sequence contains all values in order`` () =
        let x = ResizeArray()
        x.AddRange([ 1; 2; 1; 3; 2 ])
        x.Should().SequenceEqual([ 1; 2; 1; 3; 2 ])


    [<Fact>]
    let ``Passes if both subject and expected is null`` () =
        (null: seq<int>).Should().SequenceEqual(null)


    [<Fact>]
    let ``Fails with expected message if only subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().SequenceEqual([])
        |> assertExnMsg
            """
x
    should be sequence equal to
[]
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if only subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().SequenceEqual([], "some reason")
        |> assertExnMsg
            """
x
    should be sequence equal to
[]
    because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if different length`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().SequenceEqual([ 1; 2 ])
        |> assertExnMsg
            """
x
    should be sequence equal to
[1; 2]
    but expected length
2
    is different from actual length
3

[1; 2; 3]
"""


    [<Fact>]
    let ``Fails with expected message if different length with because`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().SequenceEqual([ 1; 2 ], "some reason")
        |> assertExnMsg
            """
x
    should be sequence equal to
[1; 2]
    because some reason, but expected length
2
    is different from actual length
3

[1; 2; 3]
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal`` () =
        fun () ->
            let x = [ 1; 3; 2 ]
            x.Should().SequenceEqual([ 1; 2; 3 ])
        |> assertExnMsg
            """
x
    should be sequence equal to
[1; 2; 3]
    but actual item at index 1
3
    is not equal to expected item
2
    Full sequence:
[1; 3; 2]
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal with because`` () =
        fun () ->
            let x = [ 1; 3; 2 ]
            x.Should().SequenceEqual([ 1; 2; 3 ], "some reason")
        |> assertExnMsg
            """
x
    should be sequence equal to
[1; 2; 3]
    because some reason, but actual item at index 1
3
    is not equal to expected item
2
    Full sequence:
[1; 3; 2]
"""


module ContainExactlyOneItem =


    [<Fact>]
    let ``Can be chained with AndDerived with inner value`` () =
        [ 1 ]
            .Should()
            .ContainExactlyOneItem()
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(1)


    [<Fact>]
    let ``Passes if sequence contains a single item`` () = [ 1 ].Should().ContainExactlyOneItem()


    [<Fact>]
    let ``Fails if empty`` () =
        Assert.Throws<AssertionFailedException>(fun () -> List<int>.Empty.Should().ContainExactlyOneItem() |> ignore)


    [<Fact>]
    let ``Fails if more than one item`` () =
        Assert.Throws<AssertionFailedException>(fun () -> [ 1; 2 ].Should().ContainExactlyOneItem() |> ignore)


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainExactlyOneItem()
        |> assertExnMsg
            """
x
    should contain exactly one item, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainExactlyOneItem("some reason")
        |> assertExnMsg
            """
x
    should contain exactly one item because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject contains more than one item`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().ContainExactlyOneItem()
        |> assertExnMsg
            """
x
    should contain exactly one item, but actual length was
2

[1; 2]
"""


    [<Fact>]
    let ``Fails with expected message if subject contains more than one item with because`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().ContainExactlyOneItem("some reason")
        |> assertExnMsg
            """
x
    should contain exactly one item because some reason, but actual length was
2

[1; 2]
"""


module ContainExactlyOneItemMatching =


    [<Fact>]
    let ``Can be chained with AndDerived with matched value`` () =
        [ 1; 2 ]
            .Should()
            .ContainExactlyOneItemMatching((=) 2)
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(2)


    [<Fact>]
    let ``Passes if sequence contains a single item matching the predicate`` () =
        [ 1; 2 ].Should().ContainExactlyOneItemMatching((=) 1)


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainExactlyOneItemMatching(fun _ -> true)
        |> assertExnMsg
            """
x
    should contain exactly one item matching the specified predicate, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainExactlyOneItemMatching((fun _ -> true), "some reason")
        |> assertExnMsg
            """
x
    should contain exactly one item matching the specified predicate because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if more than one item matches the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainExactlyOneItemMatching(fun x -> x < 3)
        |> assertExnMsg
            """
x
    should contain exactly one item matching the specified predicate, but found
2
    items matching the predicate:
seq [1; 2]
    Full sequence:
[1; 2; 3]
"""


    [<Fact>]
    let ``Fails with expected message if more than one item matches the predicate with because`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainExactlyOneItemMatching((fun x -> x < 3), "some reason")
        |> assertExnMsg
            """
x
    should contain exactly one item matching the specified predicate because some reason, but found
2
    items matching the predicate:
seq [1; 2]
    Full sequence:
[1; 2; 3]
"""
