open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

let extractWords = function
    | "" | null -> failwith "I need a non-empty string!"
    | path -> 
        let data = File.ReadAllText path
        let regex = new Regex("[\W_]+")
        regex.Replace(data, " ").ToLower().Split()

let removeStopWords (words : string[]) = 
    if isNull words then failwith "I need a nno-null array!"
    else 
        let stopWords = 
            File.ReadAllText(``stop words``).Split(',')
            |> Array.append ([| 'a'..'z' |] |> Array.map string)
            |> Set.ofArray

        words |> Array.filter (not << stopWords.Contains)

let frequencies = function
    | null | [||] -> failwith "I need a non-empty array!"
    | words ->
        words 
        |> Seq.groupBy id 
        |> Seq.map (fun (word, gr) -> word, Seq.length gr)
        |> Seq.toArray

let sort = function
    | null | [||] -> failwith "I need a non-empty array!"
    | wordFreqs -> 
        wordFreqs |> Array.sortByDescending snd

try
    extractWords ``pride and prejudice``
    |> removeStopWords
    |> frequencies
    |> sort
    |> Seq.take 25
    |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)
with
| exn ->
    printfn "Something's wrong\n%A" exn