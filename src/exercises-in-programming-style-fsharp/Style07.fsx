open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``pride and prejudice`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

let stopWords = 
    (File.ReadAllText ``stop words``).Split(',')
    |> Set.ofArray

let count words = 
    let rec loop (wordFreqs : Dictionary<string, int>) = 
        function
        | [] -> 
            wordFreqs 
            |> Seq.map (fun (KeyValue(word, n)) -> word, n)
        | hd::tl when stopWords.Contains hd ->
            loop wordFreqs tl
        | hd::tl ->
            match wordFreqs.TryGetValue hd with
            | true, n -> wordFreqs.[hd] <- n+1
            | _ -> wordFreqs.[hd] <- 1

            loop wordFreqs tl

    loop (Dictionary<string, int>()) words

let rec print = function
    | [] -> ()
    | (word, count)::tl ->
        printfn "%s - %d" word count
        print tl

let text = File.ReadAllText ``pride and prejudice``
let words = 
    Regex.Matches(text.ToLower(), "[a-z]{2,}") 
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Value)
    |> Seq.toList
    
count words
|> Seq.sortByDescending snd
|> Seq.take 25
|> Seq.toList
|> print