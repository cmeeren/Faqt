﻿module internal Faqt.SubjectName

open System
open System.Diagnostics
open System.IO
open System.Text.RegularExpressions
open Faqt


let private isPartOfAssertion (frame: StackFrame) =
    frame.HasMethod()
    && (frame.GetMethod().CustomAttributes
        |> Seq.exists (fun ai -> ai.AttributeType = typeof<FaqtAssertionAttribute>)
        || frame.GetMethod().DeclaringType.CustomAttributes
           |> Seq.exists (fun ai -> ai.AttributeType = typeof<ContainsFaqtAssertionsAttribute>))


let private getAssertionMethodNameAndCallingFrame () =
    let frames = StackTrace(fNeedFileInfo = true).GetFrames()

    let indexOfFrameThatCallsTopmostAssertion =
        frames
        |> Seq.indexed
        |> Seq.skipWhile (snd >> (not << isPartOfAssertion))
        |> Seq.skipWhile (snd >> isPartOfAssertion)
        |> Seq.head
        |> fst

    let assertionFrame = frames |> Seq.item (indexOfFrameThatCallsTopmostAssertion - 1)
    let userFrame = frames |> Seq.item indexOfFrameThatCallsTopmostAssertion
    assertionFrame.GetMethod().Name, userFrame

let private transformationPlaceholder = "..."


let get () =
    try
        let methodName, frame = getAssertionMethodNameAndCallingFrame ()
        let sourceCodeLines = frame.GetFileName() |> File.ReadAllLines
        let lineNo = frame.GetFileLineNumber()
        let colNo = frame.GetFileColumnNumber()

        sourceCodeLines
        |> Seq.skip (lineNo - 1)
        |> Seq.indexed
        |> Seq.takeWhile (fun (i, line) ->
            let line = line.Trim()
            // Known limitation: This does not handle multi-line expressions well (e.g., multiline method calls or strings).
            // In general, lines after the opening delimiter will not be considered.
            i = 0 || line.StartsWith(".") || line.StartsWith("//")
        )
        |> Seq.map (fun (i, line) ->
            let line = if i = 0 then line.Substring(colNo - 1) else line

            line
            // Known limitation: This will also change string contents (and ``quoted`` identifiers). A workaround is added
            // to preserve URL string literals. To remove this limitation fully, the source code must be parsed properly,
            // requiring this code to be completely rewritten (and likely end up more complex) using e.g.
            // FSharp.Compiler.Service (this was tested and abandoned early on). Alternatively, one could go through all
            // lines and all characters manually and track whether we're in a string (remembering verbatim strings, escaped
            // quotes, etc.), and only remove comments outside of strings. For now, it has been decided to live with this
            // limitation and, potentially adding ad-hoc workarounds for other common patterns.
            |> String.regexReplace "(?<!:)//.+" ""
            |> String.trim
        )
        |> Seq.filter (not << String.IsNullOrWhiteSpace)
        |> String.concat ""

        // Remove the first occurrence of the assertion method name and everything after it, so we only consider the code
        // before the assertion when deriving the subject name. If there are multiple invocations of the same assertion
        // method, we don't know anyway which invocation failed, since the stack frame only contains the location of the
        // start of the chain, so we only support deriving the subject name from the part of the expression up to the first
        // call to this method.
        |> String.regexReplace $"\.{Regex.Escape methodName}\(.*" ""

        // Replace Should...Whose and Should...Which with transformation placeholder, since it's assumed the code
        // contains something returning AndDerived. Make an exception for And.Whose and And.Which, since they are
        // methods on Testable, not AndDerived. (Note that Testable.Which doesn't exist currently. It can be added as an
        // alias of Testable.Whose if needed, but there should exist a realistic use-case for it first.)
        |> String.regexReplace "\.Should\(\)\..+?\.(?<!And\.)(Whose|Which)\." transformationPlaceholder

        // Remove Should...And; this code doesn't change the subject.
        |> String.regexReplace "\.Should\(\)\..+?\.And" ""

        // Remove remaining Subject/Whose/Which; they are methods on Testable and don't change the subject.
        |> String.regexReplace "\.(Subject|Whose|Which)\." "."

        // Remove remaining calls to Should.
        |> String.regexReplace "\.Should\(\)" ""
    with ex ->
        raise (Exception("Exception while getting subject name", ex))