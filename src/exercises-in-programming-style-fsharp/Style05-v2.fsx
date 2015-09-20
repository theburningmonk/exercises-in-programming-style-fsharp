open System
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

let split (splitChar : char) (input : string) =
    input.Split([|splitChar|], StringSplitOptions.RemoveEmptyEntries)
    
let filterCharsAndNormalize data =
    let regex = new Regex("[\W_]+")
    regex.Replace(data, " ").ToLower()

let removeStopWords path (words : string[]) = 
    let stopWords = 
        File.ReadAllText path
        |> split ','
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray
    
    words |> Array.filter (not << stopWords.Contains)

let frequencies words = 
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray

// main function
File.ReadAllText ``pride and prejudice``
|> filterCharsAndNormalize
|> split ' '
|> removeStopWords ``stop words``
|> frequencies
|> Array.sortByDescending snd
|> Seq.take 25
|> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)