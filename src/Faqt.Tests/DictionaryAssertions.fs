module DictionaryAssertions

open System
open System.Collections.Generic
open Faqt
open Xunit


module AllSatisfy =


    [<Fact>]
    let ``Passes if all of the inner assertions passes and can be chained with And`` () =
        Map.empty
            .Add("foo", 1)
            .Should()
            .AllSatisfy(fun x -> x.Key.Should().Pass())
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty.Add("foo", 1))


    [<Fact>]
    let ``Passes if subject is empty`` () =
        Map.empty<string, int>.Should().AllSatisfy(fun x -> x.Key.Should().Fail())


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
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
            let x: IDictionary<string, int> = null
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
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]
            x.Should().AllSatisfy(fun y -> y.Value.Should().Test(y.Value = 1))

        |> assertExnMsg
            """
Subject: x
Should: AllSatisfy
Failures:
- Key: test
  Failure:
    Subject: y.Value
    Should: Test
- Key: foobar
  Failure:
    Subject: y.Value
    Should: Test
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fail to satisfy the assertion`` () =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

            x
                .Should()
                .AllSatisfy((fun y -> y.Value.Should().Test(y.Value = 1)), "Some reason")

        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: AllSatisfy
Failures:
- Key: test
  Failure:
    Subject: y.Value
    Should: Test
- Key: foobar
  Failure:
    Subject: y.Value
    Should: Test
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


module SatisfyRespectively =


    [<Fact>]
    let ``Passes if same length and all of the inner assertions passes and can be chained with And`` () =
        Map.empty
            .Add("foo", 1)
            .Add("bar", 2)
            .Should()
            .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty.Add("foo", 1).Add("bar", 2))


    [<Fact>]
    let ``Passes if subject and assertions are empty`` () =
        Map.empty<string, int>.Should().SatisfyRespectively([])


    [<Fact>]
    let ``Throws ArgumentNullException if assertions is null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            Map.empty<string, int>.Should().SatisfyRespectively(null) |> ignore
        )


    [<Fact>]
    let ``Fails with expected message if subject is null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
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
            let x: IDictionary<string, int> = null
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
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

            x
                .Should()
                .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])
        |> assertExnMsg
            """
Subject: x
Should: SatisfyRespectively
Expected count: 2
Actual count: 3
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


    [<Fact>]
    let ``Fails with expected message with because if subject does not contain one item per assertion`` () =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

            x
                .Should()
                .SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: SatisfyRespectively
Expected count: 2
Actual count: 3
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


    [<Fact>]
    let ``Fails with expected message if at least one of the items fail to satisfy the assertion`` () =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

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
- Key: asd
  Failure:
    Subject: x1
    Should: Fail
- Key: foobar
  Failure:
    Subject: x3
    Should: Fail
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fail to satisfy the assertion`` () =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

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
- Key: asd
  Failure:
    Subject: x1
    Should: Fail
- Key: foobar
  Failure:
    Subject: x3
    Should: Fail
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


module HaveLength =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().HaveLength(1)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().HaveLength(2)
        |> assertExnMsg
            """
Subject: x
Should: HaveLength
Expected: 2
But was: 1
Subject value:
  a: 1
"""


module BeEmpty =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict<string, int> []
        x.Should().BeEmpty()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().BeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: BeEmpty
But was:
  a: 1
"""


module NotBeEmpty =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().NotBeEmpty()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict<string, int> []
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: NotBeEmpty
But was: {}
"""


module BeNullOrEmpty =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict<string, int> []
        x.Should().BeNullOrEmpty()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().BeNullOrEmpty()
        |> assertExnMsg
            """
Subject: x
Should: BeNullOrEmpty
But was:
  a: 1
"""


module ``Contain (seq assertion)`` =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().Contain(KeyValuePair("a", 1))


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().Contain(KeyValuePair("a", 2))
        |> assertExnMsg
            """
Subject: x
Should: Contain
Item:
  Key: a
  Value: 2
But was:
  a: 1
"""


