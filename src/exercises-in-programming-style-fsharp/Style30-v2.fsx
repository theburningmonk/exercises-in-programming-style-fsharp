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

let splitWords (data : string) = async {
    let stopWords = 
        File.ReadAllText(``stop words``).Split ','
        |> Set.ofArray

    let regex = new Regex("[a-z]{2,}")
    let words =
        data.ToLower()
        |> regex.Matches
        |> Seq.cast<Match>
        |> Seq.map (fun m -> m.Value)

    let result = 
        words 
        |> Seq.filter (not << stopWords.Contains)
        |> Seq.groupBy id
        |> Seq.map (fun (w, ws) -> w, Seq.length ws)
        |> Seq.toArray
    return result
}

let countWords lf rt =
    let dict = Dictionary<string, int>()
    
    seq {
        yield! lf
        yield! rt 
    }
    |> Seq.iter (fun (w, n') ->
       match dict.TryGetValue w with
       | true, n -> dict.[w] <- n + n'
       | _       -> dict.[w] <- n')
    
    dict
    |> Seq.map (fun (KeyValue(k, v)) -> k, v)
    |> Seq.toArray

let sort wordFreqs = 
    wordFreqs
    |> Array.sortByDescending snd

async {
    let! results =
        File.ReadAllText ``p & p``
        |> partition 200
        |> Array.map splitWords
        |> Async.Parallel
    
    results
    |> Array.reduce countWords
    |> sort
    |> Seq.take 25
    |> Seq.iter (fun (word, n) ->
        printfn "%s - %d" word n)
}
|> Async.Start