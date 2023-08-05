namespace Faqt

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Reflection.Metadata
open System.Reflection.PortableExecutable
open System.Text
open System.Text.RegularExpressions
open Faqt


type internal CallChainOrigin = {
    AssemblyPath: string
    SourceFilePath: string
    LineNumber: int
}


type private AssertionInfo = {
    Method: string
    SupportsChildAssertions: bool
    IsSeqAssertion: bool
}


type internal CallChain() =

    [<ThreadStatic; DefaultValue>]
    static val mutable private activeUserAssertions: Dictionary<CallChainOrigin, AssertionInfo list>

    [<ThreadStatic; DefaultValue>]
    static val mutable private topLevelAssertionHistory: Dictionary<CallChainOrigin, AssertionInfo list>

    static let pushAssertion callsite method supportsChildAssertions isSeqAssertion =

        let assertions =
            match CallChain.activeUserAssertions.TryGetValue callsite with
            | false, _ -> []
            | true, xs -> xs

        CallChain.activeUserAssertions[callsite] <-
            {
                Method = method
                SupportsChildAssertions = supportsChildAssertions
                IsSeqAssertion = isSeqAssertion
            }
            :: assertions

    static let tryPopAssertion callsite =
        match CallChain.activeUserAssertions.TryGetValue callsite with
        | false, _ -> ()
        | true, [] -> ()
        | true, hd :: tl ->
            CallChain.activeUserAssertions[callsite] <- tl

            match CallChain.topLevelAssertionHistory.TryGetValue callsite with
            | false, _ -> CallChain.topLevelAssertionHistory[callsite] <- [ hd ]
            | true, xs -> CallChain.topLevelAssertionHistory[callsite] <- hd :: xs

            let parentAssertionCallsite =
                CallChain.activeUserAssertions
                |> Seq.filter (fun kvp ->
                    match kvp.Value with
                    | [] -> false
                    | hd :: _ -> hd.SupportsChildAssertions
                )
                |> Seq.sortByDescending (fun kvp -> kvp.Key.LineNumber)
                |> Seq.tryHead
                |> Option.map (fun kvp -> kvp.Key)

            match parentAssertionCallsite with
            | None -> ()
            | Some parentCallsite ->
                match CallChain.topLevelAssertionHistory.TryGetValue parentCallsite with
                | false, _ -> CallChain.topLevelAssertionHistory[parentCallsite] <- [ hd ]
                | true, xs -> CallChain.topLevelAssertionHistory[parentCallsite] <- hd :: xs


    static let canPushAssertion callsite =
        match CallChain.activeUserAssertions with
        | null -> true
        | dict ->
            match dict.TryGetValue callsite with
            | false, _ -> true
            | true, [] -> true
            | true, hd :: _ -> hd.SupportsChildAssertions

    static member private EnsureInitialized() =
        if isNull CallChain.activeUserAssertions then
            CallChain.activeUserAssertions <- Dictionary()

        if isNull CallChain.topLevelAssertionHistory then
            CallChain.topLevelAssertionHistory <- Dictionary()


    static member Assert(callsite, assertionMethod, supportsChildAssertions, isSeqAssertion) =
        CallChain.EnsureInitialized()

        if not (canPushAssertion callsite) then
            IDisposable.noOp
        else
            pushAssertion callsite assertionMethod supportsChildAssertions isSeqAssertion

            { new IDisposable with
                member _.Dispose() = tryPopAssertion callsite
            }


    static member AssertItem(callsite) =
        CallChain.EnsureInitialized()

        match CallChain.topLevelAssertionHistory.TryGetValue callsite with
        | false, _ -> ()
        | true, xs ->
            CallChain.topLevelAssertionHistory[callsite] <- xs |> List.skipWhile (fun x -> not x.IsSeqAssertion)

        match CallChain.activeUserAssertions.TryGetValue callsite with
        | false, _ -> ()
        | true, xs -> CallChain.activeUserAssertions[callsite] <- xs |> List.skipWhile (fun x -> not x.IsSeqAssertion)

        // Returning IDisposable and requiring usage with the 'use' keyword gives more flexibility in changing the
        // implementation later, if needed.
        IDisposable.noOp

    static member AssertionHistory(callsite) =
        let topLevelAssertions =
            match CallChain.topLevelAssertionHistory.TryGetValue callsite with
            | true, xs -> xs |> List.map (fun x -> x.Method) |> List.rev
            | false, _ -> []

        let activeAssertions =
            match CallChain.activeUserAssertions.TryGetValue callsite with
            | true, xs -> xs |> List.map (fun x -> x.Method) |> List.rev
            | false, _ -> []

        topLevelAssertions @ activeAssertions


