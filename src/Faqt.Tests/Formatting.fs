module Formatting

open System
open System.Collections.Generic
open System.Globalization
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Serialization
open Faqt
open AssertionHelpers
open Faqt.Formatting
open Xunit
open YamlDotNet.Core
open YamlDotNet.RepresentationModel


[<Extension>]
type private Assertions =


    [<Extension>]
    static member FailWithUnserializableAtTopAndNested(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()

        t
            .With("A", TestUnserializableType())
            .With("B", [ TryFormat(TestUnserializableType()) ])
            .Fail(None)


[<Fact>]
let ``Can override and restore the default formatter`` () =

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
    let ``Rendering of top-level structure with multi-part subject name`` () =
        fun () ->
            (Some "a")
                .Should()
                .BeSome()
                .Whose.Length.Should(())
                .FailWithBecause("Some reason", "Foo", "Bar")
        |> assertExnMsg
            """
Subject:
- (Some "a")
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
        fun () ->
            (Some "a")
                .Should()
                .BeSome()
                .Whose.Length.Should(())
                .FailWith("Value", {| Subject = [ "a"; "b" ] |})
        |> assertExnMsg
            """
Subject:
- (Some "a")
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
        fun () ->
            "a"
                .Should()
                .FailWith("Value", DateTimeOffset(2000, 1, 2, 3, 4, 5, 6, TimeSpan.Zero))
        |> assertExnMsg
            """
Subject: '"a"'
Should: FailWith
Value: 2000-01-02T03:04:05.006+00:00
"""


    [<Fact>]
    let ``Rendering of DateTimeOffset with non-zero offset`` () =
        fun () ->
            "a"
                .Should()
                .FailWith("Value", DateTimeOffset(2000, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(7)))
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

  {"a":"foo","b":null,"c":[3,4]}
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
        |> assertExnMsgWildcard
            """
Subject: x
Should: FailWith
Value: |-
  PATCH https://foo.bar/asd?x=y&x=z#a HTTP/0.5
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar

  An exception occured trying to get the content:
  System.ObjectDisposedException: Cannot access a disposed object.
  Object name: 'System.Net.Http.Json.JsonContent'.
     at *
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

  {"a":"foo","b":null,"c":[3,4]}
"""


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
        |> assertExnMsgWildcard
            """
Subject: x
Should: FailWith
Value: |-
  HTTP/0.5 404 Not Found
  Lorem: Ipsum
  Foo: Baz
  Foo: Bar

  An exception occured trying to get the content:
  System.ObjectDisposedException: Cannot access a disposed object.
  Object name: 'System.Net.Http.Json.JsonContent'.
     at *
"""


module YamlFormatterBuilder =


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
            YamlFormatterBuilder.Default
                .SerializeAs(fun (_: TestInterface) -> "FOO")
                .Build()

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
            "The projected type must be different from the input type, or a stack overflow would occur (Parameter 'projection')",
            ex.Message
        )


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
            YamlFormatterBuilder.Default
                .SerializeExactAs(fun (_: TestBaseType) -> "FOO")
                .Build()

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
            YamlFormatterBuilder.Default
                .SerializeExactAs(fun (_: TestInterface) -> "FOO")
                .Build()

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
    let ``If TryFormatFallback function fails, exception bubbles up`` () =
        let format =
            YamlFormatterBuilder.Default
                .TryFormatFallback(fun _ _ -> invalidOp "foo")
                .Build()

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
