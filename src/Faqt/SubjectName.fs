module internal Faqt.SubjectName

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Reflection.Metadata
open System.Reflection.PortableExecutable
open System.Text
open System.Text.RegularExpressions
open Faqt


module EmbeddedSource =


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


let getFileLines = memoize File.ReadAllLines


let private transformationPlaceholder = "..."


let get assemblyPath sourceFilePath (assertions: IList<string>) lineNo =
    try
        let sourceCodeLines =
            try
                getFileLines sourceFilePath
            with _ ->
                (EmbeddedSource.get assemblyPath sourceFilePath)
                    .ReplaceLineEndings("\n")
                    .Split("\n")

        let lastAssertion = assertions[assertions.Count - 1]
        let lastAssertionCount = assertions |> Seq.filter ((=) lastAssertion) |> Seq.length

        sourceCodeLines
        |> Seq.skip (lineNo - 1)
        |> Seq.indexed
        |> Seq.takeWhile (fun (i, line) ->
            let line = line.Trim()
            // Known limitation: This does not handle multi-line expressions well (e.g., multiline method calls or strings).
            // In general, lines after the opening delimiter will not be considered.
            i = 0 || line.StartsWith(".") || line.StartsWith("//")
        )
        |> Seq.map (fun (_, line) ->
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
        |> String.regexRemoveAfterNth lastAssertionCount $"\.{Regex.Escape lastAssertion} *\("

        // Replace Should...Whose and Should...Which with transformation placeholder, since it's assumed the code
        // contains something returning AndDerived. Make an exception for And.Whose and And.Which, since they are
        // methods on Testable, not AndDerived. (Note that Testable.Which doesn't exist currently. It can be added as an
        // alias of Testable.Whose if needed, but there should exist a realistic use-case for it first.)
        |> String.regexReplace "\.Should\(\)\..+?\.(?<!And\.)(Whose|Which)\." transformationPlaceholder

        // Remove Should...And; this code doesn't change the subject.
        |> String.regexReplace "\.Should *\(\)\..+?\.And" ""

        // Remove remaining Subject/Whose/Which; they are methods on Testable and don't change the subject.
        |> String.regexReplace "\.(Subject|Whose|Which)\." "."

        // Remove remaining calls to Should.
        |> String.regexReplace "\.Should *\(\)" ""

        // Remove "...fun ... ->" from start of line (e.g. in single-line chains in Satisfy)
        |> String.regexReplace ".*fun .+? -> " ""

        |> String.trim
    with ex ->
        "subject"
