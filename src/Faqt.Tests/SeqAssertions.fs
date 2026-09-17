module SeqAssertions

open System
open System.Globalization
open Faqt
open Xunit


module NaNOrdering =


    type private ReverseRank(rank: int) =
        member _.Rank = rank

        override _.Equals(other) =
            match other with
            | :? ReverseRank as other -> rank = other.Rank
            | _ -> false

        override _.GetHashCode() = rank.GetHashCode()

        interface IComparable with
            member _.CompareTo(other) =
                compare (unbox<ReverseRank> other).Rank rank


    let private casesFor name (nan: 'a) (low: 'a) (high: 'a) =
        [
            [ nan; high ], [ nan; low ]
            [ low; nan ], [ high; nan ]
            [ high; nan; low ], [ low; nan; high ]
        ]
        |> List.mapi (fun position (ascending, descending) ->
            [
                "ascending", (fun () -> ascending.Should().BeAscending() |> ignore)
                "descending", (fun () -> descending.Should().BeDescending() |> ignore)
                "strictly ascending", (fun () -> ascending.Should().BeStrictlyAscending() |> ignore)
                "strictly descending", (fun () -> descending.Should().BeStrictlyDescending() |> ignore)
                "ascending by", (fun () -> (List.indexed ascending).Should().BeAscendingBy(snd) |> ignore)
                "descending by", (fun () -> (List.indexed descending).Should().BeDescendingBy(snd) |> ignore)
                "strictly ascending by",
                (fun () -> (List.indexed ascending).Should().BeStrictlyAscendingBy(snd) |> ignore)
                "strictly descending by",
                (fun () -> (List.indexed descending).Should().BeStrictlyDescendingBy(snd) |> ignore)
            ]
            |> List.map (fun (assertion, run) -> [| box $"%s{name}: %s{assertion}, position %i{position}"; box run |])
        )
        |> List.concat


    let cases = [
        yield! casesFor "double" Double.NaN 1.0 2.0
        yield! casesFor "single" Single.NaN 1.0f 2.0f
        yield! casesFor "Half" Half.NaN Half.Zero Half.One
        yield! casesFor "interface double" (Double.NaN :> IComparable) (1.0 :> IComparable) (2.0 :> IComparable)
    ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Rejects comparisons involving NaN in sequence ordering`` (_name: string) (run: unit -> unit) =
        assertFails run |> ignore


    [<Fact>]
    let ``Preserves ordering of infinities and custom comparable values`` () =
        let check ascending =
            let descending = List.rev ascending
            ascending.Should().BeAscending().And.BeStrictlyAscending() |> ignore
            descending.Should().BeDescending().And.BeStrictlyDescending() |> ignore

            (List.indexed ascending).Should().BeAscendingBy(snd).And.BeStrictlyAscendingBy(snd)
            |> ignore

            (List.indexed descending).Should().BeDescendingBy(snd).And.BeStrictlyDescendingBy(snd)
            |> ignore

        check [ Double.NegativeInfinity; 0.0; Double.PositiveInfinity ]
        check [ Single.NegativeInfinity; 0.0f; Single.PositiveInfinity ]
        check [ Half.NegativeInfinity; Half.Zero; Half.PositiveInfinity ]
        check [ ReverseRank(2); ReverseRank(1) ]
        check ([]: double list)
        check [ Double.NaN ]


module EvaluationErrors =


    let cases = evaluationErrorCases [ "AllSatisfy"; "SatisfyRespectively" ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Item errors cannot become successful negation or alternatives`` assertion composition cancellation =
        assertEvaluationError
            composition
            cancellation
            (fun error ->
                let callback (_: int) : unit = raise error

                match assertion with
                | "AllSatisfy" -> [ 1 ].Should().AllSatisfy(callback) |> ignore
                | "SatisfyRespectively" -> [ 1 ].Should().SatisfyRespectively([ callback ]) |> ignore
                | _ -> failwith "Unknown assertion"
            )


    [<Theory>]
    [<InlineData("AllSatisfy")>]
    [<InlineData("SatisfyRespectively")>]
    let ``Aggregators stop at errors after earlier item failures`` assertion =
        assertEvaluationError
            "Direct"
            false
            (fun error ->
                let callback item =
                    match item with
                    | 1 -> item.Should().Fail() |> ignore
                    | 2 -> raise error
                    | _ -> failwith "This later item must not run"

                match assertion with
                | "AllSatisfy" -> [ 1; 2; 3 ].Should().AllSatisfy(callback) |> ignore
                | "SatisfyRespectively" ->
                    [ 1; 2; 3 ].Should().SatisfyRespectively([ callback; callback; callback ])
                    |> ignore
                | _ -> failwith "Unknown assertion"
            )


    [<Theory>]
    [<InlineData("AllSatisfy")>]
    [<InlineData("SatisfyRespectively")>]
    let ``Ordinary item failures are still aggregated`` assertion =
        let callback item = item.Should().Be(0)

        let error =
            assertFails (fun () ->
                match assertion with
                | "AllSatisfy" -> [ 1; 2 ].Should().AllSatisfy(callback) |> ignore
                | "SatisfyRespectively" -> [ 1; 2 ].Should().SatisfyRespectively([ callback; callback ]) |> ignore
                | _ -> failwith "Unknown assertion"
            )

        Assert.Contains("Index: 0", error.Message)
        Assert.Contains("Index: 1", error.Message)


type RefRecord = { Id: int }


let singlePass (items: seq<'a>) : seq<'a> =
    let queue = System.Collections.Generic.Queue<'a>(items)

    seq {
        while queue.Count > 0 do
            yield queue.Dequeue()
    }


module AllSatisfy =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        [ "asd"; "123" ].Should().AllSatisfy(fun x -> x.Should().Pass()).Id<And<string list>>().And.Be([ "asd"; "123" ])


    [<Fact>]
    let ``Passes if subject is empty`` () =
        List<int>.Empty.Should().AllSatisfy(fun x -> x.Should().Fail())


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().AllSatisfy(_.Should().Pass()))


    [<Fact>]
    let ``Fails with expected message if at least one of the items fails to satisfy the assertion or throws`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .AllSatisfy(fun y ->
                    if y = "1234" then
                        failwith "foo"
                    else
                        y.Length.Should().Test(y.Length = 3)
                )
        |> assertErrorMsgWildcard
            """
Subject: x
Should: AllSatisfy
Failures:
- Index: 1
  Failure:
    Subject: y.Length
    Should: Test
- Index: 2
  Exception: |-
    System.Exception: foo
*
Subject value: [asd, test, '1234']
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fails to satisfy the assertion or throws``
        ()
        =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .AllSatisfy(
                    (fun y ->
                        if y = "1234" then
                            failwith "foo"
                        else
                            y.Length.Should().Test(y.Length = 3)
                    ),
                    "Some reason"
                )
        |> assertErrorMsgWildcard
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
  Exception: |-
    System.Exception: foo
*
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
    let ``Fails for single-pass subject and assertions when an inner assertion fails`` () =
        assertFails (fun () ->
            (singlePass [ 1; 2 ])
                .Should()
                .SatisfyRespectively(
                    singlePass [
                        fun x -> x.Should().Be(99)
                        fun x -> x.Should().Be(98)
                    ]
                )
            |> ignore
        )


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().SatisfyRespectively([]))


    [<Fact>]
    let ``Throws if assertions is null`` () =
        assertThrows (fun () -> List<int>.Empty.Should().SatisfyRespectively(Unchecked.defaultof<_>))


    [<Fact>]
    let ``Fails with expected message if subject does not contain one item per assertion`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])
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

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ], "Some reason")
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
    let ``Fails with expected message if at least one of the items fails to satisfy the assertion or throws`` () =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively(
                    [
                        (fun x1 -> x1.Should().Fail())
                        (fun x2 -> x2.Should().Pass())
                        (fun _ -> failwith "foo")
                    ]
                )
        |> assertErrorMsgWildcard
            """
Subject: x
Should: SatisfyRespectively
Failures:
- Index: 0
  Failure:
    Subject: x1
    Should: Fail
- Index: 2
  Exception: |-
    System.Exception: foo
*
Subject value: [asd, test, '1234']
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fails to satisfy the assertion or throws``
        ()
        =
        fun () ->
            let x = [ "asd"; "test"; "1234" ]

            x
                .Should()
                .SatisfyRespectively(
                    [
                        (fun x1 -> x1.Should().Fail())
                        (fun x2 -> x2.Should().Pass())
                        (fun _ -> failwith "foo")
                    ],
                    "Some reason"
                )
        |> assertErrorMsgWildcard
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
  Exception: |-
    System.Exception: foo
*
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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().HaveLength(0))


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeEmpty())


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
        Seq.empty<int>.Should().BeNullOrEmpty().Id<And<seq<int> | null>>().And.Be(Seq.empty)


    [<Fact>]
    let ``Passes if subject is null`` () =
        Unchecked.defaultof<seq<int>>.Should().BeNullOrEmpty()


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


    [<Fact>]
    let ``Returns the actual matched item as the derived value`` () =
        let actual = { Id = 1 }
        let expected = { Id = 1 }
        let derived = [ actual ].Should().Contain(expected).That

        Object.ReferenceEquals(actual, derived).Should().BeTrue() |> ignore


    let passData = [
        [| box [ "a" ]; "a" |]
        [| [ "a"; "b" ]; "a" |]
        [| [ nul<string> ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if sequence contains value`` (subject: seq<string | null>) (value: string | null) =
        subject.Should().Contain(value)


    let failData = [
        [| box List<string>.Empty; "a" |]
        [| [ "a" ]; "b" |]
        [| [ nul<string> ]; "a" |]
        [| [ "a" ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing value`` (subject: seq<string | null>) (value: string | null) =
        assertFails (fun () -> subject.Should().Contain(value))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<int>>.Should().Contain(0))


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
        [| box List<string>.Empty; "a" |]
        [| [ "a" ]; "b" |]
        [| [ nul<string> ]; "a" |]
        [| [ "a" ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if not containing value`` (subject: seq<string | null>) (value: string | null) =
        subject.Should().NotContain(value)


    let failData = [
        [| box [ "a" ]; "a" |]
        [| [ "a"; "b" ]; "a" |]
        [| [ nul<string> ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if sequence contains value`` (subject: seq<string | null>) (value: string | null) =
        assertFails (fun () -> subject.Should().NotContain(value))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().NotContain("value"))


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
        [| [ nul<string> ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if all items are equal to the specified value``
        (subject: seq<string | null>)
        (expected: string | null)
        =
        subject.Should().AllBe(expected)


    let failData = [
        [| box [ "a" ]; "b" |]
        [| [ "a"; "b" ]; "a" |]
        [| [ asNull "a"; null ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not all items are equal to the specified value``
        (subject: seq<string | null>)
        (expected: string | null)
        =
        assertFails (fun () -> subject.Should().AllBe(expected)) |> ignore


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().AllBe(""))


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
        [ "a" ].Should().AllBeMappedTo(1, (fun x -> x.Length)).Id<And<string list>>().And.Be([ "a" ])


    let passData = [
        [| box List<string>.Empty; 1 |]
        [| [ "a" ]; 1 |]
        [| [ "a"; "a" ]; 1 |]
        [| [ "ab"; "cd" ]; 2 |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if all items are equal to the specified value when projected``
        (subject: seq<string | null>)
        (expected: int)
        =
        subject.Should().AllBeMappedTo(expected, (fun x -> x.Length))


    let failData = [
        // Comment to force break
        [| box [ "a" ]; 2 |]
        [| [ "a"; "ab" ]; 1 |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not all items are equal to the specified value when projected``
        (subject: seq<string | null>)
        (expected: int)
        =
        assertFails (fun () -> subject.Should().AllBeMappedTo(expected, (fun x -> x.Length)))
        |> ignore


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().AllBeMappedTo(0, _.Length))


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


module AllBeEqual =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().AllBeEqual().Id<And<int list>>().And.Be([])


    let passData = [
        [| List<string | null>.Empty |]
        [| [ "a" ] |]
        [| [ "a"; "a" ] |]
        [| [ nul<string> ] |]
        [| [ nul<string>; null ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if all items are equal`` (subject: seq<string | null>) = subject.Should().AllBeEqual()


    [<Fact>]
    let ``Fails for single-pass sequence when items are not equal`` () =
        assertFails (fun () -> (singlePass [ 1; 2 ]).Should().AllBeEqual() |> ignore)


    let failData = [
        // Comment to force break
        [| [ asNull "a"; "b" ] |]
        [| [ "a"; null ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not all items are equal`` (subject: seq<string | null>) =
        assertFails (fun () -> subject.Should().AllBeEqual()) |> ignore


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().AllBeEqual())


    [<Fact>]
    let ``Fails with expected message if items are not equal`` () =
        fun () ->
            let x = [ 1; 1; 3; 2 ]
            x.Should().AllBeEqual()
        |> assertExnMsg
            """
