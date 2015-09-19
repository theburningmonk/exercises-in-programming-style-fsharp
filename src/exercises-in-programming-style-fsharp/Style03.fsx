open System
open System.IO

#time

let mutable wordFreqs = Array.empty<string * int>
let stopWords = 
    File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt")
        .Split(',')

let ``pride and prejudice`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

for line in File.ReadAllLines(``pride and prejudice``) do
    // File.ReadAllLines trims line ending, which causes special 
    // case in our code below that searches for words, so easier 
    // to add newline character even though it's not efficient
    let line = line + "\n"

    let mutable startIdx = None
    let mutable word = ""

    for idx = 0 to line.Length-1 do
        let c = line.[idx]

        if startIdx.IsNone then
            if Char.IsLetterOrDigit c then
                // we found the start of a word
                startIdx <- Some idx
        elif not <| Char.IsLetterOrDigit c then
            // we found the end of a word, process it
            word <- line.[startIdx.Value..idx-1].ToLower()

            if stopWords |> Array.forall ((<>) word) 
               && word.Length >= 2
            then
                let wordFreqIdx = 
                    wordFreqs |> Array.tryFindIndex (fst >> (=) word)
                        
                match wordFreqIdx with
                | None -> 
                    wordFreqs <- Array.append wordFreqs [| word, 1 |]
                | Some idx -> 
                    let word, count = wordFreqs.[idx]
                    wordFreqs.[idx] <- word, count + 1

            // reset
            startIdx <- None

wordFreqs |> Array.sortInPlaceBy (fun (_, count) -> -count)
wordFreqs.[0..24] 
|> Array.iter (fun (word, count) -> printfn "%s - %d" word count)