namespace Faqt.Benchmarks

#nowarn "104"

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open Faqt

type SeqImpl =
    | CharArray = 1
    | String = 2


[<MemoryDiagnoser>]
type SeqLength() =

    [<DefaultValue; Params(10, 1000)>]
    val mutable N: int

    [<DefaultValue; Params(SeqImpl.CharArray, SeqImpl.String)>]
    val mutable Impl: SeqImpl

    let mutable xs: seq<char> = null

    [<GlobalSetup>]
    member this.Setup() =
        match this.Impl with
        | SeqImpl.CharArray -> xs <- Array.zeroCreate this.N
        | SeqImpl.String -> xs <- System.String(' ', this.N)

    [<Benchmark(Baseline = true)>]
    member _.Default() = Seq.length xs

    [<Benchmark>]
    member _.Optimized() = Seq.stringOptimizedLength xs


[<MemoryDiagnoser>]
type SeqIsEmpty() =

    [<DefaultValue; Params(10, 1000)>]
    val mutable N: int

    [<DefaultValue; Params(SeqImpl.CharArray, SeqImpl.String)>]
    val mutable Impl: SeqImpl

    let mutable xs: seq<char> = null

    [<GlobalSetup>]
    member this.Setup() =
        match this.Impl with
        | SeqImpl.CharArray -> xs <- Array.zeroCreate this.N
        | SeqImpl.String -> xs <- System.String(' ', this.N)

    [<Benchmark(Baseline = true)>]
    member _.Default() = Seq.isEmpty xs

    [<Benchmark>]
    member _.Optimized() = Seq.stringOptimizedIsEmpty xs


module Program =

    [<EntryPoint>]
    let main _argv =
        BenchmarkRunner.Run<SeqLength>() |> ignore
        0
