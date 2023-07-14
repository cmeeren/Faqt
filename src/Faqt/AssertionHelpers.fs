module Faqt.AssertionHelpers

open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices


// TODO: Move all to type?
// TODO: Find better syntax with methodNameOverride?
type AssertionHelpers =


    /// Gets the current subject name.
    static member sub
        (
            fileName,
            lineNo,
            methodNameOverride,
            [<CallerMemberName; Optional; DefaultParameterValue("")>] methodName
        ) : string =
        let methodName = defaultArg methodNameOverride methodName
        SubjectName.get fileName methodName lineNo


/// Fails an assertion with the specified message.
let fail<'a> (message: string) : 'a =
    ("\n" + message) |> AssertionFailedException |> raise


/// Formats the specified value.
let fmt (value: 'a) : string = Formatting.format value


let private bc' (because: string) prefixSpace suffixComma : string =
    because
    |> Some
    |> Option.filter (not << String.IsNullOrEmpty)
    |> Option.map (
        String.removePrefix "because "
        >> String.trim
        >> (fun reason ->
            (if prefixSpace then " " else "")
            + "because "
            + reason
            + (if suffixComma then ", " else "")
        )
    )
    |> Option.defaultValue ""


/// Returns the specified string (with "because " if not present), or an empty string If None.
let bc (because: string) : string = bc' because false false


/// Returns the specified string (with "because " if not present) suffixed by ", ", or an empty string If None.
let bcc (because: string) : string = bc' because false true


/// Returns the specified string (with "because " if not present) prefixed by a space, or an empty string if None.
let sbc (because: string) : string = bc' because true false


/// Returns the specified string (with "because " if not present) prefixed by a space and suffixed by ", ", or an
/// empty string If None.
let sbcc (because: string) : string = bc' because true true
