open System
open System.IO

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

let characters fileName = 
    File.ReadLines fileName
    |> Seq.map (fun l -> l + "\n")
    |> Seq.collect id

let allWords fileName =
    seq {
        let mutable started = false
        let mutable word    = ""

        for c in characters fileName do
            if not started && Char.IsLetterOrDigit c then
                started <- true
                word    <- c.ToString()
            elif started && Char.IsLetterOrDigit c then
                word    <- word + c.ToString()
            elif started then
                yield word.ToLower()
                started <- false
                word    <- ""

        if word.Length > 0 then yield word
    }

let nonStopWords fileName =
    let stopWords = 
        File.ReadAllText(``stop words``).Split(',')
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray

    allWords fileName
    |> Seq.filter (not << stopWords.Contains)

type WordFreqs = Map<string, int>
let countAndSort fileName =
    nonStopWords fileName
    |> Seq.chunkBySize 5000
    |> Seq.scan (fun wordFreqs words ->
        words
        |> Seq.groupBy id
        |> Seq.fold (fun (wordFreqs : WordFreqs) (word, s) ->
            if wordFreqs.ContainsKey word
            then 
                let n = wordFreqs.[word]
                wordFreqs.Add(word, n + Seq.length s)
            else wordFreqs.Add(word, Seq.length s)
            ) wordFreqs) Map.empty<string, int>
    |> Seq.skip 1
    |> Seq.map (fun wordFreqs -> 
        wordFreqs
        |> Seq.map (fun (KeyValue(word, n)) -> word, n)
        |> Seq.sortByDescending snd)

countAndSort ``p & p``
|> Seq.iter (fun wordFreqs ->
    printfn "-------------------------------------------------"
    wordFreqs
    |> Seq.take 25
    |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n))