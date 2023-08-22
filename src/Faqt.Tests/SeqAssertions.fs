module SeqAssertions

open System
open Faqt
open Xunit


module AllSatisfy =


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
Subject: x
Should: AllSatisfy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<string> = null

            x
                .Should()
                .AllSatisfy((fun y -> y.Length.Should().Test(y.Length = 3)), "Some reason")

        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllSatisfy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if at least one of the items fail to satisfy the assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]
            x.Should().AllSatisfy(fun y -> y.Length.Should().Test(y.Length = 3))

        |> assertExnMsg
            """
Subject: x
Should: AllSatisfy
Failures:
- Index: 1
  Failure:
    Subject: y.Length
    Should: Test
- Index: 2
  Failure:
    Subject: y.Length
    Should: Test
Value: [asd, test, '1234']
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .AllSatisfy((fun y -> y.Length.Should().Test(y.Length = 3)), "Some reason")

        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllSatisfy
Failures:
- Index: 1
  Failure:
    Subject: y.Length
    Should: Test
- Index: 2
  Failure:
    Subject: y.Length
    Should: Test
Value: [asd, test, '1234']
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
Subject: x
Should: SatisfyRespectively
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<string> = null

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()) ], "Some reason")

        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: SatisfyRespectively
But was: null
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
Subject: x
Should: SatisfyRespectively
Expected length: 2
Actual length: 3
Value: [asd, test, '1234']
"""


    [<Fact>]
    let ``Fails with expected message if subject does not contain one item per assertion with because`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ], "Some reason")

        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: SatisfyRespectively
Expected length: 2
Actual length: 3
Value: [asd, test, '1234']
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
Subject: x
Should: SatisfyRespectively
Failures:
- Index: 0
  Failure:
    Subject: x1
    Should: Fail
- Index: 2
  Failure:
    Subject: x3
    Should: Fail
Value: [asd, test, '1234']
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
                    "Some reason"
                )

        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: SatisfyRespectively
Failures:
- Index: 0
  Failure:
    Subject: x1
    Should: Fail
- Index: 2
  Failure:
    Subject: x3
    Should: Fail
Value: [asd, test, '1234']
"""


module HaveLength =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().HaveLength(0).Id<And<int list>>().And.Be([])


    [<Fact>]
    let ``Passes if length = expected`` () = [ 1 ].Should().HaveLength(1)


    [<Fact>]
    let ``Throws ArgumentException if length is negative`` () =
        Assert.Throws<ArgumentException>(fun () -> [ 1 ].Should().HaveLength(-1) |> ignore)


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
            x.Should().HaveLength(0)
        |> assertExnMsg
            """
Subject: x
Should: HaveLength
Expected: 0
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().HaveLength(0, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveLength
Expected: 0
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if length does not match`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
Subject: x
Should: HaveLength
Expected: 1
But was: 0
Value: []
"""


    [<Fact>]
    let ``Fails with expected message if length does not match with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().HaveLength(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveLength
Expected: 1
But was: 0
Value: []
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
Subject: x
Should: BeEmpty
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeEmpty("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeEmpty
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = [ 1 ]
            x.Should().BeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: BeEmpty
But was: [1]
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = [ 1 ]
            x.Should().BeEmpty("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeEmpty
But was: [1]
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
Subject: x
Should: NotBeEmpty
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().NotBeEmpty("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBeEmpty
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if empty`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: NotBeEmpty
But was: []
"""


    [<Fact>]
    let ``Fails with expected message if empty with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().NotBeEmpty("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBeEmpty
But was: []
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
Subject: x
Should: BeNullOrEmpty
But was: [1]
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = seq { 1 }
            x.Should().BeNullOrEmpty("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeNullOrEmpty
But was: [1]
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
Subject: x
Should: Contain
Item: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().Contain(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Contain
Item: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not containing value`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().Contain(1)
        |> assertExnMsg
            """
Subject: x
Should: Contain
Item: 1
But was: []
"""


    [<Fact>]
    let ``Fails with expected message if not containing value with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().Contain(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Contain
Item: 1
But was: []
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
Subject: x
Should: NotContain
Item: 2
But was: [1, 2]
"""


    [<Fact>]
    let ``Fails with expected message if not containing the value with because`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().NotContain(2, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContain
Item: 2
But was: [1, 2]
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
Subject: x
Should: SequenceEqual
Expected: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if only subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().SequenceEqual([], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: SequenceEqual
Expected: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if different length`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().SequenceEqual([ 1; 2 ])
        |> assertExnMsg
            """
Subject: x
Should: SequenceEqual
Expected length: 2
Actual length: 3
Expected: [1, 2]
Actual: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message if different length with because`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().SequenceEqual([ 1; 2 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: SequenceEqual
Expected length: 2
Actual length: 3
Expected: [1, 2]
Actual: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal`` () =
        fun () ->
            let x = [ 1; 3; 2 ]
            x.Should().SequenceEqual([ 1; 2; 3 ])
        |> assertExnMsg
            """
Subject: x
Should: SequenceEqual
Failures:
- Index: 1
  Expected: 2
  Actual: 3
- Index: 2
  Expected: 3
  Actual: 2
Expected: [1, 2, 3]
Actual: [1, 3, 2]
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal with because`` () =
        fun () ->
            let x = [ 1; 3; 2 ]
            x.Should().SequenceEqual([ 1; 2; 3 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: SequenceEqual
Failures:
- Index: 1
  Expected: 2
  Actual: 3
- Index: 2
  Expected: 3
  Actual: 2
Expected: [1, 2, 3]
Actual: [1, 3, 2]
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
Subject: x
Should: ContainExactlyOneItem
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainExactlyOneItem("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainExactlyOneItem
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject contains more than one item`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().ContainExactlyOneItem()
        |> assertExnMsg
            """
Subject: x
Should: ContainExactlyOneItem
But length was: 2
Value: [1, 2]
"""


    [<Fact>]
    let ``Fails with expected message if subject contains more than one item with because`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().ContainExactlyOneItem("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainExactlyOneItem
But length was: 2
Value: [1, 2]
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
Subject: x
Should: ContainExactlyOneItemMatching
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainExactlyOneItemMatching((fun _ -> true), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainExactlyOneItemMatching
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if more than one item matches the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainExactlyOneItemMatching(fun x -> x < 3)
        |> assertExnMsg
            """
Subject: x
Should: ContainExactlyOneItemMatching
But found: 2
Matching items: [1, 2]
Value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message if more than one item matches the predicate with because`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainExactlyOneItemMatching((fun x -> x < 3), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainExactlyOneItemMatching
But found: 2
Matching items: [1, 2]
Value: [1, 2, 3]
"""


module ContainAtLeastOneItem =


    [<Fact>]
    let ``Can be chained with AndDerived with inner value`` () =
        [ 1; 2 ]
            .Should()
            .ContainAtLeastOneItem()
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(1)


    [<Fact>]
    let ``Passes if sequence contains a single item`` () = [ 1 ].Should().ContainAtLeastOneItem()


    [<Fact>]
    let ``Passes if sequence contains multiple items`` () =
        [ 1; 2 ].Should().ContainAtLeastOneItem()


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainAtLeastOneItem()
        |> assertExnMsg
            """
Subject: x
Should: ContainAtLeastOneItem
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainAtLeastOneItem("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainAtLeastOneItem
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is empty`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().ContainAtLeastOneItem()
        |> assertExnMsg
            """
Subject: x
Should: ContainAtLeastOneItem
But was: []
"""


    [<Fact>]
    let ``Fails with expected message if subject is empty with because`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().ContainAtLeastOneItem("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainAtLeastOneItem
But was: []
"""


module ContainAtLeastOneItemMatching =


    [<Fact>]
    let ``Can be chained with AndDerived with matched value`` () =
        [ 1; 2; 3 ]
            .Should()
            .ContainAtLeastOneItemMatching(fun x -> x > 1)
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(2)


    [<Fact>]
    let ``Passes if sequence contains a single item matching the predicate`` () =
        [ 1; 2 ].Should().ContainAtLeastOneItemMatching((=) 1)


    [<Fact>]
    let ``Passes if sequence contains multiple items matching the predicate`` () =
        [ 1; 2 ].Should().ContainAtLeastOneItemMatching(fun x -> x < 3)


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainAtLeastOneItemMatching(fun _ -> true)
        |> assertExnMsg
            """
Subject: x
Should: ContainAtLeastOneItemMatching
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainAtLeastOneItemMatching((fun _ -> true), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainAtLeastOneItemMatching
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if no items match the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainAtLeastOneItemMatching(fun x -> x > 3)
        |> assertExnMsg
            """
Subject: x
Should: ContainAtLeastOneItemMatching
But found: 0
Matching items: []
Value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message if no items match the predicate with because`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainAtLeastOneItemMatching((fun x -> x > 3), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainAtLeastOneItemMatching
But found: 0
Matching items: []
Value: [1, 2, 3]
"""


module ContainItemsMatching =


    [<Fact>]
    let ``Can be chained with AndDerived with matched values`` () =
        [ 1; 2; 3 ]
            .Should()
            .ContainItemsMatching(fun x -> x > 1)
            .Id<AndDerived<int list, seq<int>>>()
            .That.Should(())
            .SequenceEqual([ 2; 3 ])


    [<Fact>]
    let ``Passes if sequence contains a single item matching the predicate`` () =
        [ 1; 2 ].Should().ContainItemsMatching((=) 1)


    [<Fact>]
    let ``Passes if sequence contains multiple items matching the predicate`` () =
        [ 1; 2 ].Should().ContainItemsMatching(fun x -> x < 3)


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainItemsMatching(fun _ -> true)
        |> assertExnMsg
            """
Subject: x
Should: ContainItemsMatching
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().ContainItemsMatching((fun _ -> true), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainItemsMatching
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if no items match the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainItemsMatching(fun x -> x > 3)
        |> assertExnMsg
            """
Subject: x
Should: ContainItemsMatching
But found: 0
Value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message if no items match the predicate with because`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainItemsMatching((fun x -> x > 3), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainItemsMatching
But found: 0
Value: [1, 2, 3]
"""


module BeDistinct =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDistinct().Id<And<int list>>().And.Be([])


    [<Fact>]
    let ``Passes if distinct`` () = [ 1; 2; 3 ].Should().BeDistinct()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDistinct()
        |> assertExnMsg
            """
Subject: x
Should: BeDistinct
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDistinct("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDistinct
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not distinct`` () =
        fun () ->
            let x = [ 1; 2; 2; 2; 5; 5; 0 ]
            x.Should().BeDistinct()
        |> assertExnMsg
            """
Subject: x
Should: BeDistinct
Duplicates:
- Count: 3
  Item: 2
- Count: 2
  Item: 5
Value: [1, 2, 2, 2, 5, 5, 0]
"""


    [<Fact>]
    let ``Fails with expected message if not distinct with because`` () =
        fun () ->
            let x = [ 1; 2; 2; 2; 5; 5; 0 ]
            x.Should().BeDistinct("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDistinct
Duplicates:
- Count: 3
  Item: 2
- Count: 2
  Item: 5
Value: [1, 2, 2, 2, 5, 5, 0]
"""


module BeDistinctBy =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDistinctBy(id).Id<And<string list>>().And.Be([])


    [<Fact>]
    let ``Passes if distinct by the specified projection`` () =
        [ "a"; "as"; "asd" ].Should().BeDistinctBy(fun s -> s.Length)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDistinctBy(id)
        |> assertExnMsg
            """
Subject: x
Should: BeDistinctBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDistinctBy(id, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDistinctBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not distinct by the specified projection`` () =
        fun () ->
            let x = [ "a"; "as"; "asd"; "abc"; "b"; "foobar" ]
            x.Should().BeDistinctBy(fun s -> s.Length)
        |> assertExnMsg
            """
Subject: x
Should: BeDistinctBy
Duplicates:
- Count: 2
  Projected: 1
  Items: [a, b]
- Count: 2
  Projected: 3
  Items: [asd, abc]
Value: [a, as, asd, abc, b, foobar]
"""


    [<Fact>]
    let ``Fails with expected message if not distinct by the specified projection with because`` () =
        fun () ->
            let x = [ "a"; "as"; "asd"; "abc"; "b"; "foobar" ]
            x.Should().BeDistinctBy((fun s -> s.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDistinctBy
Duplicates:
- Count: 2
  Projected: 1
  Items: [a, b]
- Count: 2
  Projected: 3
  Items: [asd, abc]
Value: [a, as, asd, abc, b, foobar]
"""
