module FunctionAssertions

open System
open Faqt
open Xunit


let inner (inner: #exn) = Exception("", inner)

let agg (inner: seq<exn>) = AggregateException(inner)


module Throw =


    [<Fact>]
    let ``Can be chained with AndDerived with matched exception`` () =
        (fun () -> raise <| InvalidOperationException("foo"))
            .Should()
            .Throw<InvalidOperationException, _>()
            .Id<AndDerived<unit -> obj, InvalidOperationException>>()
            .Whose.Message.Should(())
            .Be("foo")


    let throw<'a when 'a :> exn> (t: Testable<unit -> obj>) = t.Throw<'a, obj>()


    let passData = [
        [| box (Exception()); throw<Exception> |]
        [| InvalidOperationException(); throw<Exception> |]
        [| InvalidOperationException(); throw<InvalidOperationException> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if throwing matching exception at top level`` (ex: exn) run = run ((fun () -> raise <| ex).Should())


    let failData = [
        [| box null; throw<Exception> |]
        [| Exception(); throw<InvalidOperationException> |]
        [| inner (InvalidOperationException()); throw<InvalidOperationException> |]
        [| agg [ InvalidOperationException() ]; throw<InvalidOperationException> |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not throwing matching exception at top level`` (ex: exn) run =
        assertFails (fun () -> run ((fun () -> if isNull ex then obj () else raise <| ex).Should()))


    [<Fact>]
    let ``Fails with expected message if succeeds`` () =
        fun () ->
            let f () = 1
            f.Should().Throw<InvalidOperationException, _>()
        |> assertExnMsg
            """
Subject: f
Should: Throw
Exception: System.InvalidOperationException
But succeeded: true
"""


    [<Fact>]
    let ``Fails with expected message with because if succeeds`` () =
        fun () ->
            let f () = 1
            f.Should().Throw<InvalidOperationException, _>("Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: Throw
Exception: System.InvalidOperationException
But succeeded: true
"""


    [<Fact>]
    let ``Fails with expected message if throwing a non-matching exception`` () =
        fun () ->
            let f () = invalidOp ""
            f.Should().Throw<ArgumentException, _>()
        |> assertExnMsgWildcard
            """
Subject: f
Should: Throw
Exception: System.ArgumentException
But threw: |-
  System.InvalidOperationException
     at *
"""


    [<Fact>]
    let ``Fails with expected message with because if throwing a non-matching exception`` () =
        fun () ->
            let f () = invalidOp ""
            f.Should().Throw<ArgumentException, _>("Some reason")
        |> assertExnMsgWildcard
            """
Subject: f
Because: Some reason
Should: Throw
Exception: System.ArgumentException
But threw: |-
  System.InvalidOperationException
     at *
"""


module ThrowInner =


    [<Fact>]
    let ``Can be chained with AndDerived with topmost/first matched exception`` () =
        (fun () ->
            raise
            <| AggregateException(
                "",
                InvalidOperationException("foo"),
                InvalidOperationException("bar", Exception("", InvalidOperationException("baz")))
            )
        )
            .Should()
            .ThrowInner<InvalidOperationException, _>()
            .Id<AndDerived<unit -> obj, InvalidOperationException>>()
            .Whose.Message.Should(())
            .Be("foo")


    let throwInner<'a when 'a :> exn> (t: Testable<unit -> obj>) = t.ThrowInner<'a, obj>()


    let passData = [
        [| box (Exception()); throwInner<Exception> |]
        [| InvalidOperationException(); throwInner<Exception> |]
        [| InvalidOperationException(); throwInner<InvalidOperationException> |]
        [| inner (InvalidOperationException()); throwInner<InvalidOperationException> |]
        [|
            inner (inner (inner (InvalidOperationException())))
            throwInner<InvalidOperationException>
        |]
        [| agg [ InvalidOperationException() ]; throwInner<InvalidOperationException> |]
        [|
            agg [ inner (InvalidOperationException()); inner (InvalidOperationException()) ]
            throwInner<InvalidOperationException>
        |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if throwing matching exception at any level`` (ex: exn) run = run ((fun () -> raise <| ex).Should())


    let failData = [
        [| box null; throwInner<Exception> |]
        [| Exception(); throwInner<ArgumentException> |]
        [| Exception("", InvalidOperationException()); throwInner<ArgumentException> |]
        [|
            AggregateException(InvalidOperationException())
            throwInner<ArgumentException>
        |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not throwing matching exception at any level`` (ex: exn) run =
        assertFails (fun () -> run ((fun () -> if isNull ex then obj () else raise <| ex).Should()))


    [<Fact>]
    let ``Fails with expected message if succeeds`` () =
        fun () ->
            let f () = 1
            f.Should().ThrowInner<InvalidOperationException, _>()
        |> assertExnMsg
            """
Subject: f
Should: ThrowInner
Exception: System.InvalidOperationException
But succeeded: true
"""


    [<Fact>]
    let ``Fails with expected message with because if succeeds`` () =
        fun () ->
            let f () = 1
            f.Should().ThrowInner<InvalidOperationException, _>("Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: ThrowInner
Exception: System.InvalidOperationException
But succeeded: true
"""


    [<Fact>]
    let ``Fails with expected message if throwing a non-matching exception`` () =
        fun () ->
            let f () = invalidOp ""
            f.Should().ThrowInner<ArgumentException, _>()
        |> assertExnMsgWildcard
            """
Subject: f
Should: ThrowInner
Exception: System.ArgumentException
But threw: |-
  System.InvalidOperationException
     at *
"""


    [<Fact>]
    let ``Fails with expected message with because if throwing a non-matching exception`` () =
        fun () ->
            let f () = invalidOp ""
            f.Should().ThrowInner<ArgumentException, _>("Some reason")
        |> assertExnMsgWildcard
            """
Subject: f
Because: Some reason
Should: ThrowInner
Exception: System.ArgumentException
But threw: |-
  System.InvalidOperationException
     at *
"""


module ThrowExactly =


    [<Fact>]
    let ``Can be chained with AndDerived with matched exception`` () =
        (fun () -> raise <| InvalidOperationException("foo"))
            .Should()
            .ThrowExactly<InvalidOperationException, _>()
            .Id<AndDerived<unit -> obj, InvalidOperationException>>()
            .Whose.Message.Should(())
            .Be("foo")


    let throwExactly<'a when 'a :> exn> (t: Testable<unit -> obj>) = t.ThrowExactly<'a, obj>()


    let passData = [
        [| box (Exception()); throwExactly<Exception> |]
        [| InvalidOperationException(); throwExactly<InvalidOperationException> |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if throwing specified exception at top level`` (ex: exn) run = run ((fun () -> raise <| ex).Should())


    let failData = [
        [| box null; throwExactly<Exception> |]
        [| InvalidOperationException(); throwExactly<Exception> |]
        [| Exception(); throwExactly<InvalidOperationException> |]
        [|
            inner (InvalidOperationException())
            throwExactly<InvalidOperationException>
        |]
        [|
            agg [ InvalidOperationException() ]
            throwExactly<InvalidOperationException>
        |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if not throwing specified exception at top level`` (ex: exn) run =
        assertFails (fun () -> run ((fun () -> if isNull ex then obj () else raise <| ex).Should()))


    [<Fact>]
    let ``Fails with expected message if succeeds`` () =
        fun () ->
            let f () = 1
            f.Should().ThrowExactly<InvalidOperationException, _>()
        |> assertExnMsg
            """
Subject: f
Should: ThrowExactly
Exception: System.InvalidOperationException
But succeeded: true
"""


    [<Fact>]
    let ``Fails with expected message with because if succeeds`` () =
        fun () ->
            let f () = 1
            f.Should().ThrowExactly<InvalidOperationException, _>("Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: ThrowExactly
Exception: System.InvalidOperationException
But succeeded: true
"""


    [<Fact>]
    let ``Fails with expected message if throwing a non-matching exception`` () =
        fun () ->
            let f () = invalidOp ""
            f.Should().ThrowExactly<ArgumentException, _>()
        |> assertExnMsgWildcard
            """
Subject: f
Should: ThrowExactly
Exception: System.ArgumentException
But threw: |-
  System.InvalidOperationException
     at *
"""


    [<Fact>]
    let ``Fails with expected message with because if throwing a non-matching exception`` () =
        fun () ->
            let f () = invalidOp ""
            f.Should().ThrowExactly<ArgumentException, _>("Some reason")
        |> assertExnMsgWildcard
            """
Subject: f
Because: Some reason
Should: ThrowExactly
Exception: System.ArgumentException
But threw: |-
  System.InvalidOperationException
     at *
"""


module NotThrow =


    [<Fact>]
    let ``Passes if not throws and can be chained with And`` () =
        (fun () -> 1).Should().NotThrow().Id<And<unit -> int>>()


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let f () = failwith "foo"
            f.Should().NotThrow()
        |> assertExnMsgWildcard
            """
Subject: f
Should: NotThrow
But threw: |-
  System.Exception: foo
     at *
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let f () = failwith "foo"
            f.Should().NotThrow("Some reason")
        |> assertExnMsgWildcard
            """
Subject: f
Because: Some reason
Should: NotThrow
But threw: |-
  System.Exception: foo
     at *
"""


module Roundtrip =


    [<Fact>]
    let ``Passes when the function does not throw and can be chained with And`` () =
        id.Should().Roundtrip("a").Id<And<string -> string>>()


    [<Fact>]
    let ``Fails with expected message when the function throws`` () =
        fun () ->
            let f _ = failwith<string> "foo"
            f.Should().Roundtrip("a")
        |> assertExnMsgWildcard
            """
Subject: f
Should: Roundtrip
Value: a
But threw: |-
  System.Exception: foo
     at *
"""


    [<Fact>]
    let ``Fails with expected message with because when the function throws`` () =
        fun () ->
            let f _ = failwith<string> "foo"
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsgWildcard
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But threw: |-
  System.Exception: foo
     at *
"""


    [<Fact>]
    let ``Fails with expected message when the function returns a different value`` () =
        fun () ->
            let f _ = "b"
            f.Should().Roundtrip("a")
        |> assertExnMsg
            """
Subject: f
Should: Roundtrip
Value: a
But returned: b
"""


    [<Fact>]
    let ``Fails with expected message with because when the function returns a different value`` () =
        fun () ->
            let f _ = "b"
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But returned: b
"""


module ``Roundtrip (option)`` =


    [<Fact>]
    let ``Passes when the function does not throw and can be chained with And`` () =
        Some.Should().Roundtrip("a").Id<And<string -> string option>>()


    [<Fact>]
    let ``Fails with expected message when the function throws`` () =
        fun () ->
            let f _ = failwith<string option> "foo"
            f.Should().Roundtrip("a")
        |> assertExnMsgWildcard
            """
Subject: f
Should: Roundtrip
Value: a
But threw: |-
  System.Exception: foo
     at *
"""


    [<Fact>]
    let ``Fails with expected message with because when the function throws`` () =
        fun () ->
            let f _ = failwith<string option> "foo"
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsgWildcard
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But threw: |-
  System.Exception: foo
     at *
"""


    [<Fact>]
    let ``Fails with expected message when the function returns None`` () =
        fun () ->
            let f _ = None
            f.Should().Roundtrip("a")
        |> assertExnMsg
            """
Subject: f
Should: Roundtrip
Value: a
But returned: null
"""


    [<Fact>]
    let ``Fails with expected message with because when the function returns None`` () =
        fun () ->
            let f _ = None
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But returned: null
"""


    [<Fact>]
    let ``Fails with expected message when the function returns Some with a different value`` () =
        fun () ->
            let f _ = Some "b"
            f.Should().Roundtrip("a")
        |> assertExnMsg
            """
Subject: f
Should: Roundtrip
Value: a
But returned:
  Some: b
"""


    [<Fact>]
    let ``Fails with expected message with because when the function returns Some with a different value`` () =
        fun () ->
            let f _ = Some "b"
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But returned:
  Some: b
"""


module ``Roundtrip (Result)`` =


    [<Fact>]
    let ``Passes when the function does not throw and can be chained with And`` () =
        Ok.Should().Roundtrip("a").Id<And<string -> Result<string, string>>>()


    [<Fact>]
    let ``Fails with expected message when the function throws`` () =
        fun () ->
            let f _ = failwith<Result<string, string>> "foo"
            f.Should().Roundtrip("a")
        |> assertExnMsgWildcard
            """
Subject: f
Should: Roundtrip
Value: a
But threw: |-
  System.Exception: foo
     at *
"""


    [<Fact>]
    let ``Fails with expected message with because when the function throws`` () =
        fun () ->
            let f _ = failwith<Result<string, string>> "foo"
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsgWildcard
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But threw: |-
  System.Exception: foo
     at *
"""


    [<Fact>]
    let ``Fails with expected message when the function returns Error`` () =
        fun () ->
            let f _ = Error "foo"
            f.Should().Roundtrip("a")
        |> assertExnMsg
            """
Subject: f
Should: Roundtrip
Value: a
But returned:
  Error: foo
"""


    [<Fact>]
    let ``Fails with expected message with because when the function returns Error "foo"`` () =
        fun () ->
            let f _ = Error "foo"
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But returned:
  Error: foo
"""


    [<Fact>]
    let ``Fails with expected message when the function returns Ok with a different value`` () =
        fun () ->
            let f _ = Ok "b"
            f.Should().Roundtrip("a")
        |> assertExnMsg
            """
Subject: f
Should: Roundtrip
Value: a
But returned:
  Ok: b
"""


    [<Fact>]
    let ``Fails with expected message with because when the function returns Ok with a different value`` () =
        fun () ->
            let f _ = Ok "b"
            f.Should().Roundtrip("a", "Some reason")
        |> assertExnMsg
            """
Subject: f
Because: Some reason
Should: Roundtrip
Value: a
But returned:
  Ok: b
"""
