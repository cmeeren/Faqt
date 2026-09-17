module Formatting

open System
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Net.Sockets
open System.Runtime.CompilerServices
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading
open Faqt
open Faqt.AssertionHelpers
open Faqt.Configuration
open Faqt.Formatting
open Xunit
open YamlDotNet.Core
open YamlDotNet.RepresentationModel


[<Extension>]
type private Assertions =


    [<Extension>]
    static member FailWithUnserializableAtTopAndNested(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()

        t.With("A", TestUnserializableType()).With("B", [ TryFormat(TestUnserializableType()) ]).Fail(None)


    [<Extension>]
    static member FailWithDuplicateData(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()
        t.With("Repeated", "first").With("Repeated", "second").Fail(None)


type private OneByteNonSeekableStream(bytes: byte[]) =
    inherit Stream()

    let mutable position = 0

    override _.CanRead = true
    override _.CanSeek = false
    override _.CanWrite = false

    override _.Length = raise <| NotSupportedException()

    override _.Position
        with get () = int64 position
        and set _ = raise <| NotSupportedException()

    override _.Flush() = ()

    override _.Read(buffer: byte[], offset, count) =
        if position >= bytes.Length then
            0
        else
            let actualCount = min 1 (min count (bytes.Length - position))
            Array.Copy(bytes, position, buffer, offset, actualCount)
            position <- position + actualCount
            actualCount

    override this.Read(buffer: Span<byte>) =
        if position >= bytes.Length then
            0
        else
            let actualCount = min 1 (min buffer.Length (bytes.Length - position))
            bytes.AsSpan(position, actualCount).CopyTo(buffer)
            position <- position + actualCount
            actualCount

    override _.Seek(_, _) = raise <| NotSupportedException()

    override _.SetLength(_) = raise <| NotSupportedException()

    override _.Write(_, _, _) = raise <| NotSupportedException()


type TestUnserializableTypeWithThrowingToString() =
    member _.WillThrow = failwith<int> "Foo"

    override _.ToString() = failwith "Bar"


[<Fact>]
let ``Can override and restore the default formatter`` () =

    fun () -> "a".Should().Fail()
    |> assertExnMsg
        """
Subject: '"a"'
Should: Fail
"""


[<Fact>]
let ``Setting null global formatter throws ArgumentNullException`` () =
    try
        Assert.Throws<ArgumentNullException>(fun () -> Formatter.Set(Unchecked.defaultof<FailureData -> string>))
        |> ignore
    finally
        Formatter.Set(YamlFormatterBuilder.Default.Build())


[<Fact>]
let ``Setting null local formatter throws ArgumentNullException`` () =
    Assert.Throws<ArgumentNullException>(fun () -> Formatter.With(Unchecked.defaultof<FailureData -> string>) |> ignore)
    |> ignore

    do
        use _ = Formatter.With(fun _ -> "OVERRIDDEN FORMATTER")

        fun () -> "a".Should().Fail()
        |> assertExnMsg
            """
OVERRIDDEN FORMATTER
"""

    fun () -> "a".Should().Fail()
    |> assertExnMsg
        """
Subject: '"a"'
Should: Fail
"""


[<Fact>]
let ``Can override and restore the default formatter in async code`` () =
    async {

        fun () -> "a".Should().Fail()
        |> assertExnMsg
            """
Subject: '"a"'
Should: Fail
"""

        do
            use _ = Formatter.With(fun _ -> "OVERRIDDEN FORMATTER")

            fun () -> "a".Should().Fail()
            |> assertExnMsg
                """
OVERRIDDEN FORMATTER
"""

        fun () -> "a".Should().Fail()
        |> assertExnMsg
            """
Subject: '"a"'
Should: Fail
"""

    }


[<Fact>]
let ``FailureData has expected members`` () =
    // Compile-time check only
    fun () ->
        let x = Unchecked.defaultof<FailureData>
        x.Subject |> ignore<string list>
        x.Because |> ignore<string option>
        x.Should |> ignore<string>
        x.Extra |> ignore<(string * obj) list>
    |> ignore


[<Fact>]
let ``Can use a completely custom formatter`` () =
    let format (data: FailureData) =
        $"""
SUBJECT
%A{data.Subject}
BECAUSE
%A{data.Because}
SHOULD
%A{data.Should}
EXTRA
%A{data.Extra}
        """
            .Trim()

    use _ = Formatter.With(format)

    fun () -> "a".Should().Fail()
    |> assertExnMsg
        """
SUBJECT
[""a""]
BECAUSE
None
SHOULD
"Fail"
EXTRA
[]
"""


module ``Default format`` =


    [<Fact>]
    let ``Rendering of top-level structure with single subject name`` () =
        fun () -> "a".Should().FailWithBecause("Some reason", "Foo", "Bar")
        |> assertExnMsg
            """
Subject: '"a"'
Because: Some reason
Should: FailWithBecause
Foo: Bar
"""


    [<Fact>]
    let ``Rendering of top-level structure with empty because`` () =
        fun () -> "a".Should().FailWithBecause("", "Foo", "Bar")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWithBecause
Foo: Bar
"""


    [<Fact>]
    let ``Rendering of top-level structure with multi-part subject name`` () =
        fun () -> (Some "a").Should().BeSome().Whose.Length.Should(()).FailWithBecause("Some reason", "Foo", "Bar")
        |> assertExnMsg
            """
Subject:
- Some "a"
- Length
Because: Some reason
Should: FailWithBecause
Foo: Bar
"""


    [<Fact>]
    let ``Rendering of top-level structure without because`` () =
        fun () -> "a".Should().FailWith("Foo", "Bar")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Foo: Bar
"""


    [<Fact>]
    let ``Rendering of top-level structure uses keys as-is`` () =
        fun () -> "a".Should().FailWith("foo", "Bar")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
foo: Bar
"""


    [<Fact>]
    let ``Rendering of plain string`` () =
        fun () -> "a".Should().FailWith("Value", "asd")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: asd
"""


    [<Fact>]
    let ``Rendering of empty string`` () =
        fun () -> "a".Should().FailWith("Value", "")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: ''
"""


    [<Fact>]
    let ``Rendering of string with non-printable character`` () =
        fun () -> "a".Should().FailWith("Value", "as\u0011d")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: "as\x11d"
"""


    [<Fact>]
    let ``Rendering of string with HTML-sensitive characters`` () =
        fun () -> "a".Should().FailWith("Value", "<p>&")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: <p>&
"""


    [<Fact>]
    let ``Rendering of string with non-ASCII characters`` () =
        fun () -> "a".Should().FailWith("Value", "生命")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 生命
"""


    [<Fact>]
    let ``Rendering of string with special YAML character`` () =
        fun () -> "a".Should().FailWith("Value", "{bar")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: '{bar'
"""


    [<Fact>]
    let ``Rendering of string starting with space`` () =
        fun () -> "a".Should().FailWith("Value", " asd")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: ' asd'
"""


    [<Fact>]
    let ``Rendering of string ending with space`` () =
        fun () -> "a".Should().FailWith("Value", "asd ")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 'asd '
"""


    [<Fact>]
    let ``Rendering of string with \n`` () =
        fun () -> "a".Should().FailWith("Value", "asd\nabc")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: |-
  asd
  abc
"""


    [<Fact>]
    let ``Rendering of string with \r\n`` () =
        fun () -> "a".Should().FailWith("Value", "asd\r\nabc")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: |-
  asd
  abc
"""


    [<Fact>]
    let ``Rendering of string "null"`` () =
        fun () -> "a".Should().FailWith("Value", "null")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 'null'
"""


    [<Fact>]
    let ``Rendering of string "true"`` () =
        fun () -> "a".Should().FailWith("Value", "true")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 'true'
"""


    [<Fact>]
    let ``Rendering of string "false"`` () =
        fun () -> "a".Should().FailWith("Value", "false")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 'false'
"""


    [<Fact>]
    let ``Rendering of string integers`` () =
        fun () -> "a".Should().FailWith("Value", "1")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: '1'
"""


    [<Fact>]
    let ``Rendering of string floats`` () =
        fun () -> "a".Should().FailWith("Value", "1.2")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: '1.2'
"""


    [<Fact>]
    let ``Rendering of string exponents`` () =
        fun () -> "a".Should().FailWith("Value", "1.2e10")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: '1.2e10'
"""


    [<Fact>]
    let ``Rendering of string with integer`` () =
        fun () -> "a".Should().FailWith("Value", "a 1 b")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: a 1 b
"""


    [<Fact>]
    let ``Rendering of integers`` () =
        fun () -> "a".Should().FailWith("Value", 1)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 1
"""


    [<Fact>]
    let ``Rendering of floats`` () =
        fun () -> "a".Should().FailWith("Value", 1.2)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 1.2
"""


    [<Fact>]
    let ``Rendering of special floating point values`` () =
        fun () -> "a".Should().FailWith("Value", Double.NaN)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: NaN
"""

        fun () -> "a".Should().FailWith("Value", Double.PositiveInfinity)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: Infinity
"""

        fun () -> "a".Should().FailWith("Value", Double.NegativeInfinity)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: -Infinity
"""


    [<Fact>]
    let ``Rendering of string sequences`` () =
        fun () -> "a".Should().FailWith("Value", [ "a"; "b" ])
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: [a, b]
"""


    [<Fact>]
    let ``Rendering of int sequences`` () =
        fun () -> "a".Should().FailWith("Value", [ 1; 2 ])
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: [1, 2]
"""


    [<Fact>]
    let ``Rendering of object sequences`` () =
        fun () -> "a".Should().FailWith("Value", [ {| A = 1; B = "a" |}; {| A = 2; B = "b" |} ])
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
- A: 1
  B: a
- A: 2
  B: b
"""


    [<Fact>]
    let ``Rendering of sequence sequences`` () =
        fun () -> "a".Should().FailWith("Value", [ [ 1; 2 ]; [ 3; 4 ] ])
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
- [1, 2]
- [3, 4]
"""


    [<Fact>]
    let ``Rendering of objects`` () =
        fun () -> "a".Should().FailWith("Value", {| A = 1; B = "a" |})
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
  A: 1
  B: a
"""


    [<Fact>]
    let ``Rendering of null`` () =
        fun () -> "a".Should().FailWith("Value", null)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: null
"""


    [<Fact>]
    let ``Subject name uses block style, but other Subject string sequences uses flow style`` () =
        fun () -> (Some "a").Should().BeSome().Whose.Length.Should(()).FailWith("Value", {| Subject = [ "a"; "b" ] |})
        |> assertExnMsg
            """
Subject:
- Some "a"
- Length
Should: FailWith
Value:
  Subject: [a, b]
"""


    type MyDu =
        | NoFields
        | SingleField of int
        | NamedAndUnnamedFields of int * string * string * foo: int


    [<Fact>]
    let ``Rendering of unions: External tag, named fields from types`` () =
        fun () -> "a".Should().FailWith("Value", NamedAndUnnamedFields(1, "a", "b", 2))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
  NamedAndUnnamedFields:
    Int32: 1
    String1: a
    String2: b
    foo: 2
"""


    [<Fact>]
    let ``Rendering of unions: Field-less cases serialized as string`` () =
        fun () -> "a".Should().FailWith("Value", NoFields)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: NoFields
"""


    [<Fact>]
    let ``Rendering of unions: Single-field DU cases in multi-case DU are not unwrapped, but the field is unwrapped``
        ()
        =
        fun () -> "a".Should().FailWith("Value", SingleField 1)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
  SingleField: 1
"""

    type MySingleCaseDu = A of int


    [<Fact>]
    let ``Rendering of unions: Single-case DUs are unwrapped`` () =
        fun () -> "a".Should().FailWith("Value", A 1)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 1
"""


    [<Fact>]
    let ``Rendering of unions: Some is not unwrapped`` () =
        fun () -> "a".Should().FailWith("Value", Some 1)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
  Some: 1
"""


    // https://github.com/Tarmil/FSharp.SystemTextJson/issues/171
    [<Fact>]
    let ``Known limitation: Rendering of unions: None is null`` () =
        fun () -> "a".Should().FailWith("Value", None)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: null
"""


    [<Fact>]
    let ``Rendering of unions: ValueSome is not unwrapped`` () =
        fun () -> "a".Should().FailWith("Value", ValueSome 1)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
  ValueSome: 1
"""


    [<Fact>]
    let ``Rendering of unions: ValueNone is not null`` () =
        fun () -> "a".Should().FailWith("Value", ValueNone)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: ValueNone
"""


    [<Fact>]
    let ``Enums are rendered as string`` () =
        fun () -> "a".Should().FailWith("Value", StringComparison.OrdinalIgnoreCase)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: OrdinalIgnoreCase
"""


    [<Fact>]
    let ``Rendering of exceptions`` () =
        let ex = InvalidOperationException("foo")

        fun () -> "a".Should().FailWith("Value", ex)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 'System.InvalidOperationException: foo'
"""


    [<Fact>]
    let ``Rendering of Type`` () =
        fun () -> "a".Should().FailWith("Value", typeof<Map<string, int>>)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
"""


    [<Fact>]
    let ``Rendering of anonymous type`` () =
        fun () -> "a".Should().FailWith("Value", typeof<{| A: int; B: Map<string, int> |}>)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: '{| A: System.Int32; B: Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32> |}'
"""


    let nestedTypeNames = [
        [|
            box typeof<Dictionary<string, int>.KeyCollection>
            box "System.Collections.Generic.Dictionary<System.String, System.Int32>+KeyCollection"
        |]
        [|
            box typeof<Dictionary<string, int>.ValueCollection>
            box "System.Collections.Generic.Dictionary<System.String, System.Int32>+ValueCollection"
        |]
        [|
            box typeof<Dictionary<string, int>.KeyCollection.Enumerator>
            box "System.Collections.Generic.Dictionary<System.String, System.Int32>+KeyCollection+Enumerator"
        |]
        [|
            box typeof<List<Dictionary<string, int>.Enumerator>>
            box
                "System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, System.Int32>+Enumerator>"
        |]
        [|
            box (typeof<Dictionary<string, int>.KeyCollection>.GetGenericTypeDefinition())
            box "System.Collections.Generic.Dictionary<TKey, TValue>+KeyCollection"
        |]
        [|
            box typeof<Environment.SpecialFolder>
            box "System.Environment+SpecialFolder"
        |]
    ]


    [<Theory>]
    [<MemberData(nameof nestedTypeNames)>]
    let ``Rendering of nested types preserves each declaring type`` (value: Type) (expected: string) =
        let error = assertFails (fun () -> ().Should().FailWith("Value", value))
        Assert.Contains("Value: " + expected, error.Message)


    [<Fact>]
    let ``Type mismatch diagnostics distinguish nested types of the same generic parent`` () =
        let value = Dictionary<string, int>().Values

        let error =
            assertFails (fun () -> value.Should().BeOfType(typeof<Dictionary<string, int>.KeyCollection>))

        Assert.Contains(
            "Expected: System.Collections.Generic.Dictionary<System.String, System.Int32>+KeyCollection",
            error.Message
        )

        Assert.Contains(
            "But was: System.Collections.Generic.Dictionary<System.String, System.Int32>+ValueCollection",
            error.Message
        )


    [<Fact>]
    let ``Rendering assigns generic arguments to the level that declares them`` () =
        let assembly =
            System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                System.Reflection.AssemblyName("Faqt.NestedTypeFormatting"),
                System.Reflection.Emit.AssemblyBuilderAccess.Run
            )

        let moduleBuilder = assembly.DefineDynamicModule("Types")

        let outer =
            moduleBuilder.DefineType("Fixtures.Outer`1", System.Reflection.TypeAttributes.Public)

        outer.DefineGenericParameters("TOuter") |> ignore

        let middle =
            outer.DefineNestedType("Middle", System.Reflection.TypeAttributes.NestedPublic)

        middle.DefineGenericParameters("TOuter") |> ignore

        let inner =
            middle.DefineNestedType("Inner`1", System.Reflection.TypeAttributes.NestedPublic)

        inner.DefineGenericParameters("TOuter", "TInner") |> ignore
        outer.CreateType() |> ignore
        middle.CreateType() |> ignore
        let definition = inner.CreateType()
        let value = definition.MakeGenericType(typeof<string>, typeof<int>)
        let error = assertFails (fun () -> ().Should().FailWith("Value", value))

        Assert.Contains("Value: Fixtures.Outer<System.String>+Middle+Inner<System.Int32>", error.Message)


    [<Fact>]
    let ``Rendering of CultureInfo`` () =
        fun () -> "a".Should().FailWith("Value", CultureInfo("nb-NO"))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: nb-NO
"""


    [<Fact>]
    let ``Rendering of CultureInfo.InvariantCulture`` () =
        fun () -> "a".Should().FailWith("Value", CultureInfo.InvariantCulture)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: invariant
"""


    [<Fact>]
    let ``Rendering of CultureInfo("")`` () =
        fun () -> "a".Should().FailWith("Value", CultureInfo(""))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: invariant
"""


    [<Fact>]
    let ``Rendering of TimeSpan`` () =
        fun () -> "a".Should().FailWith("Value", TimeSpan(1, 2, 3, 4, 5, 6))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 1.02:03:04.0050060
"""


    [<Fact>]
    let ``Rendering of DateTime`` () =
        fun () -> "a".Should().FailWith("Value", DateTime(2000, 1, 2, 3, 4, 5, 6))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 2000-01-02T03:04:05.006
"""


    [<Fact>]
    let ``Rendering of DateTimeOffset with zero offset`` () =
        fun () -> "a".Should().FailWith("Value", DateTimeOffset(2000, 1, 2, 3, 4, 5, 6, TimeSpan.Zero))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 2000-01-02T03:04:05.006+00:00
"""


    [<Fact>]
    let ``Rendering of DateTimeOffset with non-zero offset`` () =
        fun () -> "a".Should().FailWith("Value", DateTimeOffset(2000, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(7)))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 2000-01-02T03:04:05.006+07:00
"""


    [<Fact>]
    let ``Rendering of emoji string`` () =
        fun () -> "a".Should().FailWith("Value", "👍")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: "\U0001F44D"
"""


    [<Fact>]
    let ``Rendering of KeyValuePair`` () =
        fun () ->
            let x = KeyValuePair(1, "asd")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value:
  Key: 1
  Value: asd
"""


    [<Fact>]
    let ``Rendering when serialization throws`` () =
        fun () -> "".Should().FailWith("Value", TestUnserializableType())
        |> assertExnMsgWildcard
            """
Subject: '""'
Should: FailWith
Value:
  SERIALIZATION EXCEPTION: |-
    System.Exception: Foo
       at *
  ToString: TestUtils+TestUnserializableType
"""


    [<Fact>]
    let ``Rendering when serialization and ToString throw`` () =
        fun () -> "".Should().FailWith("Value", TestUnserializableTypeWithThrowingToString())
        |> assertExnMsgWildcard
            """
Subject: '""'
Should: FailWith
Value:
  SERIALIZATION EXCEPTION: |-
    System.Exception: Foo
       at *
  ToString: '[ToString() threw: Bar]'
"""


    [<Fact>]
    let ``Can render null values even if type is not serializable`` () =
        fun () -> "".Should().FailWith("Value", Unchecked.defaultof<TestUnserializableType>)
        |> assertExnMsg
            """
Subject: '""'
Should: FailWith
Value: null
"""


    [<Fact>]
    let ``Supports TryFormat with string as dictionary key`` () =
        fun () ->
            let x = dict [ TryFormat "a", 1 ]
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value:
  a: 1
"""


    [<Fact>]
    let ``Supports TryFormat with escaped string as dictionary key`` () =
        fun () ->
            let x = dict [ TryFormat "a\"b", 1 ]
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value:
  a"b: 1
"""


    [<Fact>]
    let ``Supports TryFormat with int as dictionary key`` () =
        fun () ->
            let x = dict [ TryFormat 1, 1 ]
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value:
  '1': 1
"""


    [<Fact>]
    let ``Supports TryFormat with null as dictionary key`` () =
        fun () ->
            let x = dict [ TryFormat null, 1 ]
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value:
  'null': 1
"""


    [<Fact>]
    let ``Supports TryFormatFallback for dictionary keys`` () =
        let format =
            YamlFormatterBuilder.Default.TryFormatFallback(fun _ _ -> "BROKEN").Build()

        use _ = Formatter.With(format)

        fun () ->
            let x = dict [ TryFormat(TestUnserializableType()), 1 ]
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value:
  BROKEN: 1
"""


    [<Fact>]
    let ``Rendering of HttpRequestMessage without headers or content`` () =
        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Patch, "https://foo.bar/asd?x=y&x=z#a")
            x.Version <- Version.Parse("0.5")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: PATCH https://foo.bar/asd?x=y&x=z#a HTTP/0.5
"""


    [<Fact>]
    let ``Rendering of HttpRequestMessage with headers and without content`` () =
        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Patch, "https://foo.bar/asd?x=y&x=z#a")
            x.Version <- Version.Parse("0.5")
            x.Headers.Add("Lorem", "Ipsum")
            x.Headers.Add("Foo", "Baz")
            x.Headers.Add("Foo", "Bar")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  PATCH https://foo.bar/asd?x=y&x=z#a HTTP/0.5
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar
"""


    [<Fact>]
    let ``Rendering of HttpRequestMessage with headers and content`` () =
        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Patch, "https://foo.bar/asd?x=y&x=z#a")
            x.Version <- Version.Parse("0.5")
            x.Headers.Add("Lorem", "Ipsum")
            x.Headers.Add("Foo", "Baz")
            x.Headers.Add("Foo", "Bar")
            x.Content <- JsonContent.Create({| a = "foo"; b = null; c = [ 3; 4 ] |})
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  PATCH https://foo.bar/asd?x=y&x=z#a HTTP/0.5
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar
  Content-Type: application/json; charset=utf-8
  Content-Length: 30

  [content has been formatted]
  { "a": "foo", "b": null, "c": [3, 4] }
"""


    [<Fact>]
    let ``Rendering of HttpRequestMessage with headers and content when disposed`` () =
        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Patch, "https://foo.bar/asd?x=y&x=z#a")
            x.Version <- Version.Parse("0.5")
            x.Headers.Add("Lorem", "Ipsum")
            x.Headers.Add("Foo", "Baz")
            x.Headers.Add("Foo", "Bar")
            x.Content <- JsonContent.Create({| a = "foo"; b = null; c = [ 3; 4 ] |})
            x.Dispose()
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  PATCH https://foo.bar/asd?x=y&x=z#a HTTP/0.5
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar

  [content is disposed and cannot be read]
