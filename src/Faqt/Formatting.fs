module Faqt.Formatting

open System


type private FormatterCache<'a>() =
    static member val Format: ('a -> string) voption = ValueNone with get, set


let mutable private defaultFormatter: (obj -> string) voption = ValueNone


/// Formats the specified value.
let format (x: 'a) : string =
    let formatted =
        match FormatterCache<'a>.Format with
        | ValueSome format -> format x
        | ValueNone ->
            match defaultFormatter with
            | ValueSome format -> format x
            | ValueNone -> $"%A{x}"

    let nonPrintableChars =
        formatted
        // Don't treat newlines as non-printable
        |> String.replace "\r\n" ""
        |> String.replace "\n" ""
        |> Seq.filter Char.IsControl
        |> Seq.distinct
        |> Seq.toList

    if not nonPrintableChars.IsEmpty then
        let nonPrintableCharListWithCount =
            nonPrintableChars
            |> List.countBy id
            |> List.map (fun (c, count) -> $"%i{count}x \\u%04i{int c}")
            |> String.concat ", "

        $"\t[NOTE: VALUE CONTAINS NON-PRINTABLE CHARACTERS: %s{nonPrintableCharListWithCount}]{Environment.NewLine}"
        + formatted
    else
        formatted


/// Formats the specified type using the specified formatter. Only effective when the value is at the top level; does
/// not impact formatting of this value in records, lists, etc.
let addFormatter<'a> (format: 'a -> string) =
    FormatterCache<'a>.Format <- ValueSome format


/// Formats all values using the specified formatter, except if overridden using addFormatter. The default formatter is
/// `sprintf "%A"`. (Note that it is not possible to set the default formatter to another kind of %A-based formatting,
/// as F# must know the type at compile time. Using %A here will have the same effect as %O.)
let setDefaultFormatter (format: obj -> string) = defaultFormatter <- ValueSome format
