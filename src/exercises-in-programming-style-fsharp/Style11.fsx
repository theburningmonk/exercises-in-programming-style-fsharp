open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

exception MessageNotUnderstood of string[]

type DataStorageManager () =
    let mutable data = ""

    let init filePath =
        Regex("[\W_]+")
            .Replace(File.ReadAllText(filePath), " ")
            .ToLower()
        |> (fun x -> data <- x)

    let words () = data.Split()

    member __.Dispatch (message : string[]) : obj =
       match message with
       | [| "init"; filePath |] 
           -> init filePath :> obj
       | [| "words" |] 
           -> words () :> obj
       | x -> raise <| MessageNotUnderstood x

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

    member __.Dispatch (message : string[]) : obj =
        match message with
        | [| "init" |] 
            -> init () :> obj
        | [| "is_stop_word"; word |] 
            -> isStopWord word :> obj
        | x -> raise <| MessageNotUnderstood x

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

    member __.Dispatch (message : string[]) : obj =
        match message with
        | [| "increment_count"; word |] 
            -> incrementCount word :> obj
        | [| "sorted" |] 
            -> sorted() :> obj
        | x -> raise <| MessageNotUnderstood x

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

        [| "init"; filePath |]
        |> dataStorageManager.Dispatch
        |> ignore

        [| "init" |]
        |> stopWordsManager.Dispatch
        |> ignore

    let run () =
        dataStorageManager.Dispatch [| "words" |] 
        :?> string[]
        |> Array.filter (fun word ->
            let msg = [| "is_stop_word"; word |]
            stopWordsManager.Dispatch msg :?> bool |> not)
        |> Array.iter (fun word ->
            let msg = [| "increment_count"; word |]
            wordFreqsManager.Dispatch msg |> ignore)

        wordFreqsManager.Dispatch [| "sorted" |] 
        :?> seq<string * int>
        |> Seq.take 25
        |> Seq.iter (fun (word, n) -> 
            printfn "%s - %d" word n)

    member __.Dispatch (message : string[]) : obj =
        match message with
        | [| "init"; filePath |] 
            -> init filePath :> obj
        | [| "run" |] 
            -> run() :> obj
        | x -> raise <| MessageNotUnderstood x

let controller = WordFrequencyController()
controller.Dispatch [| "init"; ``p & p`` |]
controller.Dispatch [| "run" |]