"""


    [<Fact>]
    let ``Rendering of HttpRequestMessage with custom max content length`` () =
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(10))

        fun () ->
            let x = new HttpRequestMessage(HttpMethod.Get, "/")
            x.Version <- Version.Parse("0.5")
            x.Content <- new StringContent("lorem ipsum dolor sit amet")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  GET / HTTP/0.5
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsu…
  [content truncated after 10 bytes]
"""


    [<Fact>]
    let ``Rendering of HttpResponseMessage without headers or content`` () =
        fun () ->
            let x = new HttpResponseMessage(HttpStatusCode.NotFound)
            x.Version <- Version.Parse("0.5")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: HTTP/0.5 404 Not Found
"""


    [<Fact>]
    let ``Rendering of HttpResponseMessage with empty content includes content headers`` () =
        fun () ->
            let x = new HttpResponseMessage(HttpStatusCode.OK)
            x.Version <- Version.Parse("0.5")
            let content = new StringContent("")
            content.Headers.ContentType <- System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
            x.Content <- content
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  HTTP/0.5 200 OK
  Content-Type: application/json
  Content-Length: 0
"""


    [<Fact>]
    let ``Rendering of HttpResponseMessage with headers and without content`` () =
        fun () ->
            let x = new HttpResponseMessage(HttpStatusCode.NotFound)
            x.Version <- Version.Parse("0.5")
            x.Headers.Add("Lorem", "Ipsum")
            x.Headers.Add("Foo", "Baz")
            x.Headers.Add("Foo", "Bar")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  HTTP/0.5 404 Not Found
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar
"""


    [<Fact>]
    let ``Rendering of HttpResponseMessage with headers and content`` () =
        fun () ->
            let x = new HttpResponseMessage(HttpStatusCode.NotFound)
            x.Version <- Version.Parse("0.5")
            x.Headers.Add("Lorem", "Ipsum")
            x.Headers.Add("Foo", "Baz")
            x.Headers.Add("Foo", "Bar")
            x.Content <- JsonContent.Create({| a = "foo"; b = null; c = [ 3; 4 ] |})
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  HTTP/0.5 404 Not Found
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar
  Content-Type: application/json; charset=utf-8
  Content-Length: 30

  [content has been formatted]
  { "a": "foo", "b": null, "c": [3, 4] }
"""


    [<Fact>]
    let ``Rendering of HttpResponseMessage with non-seekable non-UTF8 content`` () =
        fun () ->
            let x = new HttpResponseMessage(HttpStatusCode.OK)
            x.Version <- Version.Parse("0.5")
            let bytes = Encoding.Unicode.GetBytes("Hej å")
            let content = new StreamContent(new OneByteNonSeekableStream(bytes))

            content.Headers.ContentType <-
                System.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/plain; charset=utf-16")

            x.Content <- content
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  HTTP/0.5 200 OK
  Content-Type: text/plain; charset=utf-16

  Hej å
  [nonseekable stream consumed for preview; subsequent reads resume after the consumed bytes]
