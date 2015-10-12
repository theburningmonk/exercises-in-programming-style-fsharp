open System
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

let printText (wordFreqs : (string * int)[], cont) =
    wordFreqs 
    |> Seq.take 25
    |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

    cont()

let sort (wordFreqs : (string * int)[], cont) =
    let wordFreqs = wordFreqs |> Array.sortByDescending snd
    cont(wordFreqs, fun () -> ())

let frequencies (words : string[], cont) =
    let wordFreqs = 
        words 
        |> Seq.groupBy id 
        |> Seq.map (fun (word, gr) -> word, Seq.length gr)
        |> Seq.toArray
    cont(wordFreqs, printText)

let scan (text : string, cont) =
    cont(text.Split(), frequencies)

let filterChars (text, cont) =
    let regex = new Regex("[\W_]+")
    let text  = regex.Replace(text, " ")
    cont(text, scan)

let removeStopWords (words, cont) =
    let stopWords = 
        (File.ReadAllText ``stop words``).Split(',')
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray
    let words = words |> Array.filter (not << stopWords.Contains)
    cont(words, sort)

let normalize (input : string, cont) =
    cont(input.ToLower(), removeStopWords)

let readFile (path, cont) =
    let text = File.ReadAllText path
    cont(text, normalize)

readFile(``p & p``, filterChars)