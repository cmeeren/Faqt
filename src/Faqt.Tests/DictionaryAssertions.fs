module DictionaryAssertions

open System
open System.Collections.Generic
open System.Globalization
open Faqt
open Xunit


module EvaluationErrors =


    let cases = evaluationErrorCases [ "AllSatisfy"; "SatisfyRespectively" ]


    [<Theory>]
    [<MemberData(nameof cases)>]
    let ``Entry errors cannot become successful negation or alternatives`` assertion composition cancellation =
        assertEvaluationError
            composition
            cancellation
            (fun error ->
                let callback (_: KeyValuePair<int, int>) : unit = raise error
                let subject = Map [ 1, 1 ]

                match assertion with
                | "AllSatisfy" -> subject.Should().AllSatisfy(callback) |> ignore
                | "SatisfyRespectively" -> subject.Should().SatisfyRespectively([ callback ]) |> ignore
                | _ -> failwith "Unknown assertion"
            )


    [<Theory>]
    [<InlineData("AllSatisfy")>]
    [<InlineData("SatisfyRespectively")>]
    let ``Aggregators stop at errors after earlier entry failures`` assertion =
        assertEvaluationError
            "Direct"
            false
            (fun error ->
                let callback (item: KeyValuePair<int, int>) =
                    match item.Key with
                    | 1 -> item.Should().Fail() |> ignore
                    | 2 -> raise error
                    | _ -> failwith "This later entry must not run"

                let subject = Map [ 1, 1; 2, 2; 3, 3 ]

                match assertion with
                | "AllSatisfy" -> subject.Should().AllSatisfy(callback) |> ignore
                | "SatisfyRespectively" ->
                    subject.Should().SatisfyRespectively([ callback; callback; callback ]) |> ignore
                | _ -> failwith "Unknown assertion"
            )


    [<Theory>]
    [<InlineData("AllSatisfy")>]
    [<InlineData("SatisfyRespectively")>]
    let ``Ordinary entry failures are still aggregated`` assertion =
        let callback (item: KeyValuePair<string, int>) = item.Value.Should().Be(0)
        let subject = Map [ "first", 1; "second", 2 ]

        let error =
            assertFails (fun () ->
                match assertion with
                | "AllSatisfy" -> subject.Should().AllSatisfy(callback) |> ignore
                | "SatisfyRespectively" -> subject.Should().SatisfyRespectively([ callback; callback ]) |> ignore
                | _ -> failwith "Unknown assertion"
            )

        Assert.Contains("Key: first", error.Message)
        Assert.Contains("Key: second", error.Message)


type RefRecord = { Id: int }


let singlePass (items: seq<'a>) : seq<'a> =
    let queue = Queue<'a>(items)

    seq {
        while queue.Count > 0 do
            yield queue.Dequeue()
    }


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
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, int>>.Should().AllSatisfy(_.Should().Pass()))


    [<Fact>]
    let ``Fails with expected message if at least one of the items fails to satisfy the assertion or throws`` () =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

            x
                .Should()
                .AllSatisfy(fun y ->
                    if y.Value = 3 then
                        failwith "foo"
                    else
                        y.Value.Should().Test(y.Value = 1)
                )

        |> assertErrorMsgWildcard
            """
Subject: x
Should: AllSatisfy
Failures:
- Key: test
  Failure:
    Subject: y.Value
    Should: Test
- Key: foobar
  Exception: |-
    System.Exception: foo
*
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fails to satisfy the assertion or throws``
        ()
        =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

            x
                .Should()
                .AllSatisfy(
                    (fun y ->
                        if y.Value = 3 then
                            failwith "foo"
                        else
                            y.Value.Should().Test(y.Value = 1)
                    ),
                    "Some reason"
                )

        |> assertErrorMsgWildcard
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
  Exception: |-
    System.Exception: foo
*
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
    let ``Fails for single-pass assertions when an inner assertion fails`` () =
        assertFails (fun () ->
            (dict [ "a", 1; "b", 2 ])
                .Should()
                .SatisfyRespectively(
                    singlePass [
                        fun (x: KeyValuePair<string, int>) -> x.Value.Should().Be(99)
                        fun (x: KeyValuePair<string, int>) -> x.Value.Should().Be(98)
                    ]
                )
            |> ignore
        )


    [<Fact>]
    let ``Throws if assertions is null`` () =
        assertThrows (fun () -> Map.empty<string, int>.Should().SatisfyRespectively(Unchecked.defaultof<_>))


    [<Fact>]
    let ``Fails with expected message if subject does not contain one item per assertion`` () =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ])
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

            x.Should().SatisfyRespectively([ (fun x -> x.Should().Pass()); fun x -> x.Should().Pass() ], "Some reason")
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
    let ``Fails with expected message if at least one of the items fails to satisfy the assertion or throws`` () =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

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
- Key: asd
  Failure:
    Subject: x1
    Should: Fail
- Key: foobar
  Exception: |-
    System.Exception: foo
*
Subject value:
  asd: 1
  test: 2
  foobar: 3
"""


    [<Fact>]
    let ``Fails with expected message with because if at least one of the items fails to satisfy the assertion or throws``
        ()
        =
        fun () ->
            let x = dict [ "asd", 1; "test", 2; "foobar", 3 ]

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
- Key: asd
  Failure:
    Subject: x1
    Should: Fail
