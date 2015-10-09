open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``pride and prejudice`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

let stopWords = 
    (File.ReadAllText ``stop words``).Split(',')
    |> Set.ofArray

let rec count (wordFreqs : Dictionary<string, int>) = function
    | [] -> wordFreqs
    | (hd : string)::tl when stopWords.Contains hd ->
        count wordFreqs tl
    | (hd : string)::tl ->
        match wordFreqs.TryGetValue hd with
        | true, n -> wordFreqs.[hd] <- n+1
        | _ -> wordFreqs.[hd] <- 1

        count wordFreqs tl

let rec print = function
    | [] -> ()
    | KeyValue(word, count)::tl ->
        printfn "%s - %d" word count
        print tl

let text = (File.ReadAllText ``pride and prejudice``).ToLower()
let words = 
    Regex.Matches(text, "[a-z]{2,}") 
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Value)
    |> Seq.toList
    
count (Dictionary<string, int>()) words
|> Seq.sortByDescending (fun (KeyValue(_, n)) -> n)
|> Seq.take 25
|> Seq.toList
|> print