module internal EmbeddedSource =


    module PortableCustomDebugInfoKinds =

        let embeddedSource = Guid("0E8A571B-6926-466E-B4AD-8AB04611F5FE")


    let tryGetEncoding (codepage: int) =
        try
            Encoding.GetEncoding(codepage) |> ValueSome
        with _ ->
            ValueNone


    /// Encoding priority is inspired by Roslyn:
    /// https://github.com/dotnet/roslyn/blob/2fb8ad0ce8c49872aa781072037cf921efa3a5fd/src/Compilers/Core/Portable/EncodedStringText.cs#L27
    let readSource (stream: Stream) =
        if not (isNull CodePagesEncodingProvider.Instance) then
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)

        // TODO: Not sure if this is a correct way to decode. Will StreamReader.ReadToEnd throw if the encoding is incorrect?

        let encodingsToTry = [
            UTF8Encoding(false, true) :> Encoding
            yield! tryGetEncoding 0 |> ValueOption.toList
            yield! tryGetEncoding 1252 |> ValueOption.toList
            Encoding.Latin1
        ]

        use memoryStream = new MemoryStream()
        stream.CopyTo(memoryStream)

        encodingsToTry
        |> Seq.pick (fun enc ->
            try
                memoryStream.Seek(0L, SeekOrigin.Begin) |> ignore
                let reader = new StreamReader(memoryStream, enc)
                reader.ReadToEnd() |> Some
            with _ ->
                None
        )


    let readEmbeddedSource (reader: MetadataReader) (documentHandle: DocumentHandle) =
        let bytes =
            reader.GetCustomDebugInformation(documentHandle)
            |> Seq.map reader.GetCustomDebugInformation
            |> Seq.filter (fun cdi -> reader.GetGuid(cdi.Kind) = PortableCustomDebugInfoKinds.embeddedSource)
            |> Seq.exactlyOne
            |> fun cdi -> reader.GetBlobBytes(cdi.Value)

        let uncompressedSize = BitConverter.ToInt32(bytes, 0)
        use stream = new MemoryStream(bytes, sizeof<int>, bytes.Length - sizeof<int>)

        if uncompressedSize <> 0 then
            use decompressedStream = new DeflateStream(stream, CompressionMode.Decompress)
            readSource decompressedStream

        else
            readSource stream


    let get =
        memoize2 (fun assemblyPath sourcePath ->
            use assemblyFileStream = File.OpenRead(assemblyPath)
            use peReader = new PEReader(assemblyFileStream)

            let embeddedEntry =
                peReader.ReadDebugDirectory()
                |> Seq.filter (fun entry -> entry.Type = DebugDirectoryEntryType.EmbeddedPortablePdb)
                |> Seq.exactlyOne

            use embeddedMetadataProvider =
                peReader.ReadEmbeddedPortablePdbDebugDirectoryData(embeddedEntry)

            let pdbReader = embeddedMetadataProvider.GetMetadataReader()

            pdbReader.Documents
            |> Seq.pick (fun documentHandle ->
                let document = pdbReader.GetDocument(documentHandle)
                let documentPath = pdbReader.GetString(document.Name)

                if sourcePath = documentPath then
                    let source = readEmbeddedSource pdbReader documentHandle
                    Some source
                else
                    None
            )
        )


module internal SubjectName =


    let getFileLines = memoize File.ReadAllLines


    let private transformationPlaceholder = "..."


    let get origin (assertions: string list) =
        try
            let sourceCodeLines =
                try
                    getFileLines origin.SourceFilePath
                with _ ->
                    (EmbeddedSource.get origin.AssemblyPath origin.SourceFilePath)
                        .Replace("\r\n", "\n")
                        .Split("\n")

            let assertionCounts = assertions |> List.countBy id |> Map.ofList
            let lastAssertion = assertions[assertions.Length - 1]
            let lastAssertionCount = assertionCounts[lastAssertion]

            sourceCodeLines
            |> Seq.skip (origin.LineNumber - 1)
            |> Seq.scan
                (fun (countsLeft: Map<_, _>, _) line ->
                    if countsLeft.Values |> Seq.forall ((=) 0) then
                        countsLeft, None
                    else
                        let newCountsLeft =
                            countsLeft
                            |> Map.toSeq
                            |> Seq.map (fun (assertion, currentCount) ->
                                let countInThisLine =
                                    Regex.Matches(line, $"\.{Regex.Escape assertion}( *\<.*\>)? *\(").Count

                                assertion, currentCount - countInThisLine
                            )
                            |> Map.ofSeq

                        newCountsLeft, Some line
                )
                (assertionCounts, Some "")
            |> Seq.takeWhile (fun (_, line) -> line.IsSome)
            |> Seq.map (fun (_, line) ->
                line.Value
                // Remove comments. Try to preserve strings. Known limitation: This will remove lines starting with // from
                // multi-line strings. On a line-by-line basis, they are indistinguishable from comment lines.
                |> String.regexReplace "(?<!(\"|``).*?)//.+" ""
                |> String.trim
            )
            |> Seq.filter (not << String.IsNullOrWhiteSpace)
            |> String.concat ""

            // Remove the first occurrence of the assertion method name and everything after it, so we only consider the
            // code before the assertion when deriving the subject name. If there are multiple invocations of the same
            // assertion method, we don't know anyway which invocation failed, since the stack frame only contains the
            // location of the start of the chain, so we only support deriving the subject name from the part of the
            // expression up to the first call to this method.
            |> String.regexRemoveAfterNth lastAssertionCount $"\.{Regex.Escape lastAssertion}( *\<.*\>)? *\("

            // Replace Should...Whose, Should...WhoseValue, and Should...That with the transformation placeholder, since
            // it's assumed the code contains something returning AndDerived. Make an exception if prefixed by And, since
            // that would be methods on Testable, not AndDerived. (Not all of those methods exist on Testable yet, but may
            // be added if realistic use-cases arrive.)
            |> String.regexReplace "\.Should\(\)\..+?\.(?<!And\.)(Whose|WhoseValue|That)\." transformationPlaceholder

            // Remove Should...And; this code doesn't change the subject.
            |> String.regexReplace "\.Should *\(\)\..+?\.And" ""

            // Remove remaining Subject/Whose/WhoseValue/That; they are methods on Testable and don't change the subject.
            // (Not all of those methods exist on Testable yet, but may be added if realistic use-cases arrive.)
            |> String.regexReplace "\.(Subject|Whose|WhoseValue|That)\." "."

            // Remove remaining calls to Should.
            |> String.regexReplace "\.Should *\(\)" ""

            // Remove "...fun ... ->" from start of line (e.g. in single-line chains in Satisfy)
            |> String.regexReplace ".*fun .+? -> " ""

            |> String.trim
        with ex ->
            "subject"