- Key: foobar
  Exception: |-
    System.Exception: foo
*
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


module ``Contain key and value`` =


    [<Fact>]
    let ``Can be chained with AndDerived with found value`` () =
        (Map.ofList [ "a", 1 ])
            .Should()
            .Contain("a", 1)
            .Id<AndDerived<Map<string, int>, KeyValuePair<string, int>>>()
            .That.Should()
            .Be(KeyValuePair("a", 1))


    [<Fact>]
    let ``Returns the supplied key and stored value as the derived value`` () =
        let actualKey = { Id = 1 }
        let actualValue = { Id = 2 }
        let expectedKey = { Id = 1 }
        let expectedValue = { Id = 2 }

        let derived =
            (dict [ actualKey, actualValue ]).Should().Contain(expectedKey, expectedValue).That

        Object.ReferenceEquals(expectedKey, derived.Key).Should().BeTrue() |> ignore
        Object.ReferenceEquals(actualValue, derived.Value).Should().BeTrue() |> ignore


    [<Fact>]
    let ``Uses the dictionary key comparer`` () =
        let subject = Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        subject.Add("actual", 1)

        subject.Should().Contain("ACTUAL", 1)


    [<Fact>]
    let ``Uses the dictionary value equality`` () =
        let subject = Dictionary<string, int[]>()
        subject.Add("key", [| 1 |])

        assertFails (fun () -> subject.Should().Contain("key", [| 1 |]))


    [<Fact>]
    let ``Supports keys and values without FSharp equality`` () =
        let key x = x + 1
        let value x = x + 2

        let subject =
            Dictionary<int -> int, int -> int>(EqualityComparer<int -> int>.Default)

        subject.Add(key, value)

        subject.Should().Contain(key, value)


    let passData = [
        [| box (dict [ "a", "1" ]); "a"; "1" |]
        [| dict [ "a", "1"; "b", "2" ]; "b"; "2" |]
        [| dict [ asNull "a", asNull "1"; null, null ]; null; null |]
        [| dict [ asNull "a", "1"; null, "2" ]; null; "2" |]
        [| dict [ asNull "a", asNull "1"; "b", null ]; "b"; null |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if dict contains the specified key/value``
        (subject: IDictionary<string | null, string | null>)
        (key: string | null)
        (value: string | null)
        =
        subject.Should().Contain(key, value)


    let failData = [
        [| box (dict<string | null, string | null> []); "a"; "1" |]
        [| dict [ "a", "2" ]; "a"; "1" |]
        [| dict [ "b", "1" ]; "a"; "1" |]
        [| dict [ "a", "1" ]; "b"; "2" |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if dict does not contain the specified key/value``
        (subject: IDictionary<string | null, string | null>)
        (key: string | null)
        (value: string | null)
        =
        assertFails (fun () -> subject.Should().Contain(key, value))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, int>>.Should().Contain("", 1))


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
        Map.empty<string, int>.Should().NotContain("a", 1).Id<And<Map<string, int>>>().And.Be(Map.empty<string, int>)


    let passData = [
        [| box (dict<string | null, string | null> []); "a"; "1" |]
        [| dict [ "a", "2" ]; "a"; "1" |]
        [| dict [ "b", "1" ]; "a"; "1" |]
        [| dict [ "a", "1" ]; "b"; "2" |]
        [| dict [ "a", "1" ]; nul<string>; "1" |]
        [| dict [ "a", "1" ]; "a"; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if dict does not contain specified key/value``
        (subject: IDictionary<string | null, string | null>)
        (key: string | null)
        (value: string | null)
        =
        subject.Should().NotContain(key, value)


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, int>>.Should().NotContain("", 1))


    let failData = [
        [| box (dict [ "a", "1" ]); "a"; "1" |]
        [| dict [ "a", "1"; "b", "2" ]; "b"; "2" |]
        [| dict [ asNull "a", asNull "1"; null, null ]; null; null |]
        [| dict [ asNull "a", "1"; null, "2" ]; null; "2" |]
        [| dict [ "a", asNull "1"; "b", null ]; "b"; null |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if dict contains the specified key/value``
        (subject: IDictionary<string | null, string | null>)
        (key: string | null)
        (value: string | null)
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


    let private crossingComparers subjectA subjectPunctuation expectedA expectedUpper =
        let subject = Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        subject.Add("a", subjectA)
        subject.Add("a-", subjectPunctuation)

        let comparer =
            Comparer<string>.Create(fun a b ->
                CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreSymbols)
            )

        let expected = SortedDictionary<string, int>(comparer)
        expected.Add("a", expectedA)
        expected.Add("A", expectedUpper)
        subject, expected


    let crossingComparerCases = [
        for subjectA in [ 1; 2 ] do
            for subjectPunctuation in [ 1; 2 ] do
                for expectedA in [ 1; 2 ] do
                    for expectedUpper in [ 1; 2 ] do
                        yield [| box subjectA; box subjectPunctuation; box expectedA; box expectedUpper |]
    ]


    [<Theory>]
    [<MemberData(nameof crossingComparerCases)>]
    let ``Crossing key comparers require matching values in both directions``
        subjectA
        subjectPunctuation
        expectedA
        expectedUpper
        =
        let subject, expected =
            crossingComparers subjectA subjectPunctuation expectedA expectedUpper
        // These key mappings connect all four entries, so their values must all agree.
        let passes =
            [ subjectA; subjectPunctuation; expectedA; expectedUpper ]
            |> List.distinct
            |> List.length = 1

        let check assertion =
            if passes then
                assertion ()
            else
                assertFails assertion |> ignore

        check (fun () -> subject.Should().HaveSameItemsAs(expected) |> ignore)
        check (fun () -> expected.Should().HaveSameItemsAs(subject) |> ignore)


    [<Fact>]
    let ``Reports a value mismatch only visible from expected keys`` () =
        let subject, expected = crossingComparers 1 1 1 999
        let ex = assertFails (fun () -> subject.Should().HaveSameItemsAs(expected))
        Assert.Contains("Key: A", ex.Message)
        Assert.Contains("Expected: 999", ex.Message)
        Assert.Contains("Actual: 1", ex.Message)


    [<Fact>]
    let ``Preserves dictionary matching for keys without FSharp equality`` () =
        let key = fun value -> value + 1
        let subject = Dictionary<int -> int, int>(EqualityComparer<int -> int>.Default)
        let expected = Dictionary<int -> int, int>(EqualityComparer<int -> int>.Default)
        subject.Add(key, 1)
        expected.Add(key, 1)
        subject.Should().HaveSameItemsAs(expected) |> ignore
        expected.Should().HaveSameItemsAs(subject) |> ignore


    [<Fact>]
    let ``Different comparer equivalence classes cannot hide a count mismatch`` () =
        let subject = Dictionary<string, int>(System.StringComparer.Ordinal)
        subject.Add("a", 1)
        subject.Add("A", 1)
        let expected = Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase)
        expected.Add("a", 1)

        let ex = assertFails (fun () -> subject.Should().HaveSameItemsAs(expected))
        Assert.Contains("Expected count: 1", ex.Message)
        Assert.Contains("Actual count: 2", ex.Message)
        assertFails (fun () -> expected.Should().HaveSameItemsAs(subject)) |> ignore


    [<Fact>]
    let ``Equal counts preserve dictionary comparer matching`` () =
        let subject = Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase)
        subject.Add("a", 1)
        let expected = Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase)
        expected.Add("A", 1)

        subject.Should().HaveSameItemsAs(expected) |> ignore
        expected.Should().HaveSameItemsAs(subject) |> ignore


    [<Fact>]
    let ``Reports all value mismatches for structurally equal distinct keys`` () =
        let first = [| 1 |]
        let second = [| 1 |]
        let subject = Dictionary<int[], int>()
        subject.Add(first, 1)
        subject.Add(second, 2)
        let expected = Dictionary<int[], int>()
        expected.Add(first, 10)
        expected.Add(second, 20)

        let ex = assertFails (fun () -> subject.Should().HaveSameItemsAs(expected))
        Assert.Contains("Expected: 10", ex.Message)
        Assert.Contains("Expected: 20", ex.Message)

        subject.Should().NotSatisfy(fun value -> value.Should().HaveSameItemsAs(expected))
        |> ignore


    [<Fact>]
    let ``Can be chained with And`` () =
        Map.empty<string, int>
            .Should()
            .HaveSameItemsAs(Map.empty<string, int>)
            .Id<And<Map<string, int>>>()
            .And.Be(Map.empty<string, int>)


    let passData: IDictionary<string | null, string | null> array list = [
        [| dict []; dict [] |]
        [| dict [ "a", "1" ]; dict [ "a", "1" ] |]
        [| dict [ "a", "1"; "b", "2" ]; dict [ "b", "2"; "a", "1" ] |]
        [| dict [ "a", "1"; null, null ]; dict [ null, null; "a", "1" ] |]
        [| dict [ "a", "1"; null, "2" ]; dict [ null, "2"; "a", "1" ] |]
        [| dict [ "a", "1"; "b", null ]; dict [ "b", null; "a", "1" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if both contain the same key-value pairs``
        (subject: IDictionary<string | null, string | null>)
        (expected: IDictionary<string | null, string | null>)
        =
        subject.Should().HaveSameItemsAs(expected)


    let failData: IDictionary<string | null, string | null> array list = [
        [| dict []; dict [ "a", "1" ] |]
        [| dict [ "a", "1" ]; dict [ "a", "1"; "b", "2" ] |]
        [| dict [ "a", "1" ]; dict [ "b", "2" ] |]
        [| dict [ "a", "1"; null, null ]; dict [ "a", "1" ] |]
        [| dict [ "a", "1"; null, "2" ]; dict [ "a", "1" ] |]
        [| dict [ "a", "1"; "b", null ]; dict [ "a", "1" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if they do not contain the same key-value pairs``
        (a: IDictionary<string | null, string | null>)
        (b: IDictionary<string | null, string | null>)
        =
        assertFails (fun () -> a.Should().HaveSameItemsAs(b)) |> ignore
        assertFails (fun () -> b.Should().HaveSameItemsAs(a))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, string>>.Should().HaveSameItemsAs(dict []))


    [<Fact>]
    let ``Throws if expected is null`` () =
        assertThrows (fun () -> (dict []).Should().HaveSameItemsAs(Unchecked.defaultof<IDictionary<string, string>>))


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
- Key: a
  Expected: 2
  Actual: 1
- Key: b
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
- Key: a
  Expected: 2
  Actual: 1
- Key: b
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


module ContainAtMostOneItem =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().ContainAtMostOneItem()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict<string, int> [ "a", 1; "b", 2 ]
            x.Should().ContainAtMostOneItem()
        |> assertExnMsg
            """
Subject: x
Should: ContainAtMostOneItem
But length was: 2
Subject value:
  a: 1
  b: 2
"""


module ContainAtMostOneItemMatching =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().ContainAtMostOneItemMatching(fun kvp -> kvp.Key = "a")


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1; "b", 1; "c", 3 ]
            x.Should().ContainAtMostOneItemMatching(fun kvp -> kvp.Value = 1)
        |> assertExnMsg
            """
Subject: x
Should: ContainAtMostOneItemMatching
But found: 2
Matching items:
- Key: a
  Value: 1
- Key: b
  Value: 1
Subject value:
  a: 1
  b: 1
  c: 3
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


module NotContainItemsMatching =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () =
        let x = dict [ "a", 1 ]
        x.Should().NotContainItemsMatching(fun kvp -> kvp.Key = "b")


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().NotContainItemsMatching(fun kvp -> kvp.Key = "a")
        |> assertExnMsg
            """
Subject: x
Should: NotContainItemsMatching
But found: 1
Matching items:
- Key: a
  Value: 1
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
    let ``Returns the supplied key as the derived key`` () =
        let actualKey = { Id = 1 }
        let expectedKey = { Id = 1 }

        let derived = (dict [ actualKey, "value" ]).Should().ContainKey(expectedKey).That

        Object.ReferenceEquals(expectedKey, derived.Key).Should().BeTrue() |> ignore


    [<Fact>]
    let ``Uses the dictionary key comparer`` () =
        let subject = Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        subject.Add("actual", 1)

        subject.Should().ContainKey("ACTUAL")


    [<Fact>]
    let ``Supports keys without FSharp equality`` () =
        let key x = x + 1
        let subject = Dictionary<int -> int, int>(EqualityComparer<int -> int>.Default)
        subject.Add(key, 1)

        subject.Should().ContainKey(key)


    let passData = [
        [| box (dict [ "a", "1" ]); "a" |]
        [| dict [ "a", "1"; "b", "2" ]; "b" |]
        [| dict [ asNull "a", "1"; null, "2" ]; null |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if dict contains the key`` (subject: IDictionary<string | null, string | null>) (key: string | null) =
        subject.Should().ContainKey(key)


    let failData = [
        [| box (dict<string | null, string | null> []); "a" |]
        [| dict [ "a", "1" ]; "b" |]
        [| dict [ "a", "1" ]; null |]
        [| dict [ nul<string>, "1" ]; "a" |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing the key`` (subject: IDictionary<string | null, string | null>) (key: string | null) =
        assertFails (fun () -> subject.Should().ContainKey(key))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, int>>.Should().ContainKey(""))


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
        Map.empty<string, int>.Should().NotContainKey("a").Id<And<Map<string, int>>>().And.Be(Map.empty<string, int>)


    let passData = [
        [| box (dict<string | null, string | null> []); "a" |]
        [| dict [ "a", "1" ]; "b" |]
        [| dict [ "a", "1" ]; null |]
        [| dict [ nul<string>, "1" ]; "a" |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes not containing the key`` (subject: IDictionary<string | null, string | null>) (key: string | null) =
        subject.Should().NotContainKey(key)


    let failData = [
        [| box (dict [ "a", "1" ]); "a" |]
        [| dict [ "a", "1"; "b", "2" ]; "b" |]
        [| dict [ asNull "a", "1"; null, "2" ]; null |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if dict contains the key`` (subject: IDictionary<string | null, string | null>) (key: string | null) =
        assertFails (fun () -> subject.Should().NotContainKey(key))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, int>>.Should().NotContainKey(""))


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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


module ContainKeys =


    [<Fact>]
    let ``Retains requested and missing keys from a single-pass sequence`` () =
        let keys = singlePass [ "a"; "b"; "b"; "c" ]
        let error = assertFails (fun () -> (dict [ "a", 1 ]).Should().ContainKeys(keys))

        Assert.Contains("Keys: [a, b, b, c]\nBut was missing: [b, c]\n", error.Message.ReplaceLineEndings("\n"))


    [<Fact>]
    let ``Can be chained with AndDerived with found KeyValuePair`` () =
        Map.empty.Add("a", 1).Should().ContainKeys([ "a" ]).Id<And<Map<string, int>>>().And.Be(Map.empty.Add("a", 1))


    let passData = [
        [| box (dict [ "a", "1" ]); [ "a" ] |]
        [| dict [ "a", "1"; "b", "2" ]; [ "a" ] |]
        [| dict [ "a", "1"; "b", "2" ]; [ "b" ] |]
        [| dict [ "a", "1"; "b", "2" ]; [ "a"; "b" ] |]
        [| dict [ asNull "a", "1"; null, "2" ]; [ nul<string> ] |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if dict contains the key``
        (subject: IDictionary<string | null, string | null>)
        (keys: (string | null) list)
        =
        subject.Should().ContainKeys(keys)


    let failData = [
        [| box (dict<string | null, string | null> []); [ "a" ] |]
        [| dict [ "a", "1" ]; [ "b" ] |]
        [| dict [ "a", "1" ]; [ "a"; "b" ] |]
        [| dict [ "a", "1" ]; [ nul<string> ] |]
        [| dict [ nul<string>, "1" ]; [ "a" ] |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing the key``
        (subject: IDictionary<string | null, string | null>)
        (keys: (string | null) list)
        =
        assertFails (fun () -> subject.Should().ContainKeys(keys))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, int>>.Should().ContainKeys([]))


    [<Fact>]
    let ``Fails with expected message if not containing key`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainKeys([ "a"; "b"; "c" ])
        |> assertExnMsg
            """
Subject: x
Should: ContainKeys
Keys: [a, b, c]
But was missing: [b, c]
Subject value:
  a: 1
"""


    [<Fact>]
    let ``Fails with expected message with because if not containing key`` () =
        fun () ->
            let x = dict [ "a", 1 ]
            x.Should().ContainKeys([ "a"; "b"; "c" ], "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: ContainKeys
Keys: [a, b, c]
But was missing: [b, c]
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


    let passData = [
        [| box (dict [ "a", "1" ]); "1" |]
        [| dict [ "a", "1"; "b", "2" ]; "2" |]
        [| dict [ "a", "2"; "b", "2" ]; "2" |]
        [| dict [ "a", nul<string> ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if dict contains the value``
        (subject: IDictionary<string | null, string | null>)
        (key: string | null)
        =
        subject.Should().ContainValue(key)


    let failData = [
        [| box (dict<string, string> []); "a" |]
        [| dict [ "a", "1" ]; "2" |]
        [| dict [ "a", "1" ]; nul<string> |]
        [| dict [ "a", nul<string> ]; "a" |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not containing the value``
        (subject: IDictionary<string | null, string | null>)
        (key: string | null)
        =
        assertFails (fun () -> subject.Should().ContainValue(key))


    [<Fact>]
    let ``Throws if null`` () =
        assertThrows (fun () -> Unchecked.defaultof<IDictionary<string, int>>.Should().ContainValue(0))


    [<Fact>]
    let ``Fails with expected message if not containing key`` () =
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
    let ``Fails with expected message with because if not containing key`` () =
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
        Map.empty<string, int>.Should().NotContainValue(1).Id<And<Map<string, int>>>().And.Be(Map.empty<string, int>)


    let passData = [
        [| box (dict<string, string> []); "a" |]
        [| dict [ "a", "1" ]; "2" |]
        [| dict [ "a", "1" ]; nul<string> |]
        [| dict [ "a", nul<string> ]; "a" |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if not containing the value``
        (subject: IDictionary<string | null, string | null>)
        (key: string | null)
        =
        subject.Should().NotContainValue(key)


    let failData = [
        [| box (dict [ "a", "1" ]); "1" |]
        [| dict [ "a", "1"; "b", "2" ]; "2" |]
        [| dict [ "a", "2"; "b", "2" ]; "2" |]
        [| dict [ "a", nul<string> ]; nul<string> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if dict contains the value`` (subject: IDictionary<string | null, string | null>) (key: string | null) =
        assertFails (fun () -> subject.Should().NotContainValue(key))


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message with because`` () =
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