Subject: x
Should: AllBeEqual
But found:
- Index: 0
  Value: 1
- Index: 2
  Value: 3
Subject value: [1, 1, 3, 2]
"""


    [<Fact>]
    let ``Fails with expected message with because if items are not equal`` () =
        fun () ->
            let x = [ 1; 1; 3; 2 ]
            x.Should().AllBeEqual("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllBeEqual
But found:
- Index: 0
  Value: 1
- Index: 2
  Value: 3
Subject value: [1, 1, 3, 2]
"""


module AllBeEqualBy =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ "a" ].Should().AllBeEqualBy(fun x -> x.Length).Id<And<string list>>().And.Be([ "a" ])


    let passData = [
        [| List<string>.Empty |]
        [| [ "a" ] |]
        [| [ "a"; "a" ] |]
        [| [ "a"; "b" ] |]
        [| [ "ab"; "cd" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if all items are equal by the specified projection`` (subject: seq<string | null>) =
        subject.Should().AllBeEqualBy(fun x -> x.Length)


    [<Fact>]
    let ``Fails for single-pass sequence when projected items are not equal`` () =
        assertFails (fun () -> (singlePass [ "a"; "ab" ]).Should().AllBeEqualBy(fun x -> x.Length) |> ignore)


    let failData = [ [| [ "a"; "ab" ] |] ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not all items are equal by the specified projection`` (subject: seq<string | null>) =
        assertFails (fun () -> subject.Should().AllBeEqualBy(fun x -> x.Length))
        |> ignore


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().AllBeEqualBy(_.Length))


    [<Fact>]
    let ``Fails with expected message if items are not equal`` () =
        fun () ->
            let x = [ "a"; "b"; "abc"; "ab" ]
            x.Should().AllBeEqualBy(fun x -> x.Length)
        |> assertExnMsg
            """
Subject: x
Should: AllBeEqualBy
But found:
- Index: 0
  Projected: 1
  Value: a
- Index: 2
  Projected: 3
  Value: abc
Subject value: [a, b, abc, ab]
"""


    [<Fact>]
    let ``Fails with expected message with because if items are not equal`` () =
        fun () ->
            let x = [ "a"; "b"; "abc"; "ab" ]
            x.Should().AllBeEqualBy((fun x -> x.Length), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllBeEqualBy
But found:
- Index: 0
  Projected: 1
  Value: a
- Index: 2
  Projected: 3
  Value: abc
Subject value: [a, b, abc, ab]
"""


module SequenceEqual =


    [<Fact>]
    let ``Can be chained with And`` () =
        [ 1 ].Should().SequenceEqual([ 1 ]).Id<And<int list>>().And.Be([ 1 ])


    let passData = [
        [| List<string | null>.Empty; List<string | null>.Empty |]
        [| [ "a" ]; [ "a" ] |]
        [| [ "a"; null ]; [ "a"; null ] |]
        [| [ "a"; "b" ]; [ "a"; "b" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if both have the same items in the same order``
        (subject: seq<string | null>)
        (expected: seq<string | null>)
        =
        subject.Should().SequenceEqual(expected)


    [<Fact>]
    let ``Fails for single-pass sequences when items differ`` () =
        assertFails (fun () -> (singlePass [ 1; 2 ]).Should().SequenceEqual(singlePass [ 1; 3 ]) |> ignore)


    let failData = [
        [| box List<string | null>.Empty; [ asNull "a" ] |]
        [| [ "a" ]; [ asNull "a"; null ] |]
        [| [ "a" ]; [ "b" ] |]
        [| [ "a"; "b" ]; [ "b"; "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing the same items in the same order`` (a: seq<string | null>) (b: seq<string | null>) =
        assertFails (fun () -> a.Should().SequenceEqual(b)) |> ignore
        assertFails (fun () -> b.Should().SequenceEqual(a)) |> ignore


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().SequenceEqual([]))


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


module SinglePassMultisets =


    let cases =
        let lists: (string | null) list list = [
            []
            [ null ]
            [ "a" ]
            [ "a"; "a" ]
            [ "a"; "b" ]
            [ "b"; "a" ]
            [ null; "a"; "a" ]
        ]

        [
            for subject in lists do
                for expected in lists do
                    yield [| box subject; box expected |]
        ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Single-pass multiset assertions respect item multiplicities``
        (subject: (string | null) list)
        (expected: (string | null) list)
        =
        let isSubset subset superset =
            subset
            |> List.forall (fun item ->
                List.length (List.filter ((=) item) subset)
                <= List.length (List.filter ((=) item) superset)
            )

        let check passes assertion =
            if passes then
                assertion ()
            else
                assertFails assertion |> ignore

        check
            (subject.Length = expected.Length && isSubset subject expected)
            (fun () -> (singlePass subject).Should().HaveSameItemsAs(singlePass expected) |> ignore)

        check
            (isSubset subject expected)
            (fun () -> (singlePass subject).Should().BeSubsetOf(singlePass expected) |> ignore)

        check
            (subject.Length < expected.Length && isSubset subject expected)
            (fun () -> (singlePass subject).Should().BeProperSubsetOf(singlePass expected) |> ignore)

        check
            (isSubset expected subject)
            (fun () -> (singlePass subject).Should().BeSupersetOf(singlePass expected) |> ignore)

        check
            (subject.Length > expected.Length && isSubset expected subject)
            (fun () -> (singlePass subject).Should().BeProperSupersetOf(singlePass expected) |> ignore)


module MultisetEquality =


    let rec private removeFirst item =
        function
        | [] -> None
        | head :: tail when head = item -> Some tail
        | head :: tail -> removeFirst item tail |> Option.map (fun remaining -> head :: remaining)


    let rec private isSubset subset superset =
        match subset with
        | [] -> true
        | head :: tail ->
            match removeFirst head superset with
            | None -> false
            | Some remaining -> isSubset tail remaining


    let private casesFor name nan ordinary =
        let lists = [
            []
            [ nan ]
            [ ordinary ]
            [ nan; nan ]
            [ ordinary; nan ]
            [ nan; ordinary ]
            [ ordinary; ordinary ]
            [ nan; ordinary; nan ]
        ]

        [
            for i, subject in List.indexed lists do
                for j, expected in List.indexed lists do
                    let run () =
                        let check passes assertion =
                            if passes then
                                assertion ()
                            else
                                assertFails assertion |> ignore

                        check
                            (subject.Length = expected.Length && isSubset subject expected)
                            (fun () -> (singlePass subject).Should().HaveSameItemsAs(singlePass expected) |> ignore)

                        check
                            (isSubset subject expected)
                            (fun () -> (singlePass subject).Should().BeSubsetOf(singlePass expected) |> ignore)

                        check
                            (subject.Length < expected.Length && isSubset subject expected)
                            (fun () -> (singlePass subject).Should().BeProperSubsetOf(singlePass expected) |> ignore)

                        check
                            (isSubset expected subject)
                            (fun () -> (singlePass subject).Should().BeSupersetOf(singlePass expected) |> ignore)

                        check
                            (subject.Length > expected.Length && isSubset expected subject)
                            (fun () -> (singlePass subject).Should().BeProperSupersetOf(singlePass expected) |> ignore)

                    yield [| box $"%s{name}: %i{i}, %i{j}"; box run |]
        ]


    let cases = [
        yield! casesFor "double" Double.NaN 1.0
        yield! casesFor "single" Single.NaN 1.0f
        yield! casesFor "Half" Half.NaN Half.One
        yield! casesFor "list" [ Double.NaN ] [ 1.0 ]
        yield! casesFor "array" [| Double.NaN |] [| 1.0 |]
        yield! casesFor "record" {| Value = Double.NaN |} {| Value = 1.0 |}
        yield! casesFor "option" (Some Double.NaN) (Some 1.0)
        yield! casesFor "tuple" (Double.NaN, 0) (1.0, 0)
    ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Single-pass multisets agree with itemwise FSharp equality`` (_name: string) (run: unit -> unit) = run ()


    [<Fact>]
    let ``Reports unmatched NaNs on both sides in source order`` () =
        let ex =
            assertFails (fun () -> [ Double.NaN; 2.0; Double.NaN ].Should().HaveSameItemsAs([ 1.0; Double.NaN ]))

        let values key =
            ex.FailureData.Extra
            |> List.find (fun (name, _) -> name = key)
            |> snd
            |> unbox<seq<double>>
            |> Seq.toArray

        let missing = values "Missing items"
        let additional = values "Additional items"
        Assert.Equal(2, missing.Length)
        Assert.Equal(1.0, missing[0])
        Assert.True(Double.IsNaN missing[1])
        Assert.Equal(3, additional.Length)
        Assert.True(Double.IsNaN additional[0])
        Assert.Equal(2.0, additional[1])
        Assert.True(Double.IsNaN additional[2])


module HaveSameItemsAs =


    [<Fact>]
    let ``Can be chained with And`` () =
        Map.empty<string, int>
            .Should()
            .HaveSameItemsAs(Map.empty<string, int>)
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty<string, int>)


    let passData = [
        [| List<string | null>.Empty; List<string | null>.Empty |]
        [| [ "a" ]; [ "a" ] |]
        [| [ "a"; "b" ]; [ "a"; "b" ] |]
        [| [ "a"; "b" ]; [ "b"; "a" ] |]
        [| [ "a"; null ]; [ null; "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if both contain the same values`` (subject: seq<string | null>) (expected: seq<string | null>) =
        subject.Should().HaveSameItemsAs(expected)


    let failData = [
        [| List<string | null>.Empty; [ "a" ] |]
        [| [ "a" ]; [ "a"; "a" ] |]
        [| [ "a" ]; [ "a"; "b" ] |]
        [| [ "a" ]; [ "b" ] |]
        [| [ "a" ]; [ nul<string> ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if they do not contain the same values`` (a: seq<string | null>) (b: seq<string | null>) =
        assertFails (fun () -> a.Should().HaveSameItemsAs(b)) |> ignore
        assertFails (fun () -> b.Should().HaveSameItemsAs(a))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().HaveSameItemsAs([]))


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
        [ 1 ].Should().ContainExactlyOneItem().Id<AndDerived<int list, int>>().That.Should(()).Be(1)


    [<Fact>]
    let ``Passes for single-pass sequence and returns the only value`` () =
        (singlePass [ 1 ]).Should().ContainExactlyOneItem().That.Should(()).Be(1)


    let passData = [
        // Comment to force break for readability
        [| [ asNull "a" ] |]
        [| [ null ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if containing exactly one item`` (subject: seq<string | null>) =
        subject.Should().ContainExactlyOneItem()


    let failData = [
        // Comment to force break for readability
        [| List<string>.Empty |]
        [| [ "a"; "b" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing exactly one item`` (subject: seq<string | null>) =
        assertFails (fun () -> subject.Should().ContainExactlyOneItem()) |> ignore


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().ContainExactlyOneItem())


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
        [ 1; 2 ].Should().ContainExactlyOneItemMatching((=) 2).Id<AndDerived<int list, int>>().That.Should(()).Be(2)


    [<Fact>]
    let ``Passes for single-pass sequence and returns the only matching value`` () =
        (singlePass [ 1; 2 ]).Should().ContainExactlyOneItemMatching((=) 2).That.Should(()).Be(2)


    let passData = [
        // Comment to force break for readability
        [| [ asNull "a" ] |]
        [| [ null ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if containing exactly one item the predicate`` (subject: seq<string | null>) =
        subject.Should().ContainExactlyOneItemMatching(fun _ -> true)


    let failData = [ [| List<int>.Empty |]; [| [ 1; 1 ] |] ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing exactly one item matching the predicate`` (subject: seq<int>) =
        assertFails (fun () -> subject.Should().ContainExactlyOneItemMatching((=) 1))
        |> ignore


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().ContainExactlyOneItemMatching(fun _ -> true))


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
        [ 1; 2 ].Should().ContainAtLeastOneItem().Id<AndDerived<int list, int>>().That.Should(()).Be(1)


    [<Fact>]
    let ``Passes for single-pass sequence and returns the first value`` () =
        (singlePass [ 1; 2 ]).Should().ContainAtLeastOneItem().That.Should(()).Be(1)


    let passData = [
        // Comment to force break for readability
        [| [ null ] |]
        [| [ asNull "a" ] |]
        [| [ "a"; "b" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if contains at least one item`` (subject: seq<string | null>) =
        subject.Should().ContainAtLeastOneItem()


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().ContainAtLeastOneItem())


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


    [<Fact>]
    let ``Passes for single-pass sequence and returns the first matched value`` () =
        (singlePass [ 1; 2; 3 ]).Should().ContainAtLeastOneItemMatching(fun x -> x > 1).That.Should(()).Be(2)


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().ContainAtLeastOneItemMatching(fun _ -> true))


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


module ContainAtMostOneItem =


    [<Fact>]
    let ``Can be chained with AndDerived with only value`` () =
        [ 1 ].Should().ContainAtMostOneItem().Id<AndDerived<int list, int option>>().That.Should(()).Be(Some 1)


    [<Fact>]
    let ``Passes for single-pass sequence and returns Some value`` () =
        (singlePass [ 1 ]).Should().ContainAtMostOneItem().That.Should(()).Be(Some 1)


    [<Fact>]
    let ``Can be chained with AndDerived with None if empty`` () =
        List.empty<int>.Should().ContainAtMostOneItem().Id<AndDerived<int list, int option>>().That.Should(()).Be(None)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().ContainAtMostOneItem())


    [<Fact>]
    let ``Fails with expected message if subject contains more than one item`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().ContainAtMostOneItem()
        |> assertExnMsg
            """
Subject: x
Should: ContainAtMostOneItem
But length was: 2
Subject value: [1, 2]
"""


    [<Fact>]
    let ``Fails with expected message with because if subject contains more than one item`` () =
        fun () ->
            let x = [ 1; 2 ]
            x.Should().ContainAtMostOneItem("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainAtMostOneItem
But length was: 2
Subject value: [1, 2]
"""


module ContainAtMostOneItemMatching =


    [<Fact>]
    let ``Can be chained with AndDerived with only matching value`` () =
        [ 1; 2; 3 ]
            .Should()
            .ContainAtMostOneItemMatching(fun x -> x > 2)
            .Id<AndDerived<int list, int option>>()
            .That.Should(())
            .Be(Some 3)


    [<Fact>]
    let ``Passes for single-pass sequence and returns Some matching value`` () =
        (singlePass [ 1; 2; 3 ]).Should().ContainAtMostOneItemMatching(fun x -> x > 2).That.Should(()).Be(Some 3)


    [<Fact>]
    let ``Can be chained with AndDerived with None if no matching value`` () =
        [ 1; 2; 3 ]
            .Should()
            .ContainAtMostOneItemMatching(fun x -> x > 3)
            .Id<AndDerived<int list, int option>>()
            .That.Should(())
            .Be(None)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().ContainAtMostOneItemMatching(fun _ -> true))


    [<Fact>]
    let ``Fails with expected message if more than one item matches the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainAtMostOneItemMatching(fun x -> x > 1)
        |> assertExnMsg
            """
Subject: x
Should: ContainAtMostOneItemMatching
But found: 2
Matching items: [2, 3]
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if more than one item matches the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().ContainAtMostOneItemMatching((fun x -> x > 1), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainAtMostOneItemMatching
But found: 2
Matching items: [2, 3]
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


    [<Fact>]
    let ``Stops enumerating the source at the first match`` () =
        let subject =
            seq {
                yield 0
                yield 1
                failwith "The tail must not be enumerated"
            }

        subject.Should().ContainItemsMatching(fun x -> x > 0)


    [<Fact>]
    let ``Stops evaluating the predicate at the first match`` () =
        [ 0; 1; 2 ]
            .Should()
            .ContainItemsMatching(fun x ->
                if x = 2 then
                    failwith "The predicate must not be evaluated after the first match"

                x > 0
            )


    [<Fact>]
    let ``Derived matches can be consumed without evaluating the remaining tail`` () =
        let subject =
            seq {
                yield 0
                yield 1
                yield 2
                yield 3
                failwith "The tail must not be enumerated"
            }

        let matches = subject.Should().ContainItemsMatching(fun x -> x % 2 = 1).That
        (matches |> Seq.take 2).Should().SequenceEqual([ 1; 3 ])


    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    let ``Enumerating derived matches re-enumerates the source`` times =
        let mutable enumerationCount = 0

        let subject =
            seq {
                enumerationCount <- enumerationCount + 1
                yield 0
                yield enumerationCount
            }

        let matches = subject.Should().ContainItemsMatching(fun x -> x > 0).That

        for _ in 1 .. times - 1 do
            matches |> Seq.iter ignore

        matches.Should().SequenceEqual([ times + 1 ])


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().ContainItemsMatching(fun _ -> true))


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


module NotContainItemsMatching =


    [<Fact>]
    let ``Can be chained with AndDerived with matched values`` () =
        [].Should().NotContainItemsMatching(fun x -> x > 1).Id<And<int list>>().And.Be([])


    let passData = [
        // Comment to force break for readability
        [| [ 1 ] |]
        [| [ 1; 2 ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if not containing items matching the predicate`` (subject: seq<int>) =
        subject.Should().NotContainItemsMatching(fun x -> x > 3)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().NotContainItemsMatching(fun _ -> false))


    [<Fact>]
    let ``Fails with expected message any items match the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().NotContainItemsMatching(fun x -> x > 1)
        |> assertExnMsg
            """
Subject: x
Should: NotContainItemsMatching
But found: 2
Matching items: [2, 3]
Subject value: [1, 2, 3]
"""


    [<Fact>]
    let ``Fails with expected message with because if any items match the predicate`` () =
        fun () ->
            let x = [ 1; 2; 3 ]
            x.Should().NotContainItemsMatching((fun x -> x > 1), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContainItemsMatching
But found: 2
Matching items: [2, 3]
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


    [<Theory>]
    [<InlineData("consumed")>]
    [<InlineData("throws")>]
    [<InlineData("changes")>]
    let ``Reports duplicates from the original enumeration`` sourceBehavior =
        let items = [ 1; 2; 2; 2; 5; 5; 0 ]
        let mutable enumerationCount = 0

        let subject =
            match sourceBehavior with
            | "consumed" -> singlePass items
            | _ ->
                seq {
                    enumerationCount <- enumerationCount + 1

                    if enumerationCount = 1 then
                        yield! items
                    elif sourceBehavior = "throws" then
                        failwith "The sequence cannot be enumerated again"
                    else
                        yield! [ 9; 9 ]
                }

        let error = assertFails (fun () -> subject.Should().BeDistinct())

        Assert.Contains(
            "Duplicates:\n- Count: 3\n  Item: 2\n- Count: 2\n  Item: 5\n",
            error.Message.ReplaceLineEndings("\n")
        )


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Enumerates once to evaluate distinctness and build the duplicate report`` hasDuplicates =
        let mutable enumerationCount = 0

        let subject =
            seq {
                enumerationCount <- enumerationCount + 1
                yield 1
                yield if hasDuplicates then 1 else 2
            }

        // Isolate assertion evaluation from optional enumeration by a diagnostic formatter.
        use _ = Faqt.Formatting.Formatter.With(fun _ -> "failure")

        if hasDuplicates then
            assertFails (fun () -> subject.Should().BeDistinct()) |> ignore
        else
            subject.Should().BeDistinct() |> ignore

        Assert.Equal(1, enumerationCount)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeDistinct())


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
    let ``Passes if distinct by the specified projection`` (subject: seq<string | null>) =
        subject.Should().BeDistinctBy(fun s -> s.Length)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeDistinctBy(_.Length))


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeAscending())


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


module ``BeAscending (StringComparison)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeAscending(StringComparison.Ordinal).Id<And<string list>>().And.Be([])


    let passData = [
        [| box List<string>.Empty; StringComparison.Ordinal |]
        [| [ "a" ]; StringComparison.Ordinal |]
        [| [ "a"; "a" ]; StringComparison.Ordinal |]
        [| [ "a"; "b" ]; StringComparison.Ordinal |]
        [| [ "A"; "b" ]; StringComparison.OrdinalIgnoreCase |]
        [| [ "a"; "B" ]; StringComparison.OrdinalIgnoreCase |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly ascending using the specified comparison``
        (subject: seq<string>)
        (comparison: StringComparison)
        =
        subject.Should().BeAscending(comparison)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeAscending(StringComparison.Ordinal))


    [<Fact>]
    let ``Fails with expected message if not in ascending order`` () =
        fun () ->
            let x = [ "a"; "b"; "f"; "c"; "a"; "c" ]
            x.Should().BeAscending(StringComparison.OrdinalIgnoreCase)
        |> assertExnMsg
            """
Subject: x
Should: BeAscending
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 2
  Item: f
- Index: 3
  Item: c
Subject value: [a, b, f, c, a, c]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in ascending order`` () =
        fun () ->
            let x = [ "a"; "b"; "f"; "c"; "a"; "c" ]
            x.Should().BeAscending(StringComparison.OrdinalIgnoreCase, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscending
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 2
  Item: f
- Index: 3
  Item: c
Subject value: [a, b, f, c, a, c]
"""

module ``BeAscending (Culture CompareOptions)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeAscending(CultureInfo.InvariantCulture, CompareOptions.None).Id<And<string list>>().And.Be([])


    let passData = [
        [| box List<string>.Empty; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a"; "b" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "A"; "b" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "a"; "B" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "æ"; "ø"; "å" ]; CultureInfo("nb-NO"); CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly ascending using the specified comparison``
        (subject: seq<string>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        subject.Should().BeAscending(culture, compareOptions)


    let failData = [
        [| box [ "a"; "B" ]; CultureInfo.InvariantCulture; CompareOptions.Ordinal |]
        [| box [ "b"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "æ"; "ø"; "å" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not non-strictly ascending using the specified comparison``
        (subject: seq<string>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        assertFails (fun () -> subject.Should().BeAscending(culture, compareOptions))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () ->
            Unchecked.defaultof<seq<string>>.Should().BeAscending(CultureInfo.InvariantCulture, CompareOptions.Ordinal)
        )


    [<Fact>]
    let ``Fails with expected message if not in ascending order`` () =
        fun () ->
            let x = [ "a"; "b"; "f"; "c"; "a"; "c" ]

            x.Should().BeAscending(CultureInfo("nb-NO"), CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols)
        |> assertExnMsg
            """
Subject: x
Should: BeAscending
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 2
  Item: f
- Index: 3
  Item: c
Subject value: [a, b, f, c, a, c]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in ascending order`` () =
        fun () ->
            let x = [ "a"; "b"; "f"; "c"; "a"; "c" ]

            x
                .Should()
                .BeAscending(
                    CultureInfo("nb-NO"),
                    CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols,
                    "Some reason"
                )
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscending
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 2
  Item: f
- Index: 3
  Item: c
Subject value: [a, b, f, c, a, c]
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
    let ``Passes if non-strictly ascending by the specified projection`` (subject: seq<string | null>) =
        subject.Should().BeAscendingBy(fun s -> s.Length)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeAscendingBy(_.Length))


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


module ``BeAscendingBy (StringComparison)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        List<int>.Empty.Should().BeAscendingBy(string<int>, StringComparison.Ordinal).Id<And<int list>>().And.Be([])


    let passData = [
        [| box List<string>.Empty; StringComparison.Ordinal |]
        [| [ "a" ]; StringComparison.Ordinal |]
        [| [ "a"; "a" ]; StringComparison.Ordinal |]
        [| [ "a2"; "a1" ]; StringComparison.Ordinal |]
        [| [ "a"; "b" ]; StringComparison.Ordinal |]
        [| [ "A"; "b" ]; StringComparison.OrdinalIgnoreCase |]
        [| [ "a"; "B" ]; StringComparison.OrdinalIgnoreCase |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly ascending using the specified comparison``
        (subject: seq<string | null>)
        (comparison: StringComparison)
        =
        subject.Should().BeAscendingBy(_.Substring(0, 1), comparison)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () ->
            Unchecked.defaultof<seq<string>>.Should().BeAscendingBy(_.Substring(0, 1), StringComparison.Ordinal)
        )


    [<Fact>]
    let ``Fails with expected message if not in ascending order`` () =
        fun () ->
            let x = [ "1a"; "2b"; "3f"; "4c"; "5a"; "6c" ]
            x.Should().BeAscendingBy(_.Substring(1, 1), StringComparison.OrdinalIgnoreCase)
        |> assertExnMsg
            """
Subject: x
Should: BeAscendingBy
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 2
  Item: 3f
  Projected: f
- Index: 3
  Item: 4c
  Projected: c
Subject value: [1a, 2b, 3f, 4c, 5a, 6c]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in ascending order`` () =
        fun () ->
            let x = [ "1a"; "2b"; "3f"; "4c"; "5a"; "6c" ]

            x.Should().BeAscendingBy(_.Substring(1, 1), StringComparison.OrdinalIgnoreCase, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscendingBy
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 2
  Item: 3f
  Projected: f
- Index: 3
  Item: 4c
  Projected: c
Subject value: [1a, 2b, 3f, 4c, 5a, 6c]
"""

module ``BeAscendingBy (Culture CompareOptions)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        List<int>.Empty
            .Should()
            .BeAscendingBy(string<int>, CultureInfo.InvariantCulture, CompareOptions.None)
            .Id<And<int list>>()
            .And.Be([])


    let passData = [
        [| box List<string>.Empty; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a2"; "a1" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a"; "b" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "A"; "b" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "a"; "B" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "æ"; "ø"; "å" ]; CultureInfo("nb-NO"); CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly ascending using the specified comparison``
        (subject: seq<string | null>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        subject.Should().BeAscendingBy(_.Substring(0, 1), culture, compareOptions)


    let failData = [
        [| box [ "1a"; "2B" ]; CultureInfo.InvariantCulture; CompareOptions.Ordinal |]
        [|
            box [ "1b"; "2a" ]
            CultureInfo.InvariantCulture
            CompareOptions.IgnoreCase
        |]
        [| [ "1æ"; "2ø"; "3å" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not non-strictly ascending using the specified comparison``
        (subject: seq<string | null>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        assertFails (fun () -> subject.Should().BeAscendingBy(_.Substring(1, 1), culture, compareOptions))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () ->
            Unchecked.defaultof<seq<string>>
                .Should()
                .BeAscendingBy(_.Substring(1, 1), CultureInfo.InvariantCulture, CompareOptions.None)
        )


    [<Fact>]
    let ``Fails with expected message if not in ascending order`` () =
        fun () ->
            let x = [ "1a"; "2b"; "3f"; "4c"; "4a"; "6c" ]

            x
                .Should()
                .BeAscendingBy(
                    _.Substring(1, 1),
                    CultureInfo("nb-NO"),
                    CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols
                )
        |> assertExnMsg
            """
Subject: x
Should: BeAscendingBy
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 2
  Item: 3f
  Projected: f
- Index: 3
  Item: 4c
  Projected: c
Subject value: [1a, 2b, 3f, 4c, 4a, 6c]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in ascending order`` () =
        fun () ->
            let x = [ "1a"; "2b"; "3f"; "4c"; "4a"; "6c" ]

            x
                .Should()
                .BeAscendingBy(
                    _.Substring(1, 1),
                    CultureInfo("nb-NO"),
                    CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols,
                    "Some reason"
                )
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeAscendingBy
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 2
  Item: 3f
  Projected: f
- Index: 3
  Item: 4c
  Projected: c
Subject value: [1a, 2b, 3f, 4c, 4a, 6c]
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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeDescending())


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


module ``BeDescending (StringComparison)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDescending(StringComparison.Ordinal).Id<And<string list>>().And.Be([])


    let passData = [
        [| box List<string>.Empty; StringComparison.Ordinal |]
        [| [ "a" ]; StringComparison.Ordinal |]
        [| [ "a"; "a" ]; StringComparison.Ordinal |]
        [| [ "b"; "a" ]; StringComparison.Ordinal |]
        [| [ "b"; "A" ]; StringComparison.OrdinalIgnoreCase |]
        [| [ "B"; "a" ]; StringComparison.OrdinalIgnoreCase |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly descending using the specified comparison``
        (subject: seq<string>)
        (comparison: StringComparison)
        =
        subject.Should().BeDescending(comparison)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeDescending(StringComparison.Ordinal))


    [<Fact>]
    let ``Fails with expected message if not in descending order`` () =
        fun () ->
            let x = [ "c"; "a"; "c"; "f"; "b"; "a" ]
            x.Should().BeDescending(StringComparison.OrdinalIgnoreCase)
        |> assertExnMsg
            """
Subject: x
Should: BeDescending
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 1
  Item: a
- Index: 2
  Item: c
Subject value: [c, a, c, f, b, a]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in descending order`` () =
        fun () ->
            let x = [ "c"; "a"; "c"; "f"; "b"; "a" ]
            x.Should().BeDescending(StringComparison.OrdinalIgnoreCase, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescending
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 1
  Item: a
- Index: 2
  Item: c
Subject value: [c, a, c, f, b, a]
"""


module ``BeDescending (Culture CompareOptions)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().BeDescending(CultureInfo.InvariantCulture, CompareOptions.None).Id<And<string list>>().And.Be([])


    let passData = [
        [| box List<string>.Empty; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "b"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "b"; "A" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "B"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "å"; "ø"; "æ" ]; CultureInfo("nb-NO"); CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly descending using the specified comparison``
        (subject: seq<string>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        subject.Should().BeDescending(culture, compareOptions)


    let failData = [
        [| box [ "B"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.Ordinal |]
        [| box [ "a"; "b" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "å"; "ø"; "æ" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not non-strictly descending using the specified comparison``
        (subject: seq<string>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        assertFails (fun () -> subject.Should().BeDescending(culture, compareOptions))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () ->
            Unchecked.defaultof<seq<string>>
                .Should()
                .BeDescending(CultureInfo.InvariantCulture, CompareOptions.Ordinal)
        )


    [<Fact>]
    let ``Fails with expected message if not in descending order`` () =
        fun () ->
            let x = [ "c"; "a"; "c"; "f"; "b"; "a" ]

            x.Should().BeDescending(CultureInfo("nb-NO"), CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols)
        |> assertExnMsg
            """
Subject: x
Should: BeDescending
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 1
  Item: a
- Index: 2
  Item: c
Subject value: [c, a, c, f, b, a]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in descending order`` () =
        fun () ->
            let x = [ "c"; "a"; "c"; "f"; "b"; "a" ]

            x
                .Should()
                .BeDescending(
                    CultureInfo("nb-NO"),
                    CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols,
                    "Some reason"
                )
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescending
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 1
  Item: a
- Index: 2
  Item: c
Subject value: [c, a, c, f, b, a]
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
    let ``Passes if non-strictly descending by the specified projection`` (subject: seq<string | null>) =
        subject.Should().BeDescendingBy(fun s -> s.Length)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeDescendingBy(fun s -> s.Length))


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


module ``BeDescendingBy (StringComparison)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        List<int>.Empty.Should().BeDescendingBy(string<int>, StringComparison.Ordinal).Id<And<int list>>().And.Be([])


    let passData = [
        [| box List<string>.Empty; StringComparison.Ordinal |]
        [| [ "a" ]; StringComparison.Ordinal |]
        [| [ "a"; "a" ]; StringComparison.Ordinal |]
        [| [ "a1"; "a2" ]; StringComparison.Ordinal |]
        [| [ "b"; "a" ]; StringComparison.Ordinal |]
        [| [ "b"; "A" ]; StringComparison.OrdinalIgnoreCase |]
        [| [ "B"; "a" ]; StringComparison.OrdinalIgnoreCase |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly descending using the specified comparison``
        (subject: seq<string | null>)
        (comparison: StringComparison)
        =
        subject.Should().BeDescendingBy(_.Substring(0, 1), comparison)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () ->
            Unchecked.defaultof<seq<string>>.Should().BeDescendingBy(_.Substring(0, 1), StringComparison.Ordinal)
        )


    [<Fact>]
    let ``Fails with expected message if not in descending order`` () =
        fun () ->
            let x = [ "6c"; "5a"; "4c"; "3f"; "2b"; "1a" ]

            x.Should().BeDescendingBy(_.Substring(1, 1), StringComparison.OrdinalIgnoreCase)
        |> assertExnMsg
            """
Subject: x
Should: BeDescendingBy
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 1
  Item: 5a
  Projected: a
- Index: 2
  Item: 4c
  Projected: c
Subject value: [6c, 5a, 4c, 3f, 2b, 1a]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in descending order`` () =
        fun () ->
            let x = [ "6c"; "5a"; "4c"; "3f"; "2b"; "1a" ]

            x.Should().BeDescendingBy(_.Substring(1, 1), StringComparison.OrdinalIgnoreCase, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescendingBy
Using StringComparison: OrdinalIgnoreCase
But found:
- Index: 1
  Item: 5a
  Projected: a
- Index: 2
  Item: 4c
  Projected: c
Subject value: [6c, 5a, 4c, 3f, 2b, 1a]
"""

module ``BeDescendingBy (Culture CompareOptions)`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        List<int>.Empty
            .Should()
            .BeDescendingBy(string<int>, CultureInfo.InvariantCulture, CompareOptions.None)
            .Id<And<int list>>()
            .And.Be([])


    let passData = [
        [| box List<string>.Empty; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "a1"; "a2" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "b"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
        [| [ "b"; "A" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "B"; "a" ]; CultureInfo.InvariantCulture; CompareOptions.IgnoreCase |]
        [| [ "å"; "ø"; "æ" ]; CultureInfo("nb-NO"); CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if non-strictly descending using the specified comparison``
        (subject: seq<string | null>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        subject.Should().BeDescendingBy(_.Substring(0, 1), culture, compareOptions)


    let failData = [
        [| box [ "2B"; "1a" ]; CultureInfo.InvariantCulture; CompareOptions.Ordinal |]
        [|
            box [ "2a"; "1b" ]
            CultureInfo.InvariantCulture
            CompareOptions.IgnoreCase
        |]
        [| [ "3å"; "2ø"; "1æ" ]; CultureInfo.InvariantCulture; CompareOptions.None |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not non-strictly descending using the specified comparison``
        (subject: seq<string | null>)
        (culture: CultureInfo)
        (compareOptions: CompareOptions)
        =
        assertFails (fun () -> subject.Should().BeDescendingBy(_.Substring(1, 1), culture, compareOptions))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () ->
            Unchecked.defaultof<seq<string>>
                .Should()
                .BeDescendingBy(_.Substring(1, 1), CultureInfo.InvariantCulture, CompareOptions.None)
        )


    [<Fact>]
    let ``Fails with expected message if not in descending order`` () =
        fun () ->
            let x = [ "6c"; "5a"; "4c"; "3f"; "2b"; "1a" ]

            x
                .Should()
                .BeDescendingBy(
                    _.Substring(1, 1),
                    CultureInfo("nb-NO"),
                    CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols
                )
        |> assertExnMsg
            """
Subject: x
Should: BeDescendingBy
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 1
  Item: 5a
  Projected: a
- Index: 2
  Item: 4c
  Projected: c
Subject value: [6c, 5a, 4c, 3f, 2b, 1a]
"""


    [<Fact>]
    let ``Fails with expected message with because if not in descending order`` () =
        fun () ->
            let x = [ "6c"; "5a"; "4c"; "3f"; "2b"; "1a" ]

            x
                .Should()
                .BeDescendingBy(
                    _.Substring(1, 1),
                    CultureInfo("nb-NO"),
                    CompareOptions.IgnoreCase ||| CompareOptions.IgnoreSymbols,
                    "Some reason"
                )
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeDescendingBy
In culture: nb-NO
With CompareOptions: IgnoreCase, IgnoreSymbols
But found:
- Index: 1
  Item: 5a
  Projected: a
- Index: 2
  Item: 4c
  Projected: c
Subject value: [6c, 5a, 4c, 3f, 2b, 1a]
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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeStrictlyAscending())


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
    let ``Passes if strictly ascending by the specified projection`` (subject: seq<string | null>) =
        subject.Should().BeStrictlyAscendingBy(fun s -> s.Length)


    let failData = [
        // Comment to force break for readability
        [| [ "a"; "b" ] |]
        [| [ "asd"; "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not strictly ascending by the specified projection`` (subject: seq<string | null>) =
        assertFails (fun () -> subject.Should().BeStrictlyAscendingBy(fun s -> s.Length))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeStrictlyAscendingBy(fun s -> s.Length))


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeStrictlyDescending())


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
    let ``Passes if strictly descending by the specified projection`` (subject: seq<string | null>) =
        subject.Should().BeStrictlyDescendingBy(fun s -> s.Length)


    let failData = [
        // Comment to force break for readability
        [| [ "a"; "b" ] |]
        [| [ "a"; "asd" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not strictly descending by the specified projection`` (subject: seq<string | null>) =
        assertFails (fun () -> subject.Should().BeStrictlyDescendingBy(fun s -> s.Length))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeStrictlyDescendingBy(fun s -> s.Length))


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeSupersetOf([]))


    [<Fact>]
    let ``Throws if subset is null`` () =
        assertThrows (fun () -> [ 1 ].Should().BeSupersetOf(Unchecked.defaultof<_>))


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeProperSupersetOf([]))


    [<Fact>]
    let ``Throws if subset is null`` () =
        assertThrows (fun () -> [ 1 ].Should().BeProperSupersetOf(Unchecked.defaultof<_>))


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
    let ``Throws if superset is null`` () =
        assertThrows (fun () -> [ 1 ].Should().BeSubsetOf(Unchecked.defaultof<_>))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeSubsetOf([]))


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
    let ``Throws if superset is null`` () =
        assertThrows (fun () -> [ 1 ].Should().BeProperSubsetOf(Unchecked.defaultof<_>))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().BeProperSubsetOf([]))


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
    let ``Passes for separately allocated structurally equal arrays`` () =
        [ [| 1; 2 |] ].Should().IntersectWith([ [| 1; 2 |] ])


    [<Fact>]
    let ``Passes for structurally equal nested arrays`` () =
        [ [| [| 1; 2 |] |] ].Should().IntersectWith([ [| [| 1; 2 |] |] ])


    [<Fact>]
    let ``Fails for structurally different arrays`` () =
        assertFails (fun () -> [ [| 1; 2 |] ].Should().IntersectWith([ [| 2; 1 |] ]))


    [<Fact>]
    let ``Passes for shared null items`` () =
        [ nul<string> ].Should().IntersectWith([ nul<string> ])


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().IntersectWith([]))


    [<Fact>]
    let ``Throws if other is null`` () =
        assertThrows (fun () -> [ 1 ].Should().IntersectWith(Unchecked.defaultof<_>))


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


    let private nanCasesFor name nan one two =
        let lists = [ []; [ nan ]; [ one ]; [ two ]; [ nan; nan ]; [ nan; one ]; [ one; nan; one ] ]

        [
            for i, subject in List.indexed lists do
                for j, other in List.indexed lists do
                    let run () =
                        let intersects =
                            subject |> List.exists (fun item -> other |> List.exists ((=) item))

                        let assertDisjoint () =
                            (singlePass subject).Should().NotIntersectWith(singlePass other) |> ignore

                        if intersects then
                            assertFails assertDisjoint |> ignore
                        else
                            assertDisjoint ()

                    yield [| box $"%s{name}: %i{i}, %i{j}"; box run |]
        ]


    let nanCases = [
        yield! nanCasesFor "double" Double.NaN 1.0 2.0
        yield! nanCasesFor "single" Single.NaN 1.0f 2.0f
        yield! nanCasesFor "Half" Half.NaN Half.Zero Half.One
        yield! nanCasesFor "array" [| Double.NaN |] [| 1.0 |] [| 2.0 |]
        yield! nanCasesFor "record" {| Value = Double.NaN |} {| Value = 1.0 |} {| Value = 2.0 |}
    ]


    [<Theory>]
    [<MemberData(nameof nanCases)>]
    let ``NaN intersection agrees with itemwise FSharp equality for single-pass inputs``
        (_name: string)
        (run: unit -> unit)
        =
        run ()


    [<Fact>]
    let ``Diagnostics exclude unmatched NaNs and retain distinct common values`` () =
        let ex =
            assertFails (fun () -> [ Double.NaN; 1.0; 1.0; 2.0 ].Should().NotIntersectWith([ 1.0; Double.NaN; 1.0 ]))

        let commonItems =
            ex.FailureData.Extra
            |> List.find (fun (key, _) -> key = "But found common items")
            |> snd
            |> unbox<seq<double>>
            |> Seq.toList

        Assert.Equal<double list>([ 1.0 ], commonItems)


    [<Fact>]
    let ``Fails for separately allocated structurally equal arrays`` () =
        assertFails (fun () -> [ [| 1; 2 |] ].Should().NotIntersectWith([ [| 1; 2 |] ]))


    [<Fact>]
    let ``Fails for structurally equal nested arrays`` () =
        assertFails (fun () -> [ [| [| 1; 2 |] |] ].Should().NotIntersectWith([ [| [| 1; 2 |] |] ]))


    [<Fact>]
    let ``Passes for structurally different arrays`` () =
        [ [| 1; 2 |] ].Should().NotIntersectWith([ [| 2; 1 |] ])


    [<Fact>]
    let ``Fails for shared null items`` () =
        assertFails (fun () -> [ nul<string> ].Should().NotIntersectWith([ nul<string> ]))


    [<Fact>]
    let ``Can be chained with And`` () =
        [].Should().NotIntersectWith([]).Id<And<int list>>().And.Be([])


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<seq<string>>.Should().NotIntersectWith([]))


    [<Fact>]
    let ``Throws if other is null`` () =
        assertThrows (fun () -> [ 1 ].Should().NotIntersectWith(Unchecked.defaultof<_>))


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
