open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type DSManagerMessage =
    | Init of filePath : string
    | Words

type DataStorageManager () =
    let mutable data = ""

    let init filePath =
        Regex("[\W_]+")
            .Replace(File.ReadAllText(filePath), " ")
            .ToLower()
        |> (fun x -> data <- x)

    let words () = data.Split()

    member __.Dispatch = function
        | Init filePath -> init filePath :> obj
        | Words         -> words () :> obj

type SWManagerMessage =
    | Init
    | IsStopWord of word : string

type StopWordsManager () =
    let mutable stopWords = Set []

    let init () =
        let stopWords' = 
            File.ReadAllText(``stop words``).Split(',')
        let lowerCases = 
            [|'a'..'z'|] |> Array.map string
        stopWords <- Array.append stopWords' lowerCases
                     |> Set.ofArray

    let isStopWord word = stopWords.Contains word

    member __.Dispatch = function
        | Init            -> init () :> obj
        | IsStopWord word -> isStopWord word :> obj

type WFManagerMessage =
    | IncrementCount of word : string
    | Sorted

type WordFrequencyManager () =
    let wordFreqs = Dictionary<string, int>()

    let incrementCount word =
        match wordFreqs.TryGetValue word with
        | true, n -> wordFreqs.[word] <- n+1
        | _       -> wordFreqs.[word] <- 1

    let sorted () =
        wordFreqs
        |> Seq.map (fun (KeyValue(word, n)) -> word, n)
        |> Seq.sortByDescending snd

    member __.Dispatch = function
        | IncrementCount word -> incrementCount word :> obj
        | Sorted              -> sorted () :> obj

type WFControllerMessage =
    | Init of filePath : string
    | Run

type WordFrequencyController () =
    let mutable dataStorageManager = 
        Unchecked.defaultof<DataStorageManager>
    let mutable stopWordsManager =
        Unchecked.defaultof<StopWordsManager>
    let mutable wordFreqsManager =
        Unchecked.defaultof<WordFrequencyManager>

    let init filePath =
        dataStorageManager <- DataStorageManager()
        stopWordsManager   <- StopWordsManager()
        wordFreqsManager   <- WordFrequencyManager()

        DSManagerMessage.Init filePath
        |> dataStorageManager.Dispatch
        |> ignore

        SWManagerMessage.Init
        |> stopWordsManager.Dispatch
        |> ignore

    let run () =
        (dataStorageManager.Dispatch Words) :?> string[]
        |> Array.filter (fun word ->
            let msg = IsStopWord word
            stopWordsManager.Dispatch msg :?> bool |> not)
        |> Array.iter (fun word ->
            let msg = IncrementCount word
            wordFreqsManager.Dispatch msg |> ignore)

        (wordFreqsManager.Dispatch Sorted) :?> seq<string * int>
        |> Seq.take 25
        |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

    member __.Dispatch = function
        | Init filePath -> init filePath :> obj
        | Run -> run() :> obj

let controller = WordFrequencyController()
controller.Dispatch (Init ``p & p``)
controller.Dispatch Run