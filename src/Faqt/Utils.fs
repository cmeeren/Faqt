﻿[<AutoOpen>]
module internal Faqt.Utils

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Diagnostics.CodeAnalysis
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection


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


module String =


    /// Removes the given prefix from the string if the string starts with the prefix. If not, the string is returned
    /// unmodified.
    let removePrefix (prefix: string) (str: string) =
        if str.StartsWith(prefix, StringComparison.Ordinal) then
            str.Substring prefix.Length
        else
            str


    let trim (str: string) = str.Trim()


    let replace (oldValue: string) (newValue: string) (str: string) = str.Replace(oldValue, newValue)


    let regexReplace
        ([<StringSyntax(StringSyntaxAttribute.Regex)>] pattern: string)
        (replacement: string)
        (str: string)
        =
        Regex.Replace(str, pattern, replacement)