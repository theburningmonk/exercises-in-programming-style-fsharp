open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

let partition chunkSize (data : string) =
    data.Split('\n')
    |> Array.chunkBySize chunkSize
    |> Array.map (fun lines -> 
        System.String.Join("\n", lines))

let splitWords (data : string) =
    let stopWords = 
        File.ReadAllText(``stop words``).Split ','
        |> Set.ofArray

    let regex = new Regex("[a-z]{2,}")
    let words =
        data.ToLower()
        |> regex.Matches
        |> Seq.cast<Match>
        |> Seq.map (fun m -> m.Value)

    words 
    |> Seq.filter (not << stopWords.Contains)
    |> Seq.map (fun w -> w, 1)
    |> Seq.toArray

let regroup (pairsList : (string*int)[][]) =
    pairsList
    |> Seq.collect id
    |> Seq.groupBy fst
    |> Seq.map (fun (word, gr) -> word, Seq.toArray gr)
    |> Map.ofSeq

let countWords (mapping : Map<string, (string*int)[]>) = 
    mapping
    |> Seq.map (fun (KeyValue(k, v)) ->
        k, v |> Seq.sumBy snd)
    |> Seq.toArray

let sort wordFreqs = 
    wordFreqs
    |> Array.sortByDescending snd

File.ReadAllText ``p & p``
|> partition 200
|> Array.map splitWords
|> regroup
|> countWords
|> sort
|> Seq.take 25
|> Seq.iter (fun (word, n) ->
    printfn "%s - %d" word n)