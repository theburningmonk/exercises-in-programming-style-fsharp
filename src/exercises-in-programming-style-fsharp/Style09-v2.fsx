open System
open System.IO
open System.Text.RegularExpressions

#time

// version 2 (with Computation Expression)

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

type Result<'a> = Result of 'a

[<Sealed>]
type TheOneBuilder () =
    member __.Bind (Result p, cont : 'a -> Result<'b>) = 
        cont p

    member __.Zero () = Result ()

    member __.Return x = x
    member __.ReturnFrom (Result x) = x

let theOne = TheOneBuilder()

let readFile path = 
    File.ReadAllText path |> Result

let filterChars data = 
    (new Regex("[\W_]+")).Replace(data, " ") |> Result

let normalize (input : string) = 
    input.ToLower() |> Result

let scan (input : string) = 
    input.Split() |> Result
    
let removeStopWords (words : string[]) = 
    let raw = theOne {
        return! readFile ``stop words``
    }

    let stopWords = 
        raw.Split(',')
        |> Array.append 
            ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray
    
    words 
    |> Array.filter (not << stopWords.Contains)
    |> Result

let frequencies words = 
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray
    |> Result

let sort (wordFreqs : (string * int)[]) = 
    wordFreqs 
    |> Array.sortByDescending (snd)
    |> Result

let top25Freqs (wordFreqs : (string * int)[]) =
    wordFreqs
    |> Seq.take 25
    |> Seq.map (fun (word, n) -> sprintf "%s - %d\n" word n)
    |> Seq.reduce (+)
    |> Result

let printMe = printf "%s"

let (>>=) (Result p) (cont : 'a -> Result<'b>) = cont p

theOne {
    let! text = 
        readFile ``p & p``
        >>= filterChars
        >>= normalize
    let! words = scan text >>= removeStopWords
    let! wordFreqs = frequencies words >>= sort
    let! top25 = top25Freqs wordFreqs
    do printMe top25
}