module ``Contain(key, value)`` =


    [<Fact>]
    let ``Can be chained with AndDerived with found value`` () =
        (Map.ofList [ "a", 1 ])
            .Should()
            .Contain("a", 1)
            .Id<AndDerived<Map<string, int>, KeyValuePair<string, int>>>()
            .That.Should()
            .Be(KeyValuePair("a", 1))


    let passData = [
        [| box (dict [ "a", "1" ]); "a"; "1" |]
        [| dict [ "a", "1"; "b", "2" ]; "b"; "2" |]
        [| dict [ "a", "1"; null, null ]; null; null |]
        [| dict [ "a", "1"; null, "2" ]; null; "2" |]
        [| dict [ "a", "1"; "b", null ]; "b"; null |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if dict contains the specified key/value``
        (subject: IDictionary<string, string>)
        (key: string)
        (value: string)
        =
        subject.Should().Contain(key, value)


    let failData = [
        [| box null; "a"; "1" |]
        [| dict<string, string> []; "a"; "1" |]
        [| dict [ "a", "2" ]; "a"; "1" |]
        [| dict [ "b", "1" ]; "a"; "1" |]
        [| dict [ "a", "1" ]; "b"; "2" |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if dict is null or does not contain the specified key/value``
        (subject: IDictionary<string, string>)
        (key: string)
        (value: string)
        =
        assertFails (fun () -> subject.Should().Contain(key, value))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            x.Should().Contain("c", 3)
        |> assertExnMsg
            """
Subject: x
Should: Contain
Item:
  Key: c
  Value: 3
But was:
  a: 1
  b: 2
"""


    [<Fact>]
    let ``Fails with expected message with because if not containing value`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            x.Should().Contain("c", 3, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Contain
Item:
  Key: c
  Value: 3
But was:
  a: 1
  b: 2
"""


module ``NotContain (seq assertion)`` =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().NotContain(KeyValuePair("a", 2))


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().NotContain(KeyValuePair("a", 1))
        |> assertExnMsg
            """
Subject: x
Should: NotContain
Item:
  Key: a
  Value: 1
But was:
  a: 1
"""


module NotContain =


    [<Fact>]
    let ``Can be chained with And`` () =
        Map.empty<string, int>
            .Should()
            .NotContain("a", 1)
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty<string, int>)


    let passData = [
        [| box null; "a"; "1" |]
        [| dict<string, string> []; "a"; "1" |]
        [| dict [ "a", "2" ]; "a"; "1" |]
        [| dict [ "b", "1" ]; "a"; "1" |]
        [| dict [ "a", "1" ]; "b"; "2" |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if dict is null or does not contain specified key/value``
        (subject: IDictionary<string, string>)
        (key: string)
        (value: string)
        =
        subject.Should().NotContain(key, value)


    let failData = [
        [| box (dict [ "a", "1" ]); "a"; "1" |]
        [| dict [ "a", "1"; "b", "2" ]; "b"; "2" |]
        [| dict [ "a", "1"; null, null ]; null; null |]
        [| dict [ "a", "1"; null, "2" ]; null; "2" |]
        [| dict [ "a", "1"; "b", null ]; "b"; null |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if dict contains the specified key/value``
        (subject: IDictionary<string, string>)
        (key: string)
        (value: string)
        =
        assertFails (fun () -> subject.Should().NotContain(key, value))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            x.Should().NotContain("a", 1)
        |> assertExnMsg
            """
Subject: x
Should: NotContain
Item:
  Key: a
  Value: 1
But was:
  a: 1
  b: 2
"""


    [<Fact>]
    let ``Fails with expected message with because if not containing value`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            x.Should().NotContain("a", 1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContain
Item:
  Key: a
  Value: 1
But was:
  a: 1
  b: 2
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
        [| dict<string, string> []; dict<string, string> [] |]
        [| dict [ "a", "1" ]; dict [ "a", "1" ] |]
        [| dict [ "a", "1"; "b", "2" ]; dict [ "b", "2"; "a", "1" ] |]
        [| dict [ "a", "1"; null, null ]; dict [ null, null; "a", "1" ] |]
        [| dict [ "a", "1"; null, "2" ]; dict [ null, "2"; "a", "1" ] |]
        [| dict [ "a", "1"; "b", null ]; dict [ "b", null; "a", "1" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if both are null or contain the same key-value pairs``
        (subject: IDictionary<string, string>)
        (expected: IDictionary<string, string>)
        =
        subject.Should().HaveSameItemsAs(expected)


    let failData = [
        [| box null; dict<string, string> [] |]
        [| dict<string, string> []; dict [ "a", "1" ] |]
        [| dict [ "a", "1" ]; dict [ "a", "1"; "b", "2" ] |]
        [| dict [ "a", "1" ]; dict [ "b", "2" ] |]
        [| dict [ "a", "1"; null, null ]; dict [ "a", "1" ] |]
        [| dict [ "a", "1"; null, "2" ]; dict [ "a", "1" ] |]
        [| dict [ "a", "1"; "b", null ]; dict [ "a", "1" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if only one is null or they do not contain the same key-value pairs``
        (subject: IDictionary<string, string>)
        (expected: IDictionary<string, string>)
        =
        assertFails (fun () -> subject.Should().HaveSameItemsAs(expected))


    [<Fact>]
    let ``Fails with expected message if only subject is null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
            x.Should().HaveSameItemsAs(dict [])
        |> assertExnMsg
            """
Subject: x
Should: HaveSameItemsAs
Expected: {}
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if only subject is null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
            x.Should().HaveSameItemsAs(dict [], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveSameItemsAs
Expected: {}
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if items are not equal`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2; "c", 3; "d", 4 ]
            let y = dict [ "a", 2; "b", 1; "c", 3; "e", 5 ]
            x.Should().HaveSameItemsAs(y)
        |> assertExnMsg
            """
Subject: x
Should: HaveSameItemsAs
Missing keys: [e]
Additional keys: [d]
Different values:
  a:
    Expected: 2
    Actual: 1
  b:
    Expected: 1
    Actual: 2
Expected:
  a: 2
  b: 1
  c: 3
  e: 5
Actual:
  a: 1
  b: 2
  c: 3
  d: 4
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2; "c", 3; "d", 4 ]
            let y = dict [ "a", 2; "b", 1; "c", 3; "e", 5 ]
            x.Should().HaveSameItemsAs(y, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveSameItemsAs
Missing keys: [e]
Additional keys: [d]
Different values:
  a:
    Expected: 2
    Actual: 1
  b:
    Expected: 1
    Actual: 2
Expected:
  a: 2
  b: 1
  c: 3
  e: 5
Actual:
  a: 1
  b: 2
  c: 3
  d: 4
"""


module ContainExactlyOneItem =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().ContainExactlyOneItem()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            x.Should().ContainExactlyOneItem()
        |> assertExnMsg
            """
Subject: x
Should: ContainExactlyOneItem
But length was: 2
Subject value:
  a: 1
  b: 2
"""


module ContainExactlyOneItemMatching =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1; "b", 2 ]
        x.Should().ContainExactlyOneItemMatching(fun kvp -> kvp.Key = "a")


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            x.Should().ContainExactlyOneItemMatching(fun kvp -> kvp.Key <> "c")
        |> assertExnMsg
            """
Subject: x
Should: ContainExactlyOneItemMatching
But found: 2
Matching items:
- Key: a
  Value: 1
- Key: b
  Value: 2
Subject value:
  a: 1
  b: 2
"""


module ContainAtLeastOneItem =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().ContainAtLeastOneItem()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict<string, int> []
            x.Should().ContainAtLeastOneItem()
        |> assertExnMsg
            """
Subject: x
Should: ContainAtLeastOneItem
But was: {}
"""


module ContainAtLeastOneItemMatching =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().ContainAtLeastOneItemMatching(fun kvp -> kvp.Key = "a")


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainAtLeastOneItemMatching(fun kvp -> kvp.Key = "b")
        |> assertExnMsg
            """
Subject: x
Should: ContainAtLeastOneItemMatching
But found: 0
Matching items: []
Subject value:
  a: 1
"""


module ContainItemsMatching =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().ContainItemsMatching(fun kvp -> kvp.Key = "a")


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainItemsMatching(fun kvp -> kvp.Key = "b")
        |> assertExnMsg
            """
Subject: x
Should: ContainItemsMatching
But found: 0
Subject value:
  a: 1
"""


module BeSupersetOf =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1; "b", 2 ]
        let y = dict [ "a", 1 ]
        x.Should().BeSupersetOf(y)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            let y = dict [ "a", 1; "b", 2 ]
            x.Should().BeSupersetOf(y)
        |> assertExnMsg
            """
Subject: x
Should: BeSupersetOf
Subset:
  a: 1
  b: 2
But lacked:
- Key: b
  Value: 2
Subject value:
  a: 1
"""


module BeProperSupersetOf =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1; "b", 2 ]
        let y = dict [ "a", 1 ]
        x.Should().BeProperSupersetOf(y)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            let y = dict [ "a", 1; "b", 2 ]
            x.Should().BeProperSupersetOf(y)
        |> assertExnMsg
            """
Subject: x
Should: BeProperSupersetOf
Subset:
  a: 1
  b: 2
But lacked:
- Key: b
  Value: 2
Subject value:
  a: 1
"""


module BeSubsetOf =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        let y = dict [ "a", 1; "b", 2 ]
        x.Should().BeSubsetOf(y)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            let y = dict [ "a", 1 ]
            x.Should().BeSubsetOf(y)
        |> assertExnMsg
            """
Subject: x
Should: BeSubsetOf
Superset:
  a: 1
But had extra items:
- Key: b
  Value: 2
Subject value:
  a: 1
  b: 2
"""


module BeProperSubsetOf =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        let y = dict [ "a", 1; "b", 2 ]
        x.Should().BeProperSubsetOf(y)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            let y = dict [ "a", 1 ]
            x.Should().BeProperSubsetOf(y)
        |> assertExnMsg
            """
Subject: x
Should: BeProperSubsetOf
Superset:
  a: 1
But had extra items:
- Key: b
  Value: 2
Subject value:
  a: 1
  b: 2
"""


module IntersectWith =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1; "b", 2 ]
        let y = dict [ "a", 1; "c", 3 ]
        x.Should().IntersectWith(y)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            let y = dict [ "b", 2 ]
            x.Should().IntersectWith(y)
        |> assertExnMsg
            """
Subject: x
Should: IntersectWith
Other:
  b: 2
But had no common items: []
Subject value:
  a: 1
"""


module NotIntersectWith =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        let y = dict [ "b", 2 ]
        x.Should().NotIntersectWith(y)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 2 ]
            let y = dict [ "a", 1; "c", 3 ]
            x.Should().NotIntersectWith(y)
        |> assertExnMsg
            """
Subject: x
Should: NotIntersectWith
Other:
  a: 1
  c: 3
But found common items:
- Key: a
  Value: 1
Subject value:
  a: 1
  b: 2
"""


module ContainKey =


    [<Fact>]
    let ``Can be chained with AndDerived with found KeyValuePair`` () =
        Map.empty
            .Add("a", 1)
            .Should()
            .ContainKey("a")
            .Id<AndDerived<Map<string, int>, KeyValuePair<string, int>>>()
            .Whose.Value.Should(())
            .Be(1)


    [<Fact>]
    let ``Passes if dict contains the key`` () =
        let x = dict [ "a", 1; "b", 2 ]
        x.Should().ContainKey("a")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
            x.Should().ContainKey("a")
        |> assertExnMsg
            """
Subject: x
Should: ContainKey
Key: a
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
            x.Should().ContainKey("a", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainKey
Key: a
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not containing key`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainKey("b")
        |> assertExnMsg
            """
Subject: x
Should: ContainKey
Key: b
But was:
  a: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if not containing key`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainKey("b", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainKey
Key: b
But was:
  a: 1
"""


module NotContainKey =


    [<Fact>]
    let ``Can be chained with And`` () =
        Map.empty<string, int>
            .Should()
            .NotContainKey("a")
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty<string, int>)


    [<Fact>]
    let ``Passes if dict does not contain the key`` () =
        let x = dict [ "a", 1 ]
        x.Should().NotContainKey("b")


    [<Fact>]
    let ``Passes if null`` () =
        let x: IDictionary<string, int> = null
        x.Should().NotContainKey("a")


    [<Fact>]
    let ``Fails with expected message if containing key`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().NotContainKey("a")
        |> assertExnMsg
            """
Subject: x
Should: NotContainKey
Key: a
But found value: 1
Subject value:
  a: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if containing key`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().NotContainKey("a", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContainKey
Key: a
But found value: 1
Subject value:
  a: 1
"""


module ContainValue =


    [<Fact>]
    let ``Can be chained with AndDerived with found KeyValuePair`` () =
        Map.empty
            .Add("a", 1)
            .Should()
            .ContainValue(1)
            .Id<AndDerived<Map<string, int>, KeyValuePair<string, int>>>()
            .Whose.Key.Should(())
            .Be("a")


    [<Fact>]
    let ``Passes if dict contains the value`` () =
        let x = dict [ "a", 1; "b", 2 ]
        x.Should().ContainValue(1)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
            x.Should().ContainValue(1)
        |> assertExnMsg
            """
Subject: x
Should: ContainValue
Value: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message with because if null`` () =
        fun () ->
            let x: IDictionary<string, int> = null
            x.Should().ContainValue(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainValue
Value: 1
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if not containing value`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainValue(2)
        |> assertExnMsg
            """
Subject: x
Should: ContainValue
Value: 2
But was:
  a: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if not containing value`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainValue(2, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainValue
Value: 2
But was:
  a: 1
"""


module NotContainValue =


    [<Fact>]
    let ``Can be chained with And`` () =
        Map.empty<string, int>
            .Should()
            .NotContainValue(1)
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty<string, int>)


    [<Fact>]
    let ``Passes if dict does not contain the value`` () =
        let x = dict [ "a", 1 ]
        x.Should().NotContainValue(2)


    [<Fact>]
    let ``Passes if null`` () =
        let x: IDictionary<string, int> = null
        x.Should().NotContainValue(1)


    [<Fact>]
    let ``Fails with expected message if containing value`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 1 ]
            x.Should().NotContainValue(1)
        |> assertExnMsg
            """
Subject: x
Should: NotContainValue
Value: 1
But found value for keys: [a, b]
Subject value:
  a: 1
  b: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if containing value`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 1 ]
            x.Should().NotContainValue(1, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContainValue
Value: 1
But found value for keys: [a, b]
Subject value:
  a: 1
  b: 1
"""
