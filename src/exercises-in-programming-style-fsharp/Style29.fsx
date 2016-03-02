open System.Collections.Generic
open System.Collections.Concurrent
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

// two data spaces
let wordSpace = ConcurrentQueue<string>()
let freqSpace = ConcurrentQueue<(string * int)[]>()

let stopWords = 
    File.ReadAllText(``stop words``).Split(',')
    |> Set.ofArray

 let processWords () = 
    let wordFreqs = Dictionary<string, int>()

    let rec loop () =
        match wordSpace.TryDequeue() with
        | true, word when stopWords.Contains word ->
            loop ()
        | true, word ->
            match wordFreqs.TryGetValue word with
            | true, n -> wordFreqs.[word] <- n+1
            | _       -> wordFreqs.[word] <- 1
            loop ()
        | _ -> 
            wordFreqs
            |> Seq.map (fun (KeyValue(word, n)) -> word, n)
            |> Seq.toArray
            |> freqSpace.Enqueue

    loop ()

let regex = new Regex("[a-z]{2,}")
let words =
    File.ReadAllText(``p & p``).ToLower()
    |> regex.Matches
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Value)
words |> Seq.iter wordSpace.Enqueue

{ 1..5 }
|> Seq.map (fun _ -> async { do processWords() })
|> Async.Parallel
|> Async.Ignore
|> Async.RunSynchronously

let wordFreqs =
    let wordFreqs = Dictionary<string, int>()
    while not freqSpace.IsEmpty do
        match freqSpace.TryDequeue () with
        | true, data ->
            data 
            |> Array.iter (fun (word, n) ->
                match wordFreqs.TryGetValue word with
                | true, n' -> wordFreqs.[word] <- n+n'
                | _        -> wordFreqs.[word] <- n)
        | _ -> ()

    wordFreqs
    |> Seq.map (fun (KeyValue(word, n)) -> word, n)
    |> Seq.sortByDescending snd
    |> Seq.toArray

 wordFreqs
 |> Seq.take 25
 |> Seq.iter (fun (word, n) ->
    printfn "%s - %d" word n)