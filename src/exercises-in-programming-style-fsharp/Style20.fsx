open System
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

let extractWords = function
    | "" | null -> [||]
    | path -> 
        try
            let data = File.ReadAllText path
            let regex = new Regex("[\W_]+")
            regex.Replace(data, " ").ToLower().Split()
        with
        | exn -> 
            printfn "Error when opening %s\n%A" path exn
            [||]

let removeStopWords (words : string[]) = 
    if isNull words then [||]
    else 
        try
            let stopWords = 
                File.ReadAllText(``stop words``).Split(',')
                |> Array.append ([| 'a'..'z' |] |> Array.map string)
                |> Set.ofArray

            words |> Array.filter (not << stopWords.Contains)
        with 
        | exn ->
            printfn "Error opening %s\n%A" ``stop words`` exn
            words

let frequencies = function
    | null | [||] -> [||]
    | words ->
        words 
        |> Seq.groupBy id 
        |> Seq.map (fun (word, gr) -> word, Seq.length gr)
        |> Seq.toArray

let sort = function
    | null | [||] -> [||]
    | wordFreqs -> 
        wordFreqs |> Array.sortByDescending snd

extractWords ``pride and prejudice``
|> removeStopWords
|> frequencies
|> sort
|> Seq.take 25
|> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)