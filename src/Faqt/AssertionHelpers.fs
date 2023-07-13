module Faqt.AssertionHelpers


/// Fails an assertion with the specified message.
let fail<'a> (message: string) : 'a =
    ("\n" + message) |> AssertionFailedException |> raise


/// Gets the current subject name.
let sub () : string = SubjectName.get ()


/// Formats the specified value.
let fmt (value: 'a) : string = Formatting.format value


let private bc' (because: string option) prefixSpace suffixComma : string =
    because
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
let bc (because: string option) : string = bc' because false false


/// Returns the specified string (with "because " if not present) suffixed by ", ", or an empty string If None.
let bcc (because: string option) : string = bc' because false true


/// Returns the specified string (with "because " if not present) prefixed by a space, or an empty string if None.
let sbc (because: string option) : string = bc' because true false


/// Returns the specified string (with "because " if not present) prefixed by a space and suffixed by ", ", or an
/// empty string If None.
let sbcc (because: string option) : string = bc' because true true