"""


    [<Theory>]
    [<InlineData(true)>]
    [<InlineData(false)>]
    let ``Repeated HTTP message rendering includes the complete body`` isRequest =
        use request = new HttpRequestMessage(HttpMethod.Post, "/")
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)

        let message =
            if isRequest then
                request.Content <- new StringContent("complete body")
                box request
            else
                response.Content <- new StringContent("complete body")
                box response

        let render () =
            assertFails (fun () -> message.Should().FailWith("Value", message))

        render () |> ignore

        Assert.Contains("complete body", (render ()).Message)


    [<Theory>]
    [<InlineData(true, true, 0)>]
    [<InlineData(true, false, 0)>]
    [<InlineData(false, true, 0)>]
    [<InlineData(false, false, 0)>]
    [<InlineData(true, true, 8)>]
    [<InlineData(true, false, 8)>]
    [<InlineData(false, true, 8)>]
    [<InlineData(false, false, 8)>]
    let ``HTTP previews bound reads and preserve or report stream consumption`` isRequest canSeek limit =
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(limit))
        let mutable position = 3L
        let mutable bytesRead = 0
        let mutable disposed = false

        use stream =
            { new Stream() with
                override _.CanRead = not disposed
                override _.CanSeek = canSeek
                override _.CanWrite = false

                override _.Length =
                    if canSeek then
                        Int64.MaxValue
                    else
                        raise (NotSupportedException())

                override _.Position
                    with get () = position
                    and set value =
                        if not canSeek then
                            raise (NotSupportedException())

                        position <- value

                override _.Read(buffer, offset, count) =
                    if bytesRead + count > limit + 1 then
                        failwith "Read exceeded preview budget"

                    for i in 0 .. count - 1 do
                        buffer[offset + i] <- byte (int 'a' + int ((position + int64 i) % 26L))

                    bytesRead <- bytesRead + count
                    position <- position + int64 count
                    count

                override _.Flush() = ()
                override _.Seek(_, _) = raise (NotSupportedException())
                override _.SetLength(_) = raise (NotSupportedException())
                override _.Write(_, _, _) = raise (NotSupportedException())

                override _.Dispose(disposing) =
                    disposed <- true
                    base.Dispose(disposing)
            }

        use content = new StreamContent(stream)
        use request = new HttpRequestMessage(HttpMethod.Post, "/")
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        content.Headers.Add("X-Content-Header", "preserved")

        let message =
            if isRequest then
                request.Content <- content
                box request
            else
                response.Content <- content
                box response

        let rendered =
            (assertFails (fun () -> message.Should().FailWith("Value", message))).Message

        Assert.Contains("X-Content-Header: preserved", rendered)
        Assert.DoesNotContain("An exception", rendered)
        Assert.False(disposed)

        if limit = 0 then
            Assert.Equal(0, bytesRead)
            Assert.Equal(3L, position)
            Assert.Contains("content omitted", rendered)
        else
            Assert.Contains("defghijk", rendered)
            Assert.Contains("truncated after 8 bytes", rendered)
            Assert.Equal(9, bytesRead)

            if canSeek then
                Assert.Equal(3L, position)
            else
                Assert.Equal(12L, position)
                Assert.Contains("nonseekable stream consumed", rendered)
                Assert.DoesNotContain("Content-Length:", rendered)


    [<Theory>]
    [<InlineData(0)>]
    [<InlineData(7)>]
    [<InlineData(8)>]
    [<InlineData(9)>]
    let ``HTTP previews distinguish complete bodies from truncated prefixes`` length =
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(8))
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        response.Content <- new StringContent(String('a', length))
        let rendered = (assertFails (fun () -> response.Should().Be200Ok())).Message

        if length > 8 then
            Assert.Contains("truncated after 8 bytes", rendered)
        else
            Assert.DoesNotContain("truncated", rendered)


    [<Fact>]
    let ``HTTP preview restores a seekable stream position after a read error`` () =
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(8))

        use stream =
            { new MemoryStream(Encoding.UTF8.GetBytes("prefixBODY")) with
                override this.Read(buffer, offset, count) =
                    if this.Position > 6L then
                        failwith "read failure"

                    base.Read(buffer, offset, min 1 count)
            }

        stream.Position <- 6L
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        response.Content <- new StreamContent(stream)
        let rendered = (assertFails (fun () -> response.Should().Be200Ok())).Message
        Assert.Contains("read failure", rendered)
        Assert.Equal(6L, stream.Position)


    [<Theory>]
    [<InlineData(false, false)>]
    [<InlineData(false, true)>]
    [<InlineData(true, false)>]
    [<InlineData(true, true)>]
    let ``HTTP client response previews leave remaining content readable`` buffered obtainFirst =
        task {
            use timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10.0))
            let ct = timeout.Token
            use listener = new TcpListener(IPAddress.Loopback, 0)
            listener.Start()
            let port = (listener.LocalEndpoint :?> IPEndPoint).Port
            let body = "abcdefghijklmnopqrstuvwxyz"

            let serve =
                task {
                    use! connection = listener.AcceptTcpClientAsync(ct)
                    use stream = connection.GetStream()
                    use reader = new StreamReader(stream, leaveOpen = true)
                    let mutable finished = false

                    while not finished do
                        let! line = reader.ReadLineAsync(ct)
                        finished <- String.IsNullOrEmpty(line)

                    let bytes =
                        Encoding.ASCII.GetBytes(
                            "HTTP/1.1 400 Bad Request\r\nContent-Length: 26\r\nConnection: close\r\n\r\n"
                            + body
                        )

                    do! stream.WriteAsync(bytes.AsMemory(), ct)
                }

            use handler = new HttpClientHandler(UseProxy = false)
            use client = new HttpClient(handler)

            let completion =
                if buffered then
                    HttpCompletionOption.ResponseContentRead
                else
                    HttpCompletionOption.ResponseHeadersRead

            use! response = client.GetAsync($"http://127.0.0.1:%i{port}/", completion, ct)
            do! serve

            // Keep thread-local configuration and assertion scope entirely after the asynchronous setup.
            use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(8))
            let initialPosition = if obtainFirst then 1 else 0

            if obtainFirst then
                Assert.Equal(int 'a', response.Content.ReadAsStream().ReadByte())

            let rendered = (assertFails (fun () -> response.Should().Be200Ok())).Message
            Assert.Contains(body.Substring(initialPosition, 8), rendered)
            Assert.Contains("truncated after 8 bytes", rendered)
            Assert.DoesNotContain("An exception", rendered)
            Assert.Equal(not buffered, rendered.Contains("nonseekable stream consumed"))

            let stream = response.Content.ReadAsStream()
            let nextPosition = if buffered then initialPosition else initialPosition + 9
            Assert.Equal(int body[nextPosition], stream.ReadByte())

            let repeated = (assertFails (fun () -> response.Should().Be200Ok())).Message
            Assert.Contains(body.Substring(nextPosition + 1, 8), repeated)
            Assert.DoesNotContain("An exception", repeated)
        }


    [<Fact>]
    let ``Truncated HTTP input is not formatted as a complete JSON document`` () =
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(2))
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        response.Content <- new StringContent("{} trailing data")
        let rendered = (assertFails (fun () -> response.Should().Be200Ok())).Message
        Assert.Contains("{}", rendered)
        Assert.Contains("truncated after 2 bytes", rendered)
        Assert.DoesNotContain("content has been formatted", rendered)

    [<Theory>]
    [<InlineData(true, "utf-16")>]
    [<InlineData(false, "utf-16")>]
    [<InlineData(true, "iso-8859-1")>]
    [<InlineData(false, "iso-8859-1")>]
    let ``HTTP rendering honors quoted charset parameters`` isRequest (charset: string) =
        use request = new HttpRequestMessage(HttpMethod.Post, "/")
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        use content = new ByteArrayContent(Encoding.GetEncoding(charset).GetBytes("Hej å"))

        content.Headers.ContentType <-
            System.Net.Http.Headers.MediaTypeHeaderValue.Parse($"text/plain; charset=\"%s{charset}\"")

        let message =
            if isRequest then
                request.Content <- content
                box request
            else
                response.Content <- content
                box response

        let rendered =
            (assertFails (fun () -> message.Should().FailWith("Value", message))).Message

        Assert.Contains("Hej å", rendered)


    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(5)>]
    let ``Rendering partially or fully read seekable HTTP content previews the remaining body`` bytesRead =
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        response.Content <- new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("hello")))
        let stream = response.Content.ReadAsStream()

        for _ in 1..bytesRead do
            stream.ReadByte() |> ignore

        let failure = assertFails (fun () -> response.Should().Be200Ok())

        Assert.DoesNotContain("hello", failure.Message)
        Assert.Contains("hello".Substring(bytesRead), failure.Message)
        Assert.Equal(int64 bytesRead, stream.Position)


    [<Fact>]
    let ``Nested HTTP assertion failures include the complete body`` () =
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        response.Content <- new StringContent("complete body")

        let failure =
            assertFails (fun () -> response.Should().Satisfy(fun r -> r.Should().Be200Ok()))

        Assert.Equal(2, failure.Message.Split("complete body").Length - 1)


    [<Theory>]
    [<InlineData(0, false)>]
    [<InlineData(1, false)>]
    [<InlineData(4, false)>]
    [<InlineData(0, true)>]
    [<InlineData(1, true)>]
    [<InlineData(4, true)>]
    let ``Rendering StreamContent preserves its current position across repeated previews`` bytesRead renderFirst =
        use stream = new MemoryStream(Encoding.UTF8.GetBytes("prefixBODY"))
        stream.Position <- 6L
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        response.Version <- Version.Parse("0.5")
        response.Content <- new StreamContent(stream)
        let contentStream = response.Content.ReadAsStream()

        for _ in 1..bytesRead do
            contentStream.ReadByte() |> ignore

        if renderFirst then
            assertFails (fun () -> response.Should().Be200Ok()) |> ignore

        let rendered = (assertFails (fun () -> response.Should().Be200Ok())).Message
        Assert.Contains("Content-Length: 4", rendered)
        Assert.Contains("BODY".Substring(bytesRead), rendered)
        Assert.DoesNotContain("prefix", rendered)

        if bytesRead > 0 then
            Assert.DoesNotContain("BODY", rendered)

        Assert.Equal(6L + int64 bytesRead, stream.Position)


    [<Theory>]
    [<InlineData(-1L)>]
    [<InlineData(1L)>]
    [<InlineData(10L)>]
    let ``Rendering StreamContent does not infer its offset from an overridden Content-Length`` headerLength =
        use stream = new MemoryStream(Encoding.UTF8.GetBytes("prefixBODY"))
        stream.Position <- 6L
        use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        response.Version <- Version.Parse("0.5")
        response.Content <- new StreamContent(stream)

        response.Content.Headers.ContentLength <-
            if headerLength < 0L then
                Nullable()
            else
                Nullable(headerLength)

        response.Content.ReadAsStream().ReadByte() |> ignore
        let rendered = (assertFails (fun () -> response.Should().Be200Ok())).Message

        if headerLength < 0L then
            Assert.DoesNotContain("Content-Length:", rendered)
        else
            Assert.Contains($"Content-Length: %i{headerLength}", rendered)

        Assert.Contains("ODY", rendered)
        Assert.DoesNotContain("BODY", rendered)
        Assert.DoesNotContain("prefix", rendered)
        Assert.Equal(7L, stream.Position)


    [<Fact>]
    let ``Rendering of HttpResponseMessage with headers and content when disposed`` () =
        fun () ->
            let x = new HttpResponseMessage(HttpStatusCode.NotFound)
            x.Version <- Version.Parse("0.5")
            x.Headers.Add("Lorem", "Ipsum")
            x.Headers.Add("Foo", "Baz")
            x.Headers.Add("Foo", "Bar")
            x.Content <- JsonContent.Create({| a = "foo"; b = null; c = [ 3; 4 ] |})
            x.Dispose()
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  HTTP/0.5 404 Not Found
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar

  [content is disposed and cannot be read]
"""


    [<Fact>]
    let ``Rendering of HttpResponseMessage with custom max content length`` () =
        use _ = Config.With(FaqtConfig.Default.SetHttpContentMaxLength(10))

        fun () ->
            let x = new HttpResponseMessage(HttpStatusCode.NotFound)
            x.Version <- Version.Parse("0.5")
            x.Content <- new StringContent("lorem ipsum dolor sit amet")
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: |-
  HTTP/0.5 404 Not Found
  Content-Type: text/plain; charset=utf-8
  Content-Length: 26

  lorem ipsu…
  [content truncated after 10 bytes]
