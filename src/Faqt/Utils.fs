[<AutoOpen>]
module internal Faqt.Utils

open System
open System.Collections.Concurrent
open System.Collections.Generic
#if NET7_0_OR_GREATER
open System.Diagnostics.CodeAnalysis
#endif
open System.IO
open System.Net.Http
open System.Text
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection


/// A wrapper to use for dictionary keys to avoid problems if the key is null.
[<Struct>]
type Key<'a> = Key of 'a


/// Memoizes the specified function using reference equality on the input argument.
///
/// Don't call with additional arguments as ad-hoc tuples or records, since these will never be reference equal.
let memoizeRefEq (f: 'a -> 'b) =
    let equalityComparer =
        { new IEqualityComparer<'a> with
            member _.Equals(a, b) = LanguagePrimitives.PhysicalEquality a b
            member _.GetHashCode(a) = LanguagePrimitives.PhysicalHash a
        }

    let cache = new ConcurrentDictionary<'a, 'b>(equalityComparer)
    fun a -> cache.GetOrAdd(a, f)


/// Memoizes the specified function using normal equality (=) on the input argument.
///
/// Don't call with additional arguments as ad-hoc tuples or records, since these will never be reference equal.
let memoize (f: 'a -> 'b) =
    let cache = new ConcurrentDictionary<'a, 'b>()
    fun a -> cache.GetOrAdd(a, f)


/// Memoizes the specified function using normal equality (=) on the input arguments.
///
/// Don't call with additional arguments as ad-hoc tuples or records, since these will never be reference equal.
let memoize2 (f: 'a -> 'b -> 'c) =
    let cache = new ConcurrentDictionary<'a * 'b, 'c>()
    fun a b -> cache.GetOrAdd((a, b), (fun (a, b) -> f a b))


let private preComputeUnionReaderCached =
    memoizeRefEq FSharpValue.PreComputeUnionReader


let private preComputeUnionTagReaderCached: Type -> (obj -> int) =
    memoizeRefEq FSharpValue.PreComputeUnionTagReader


let private makeCaseTupleTypeCached =
    memoizeRefEq (fun (unionCaseInfo: UnionCaseInfo) ->
        FSharpType.MakeTupleType [| for field in unionCaseInfo.GetFields() -> field.PropertyType |]
    )


type FSharpValue with


    static member PreComputeUnionReaderCached(unionCaseInfo) =
        preComputeUnionReaderCached unionCaseInfo


    static member PreComputeUnionTagReaderCached(unionType) =
        preComputeUnionTagReaderCached unionType


type FSharpType with


    static member MakeCaseTupleTypeCached(unionCaseInfo) = makeCaseTupleTypeCached unionCaseInfo


type Type with


    member this.AssertionName =
        if this.IsGenericType then
            let fullNameWithoutGenerics = this.FullName.Substring(0, this.FullName.IndexOf("`"))

            this.GenericTypeArguments
            |> Seq.map (fun t -> t.AssertionName)
            |> String.concat ", "
            |> fun s -> fullNameWithoutGenerics + "<" + s + ">"
        else
            this.FullName


module String =


    /// Removes the given prefix from the string if the string starts with the prefix. If not, the string is returned
    /// unmodified.
    let removePrefix (prefix: string) (str: string) =
        if str.StartsWith(prefix, StringComparison.Ordinal) then
            str.Substring prefix.Length
        else
            str


    let trim (str: string) = str.Trim()


    let split (separator: string) (str: string) = str.Split(separator)


    let replace (oldValue: string) (newValue: string) (str: string) = str.Replace(oldValue, newValue)


    let regexReplace
        (
#if NET7_0_OR_GREATER
        [<StringSyntax(StringSyntaxAttribute.Regex)>]
#endif
        pattern: string)
        (replacement: string)
        (str: string)
        =
        Regex.Replace(str, pattern, replacement)


    let regexRemoveAfterNth
        (n: int)
        (
#if NET7_0_OR_GREATER
        [<StringSyntax(StringSyntaxAttribute.Regex)>]
#endif
        pattern: string)
        (str: string)
        =
        let index = Regex.Matches(str, pattern).Item(n - 1).Index
        str.Substring(0, index)


    let formatSimple args str =
        (str, Seq.indexed args)
        ||> Seq.fold (fun str (i, arg) -> str |> replace $"{{%i{i}}}" arg)


    /// If the string is longer than the specified length, replaces the overshooting part of the string with the
    /// indicator string (which could be an ellipsis or a truncation notice).
    let truncate (indicator: string) length (str: string) =
        if str.Length > length then
            str.Substring(0, length) + indicator
        else
            str


module IDisposable =

    let noOp =
        { new IDisposable with
            member _.Dispose() = ()
        }


module Seq =


    let stringOptimizedLength (xs: seq<'a>) =
        match box xs with
        | :? string as x -> x.Length
        | _ -> Seq.length xs


    let stringOptimizedIsEmpty (xs: seq<'a>) =
        match box xs with
        | :? string as x -> x.Length = 0
        | _ -> Seq.isEmpty xs


module HttpContent =


    let serializeAppend maxLength (sb: StringBuilder) (c: HttpContent) =
        try
            if not (isNull c) && c.Headers.ContentLength <> Nullable(0L) then
                for h in c.Headers do
                    for v in h.Value do
                        sb.AppendLine().Append(h.Key).Append(": ").Append(v) |> ignore

                let s = c.ReadAsStream()
                let a = Array.zeroCreate (int s.Length)
                s.Seek(0, SeekOrigin.Begin) |> ignore
                s.Read(Span(a)) |> ignore

                let strContent =
                    Encoding.UTF8.GetString(a)
                    |> String.truncate "<content truncated>" (1024 * 1024)

                sb.AppendLine().AppendLine().Append(strContent) |> ignore
        with ex ->
            sb
                .AppendLine()
                .AppendLine()
                .AppendLine("An exception occured trying to get the content:")
                .Append(ex.ToString())
            |> ignore


module HttpRequestMessage =


    let serialize maxLength (m: HttpRequestMessage) =
        let sb = StringBuilder()

        sb
            .Append(m.Method.ToString())
            .Append(" ")
            .Append(m.RequestUri.ToString())
            .Append(" HTTP/")
            .Append(m.Version.ToString())
        |> ignore

        for h in m.Headers do
            for v in h.Value do
                sb.AppendLine().Append(h.Key).Append(": ").Append(v) |> ignore

        HttpContent.serializeAppend maxLength sb m.Content

        sb.ToString()


module HttpResponseMessage =


    let serialize maxLength (m: HttpResponseMessage) =
        let sb = StringBuilder()

        sb
            .Append("HTTP/")
            .Append(m.Version.ToString())
            .Append(" ")
            .Append(int m.StatusCode)
            .Append(" ")
            .Append(m.ReasonPhrase)
        |> ignore

        for h in m.Headers do
            for v in h.Value do
                sb.AppendLine().Append(h.Key).Append(": ").Append(v) |> ignore

        HttpContent.serializeAppend maxLength sb m.Content

        sb.ToString()
