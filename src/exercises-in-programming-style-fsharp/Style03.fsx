open System
open System.IO

#time

let mutable wordFreqs = Array.empty<string * int>
let stopWords = 
    File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt")
        .Split(',')

let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

for line in File.ReadAllLines(``pride and prejudice``) do
    let mutable startIdx = None
    let mutable idx  = 0
    let mutable word = ""

    for c in line do
        if startIdx.IsNone then
            if Char.IsLetterOrDigit c then
                // we found the start of a word
                startIdx <- Some idx
        else
            if not <| Char.IsLetterOrDigit c then
                // we found the end of a word, process it
                word  <- line.[startIdx.Value..idx-1].ToLower()

                if stopWords |> Array.forall ((<>) word) && word.Length >= 2
                then
                    let wordFreqIdx = 
                        wordFreqs 
                        |> Array.tryFindIndex (fun (word', _) -> word = word')
                        
                    match wordFreqIdx with
                    | None -> wordFreqs <- Array.append wordFreqs [| word, 1 |]
                    | Some idx -> 
                        let word, count = wordFreqs.[idx]
                        wordFreqs.[idx] <- word, count + 1

                // reset
                startIdx <- None

        idx <- idx+1

wordFreqs |> Array.sortInPlaceBy (fun (_, count) -> -count)
wordFreqs.[0..24] 
|> Array.iter (fun (word, count) -> printfn "%s - %d" word count)