"""


    [<Fact>]
    let ``Rendering of HttpStatusCode`` () =
        fun () ->
            let x = HttpStatusCode.BadRequest
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: 400 Bad Request
"""


    [<Fact>]
    let ``Rendering of non-existent HttpStatusCode`` () =
        fun () ->
            let x = enum<HttpStatusCode> 999
            x.Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: x
Should: FailWith
Value: '999'
"""


    [<Fact>]
    let ``Rendering of byte array`` () =
        fun () -> ().Should().FailWith("Value", [| 0uy; 1uy; 254uy; 255uy |])
        |> assertExnMsg
            """
Subject: ()
Should: FailWith
Value: 0001FEFF
"""


    [<Fact>]
    let ``Rendering of byte sequence`` () =
        fun () ->
            ()
                .Should()
                .FailWith(
                    "Value",
                    seq {
                        0uy
                        1uy
                        254uy
                        255uy
                    }
                )
        |> assertExnMsg
            """
Subject: ()
Should: FailWith
Value: 0001FEFF
"""


    [<Fact>]
    let ``Dictionary format with non-string keys`` () =
        fun () -> ().Should().FailWith("Value", dict [ 1, "a"; 2, "b" ])
        |> assertExnMsg
            """
Subject: ()
Should: FailWith
Value:
  '1': a
  '2': b
