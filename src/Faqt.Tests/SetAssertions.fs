module SetAssertions

open System
open Faqt
open Xunit


type RefRecord = { Id: int }


type ComparisonItem(id: int) =
    member _.Id = id

    override this.Equals(other) = Object.ReferenceEquals(this, other)

    override this.GetHashCode() =
        System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this)

    interface IComparable with
        member _.CompareTo(other) =
            match other with
            | null -> 1
            | :? ComparisonItem as other -> compare id other.Id
            | _ -> invalidArg (nameof other) "Expected a ComparisonItem"


module Contain =


    [<Fact>]
    let ``Can be chained with AndDerived with found value`` () =
        (set [ 1 ]).Should().Contain(1).Id<AndDerived<Set<int>, int>>().That.Should().Be(1)


    [<Fact>]
    let ``Returns the actual matched item as the derived value`` () =
        let actual = { Id = 1 }
        let expected = { Id = 1 }
        let derived = (set [ actual ]).Should().Contain(expected).That

        Object.ReferenceEquals(actual, derived).Should().BeTrue() |> ignore


    [<Fact>]
    let ``Passes when the set contains NaN`` () =
        (set [ Double.NaN ]).Should().Contain(Double.NaN)


    [<Fact>]
    let ``Returns the stored item when comparison and equality differ`` () =
        let actual = ComparisonItem(1)
        let expected = ComparisonItem(1)
        let derived = (set [ actual ]).Should().Contain(expected).That

        Object.ReferenceEquals(actual, derived).Should().BeTrue() |> ignore


    let passData = [
        [| box (set [ "a" ]); "a" |]
        [| set [ "a"; "b" ]; "a" |]
        [| set [ nul<string> ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if set contains value`` (subject: Set<string | null>) (value: string | null) =
        subject.Should().Contain(value)


    let failData = [
        [| box Set.empty<string>; "a" |]
        [| set [ "a" ]; "b" |]
        [| set [ nul<string> ]; "a" |]
        [| set [ "a" ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing value`` (subject: Set<string | null>) (value: string | null) =
        assertFails (fun () -> subject.Should().Contain(value))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<Set<string>>.Should().Contain(""))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = Set.empty<int>
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
            let x = Set.empty<int>
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
        (set [ 1 ]).Should().NotContain(2).Id<And<Set<int>>>().And.Be(set [ 1 ])


    let passData = [
        [| box Set.empty<string>; "a" |]
        [| set [ "a" ]; "b" |]
        [| set [ nul<string> ]; "a" |]
        [| set [ "a" ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if not containing value`` (subject: Set<string | null>) (value: string | null) =
        subject.Should().NotContain(value)


    let failData = [
        [| box (set [ "a" ]); "a" |]
        [| set [ "a"; "b" ]; "a" |]
        [| set [ nul<string> ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if sequence contains value`` (subject: Set<string | null>) (value: string | null) =
        assertFails (fun () -> subject.Should().NotContain(value))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<Set<string>>.Should().NotContain(""))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = set [ 1; 2 ]
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
            let x = set [ 1; 2 ]
            x.Should().NotContain(2, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContain
Item: 2
But was: [1, 2]
"""
