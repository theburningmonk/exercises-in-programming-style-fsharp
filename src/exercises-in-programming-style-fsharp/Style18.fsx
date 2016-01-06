open System
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

let extractWords path =
    let regex = new Regex("[\W_]+")
    let data  = File.ReadAllText path
    let stopWords = 
        File.ReadAllText(``stop words``).Split ','
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray

    regex.Replace(data, " ").ToLower().Split()
    |> Array.filter (not << stopWords.Contains)

let frequencies (words : string[]) = 
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray

let printTop25 (wordFreqs : (string * int)[])=
    wordFreqs
    |> Seq.sortByDescending snd
    |> Seq.take 25
    |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

module Profiled =
    let profile name f args = 
        let stopwatch = System.Diagnostics.Stopwatch()
        stopwatch.Start()

        let res = f args

        stopwatch.Stop()
        printfn
            "[%s] took %d milliseconds" 
            name
            stopwatch.ElapsedMilliseconds

        res

    let extractWords = profile "extractWords" extractWords
    let frequencies  = profile "frequencies" frequencies
    let printTop25   = profile "printTop25" printTop25

open Profiled

extractWords ``p & p``
|> frequencies
|> printTop25