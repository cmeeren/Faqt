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
            x.Should().AllSatisfy(fun _ -> failwith "unreachable")
        |> assertExnMsg
            """
Subject: x
Should: AllSatisfy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if subject is null`` () =
        fun () ->
            let x: seq<string> = null
            x.Should().AllSatisfy((fun _ -> failwith "unreachable"), "Some reason")
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
Subject value: [asd, test, '1234']
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fail to satisfy the assertion`` () =
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
Subject value: [asd, test, '1234']
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
    let ``Throws ArgumentNullException if assertions is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> List<int>.Empty.Should().SatisfyRespectively(null) |> ignore)


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
    let ``Fails with expected message with because if subject is null`` () =
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
Subject value: [asd, test, '1234']
"""


    [<Fact>]
    let ``Fails with expected message with because if subject does not contain one item per assertion`` () =
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
Subject value: [asd, test, '1234']
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
Subject value: [asd, test, '1234']
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fail to satisfy the assertion`` () =
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
Subject value: [asd, test, '1234']
"""


module HaveLength =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().HaveLength(0).Id<And<int list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| box List<int>.Empty; 0 |]
        [| [ 1 ]; 1 |]
        [| [ 1; 2 ]; 2 |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if length = expected`` (subject: seq<int>) (expected: int) = subject.Should().HaveLength(expected)


    let failData = [
        // Comment to force break for readability
        [| box List<int>.Empty; 1 |]
        [| [ 1 ]; 2 |]
        [| [ 1; 2 ]; 0 |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if length <> expected`` (subject: seq<int>) (expected: int) =
        assertFails (fun () -> subject.Should().HaveLength(expected))


    [<Fact>]
    let ``Throws ArgumentException if length is negative`` () =
        Assert.Throws<ArgumentException>(fun () -> [ 1 ].Should().HaveLength(-1) |> ignore)


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
    let ``Fails with expected message with because if null`` () =
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
Subject value: []
"""


    [<Fact>]
    let ``Fails with expected message with because if length does not match`` () =
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
Subject value: []
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
    let ``Fails with expected message with because if null`` () =
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
    let ``Fails with expected message with because if not empty`` () =
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
    let ``Fails with expected message with because if empty`` () =
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
    let ``Passes if subject is null`` () =
        (null: seq<int>).Should().BeNullOrEmpty()


    [<Fact>]
    let ``Passes if subject is empty`` () = Seq.empty<int>.Should().BeNullOrEmpty()


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
    let ``Fails with expected message with because if not empty`` () =
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


    let passData = [
        [| box [ "a" ]; "a" |]
        [| [ "a"; "b" ]; "a" |]
        [| [ (null: string) ]; (null: string) |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if sequence contains value`` (subject: seq<string>) (value: string) = subject.Should().Contain(value)


    let failData = [
        [| box<seq<string>> null; "a" |]
        [| List<string>.Empty; "a" |]
        [| [ "a" ]; "b" |]
        [| [ (null: string) ]; "a" |]
        [| [ "a" ]; (null: string) |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if null or not containing value`` (subject: seq<string>) (value: string) =
        assertFails (fun () -> subject.Should().Contain(value))


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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


    let passData = [
        [| box<seq<string>> null; "a" |]
        [| List<string>.Empty; "a" |]
        [| [ "a" ]; "b" |]
        [| [ (null: string) ]; "a" |]
        [| [ "a" ]; (null: string) |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if null or not containing value`` (subject: seq<string>) (value: string) =
        subject.Should().NotContain(value)


    let failData = [
        [| box [ "a" ]; "a" |]
        [| [ "a"; "b" ]; "a" |]
        [| [ (null: string) ]; (null: string) |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if sequence contains value`` (subject: seq<string>) (value: string) =
        assertFails (fun () -> subject.Should().NotContain(value))


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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


module AllBe =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().AllBe(1).Id<And<int list>>().And.Be([ 1 ])


    let passData = [
        [| box List<string>.Empty; "a" |]
        [| [ "a" ]; "a" |]
        [| [ "a"; "a" ]; "a" |]
        [| [ (null: string) ]; (null: string) |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if all items are equal to the specified value`` (subject: seq<string>) (expected: string) =
        subject.Should().AllBe(expected)


    let failData = [
        [| box<seq<string>> null; "a" |]
        [| [ "a" ]; "b" |]
        [| [ "a"; "b" ]; "a" |]
        [| [ "a"; null ]; (null: string) |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if null or not all items are equal to the specified value`` (subject: seq<string>) (expected: string) =
        assertFails (fun () -> subject.Should().AllBe(expected)) |> ignore


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().AllBe(1)
        |> assertExnMsg
            """
Subject: x
Should: AllBe
Expected: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if subject is null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().AllBe(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllBe
Expected: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal`` () =
        fun () ->
            let x = [ 1; 3; 2 ]
            x.Should().AllBe(3)
        |> assertExnMsg
            """
Subject: x
Should: AllBe
Expected: 3
Failures:
- Index: 0
  Value: 1
- Index: 2
  Value: 2
Subject value: [1, 3, 2]
"""


    [<Fact>]
    let ``Fails with expected message with because if items are not equal`` () =
        fun () ->
            let x = [ 1; 3; 2 ]
            x.Should().AllBe(3, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllBe
Expected: 3
Failures:
- Index: 0
  Value: 1
- Index: 2
  Value: 2
Subject value: [1, 3, 2]
"""


module ``AllBe with projection`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ "a" ]
            .Should()
            .AllBeMappedTo(1, (fun x -> x.Length))
            .Id<And<string list>>()
            .And.Be([ "a" ])


    let passData = [
        [| box List<string>.Empty; 1 |]
        [| [ "a" ]; 1 |]
        [| [ "a"; "a" ]; 1 |]
        [| [ "ab"; "cd" ]; 2 |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if all items are equal to the specified value when projected`` (subject: seq<string>) (expected: int) =
        subject.Should().AllBeMappedTo(expected, (fun x -> x.Length))


    let failData = [
        // Comment to force break
        [| box<seq<string>> null; 1 |]
        [| [ "a" ]; 2 |]
        [| [ "a"; "ab" ]; 1 |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if null or not all items are equal to the specified value when projected``
        (subject: seq<string>)
        (expected: int)
        =
        assertFails (fun () -> subject.Should().AllBeMappedTo(expected, (fun x -> x.Length)))
        |> ignore


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: seq<string> = null
            x.Should().AllBeMappedTo(1, (fun x -> x.Length))
        |> assertExnMsg
            """
Subject: x
Should: AllBeMappedTo
Expected: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if subject is null`` () =
        fun () ->
            let x: seq<string> = null
            x.Should().AllBeMappedTo(1, (fun x -> x.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllBeMappedTo
Expected: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal when projected`` () =
        fun () ->
            let x = [ "a"; "ab"; "abc" ]
            x.Should().AllBeMappedTo(2, (fun x -> x.Length))
        |> assertExnMsg
            """
Subject: x
Should: AllBeMappedTo
Expected: 2
Failures:
- Index: 0
  Projected: 1
  Value: a
- Index: 2
  Projected: 3
  Value: abc
Subject value: [a, ab, abc]
"""


    [<Fact>]
    let ``Fails with expected message with because if items are not equal when projected`` () =
        fun () ->
            let x = [ "a"; "ab"; "abc" ]
            x.Should().AllBeMappedTo(2, (fun x -> x.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllBeMappedTo
Expected: 2
Failures:
- Index: 0
  Projected: 1
  Value: a
- Index: 2
  Projected: 3
  Value: abc
Subject value: [a, ab, abc]
"""


module SequenceEqual =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().SequenceEqual([ 1 ]).Id<And<int list>>().And.Be([ 1 ])


    let passData = [
        [| box null; null |]
        [| List<string>.Empty; List<string>.Empty |]
        [| [ "a" ]; [ "a" ] |]
        [| [ "a"; null ]; [ "a"; null ] |]
        [| [ "a"; "b" ]; [ "a"; "b" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if both are null or have the same items in the same order``
        (subject: seq<string>)
        (expected: seq<string>)
        =
        subject.Should().SequenceEqual(expected)


    let failData = [
        [| box null; List<string>.Empty |]
        [| List<string>.Empty; [ "a" ] |]
        [| [ "a" ]; [ "a"; null ] |]
        [| [ "a" ]; [ "b" ] |]
        [| [ "a"; "b" ]; [ "b"; "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing the same items in the same order`` (a: seq<string>) (b: seq<string>) =
        assertFails (fun () -> a.Should().SequenceEqual(b)) |> ignore
        assertFails (fun () -> b.Should().SequenceEqual(a)) |> ignore


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
    let ``Fails with expected message with because if only subject is null`` () =
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
    let ``Fails with expected message if only expected is null`` () =
        fun () ->
            let x = List<int>.Empty
            x.Should().SequenceEqual(null)
        |> assertExnMsg
            """
Subject: x
Should: SequenceEqual
Expected: null
But was: []
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
    let ``Fails with expected message with because if different length`` () =
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
    let ``Fails with expected message with because if items are not equal`` () =
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


module HaveSameItemsAs =


    [<Fact>]
    let ``Can be chained with And`` () =
        Map.empty<string, int>
            .Should()
            .HaveSameItemsAs(Map.empty<string, int>)
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty<string, int>)


    let passData = [
        [| box null; null |]
        [| List<string>.Empty; List<string>.Empty |]
        [| [ "a" ]; [ "a" ] |]
        [| [ "a"; "b" ]; [ "a"; "b" ] |]
        [| [ "a"; "b" ]; [ "b"; "a" ] |]
        [| [ "a"; (null: string) ]; [ (null: string); "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if both are null or contain the same values`` (subject: seq<string>) (expected: seq<string>) =
        subject.Should().HaveSameItemsAs(expected)


    let failData = [
        [| box null; List<string>.Empty |]
        [| List<string>.Empty; [ "a" ] |]
        [| [ "a" ]; [ "a"; "a" ] |]
        [| [ "a" ]; [ "a"; "b" ] |]
        [| [ "a" ]; [ "b" ] |]
        [| [ "a" ]; [ (null: string) ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if only one is null or they do not contain the same values`` (a: seq<string>) (b: seq<string>) =
        assertFails (fun () -> a.Should().HaveSameItemsAs(b)) |> ignore
        assertFails (fun () -> b.Should().HaveSameItemsAs(a))


    [<Fact>]
    let ``Fails with expected message if only subject is null`` () =
        fun () ->
            let x: seq<string> = null
            x.Should().HaveSameItemsAs([])
        |> assertExnMsg
            """
Subject: x
Should: HaveSameItemsAs
Expected: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if only expected is null`` () =
        fun () ->
            let x = List<string>.Empty
            x.Should().HaveSameItemsAs(null)
        |> assertExnMsg
            """
Subject: x
Should: HaveSameItemsAs
Expected: null
But was: []
"""


    [<Fact>]
    let ``Fails with expected message with because if only subject is null`` () =
        fun () ->
            let x: seq<string> = null
            x.Should().HaveSameItemsAs([], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveSameItemsAs
Expected: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal with duplicates`` () =
        fun () ->
            let x = [ 7; 1; 3; 1; 2; 5; 4; 2 ]
            x.Should().HaveSameItemsAs([ 1; 3; 3; 5; 4; 9; 2 ])
        |> assertExnMsg
            """
Subject: x
Should: HaveSameItemsAs
Missing items: [3, 9]
Additional items: [7, 1, 2]
Expected: [1, 3, 3, 5, 4, 9, 2]
Actual: [7, 1, 3, 1, 2, 5, 4, 2]
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = [ 1 ]
            x.Should().HaveSameItemsAs([ 1; 2 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveSameItemsAs
Missing items: [2]
Additional items: []
Expected: [1, 2]
Actual: [1]
"""


module ContainExactlyOneItem =


    [<Fact>]
    let ``Passes if sequence contains exactly one item and can be chained with AndDerived with inner value`` () =
        [ 1 ]
            .Should()
            .ContainExactlyOneItem()
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(1)


    let failData = [
        // Comment to force break for readability
        [| box null |]
        [| List<string>.Empty |]
        [| [ "a"; "b" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing exactly one item`` (subject: seq<string>) =
        assertFails (fun () -> subject.Should().ContainExactlyOneItem()) |> ignore


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
    let ``Fails with expected message with because if subject is null`` () =
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
Subject value: [1, 2]
"""


    [<Fact>]
    let ``Fails with expected message with because if subject contains more than one item`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().ContainExactlyOneItem("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainExactlyOneItem
But length was: 2
Subject value: [1, 2]
"""


module ContainExactlyOneItemMatching =


    [<Fact>]
    let ``Passes if sequence contains exactly one item matching the predicate and can be chained with AndDerived with matched value``
        ()
        =
        [ 1; 2 ]
            .Should()
            .ContainExactlyOneItemMatching((=) 2)
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(2)


    let failData = [
        // Comment to force break for readability
        [| box null |]
        [| List<int>.Empty |]
        [| [ 1; 1 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing exactly one item matching the predicate`` (subject: seq<int>) =
        assertFails (fun () -> subject.Should().ContainExactlyOneItemMatching((=) 1))
        |> ignore


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
    let ``Fails with expected message with because if subject is null`` () =
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
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if more than one item matches the predicate`` () =
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
Subject value: [1, 2, 3]
"""


module ContainAtLeastOneItem =


    [<Fact>]
    let ``Can be chained with AndDerived with first value`` () =
        [ 1; 2 ]
            .Should()
            .ContainAtLeastOneItem()
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(1)


    let passData = [
        // Comment to force break for readability
        [| [ 1 ] |]
        [| [ 1; 2 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if contains at least one item`` (subject: seq<int>) =
        subject.Should().ContainAtLeastOneItem()


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
    let ``Fails with expected message with because if subject is null`` () =
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
    let ``Fails with expected message with because if subject is empty`` () =
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
    let ``Can be chained with AndDerived with first matched value`` () =
        [ 1; 2; 3 ]
            .Should()
            .ContainAtLeastOneItemMatching(fun x -> x > 1)
            .Id<AndDerived<int list, int>>()
            .That.Should(())
            .Be(2)


    let passData = [
        // Comment to force break for readability
        [| [ 1 ] |]
        [| [ 1; 2 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if contains at least one item matching the predicate`` (subject: seq<int>) =
        subject.Should().ContainAtLeastOneItemMatching(fun x -> x < 3)


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
    let ``Fails with expected message with because if subject is null`` () =
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
    let ``Fails with expected message if no item matches the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainAtLeastOneItemMatching(fun x -> x > 3)
        |> assertExnMsg
            """
Subject: x
Should: ContainAtLeastOneItemMatching
But found: 0
Matching items: []
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if no item matches the predicate`` () =
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
Subject value: [1, 2, 3]
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


    let passData = [
        // Comment to force break for readability
        [| [ 1 ] |]
        [| [ 1; 2 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if contains at least one item matching the predicate`` (subject: seq<int>) =
        subject.Should().ContainItemsMatching(fun x -> x < 3)


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
    let ``Fails with expected message with because if subject is null`` () =
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
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if no items match the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainItemsMatching((fun x -> x > 3), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainItemsMatching
But found: 0
Subject value: [1, 2, 3]
"""


module BeDistinct =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDistinct().Id<And<int list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<int>.Empty |]
        [| [ 1 ] |]
        [| [ 1; 2 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if distinct`` (subject: seq<int>) = subject.Should().BeDistinct()


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
    let ``Fails with expected message with because if null`` () =
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
Subject value: [1, 2, 2, 2, 5, 5, 0]
"""


    [<Fact>]
    let ``Fails with expected message with because if not distinct`` () =
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
Subject value: [1, 2, 2, 2, 5, 5, 0]
"""


module BeDistinctBy =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDistinctBy(id).Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<string>.Empty |]
        [| [ "a" ] |]
        [| [ "a"; "as"; "asd" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if distinct by the specified projection`` (subject: seq<string>) =
        subject.Should().BeDistinctBy(fun s -> s.Length)


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
    let ``Fails with expected message with because if null`` () =
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
Subject value: [a, as, asd, abc, b, foobar]
"""


    [<Fact>]
    let ``Fails with expected message with because if not distinct by the specified projection`` () =
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
Subject value: [a, as, asd, abc, b, foobar]
"""


module BeAscending =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeAscending().Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<int>.Empty |]
        [| [ 1 ] |]
        [| [ 1; 1 ] |]
        [| [ 1; 2 ] |]
        [| [ 1; 3 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly ascending`` (subject: seq<int>) = subject.Should().BeAscending()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeAscending()
        |> assertExnMsg
            """
Subject: x
Should: BeAscending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeAscending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not in ascending order`` () =
        fun () ->
            let x = [ 1; 2; 6; 3; 1; 3 ]
            x.Should().BeAscending()
        |> assertExnMsg
            """
Subject: x
Should: BeAscending
But found:
- Index: 2
  Item: 6
- Index: 3
  Item: 3
Subject value: [1, 2, 6, 3, 1, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in ascending order`` () =
        fun () ->
            let x = [ 1; 2; 6; 3; 1; 3 ]
            x.Should().BeAscending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscending
But found:
- Index: 2
  Item: 6
- Index: 3
  Item: 3
Subject value: [1, 2, 6, 3, 1, 3]
"""


module BeAscendingBy =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeAscendingBy(id).Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<string>.Empty |]
        [| [ "a" ] |]
        [| [ "a"; "a" ] |]
        [| [ "a"; "as"; "baz"; "asd" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly ascending by the specified projection`` (subject: seq<string>) =
        subject.Should().BeAscendingBy(fun s -> s.Length)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeAscendingBy(id)
        |> assertExnMsg
            """
Subject: x
Should: BeAscendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeAscendingBy(id, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not in ascending order by the specified projection`` () =
        fun () ->
            let x = [ "a"; "as"; "foobar"; "asd"; "a"; "bar" ]
            x.Should().BeAscendingBy(fun s -> s.Length)
        |> assertExnMsg
            """
Subject: x
Should: BeAscendingBy
But found:
- Index: 2
  Item: foobar
  Projected: 6
- Index: 3
  Item: asd
  Projected: 3
Subject value: [a, as, foobar, asd, a, bar]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in ascending order by the specified projection`` () =
        fun () ->
            let x = [ "a"; "as"; "foobar"; "asd"; "a"; "bar" ]
            x.Should().BeAscendingBy((fun s -> s.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscendingBy
But found:
- Index: 2
  Item: foobar
  Projected: 6
- Index: 3
  Item: asd
  Projected: 3
Subject value: [a, as, foobar, asd, a, bar]
"""


module BeDescending =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDescending().Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<int>.Empty |]
        [| [ 1 ] |]
        [| [ 1; 1 ] |]
        [| [ 2; 1 ] |]
        [| [ 3; 1 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly descending`` (subject: seq<int>) = subject.Should().BeDescending()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDescending()
        |> assertExnMsg
            """
Subject: x
Should: BeDescending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDescending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not in descending order`` () =
        fun () ->
            let x = [ 3; 1; 3; 6; 2; 1 ]
            x.Should().BeDescending()
        |> assertExnMsg
            """
Subject: x
Should: BeDescending
But found:
- Index: 1
  Item: 1
- Index: 2
  Item: 3
Subject value: [3, 1, 3, 6, 2, 1]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in descending order`` () =
        fun () ->
            let x = [ 3; 1; 3; 6; 2; 1 ]
            x.Should().BeDescending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescending
But found:
- Index: 1
  Item: 1
- Index: 2
  Item: 3
Subject value: [3, 1, 3, 6, 2, 1]
"""


module BeDescendingBy =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDescendingBy(id).Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<string>.Empty |]
        [| [ "a" ] |]
        [| [ "a"; "a" ] |]
        [| [ "asd"; "baz"; "as"; "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly descending by the specified projection`` (subject: seq<string>) =
        subject.Should().BeDescendingBy(fun s -> s.Length)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDescendingBy(id)
        |> assertExnMsg
            """
Subject: x
Should: BeDescendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeDescendingBy(id, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not in descending order by the specified projection`` () =
        fun () ->
            let x = [ "bar"; "as"; "a"; "foobar"; "asd"; "a" ]
            x.Should().BeDescendingBy(fun s -> s.Length)
        |> assertExnMsg
            """
Subject: x
Should: BeDescendingBy
But found:
- Index: 2
  Item: a
  Projected: 1
- Index: 3
  Item: foobar
  Projected: 6
Subject value: [bar, as, a, foobar, asd, a]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in descending order by the specified projection`` () =
        fun () ->
            let x = [ "bar"; "as"; "a"; "foobar"; "asd"; "a" ]
            x.Should().BeDescendingBy((fun s -> s.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescendingBy
But found:
- Index: 2
  Item: a
  Projected: 1
- Index: 3
  Item: foobar
  Projected: 6
Subject value: [bar, as, a, foobar, asd, a]
"""


module BeStrictlyAscending =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeStrictlyAscending().Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<int>.Empty |]
        [| [ 1 ] |]
        [| [ 1 ] |]
        [| [ 1; 2 ] |]
        [| [ 1; 3 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if strictly ascending`` (subject: seq<int>) = subject.Should().BeStrictlyAscending()


    let failData = [
        // Comment to force break for readability
        [| [ 2; 1 ] |]
        [| [ 1; 1 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not strictly ascending`` (subject: seq<int>) =
        assertFails (fun () -> subject.Should().BeStrictlyAscending())


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyAscending()
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyAscending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyAscending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyAscending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not strictly ascending`` () =
        fun () ->
            let x = [ 1; 2; 3; 3; 5 ]
            x.Should().BeStrictlyAscending()
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyAscending
But found:
- Index: 2
  Item: 3
- Index: 3
  Item: 3
Subject value: [1, 2, 3, 3, 5]
"""


    [<Fact>]
    let ``Fails with expected message with because if not strictly ascending`` () =
        fun () ->
            let x = [ 1; 2; 3; 3; 5 ]
            x.Should().BeStrictlyAscending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyAscending
But found:
- Index: 2
  Item: 3
- Index: 3
  Item: 3
Subject value: [1, 2, 3, 3, 5]
"""


module BeStrictlyAscendingBy =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeStrictlyAscendingBy(id).Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<string>.Empty |]
        [| [ "a" ] |]
        [| [ "a"; "as"; "lorem"; "foobar" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if strictly ascending by the specified projection`` (subject: seq<string>) =
        subject.Should().BeStrictlyAscendingBy(fun s -> s.Length)


    let failData = [
        // Comment to force break for readability
        [| [ "a"; "b" ] |]
        [| [ "asd"; "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not strictly ascending by the specified projection`` (subject: seq<string>) =
        assertFails (fun () -> subject.Should().BeStrictlyAscendingBy(fun s -> s.Length))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyAscendingBy(id)
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyAscendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyAscendingBy(id, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyAscendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not strictly ascending by the specified projection`` () =
        fun () ->
            let x = [ "a"; "as"; "asd"; "foo"; "foobar" ]
            x.Should().BeStrictlyAscendingBy(fun s -> s.Length)
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyAscendingBy
But found:
- Index: 2
  Item: asd
  Projected: 3
- Index: 3
  Item: foo
  Projected: 3
Subject value: [a, as, asd, foo, foobar]
"""


    [<Fact>]
    let ``Fails with expected message with because if not strictly ascending by the specified projection`` () =
        fun () ->
            let x = [ "a"; "as"; "asd"; "foo"; "foobar" ]
            x.Should().BeStrictlyAscendingBy((fun s -> s.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyAscendingBy
But found:
- Index: 2
  Item: asd
  Projected: 3
- Index: 3
  Item: foo
  Projected: 3
Subject value: [a, as, asd, foo, foobar]
"""


module BeStrictlyDescending =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeStrictlyDescending().Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<int>.Empty |]
        [| [ 1 ] |]
        [| [ 1 ] |]
        [| [ 2; 1 ] |]
        [| [ 3; 1 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if strictly descending`` (subject: seq<int>) = subject.Should().BeStrictlyDescending()


    let failData = [
        // Comment to force break for readability
        [| [ 1; 2 ] |]
        [| [ 1; 1 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not strictly descending`` (subject: seq<int>) =
        assertFails (fun () -> subject.Should().BeStrictlyDescending())


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyDescending()
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyDescending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyDescending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyDescending
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not strictly descending`` () =
        fun () ->
            let x = [ 6; 5; 3; 3; 2; 1 ]
            x.Should().BeStrictlyDescending()
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyDescending
But found:
- Index: 2
  Item: 3
- Index: 3
  Item: 3
Subject value: [6, 5, 3, 3, 2, 1]
"""


    [<Fact>]
    let ``Fails with expected message with because if not strictly descending`` () =
        fun () ->
            let x = [ 6; 5; 3; 3; 2; 1 ]
            x.Should().BeStrictlyDescending("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyDescending
But found:
- Index: 2
  Item: 3
- Index: 3
  Item: 3
Subject value: [6, 5, 3, 3, 2, 1]
"""


module BeStrictlyDescendingBy =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeStrictlyDescendingBy(id).Id<And<string list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| List<string>.Empty |]
        [| [ "a" ] |]
        [| [ "foobar"; "lorem"; "as"; "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if strictly descending by the specified projection`` (subject: seq<string>) =
        subject.Should().BeStrictlyDescendingBy(fun s -> s.Length)


    let failData = [
        // Comment to force break for readability
        [| [ "a"; "b" ] |]
        [| [ "a"; "asd" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not strictly descending by the specified projection`` (subject: seq<string>) =
        assertFails (fun () -> subject.Should().BeStrictlyDescendingBy(fun s -> s.Length))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyDescendingBy(id)
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyDescendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeStrictlyDescendingBy(id, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyDescendingBy
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not in strictly descending order by the specified projection`` () =
        fun () ->
            let x = [ "foobar"; "foo"; "bar"; "as"; "a" ]
            x.Should().BeStrictlyDescendingBy(fun s -> s.Length)
        |> assertExnMsg
            """
Subject: x
Should: BeStrictlyDescendingBy
But found:
- Index: 1
  Item: foo
  Projected: 3
- Index: 2
  Item: bar
  Projected: 3
Subject value: [foobar, foo, bar, as, a]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in strictly descending order by the specified projection``
        ()
        =
        fun () ->
            let x = [ "foobar"; "foo"; "bar"; "as"; "a" ]
            x.Should().BeStrictlyDescendingBy((fun s -> s.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeStrictlyDescendingBy
But found:
- Index: 1
  Item: foo
  Projected: 3
- Index: 2
  Item: bar
  Projected: 3
Subject value: [foobar, foo, bar, as, a]
"""


module BeSupersetOf =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeSupersetOf([]).Id<And<int list>>().And.Be([])


    let passData = [
        [| []; [] |] // Both empty
        [| [ 1 ]; [] |] // Non-empty vs. empty
        [| [ 1 ]; [ 1 ] |] // Equal with single item
        [| [ 1; 1 ]; [ 1 ] |] // Additional duplicate item
        [| [ 1; 1 ]; [ 1; 1 ] |] // Equal with multiple duplicate items
        [| [ 1; 2 ]; [ 1 ] |] // Additional distinct item
        [| [ 1; 2 ]; [ 1; 2 ] |] // Equal with multiple distinct items
        [| [ 1; 1; 1 ]; [ 1; 1 ] |] // Duplicate items and additional duplicate item
        [| [ 1; 1; 2 ]; [ 1; 1 ] |] // Duplicate items and additional distinct item
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if superset`` (subject: int list) (subset: int list) = subject.Should().BeSupersetOf(subset)


    let failData = [
        [| []; [ 1 ] |] // Empty vs. non-empty
        [| [ 1 ]; [ 1; 1 ] |] // Missing duplicate item
        [| [ 1 ]; [ 1; 2 ] |] // Missing distinct item
        [| [ 1 ]; [ 2 ] |] // Disjoint
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not superset`` (subject: int list) (subset: int list) =
        assertFails (fun () -> subject.Should().BeSupersetOf(subset))


    [<Fact>]
    let ``Throws ArgumentNullException if subset is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> [ 1 ].Should().BeSupersetOf(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeSupersetOf([])
        |> assertExnMsg
            """
Subject: x
Should: BeSupersetOf
Subset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeSupersetOf([], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeSupersetOf
Subset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is missing only a duplicate item in the subset`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeSupersetOf([ 1; 2; 2; 3 ])
        |> assertExnMsg
            """
Subject: x
Should: BeSupersetOf
Subset: [1, 2, 2, 3]
But lacked: [2]
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if subject is missing a duplicate item in the subset`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeSupersetOf([ 1; 2; 2; 3 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeSupersetOf
Subset: [1, 2, 2, 3]
But lacked: [2]
Subject value: [1, 2, 3]
"""


module BeProperSupersetOf =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().BeProperSupersetOf([]).Id<And<int list>>().And.Be([ 1 ])


    let passData = [
        [| [ 1 ]; [] |] // Non-empty vs. empty
        [| [ 1; 1 ]; [ 1 ] |] // Additional duplicate item
        [| [ 1; 2 ]; [ 1 ] |] // Additional distinct item
        [| [ 1; 1; 1 ]; [ 1; 1 ] |] // Duplicate items and additional duplicate item
        [| [ 1; 1; 2 ]; [ 1; 1 ] |] // Duplicate items and additional distinct item
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if proper superset`` (subject: int list) (subset: int list) =
        subject.Should().BeProperSupersetOf(subset)


    let failData = [
        [| []; [] |] // Both empty
        [| []; [ 1 ] |] // Empty vs. non-empty
        [| [ 1 ]; [ 1 ] |] // Equal with single item
        [| [ 1; 1 ]; [ 1; 1 ] |] // Equal with multiple duplicate items
        [| [ 1; 2 ]; [ 1; 2 ] |] // Equal with multiple distinct items
        [| [ 1 ]; [ 1; 1 ] |] // Missing duplicate item
        [| [ 1 ]; [ 1; 2 ] |] // Missing distinct item
        [| [ 1 ]; [ 2 ] |] // Disjoint
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not proper superset`` (subject: int list) (subset: int list) =
        assertFails (fun () -> subject.Should().BeProperSupersetOf(subset))


    [<Fact>]
    let ``Throws ArgumentNullException if subset is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> [ 1 ].Should().BeProperSupersetOf(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeProperSupersetOf([])
        |> assertExnMsg
            """
Subject: x
Should: BeProperSupersetOf
Subset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeProperSupersetOf([], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeProperSupersetOf
Subset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject is missing only a duplicate item in the subset`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeProperSupersetOf([ 1; 2; 2; 3 ])
        |> assertExnMsg
            """
Subject: x
Should: BeProperSupersetOf
Subset: [1, 2, 2, 3]
But lacked: [2]
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if subject is missing a duplicate item in the subset`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeProperSupersetOf([ 1; 2; 2; 3 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeProperSupersetOf
Subset: [1, 2, 2, 3]
But lacked: [2]
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message if subject has no extra items`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeProperSupersetOf([ 1; 2; 3 ])
        |> assertExnMsg
            """
Subject: x
Should: BeProperSupersetOf
Subset: [1, 2, 3]
But had no additional items: []
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if subject has no extra items`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeProperSupersetOf([ 1; 2; 3 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeProperSupersetOf
Subset: [1, 2, 3]
But had no additional items: []
Subject value: [1, 2, 3]
"""


module BeSubsetOf =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeSubsetOf([]).Id<And<int list>>().And.Be([])


    let passData = [
        [| []; [] |] // Both empty
        [| []; [ 1 ] |] // Empty vs. non-empty
        [| [ 1 ]; [ 1 ] |] // Equal with single item
        [| [ 1 ]; [ 1; 1 ] |] // Missing duplicate item
        [| [ 1; 1 ]; [ 1; 1 ] |] // Equal with multiple duplicate items
        [| [ 1 ]; [ 1; 2 ] |] // Missing distinct item
        [| [ 1; 2 ]; [ 1; 2 ] |] // Equal with multiple distinct items
        [| [ 1; 1 ]; [ 1; 1; 1 ] |] // Duplicate items and missing duplicate item
        [| [ 1; 1 ]; [ 1; 1; 2 ] |] // Duplicate items and missing distinct item
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if subset`` (subject: int list) (superset: int list) = subject.Should().BeSubsetOf(superset)


    let failData = [
        [| [ 1 ]; [] |] // Non-empty vs. empty
        [| [ 1; 1 ]; [ 1 ] |] // Additional duplicate item
        [| [ 1; 2 ]; [ 1 ] |] // Additional distinct item
        [| [ 2 ]; [ 1 ] |] // Disjoint
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not subset`` (subject: int list) (superset: int list) =
        assertFails (fun () -> subject.Should().BeSubsetOf(superset))


    [<Fact>]
    let ``Throws ArgumentNullException if superset is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> [ 1 ].Should().BeSubsetOf(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeSubsetOf([])
        |> assertExnMsg
            """
Subject: x
Should: BeSubsetOf
Superset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeSubsetOf([], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeSubsetOf
Superset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject has only an extra duplicate item`` () =
        fun () ->
            let x = [ 1; 2; 2; 3 ]
            x.Should().BeSubsetOf([ 1; 2; 3 ])
        |> assertExnMsg
            """
Subject: x
Should: BeSubsetOf
Superset: [1, 2, 3]
But had extra items: [2]
Subject value: [1, 2, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if subject has only an extra duplicate item`` () =
        fun () ->
            let x = [ 1; 2; 2; 3 ]
            x.Should().BeSubsetOf([ 1; 2; 3 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeSubsetOf
Superset: [1, 2, 3]
But had extra items: [2]
Subject value: [1, 2, 2, 3]
"""


module BeProperSubsetOf =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeProperSubsetOf([ 1 ]).Id<And<int list>>().And.Be([])


    let passData = [
        [| []; [ 1 ] |] // Empty vs. non-empty
        [| [ 1 ]; [ 1; 1 ] |] // Missing duplicate item
        [| [ 1 ]; [ 1; 2 ] |] // Missing distinct item
        [| [ 1; 1 ]; [ 1; 1; 1 ] |] // Duplicate items and missing duplicate item
        [| [ 1; 1 ]; [ 1; 1; 2 ] |] // Duplicate items and missing distinct item
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if proper subset`` (subject: int list) (superset: int list) =
        subject.Should().BeProperSubsetOf(superset)


    let failData = [
        [| []; [] |] // Both empty
        [| [ 1 ]; [] |] // Non-empty vs. empty
        [| [ 1 ]; [ 1 ] |] // Equal with single item
        [| [ 1; 1 ]; [ 1 ] |] // Additional duplicate item
        [| [ 1; 2 ]; [ 1 ] |] // Additional distinct item
        [| [ 1; 1 ]; [ 1; 1 ] |] // Equal with multiple duplicate items
        [| [ 1; 2 ]; [ 1; 2 ] |] // Equal with multiple distinct items
        [| [ 2 ]; [ 1 ] |] // Disjoint
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not proper subset`` (subject: int list) (superset: int list) =
        assertFails (fun () -> subject.Should().BeProperSubsetOf(superset))


    [<Fact>]
    let ``Throws ArgumentNullException if superset is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> [ 1 ].Should().BeProperSubsetOf(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeProperSubsetOf([])
        |> assertExnMsg
            """
Subject: x
Should: BeProperSubsetOf
Superset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().BeProperSubsetOf([], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeProperSubsetOf
Superset: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject has only an extra duplicate item`` () =
        fun () ->
            let x = [ 1; 2; 2; 3 ]
            x.Should().BeProperSubsetOf([ 1; 2; 3 ])
        |> assertExnMsg
            """
Subject: x
Should: BeProperSubsetOf
Superset: [1, 2, 3]
But had extra items: [2]
Subject value: [1, 2, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if subject has only an extra duplicate item`` () =
        fun () ->
            let x = [ 1; 2; 2; 3 ]
            x.Should().BeProperSubsetOf([ 1; 2; 3 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeProperSubsetOf
Superset: [1, 2, 3]
But had extra items: [2]
Subject value: [1, 2, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message if superset has no extra items`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeProperSubsetOf([ 1; 2; 3 ])
        |> assertExnMsg
            """
Subject: x
Should: BeProperSubsetOf
Superset: [1, 2, 3]
But superset had no additional items: []
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if superset has no extra items`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().BeProperSubsetOf([ 1; 2; 3 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeProperSubsetOf
Superset: [1, 2, 3]
But superset had no additional items: []
Subject value: [1, 2, 3]
"""


module IntersectWith =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().IntersectWith([ 1 ]).Id<And<int list>>().And.Be([ 1 ])


    let passData = [
        [| [ 1 ]; [ 1 ] |] // Non-empty and equal
        [| [ 1 ]; [ 1; 2 ] |] // Other has additional distinct item
        [| [ 1 ]; [ 1; 2 ] |] // Other has additional duplicate item
        [| [ 1; 2 ]; [ 1 ] |] // Subject has additional item
        [| [ 1; 2 ]; [ 1 ] |] // Subject has additional distinct item
        [| [ 1; 1 ]; [ 1 ] |] // Subject has additional duplicate item
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if intersects`` (subject: int list) (other: int list) = subject.Should().IntersectWith(other)


    let failData = [
        [| []; [] |] // Both empty
        [| []; [ 1 ] |] // Subject empty
        [| [ 1 ]; [] |] // Other empty
        [| [ 1 ]; [ 2 ] |] // Disjoint
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not intersects`` (subject: int list) (other: int list) =
        assertFails (fun () -> subject.Should().IntersectWith(other))


    [<Fact>]
    let ``Throws ArgumentNullException if other is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> [ 1 ].Should().IntersectWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().IntersectWith([])
        |> assertExnMsg
            """
Subject: x
Should: IntersectWith
Other: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: seq<int> = null
            x.Should().IntersectWith([], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: IntersectWith
Other: []
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if the sets are disjoint`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().IntersectWith([ 3; 4 ])
        |> assertExnMsg
            """
Subject: x
Should: IntersectWith
Other: [3, 4]
But had no common items: []
Subject value: [1, 2]
"""


    [<Fact>]
    let ``Fails with expected message with because if the sets are disjoint`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().IntersectWith([ 3; 4 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: IntersectWith
Other: [3, 4]
But had no common items: []
Subject value: [1, 2]
"""


module NotIntersectWith =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().NotIntersectWith([]).Id<And<int list>>().And.Be([])


    [<Fact>]
    let ``Passes if null`` = (null: seq<int>).Should().NotIntersectWith([])


    let passData = [
        [| []; [] |] // Both empty
        [| []; [ 1 ] |] // Subject empty
        [| [ 1 ]; [] |] // Other empty
        [| [ 1 ]; [ 2 ] |] // Disjoint
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if not intersects`` (subject: int list) (other: int list) =
        subject.Should().NotIntersectWith(other)


    let failData = [
        [| [ 1 ]; [ 1 ] |] // Non-empty and equal
        [| [ 1 ]; [ 1; 2 ] |] // Other has additional distinct item
        [| [ 1 ]; [ 1; 2 ] |] // Other has additional duplicate item
        [| [ 1; 2 ]; [ 1 ] |] // Subject has additional item
        [| [ 1; 2 ]; [ 1 ] |] // Subject has additional distinct item
        [| [ 1; 1 ]; [ 1 ] |] // Subject has additional duplicate item
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if intersects`` (subject: int list) (other: int list) =
        assertFails (fun () -> subject.Should().NotIntersectWith(other))


    [<Fact>]
    let ``Throws ArgumentNullException if other is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> [ 1 ].Should().NotIntersectWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if the sets intersect`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().NotIntersectWith([ 2; 3; 4 ])
        |> assertExnMsg
            """
Subject: x
Should: NotIntersectWith
Other: [2, 3, 4]
But found common items: [2, 3]
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if the sets intersect`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().NotIntersectWith([ 2; 3; 4 ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotIntersectWith
Other: [2, 3, 4]
But found common items: [2, 3]
Subject value: [1, 2, 3]
"""

/////////////////////////////////////////////////////////////////////////////////////////////////
// Note: When adding new tests above, if the assertions are relevant for Dictionary or String, //
// also add "Can use seq assertion" tests to DictionaryAssertions or StringAssertions.         //
/////////////////////////////////////////////////////////////////////////////////////////////////
