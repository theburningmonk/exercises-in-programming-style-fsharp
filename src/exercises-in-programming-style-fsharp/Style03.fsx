open System
open System.Collections.Generic
open System.IO

#time

let wordFreqs = new Dictionary<string, int>()
let stopWords = 
    File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt")
        .Split(',')

let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

for line in File.ReadAllLines(``pride and prejudice``) do
    let mutable startChar = None
    let mutable i = 0
    let mutable word  = ""

    for c in line do
        if startChar.IsNone then
            if Char.IsLetterOrDigit c then
                // we found the start of a word
                startChar <- Some i
        else
            if not <| Char.IsLetterOrDigit c then
                // we found the end of a word, process it
                word  <- line.[startChar.Value..i-1].ToLower()

                if stopWords |> Array.forall ((<>) word) 
                   && word.Length >= 2   
                then
                    if not <| wordFreqs.ContainsKey word then
                         wordFreqs.[word]  <- 1
                    else wordFreqs.[word] <- wordFreqs.[word] + 1

                // reset
                startChar <- None

        i <- i+1

wordFreqs 
|> Seq.sortByDescending (fun kvp -> kvp.Value)
|> Seq.take 25
|> Seq.iter (fun kvp -> printfn "%s - %d" kvp.Key kvp.Value)