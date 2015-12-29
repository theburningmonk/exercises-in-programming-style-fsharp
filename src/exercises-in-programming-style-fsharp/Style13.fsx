open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type IDataStorageManager =
    abstract member Words : string[]

type IStopWordsManager =
    abstract member IsStopWord : string -> bool

type IWordFrequencyManager =
    abstract member IncrementCount : string -> unit
    abstract member Sorted : unit -> seq<string * int>
    
type IWordFrequencyController =
    abstract member Run : unit -> unit

type DataStorageManager (filepath) =
    let data = 
        Regex("[\W_]+")
            .Replace(File.ReadAllText(filepath), " ")
            .ToLower()

    interface IDataStorageManager with
        member __.Words = data.Split()
    
type StopWordsManager () =
    let stopWords = 
        File.ReadAllText(``stop words``)
        |> (fun s -> s.Split(','))
        |> Array.append ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray

    interface IStopWordsManager with
        member __.IsStopWord word = stopWords.Contains word

type WordFrequencyManager () =
    let wordFreqs = Dictionary<string, int>()

    interface IWordFrequencyManager with
        member __.IncrementCount word =
            match wordFreqs.TryGetValue word with
            | true, n -> wordFreqs.[word] <- n+1
            | _ -> wordFreqs.[word] <- 1

        member __.Sorted () =
            wordFreqs
            |> Seq.map (fun (KeyValue(word, n)) -> word, n)
            |> Seq.sortByDescending snd

type WordFrequencyController (filePath) =
    let dataStorageManager : IDataStorageManager = 
        DataStorageManager(filePath) :> IDataStorageManager
    let stopWordsManager : IStopWordsManager = 
        StopWordsManager() :> IStopWordsManager
    let wordFreqsManager : IWordFrequencyManager = 
        WordFrequencyManager() :> IWordFrequencyManager

    interface IWordFrequencyController with
        member __.Run () =
            dataStorageManager.Words
            |> Array.filter (stopWordsManager.IsStopWord >> not)
            |> Array.iter wordFreqsManager.IncrementCount

            wordFreqsManager.Sorted()
            |> Seq.take 25
            |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

let controller = 
    WordFrequencyController(``p & p``) :> IWordFrequencyController
    
controller.Run()