"""


    [<Fact>]
    let ``Map format with non-string keys`` () =
        fun () -> ().Should().FailWith("Value", Map.ofList [ 1, "a"; 2, "b" ])
        |> assertExnMsg
            """
Subject: ()
Should: FailWith
Value:
  '1': a
  '2': b
"""


module TryFormatCycles =


    type Node(getNext: unit -> (obj | null)) =
        member _.Next = getNext ()


    let private data (value: obj | null) =
        match value with
        | null -> invalidArg (nameof value) "Expected a non-null test value"
        | value -> {
            Subject = [ "subject" ]
            Because = None
            Should = "Fail"
            Extra = [ "Value", value; "After", "preserved" ]
          }


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Cycles crossing TryFormat use the fallback before exhausting the stack`` dictionaryKey =
        let mutable reads = 0
        let mutable value = obj ()

        let node =
            Node(fun () ->
                reads <- reads + 1

                // Bound the broken implementation so this regression fails without crashing the test host.
                if reads >= 8 then
                    invalidOp "Test cycle guard reached"

                if dictionaryKey then
                    box (dict [ TryFormat value, 1 ])
                else
                    box (TryFormat value)
            )

        value <- node
        let output = YamlFormatterBuilder.Default.Build () (data value)
        Assert.InRange(reads, 1, 7)
        Assert.Contains("SERIALIZATION EXCEPTION", output)
        Assert.Contains("JsonException", output)
        Assert.Contains("After: preserved", output)


    [<Theory>]
    [<InlineData(0, 80, true)>]
    [<InlineData(4, 12, true)>]
    [<InlineData(96, 80, false)>]
    let ``Nested TryFormat values respect the configured recursion limit`` maxDepth wrapperCount expectFallback =
        let format =
            YamlFormatterBuilder.Default
                .ConfigureJsonSerializerOptions(fun options -> options.MaxDepth <- maxDepth)
                .Build()

        let value =
            (box "leaf", [ 1..wrapperCount ])
            ||> List.fold (fun value _ -> box (TryFormat value))

        let output = format (data value)
        Assert.Equal(expectFallback, output.Contains("SERIALIZATION EXCEPTION"))
        Assert.Contains("After: preserved", output)
        Assert.Contains("Value: healthy", format (data (box "healthy")))


    [<Fact>]
    let ``Shared values in sibling branches are not cycles`` () =
        let shared = Node(fun () -> box "leaf")

        let output =
            YamlFormatterBuilder.Default.Build () (data (box [ TryFormat shared; TryFormat shared ]))

        Assert.DoesNotContain("SERIALIZATION EXCEPTION", output)
        Assert.Equal(2, output.Split("Next: leaf").Length - 1)


    [<Fact>]
    let ``Concurrent formatting of the same value does not share cycle tracking`` () =
        use barrier = new Barrier(2)

        let shared =
            Node(fun () ->
                Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(10.0)))
                box "leaf"
            )

        let format = YamlFormatterBuilder.Default.Build()

        let start () =
            System.Threading.Tasks.Task.Factory.StartNew(
                (fun () -> format (data (box shared))),
                System.Threading.Tasks.TaskCreationOptions.LongRunning
            )

        let first = start ()
        let second = start ()

        for output in [ first.GetAwaiter().GetResult(); second.GetAwaiter().GetResult() ] do
            Assert.DoesNotContain("SERIALIZATION EXCEPTION", output)
            Assert.Contains("Next: leaf", output)


    [<Fact>]
    let ``Cycle tracking is restored when the fallback throws`` () =
        let mutable shouldThrow = true

        let value =
            Node(fun () ->
                if shouldThrow then
                    invalidOp "Cannot serialize yet"

                box "leaf"
            )

        let format =
            YamlFormatterBuilder.Default.TryFormatFallback(fun _ _ -> invalidOp "Fallback failed").Build()

        Assert.Throws<InvalidOperationException>(fun () -> format (data value) |> ignore)
        |> ignore

        shouldThrow <- false
        Assert.Contains("Next: leaf", format (data value))


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Fallbacks that wrap the failing value cannot recursively retry their own cycle error`` dictionaryKey =
        let mutable fallbackCalls = 0

        let format =
            YamlFormatterBuilder.Default
                .TryFormatFallback(fun _ value ->
                    fallbackCalls <- fallbackCalls + 1

                    if fallbackCalls >= 8 then
                        invalidOp "Test fallback guard reached"

                    TryFormat value :> obj
                )
                .Build()

        let value: obj =
            if dictionaryKey then
                dict [ TryFormat(TestUnserializableType()), 1 ]
            else
                TestUnserializableType()

        Assert.Throws<JsonException>(fun () -> format (data value) |> ignore) |> ignore
        Assert.InRange(fallbackCalls, 1, 7)


module YamlFallback =


    let private diagnosticJson (error: AssertionFailedException) =
        error.Message.Substring(("Assertion failed." + Environment.NewLine).Length)
        |> JsonDocument.Parse


    [<Theory>]
    [<InlineData("{\"key\":1,\"key\":2}")>]
    [<InlineData("{\"nested\":{\"key\":1,\"key\":2}}")>]
    [<InlineData("[{\"key\":1,\"key\":2}]")>]
    [<InlineData("{\"key\":null,\"key\":[true,{\"other\":\"value\"}]}")>]
    let ``Preserves duplicate JSON properties in assertion diagnostics`` (json: string) =
        use value = JsonDocument.Parse(json)
        let error = assertFails (fun () -> ().Should().FailWith("Value", value.RootElement))
        use diagnostic = diagnosticJson error
        Assert.Equal("FailWith", diagnostic.RootElement.GetProperty("Should").GetString())

        Assert.Equal(
            JsonSerializer.Serialize(value.RootElement),
            diagnostic.RootElement.GetProperty("Value").GetRawText()
        )


    [<Fact>]
    let ``Preserves repeated extra data keys`` () =
        let error = assertFails (fun () -> ().Should().FailWithDuplicateData())
        use diagnostic = diagnosticJson error

        let values =
            diagnostic.RootElement.EnumerateObject()
            |> Seq.filter (fun property -> property.Name = "Repeated")
            |> Seq.map (fun property -> property.Value.GetString())
            |> Seq.toList

        Assert.Equal<(string | null) list>([ "first"; "second" ], values)


    [<Fact>]
    let ``Preserves dictionary keys that serialize to the same property name`` () =
        let value = dict [ TryFormat 1, "number"; TryFormat "1", "string" ]
        let error = assertFails (fun () -> ().Should().FailWith("Value", value))
        use diagnostic = diagnosticJson error
        Assert.Equal("{\"1\":\"number\",\"1\":\"string\"}", diagnostic.RootElement.GetProperty("Value").GetRawText())


    [<Theory>]
    [<InlineData("NotSatisfy")>]
    [<InlineData("SatisfyAny")>]
    let ``Duplicate JSON properties remain ordinary assertion failures in composed assertions`` composition =
        use value = JsonDocument.Parse("{\"key\":1,\"key\":2}")

        let fail () =
            ().Should().FailWith("Value", value.RootElement) |> ignore

        match composition with
        | "NotSatisfy" -> ().Should().NotSatisfy(fail) |> ignore
        | _ ->
            let mutable reachedAlternative = false

            ().Should().SatisfyAny([ fail; (fun () -> reachedAlternative <- true) ])
            |> ignore

            Assert.True(reachedAlternative)


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Yaml exceptions from custom visitors still propagate`` throwFromFactory =
        let original = YamlException("Custom visitor failed")

        let format =
            YamlFormatterBuilder.Default
                .SetYamlVisitor(fun _ ->
                    if throwFromFactory then
                        raise original

                    { new YamlVisitorBase() with
                        override _.Visit(_: YamlMappingNode) : unit = raise original
                    }
                )
                .Build()

        use _ = Formatter.With(format)
        let actual = Assert.Throws<YamlException>(fun () -> ().Should().Fail() |> ignore)
        Assert.Same(original, actual)


module YamlFormatterBuilder =


    [<Fact>]
    let ``ConfigureJsonSerializerOptions throws if null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            YamlFormatterBuilder.Default.ConfigureJsonSerializerOptions(
                Unchecked.defaultof<JsonSerializerOptions -> unit>
            )
            |> ignore
        )
        |> ignore


    [<Fact>]
    let ``ConfigureJsonFSharpOptions throws if null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            YamlFormatterBuilder.Default.ConfigureJsonFSharpOptions(
                Unchecked.defaultof<JsonFSharpOptions -> JsonFSharpOptions>
            )
            |> ignore
        )
        |> ignore


    [<Fact>]
    let ``AddConverter throws if null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            YamlFormatterBuilder.Default.AddConverter(Unchecked.defaultof<JsonConverter>)
            |> ignore
        )
        |> ignore


    [<Fact>]
    let ``SerializeAs throws if projection is null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            YamlFormatterBuilder.Default.SerializeAs(Unchecked.defaultof<string -> int>)
            |> ignore
        )
        |> ignore


    [<Fact>]
    let ``SerializeExactAs throws if projection is null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            YamlFormatterBuilder.Default.SerializeExactAs(Unchecked.defaultof<string -> int>)
            |> ignore
        )
        |> ignore


    [<Fact>]
    let ``ConfigureJsonSerializerOptions works`` () =
        let format =
            YamlFormatterBuilder.Default
                .ConfigureJsonSerializerOptions(fun opts -> opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase)
                .ConfigureJsonSerializerOptions(fun opts -> opts.NumberHandling <- JsonNumberHandling.WriteAsString)
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", {| A = 1 |})
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
  a: '1'
"""


    type SingleCaseDu = SingleCaseDu of int


    [<Fact>]
    let ``ConfigureJsonFSharpOptions works 1`` () =
        let format =
            YamlFormatterBuilder.Default
                .ConfigureJsonFSharpOptions(fun _ -> JsonFSharpOptions.Default().WithUnionUnwrapSingleCaseUnions(true))
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", SingleCaseDu 1)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 1
"""


    [<Fact>]
    let ``ConfigureJsonFSharpOptions works 2`` () =
        let format =
            YamlFormatterBuilder.Default
                .ConfigureJsonFSharpOptions(fun _ -> JsonFSharpOptions.Default().WithUnionUnwrapSingleCaseUnions(false))
                .ConfigureJsonFSharpOptions(fun opts -> opts.WithUnionTagName("Test1"))
                .ConfigureJsonFSharpOptions(fun opts -> opts.WithUnionTagName("Test2"))
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", SingleCaseDu 1)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value:
  Test2: SingleCaseDu
  Fields: [1]
"""


    type StringOptionConverter(upperCase) =
        inherit JsonConverter<string option>()

        let maybeUpper (s: string) =
            if upperCase then s.ToUpperInvariant() else s

        override this.Read(_, _, _) = failwith ""

        override this.Write(writer, value, options) =
            match value with
            | None -> JsonSerializer.Serialize(writer, maybeUpper "None", options)
            | Some s -> JsonSerializer.Serialize(writer, maybeUpper $"Some {s}", options)


    [<Fact>]
    let ``AddConverter works and allows overriding existing converters`` () =
        let format =
            YamlFormatterBuilder.Default
                .AddConverter(StringOptionConverter(false))
                .AddConverter(StringOptionConverter(true))
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", Some "b")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: SOME B
"""


    [<Fact>]
    let ``SerializeAs works and allows overriding existing converters`` () =
        let format =
            YamlFormatterBuilder.Default
                .SerializeAs(string<string option>)
                .SerializeAs(string<string option> >> fun s -> s.ToUpperInvariant())
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", Some "b")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: SOME(B)
"""


    [<Fact>]
    let ``SerializeAs also applies to subtypes`` () =
        let format =
            YamlFormatterBuilder.Default.SerializeAs(fun (_: TestBaseType) -> "FOO").Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", TestSubType())
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: FOO
"""


    [<Fact>]
    let ``SerializeAs also applies to interfaces`` () =
        let format =
            YamlFormatterBuilder.Default.SerializeAs(fun (_: TestInterface) -> "FOO").Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", TestSubType())
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: FOO
"""


    [<Fact>]
    let ``SerializeAs throws expected exception if input and output types are identical`` () =
        let ex =
            Assert.Throws<ArgumentException>(fun () -> YamlFormatterBuilder.Default.SerializeAs(id<string>) |> ignore)

        Assert.Equal(
            "The projected type must not be assignable to the input type, or a stack overflow would occur (Parameter 'projection')",
            ex.Message
        )


    [<Theory>]
    [<InlineData("subclass")>]
    [<InlineData("interface class")>]
    [<InlineData("interface struct")>]
    [<InlineData("object")>]
    let ``SerializeAs rejects output types matched by its own converter`` scenario =
        let ex =
            Assert.Throws<ArgumentException>(fun () ->
                match scenario with
                | "subclass" ->
                    YamlFormatterBuilder.Default.SerializeAs(fun (_: TestBaseType) -> TestSubType())
                    |> ignore
                | "interface class" ->
                    YamlFormatterBuilder.Default.SerializeAs(fun (_: IConvertible) -> "value")
                    |> ignore
                | "interface struct" -> YamlFormatterBuilder.Default.SerializeAs(fun (_: IConvertible) -> 42) |> ignore
                | "object" -> YamlFormatterBuilder.Default.SerializeAs(fun (_: obj) -> "value") |> ignore
                | _ -> failwith "Unknown scenario"
            )

        Assert.Equal("projection", ex.ParamName)


    [<Fact>]
    let ``SerializeExactAs allows projecting to a subtype without recursively applying the converter`` () =
        let mutable calls = 0

        let format =
            YamlFormatterBuilder.Default
                .SerializeExactAs(fun (_: TestBaseType) ->
                    calls <- calls + 1
                    TestSubType()
                )
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", TestBaseType())
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: {}
"""

        Assert.Equal(1, calls)


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Boxed projections cannot select their own converter again`` exact =
        let mutable calls = 0

        let projection (value: int) : obj =
            calls <- calls + 1

            // Bound the broken implementation so this regression cannot overflow the test host's stack.
            if calls >= 8 then
                invalidOp "Test projection guard reached"

            value

        let builder =
            if exact then
                YamlFormatterBuilder.Default.SerializeExactAs(projection)
            else
                YamlFormatterBuilder.Default.SerializeAs(projection)

        use _ = Formatter.With(builder.Build())
        let error = assertFails (fun () -> ().Should().FailWith("Value", 42))
        Assert.Equal(1, calls)
        Assert.Contains("JsonException", error.Message)
        Assert.DoesNotContain("Test projection guard reached", error.Message)


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Boxed projections can return a different type or null`` exact =
        let projection (value: int) : obj | null =
            if value = 0 then null else string value

        let builder =
            if exact then
                YamlFormatterBuilder.Default.SerializeExactAs(projection)
            else
                YamlFormatterBuilder.Default.SerializeAs(projection)

        use _ = Formatter.With(builder.Build())
        let error = assertFails (fun () -> ().Should().FailWith("Value", [ 0; 42 ]))
        Assert.Contains("Value: [null, '42']", error.Message)


    type BoxedObjectConverter() =
        inherit JsonConverter<obj>()

        override _.Read(_, _, _) = failwith "Can only write"

        override _.Write(writer, _, _) =
            writer.WriteStringValue("object-converter")


    type BoxedObjectConverterFactory() =
        inherit JsonConverterFactory()

        override _.CanConvert(t) = t = typeof<obj>

        override _.CreateConverter(_, _) = BoxedObjectConverter()


    [<Theory>]
    [<InlineData(false, false)>]
    [<InlineData(true, false)>]
    [<InlineData(false, true)>]
    [<InlineData(true, true)>]
    let ``Boxed projections respect custom object converters`` exact useFactory =
        let projection (value: int) : obj = value

        let builder =
            if exact then
                YamlFormatterBuilder.Default.SerializeExactAs(projection)
            else
                YamlFormatterBuilder.Default.SerializeAs(projection)

        let converter: JsonConverter =
            if useFactory then
                BoxedObjectConverterFactory()
            else
                BoxedObjectConverter()

        use _ = Formatter.With(builder.AddConverter(converter).Build())
        let error = assertFails (fun () -> ().Should().FailWith("Value", 42))
        Assert.Contains("Value: object-converter", error.Message)


    type ProjectionNode = {
        Value: int
        Children: ProjectionNode list
    }


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Projections can serialize child values with the same converter`` exact =
        let projection (node: ProjectionNode) = {|
            Text = string node.Value
            Children = node.Children
        |}

        let builder =
            if exact then
                YamlFormatterBuilder.Default.SerializeExactAs(projection)
            else
                YamlFormatterBuilder.Default.SerializeAs(projection)

        let node = {
            Value = 1
            Children = [ { Value = 2; Children = [] } ]
        }

        use _ = Formatter.With(builder.Build())
        let error = assertFails (fun () -> ().Should().FailWith("Value", node))
        Assert.Contains("Text: '1'", error.Message)
        Assert.Contains("Text: '2'", error.Message)
        Assert.DoesNotContain("SERIALIZATION EXCEPTION", error.Message)


    [<Fact>]
    let ``Projection tracking is restored after a failed write`` () =
        let mutable configuredOptions = Unchecked.defaultof<JsonSerializerOptions>

        YamlFormatterBuilder.Default
            .ConfigureJsonSerializerOptions(fun options -> configuredOptions <- options)
            .SerializeAs(fun (value: int) ->
                if value = 0 then
                    invalidOp "Projection failed"

                string value
            )
            .Build()
        |> ignore

        use stream = new MemoryStream()
        use writer = new Utf8JsonWriter(stream)

        Assert.Throws<InvalidOperationException>(fun () -> JsonSerializer.Serialize(writer, 0, configuredOptions))
        |> ignore

        JsonSerializer.Serialize(writer, 42, configuredOptions)
        writer.Flush()
        Assert.Equal("\"42\"", Encoding.UTF8.GetString(stream.ToArray()))


    [<Fact>]
    let ``Boxed subtype projections respect the selected converter`` () =
        let format =
            YamlFormatterBuilder.Default
                .SerializeAs(fun (_: TestBaseType) -> TestSubType() :> obj)
                .SerializeExactAs(fun (_: TestSubType) -> "subtype")
                .Build()

        use _ = Formatter.With(format)
        let error = assertFails (fun () -> ().Should().FailWith("Value", TestBaseType()))
        Assert.Contains("Value: subtype", error.Message)


    [<Fact>]
    let ``Exact boxed subtype projections do not select the base converter`` () =
        let format =
            YamlFormatterBuilder.Default.SerializeExactAs(fun (_: TestBaseType) -> TestSubType() :> obj).Build()

        use _ = Formatter.With(format)
        let error = assertFails (fun () -> ().Should().FailWith("Value", TestBaseType()))
        Assert.Contains("Value: {}", error.Message)


    [<Fact>]
    let ``Statically typed base projections preserve static serialization`` () =
        let format =
            YamlFormatterBuilder.Default.SerializeAs(fun (value: TestSubType) -> value :> TestBaseType).Build()

        use _ = Formatter.With(format)
        let error = assertFails (fun () -> ().Should().FailWith("Value", TestSubType()))
        Assert.Contains("Value: {}", error.Message)


    [<Fact>]
    let ``SerializeExactAs works and allows overriding existing converters`` () =
        let format =
            YamlFormatterBuilder.Default
                .SerializeExactAs(string<string option>)
                .SerializeAs(string<string option> >> fun s -> s.ToUpperInvariant())
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", Some "b")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: SOME(B)
"""


    [<Fact>]
    let ``SerializeExactAs does not apply to subtypes`` () =
        let format =
            YamlFormatterBuilder.Default.SerializeExactAs(fun (_: TestBaseType) -> "FOO").Build()

        use _ = Formatter.With(format)

        let x = TestSubType()
        x :> TestBaseType |> ignore // Sanity check to avoid false negatives

        fun () -> "a".Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: {}
