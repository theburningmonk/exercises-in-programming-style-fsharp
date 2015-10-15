open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type DataStorageManager (filepath) =
    let regex = new Regex("[\W_]+")
    let data  = regex.Replace(File.ReadAllText(filepath), " ").ToLower()

    member __.Words = data.Split()
    
type StopWordsManager () =
    let stopWords = 
        File.ReadAllText(``stop words``)
        |> (fun s -> s.Split(','))
        |> Array.append ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray

    member __.IsStopWord word = stopWords.Contains word

type WordFrequencyManager () =
    let wordFreqs = Dictionary<string, int>()

    member __.IncrementCount word =
        match wordFreqs.TryGetValue word with
        | true, n -> wordFreqs.[word] <- n+1
        | _ -> wordFreqs.[word] <- 1

    member __.Sorted () =
        wordFreqs
        |> Seq.map (fun (KeyValue(word, n)) -> word, n)
        |> Seq.sortBy (fun (word, n) -> -n)

type WordFrequencyController (filePath) =
    let dataStorageManager = DataStorageManager(filePath)
    let stopWordsManager   = StopWordsManager()
    let wordFreqsManager   = WordFrequencyManager()

    member __.Run () =
        dataStorageManager.Words
        |> Array.filter (stopWordsManager.IsStopWord >> not)
        |> Array.iter wordFreqsManager.IncrementCount

        wordFreqsManager.Sorted()
        |> Seq.take 25
        |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

WordFrequencyController(``p & p``).Run()