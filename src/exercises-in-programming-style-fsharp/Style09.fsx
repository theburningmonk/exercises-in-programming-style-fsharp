open System
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
    
let readFile path = File.ReadAllText path

let filterChars data = 
    (new Regex("[\W_]+")).Replace(data, " ")

let normalize (input : string) = input.ToLower()

let scan (input : string) = input.Split()
    
let removeStopWords (words : string[]) = 
    let stopWords = 
        (readFile ``stop words``).Split(',')
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray
    
    words |> Array.filter (not << stopWords.Contains)

let frequencies words = 
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray

let sort (wordFreqs : (string * int)[]) = 
    wordFreqs |> Array.sortByDescending (snd)

let top25Freqs (wordFreqs : (string * int)[]) =
    wordFreqs
    |> Seq.take 25
    |> Seq.map (fun (word, n) -> sprintf "%s - %d\n" word n)
    |> Seq.reduce (+)

let printMe = printf "%s"

// version 1 (with simple bind bind)
let (>>=) p cont = cont p

readFile ``p & p``
>>= filterChars
>>= normalize
>>= scan
>>= removeStopWords
>>= frequencies
>>= sort
>>= top25Freqs
|> printMe

// version 2 (with Computation Expression)
[<Sealed>]
type TheOneBuilder () =
    member b.Bind (p, cont) = cont p
    member b.Return x = x

let theOne = TheOneBuilder()

let result = 
    theOne {
        let! text = readFile ``p & p``
        let! text = filterChars text
        let! text = normalize text
        let! words = scan text
        let! words = removeStopWords words
        let! wordFreqs = frequencies words
        let! wordFreqs = sort wordFreqs
        let! result = top25Freqs wordFreqs
        return result
    }

printMe result