"""


    // This test is important because we use obj several places (notably in TryFormat), causing converters' CanConvert
    // to be called with type = System.Object unless type information is passed to JsonSerializer.Serialize.
    [<Fact>]
    let ``SerializeExactAs does not apply to obj as subtype`` () =
        let format =
            YamlFormatterBuilder.Default.SerializeExactAs(fun (_: obj) -> "FOO").Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWith("Value", "asd")
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: asd
"""


    [<Fact>]
    let ``SerializeExactAs does not apply to interfaces`` () =
        let format =
            YamlFormatterBuilder.Default.SerializeExactAs(fun (_: TestInterface) -> "FOO").Build()

        use _ = Formatter.With(format)

        let x = TestSubType()
        x :> TestInterface |> ignore // Sanity check to avoid false negatives

        fun () -> "a".Should().FailWith("Value", x)
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: {}
"""


    [<Fact>]
    let ``SerializeExactAs throws expected exception if input and output types are identical`` () =
        let ex =
            Assert.Throws<ArgumentException>(fun () ->
                YamlFormatterBuilder.Default.SerializeExactAs(id<string>) |> ignore
            )

        Assert.Equal(
            "The projected type must be different from the input type, or a stack overflow would occur (Parameter 'projection')",
            ex.Message
        )


    [<Fact>]
    let ``TryFormatFallback works and is used by default for top-level items`` () =
        let format =
            YamlFormatterBuilder.Default
                .TryFormatFallback(fun _ _ -> "IGNORED")
                .TryFormatFallback(fun ex obj -> {|
                    Error = ex.Message
                    ToString = obj.ToString()
                |})
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().FailWithUnserializableAtTopAndNested()
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWithUnserializableAtTopAndNested
A:
  Error: Foo
  ToString: TestUtils+TestUnserializableType
B:
- Error: Foo
  ToString: TestUtils+TestUnserializableType
"""


    [<Fact>]
    let ``TryFormatFallback throws if null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            YamlFormatterBuilder.Default.TryFormatFallback(Unchecked.defaultof<exn -> obj -> obj>)
            |> ignore
        )
        |> ignore


    [<Fact>]
    let ``If TryFormatFallback function fails, exception bubbles up`` () =
        let format =
            YamlFormatterBuilder.Default.TryFormatFallback(fun _ _ -> invalidOp "foo").Build()

        use _ = Formatter.With(format)

        let f () =
            "a".Should().FailWithUnserializableAtTopAndNested()

        let ex = Assert.Throws<InvalidOperationException>(f >> ignore)
        Assert.Equal("foo", ex.Message)


    type TestYamlVisitor(_doc: YamlDocument, style: ScalarStyle) =
        inherit YamlVisitorBase()

        override _.Visit(scalar: YamlScalarNode) =
            scalar.Style <- style
            base.Visit(scalar)


    [<Fact>]
    let ``SetYamlVisitor throws if null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            YamlFormatterBuilder.Default.SetYamlVisitor(Unchecked.defaultof<YamlDocument -> YamlVisitorBase>)
            |> ignore
        )
        |> ignore


    [<Fact>]
    let ``SetYamlVisitor works`` () =
        let format =
            YamlFormatterBuilder.Default
                .SetYamlVisitor(fun doc -> TestYamlVisitor(doc, ScalarStyle.DoubleQuoted))
                .SetYamlVisitor(fun doc -> TestYamlVisitor(doc, ScalarStyle.SingleQuoted))
                .Build()

        use _ = Formatter.With(format)

        fun () -> "a".Should().Fail()
        |> assertExnMsg
            """
{'Subject': '"a"', 'Should': 'Fail'}
"""
