open System
open System.IO
open System.Text.RegularExpressions

#time

let ``pride and prejudice`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

let readFile path = File.ReadAllText path

let filterCharsAndNormalize data =
    let regex = new Regex("[\W_]+")
    regex.Replace(data, " ").ToLower()

let scan (data : string) = 
    data.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)

let stopWordsPath = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let removeStopWords (words : string[]) = 
    let stopWords = 
        File.ReadAllText(stopWordsPath).Split(',')
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray
    
    words |> Array.filter (not << stopWords.Contains)

let frequencies words = 
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray

let sort wordFreqs = wordFreqs |> Array.sortByDescending snd

let printAll wordFreqs = 
    for (word, n) in wordFreqs do
        printfn "%s - %d" word n

// main function
readFile ``pride and prejudice``
|> filterCharsAndNormalize
|> scan
|> removeStopWords
|> frequencies
|> sort
|> Seq.take 25
|> printAll