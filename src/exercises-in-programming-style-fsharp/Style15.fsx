open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type Handler = obj -> unit

type IEventManager =
    abstract member Subscribe : string * Handler -> unit
    abstract member Publish   : string * obj -> unit

type EventManager () =
    let subscriptions = Dictionary<string, Handler list>()

    interface IEventManager with
        member __.Subscribe (key, handler) =
            match subscriptions.TryGetValue key with
            | true, handlers -> 
                subscriptions.[key] <- handler::handlers
            | _ ->
                subscriptions.[key] <- [ handler ]

        member __.Publish (key, item) =
            match subscriptions.TryGetValue key with
            | true, handlers ->
                handlers |> List.iter (fun h -> h item)
            | _ -> ()

type RunArgs = 
    {
        DataFile      : string
        StopWordsFile : string
    }

type DataStorage (evtManager : IEventManager) =
    let run (args : obj) =
        let args = args :?> RunArgs
        let text = File.ReadAllText args.DataFile
        let words = 
            Regex("[\W_]+")
                    .Replace(text, " ")
                    .ToLower()
                    .Split()
        evtManager.Publish ("loaded", (args, words))

    do evtManager.Subscribe ("run", run)

type StopWordFilter (evtManager : IEventManager) =
    let loadStopWords { StopWordsFile = path } =
        File.ReadAllText path
        |> (fun s -> s.Split(','))
        |> Array.append ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray

    let filter (args : obj) =
        let args, words = (args :?> RunArgs * string[])
        let stopWords   = loadStopWords args
        let filteredWords =
            words |> Array.filter (stopWords.Contains >> not)
        evtManager.Publish ("filtered", filteredWords)

    do evtManager.Subscribe ("loaded", filter)

type WordFrequencyCounter (evtManager : IEventManager) =
    let sortByFreq (args : obj) =
        let words     = args :?> string[]
        let wordFreqs = Dictionary<string, int>()

        words
        |> Seq.iter (fun w ->
            match wordFreqs.TryGetValue w with
            | true, n -> wordFreqs.[w] <- n + 1
            | _       -> wordFreqs.[w] <- 1)

        wordFreqs
        |> Seq.map (fun (KeyValue(word, n)) -> word, n)
        |> Seq.sortByDescending snd

    let print words =
        words
        |> Seq.take 25
        |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

    do evtManager.Subscribe ("filtered", sortByFreq >> print)

let evtManager = new EventManager() :> IEventManager

let dataStorage     = DataStorage(evtManager)
let stopWordFilter  = StopWordFilter(evtManager)
let wordFreqCounter = WordFrequencyCounter(evtManager)

let args = { DataFile = ``p & p``; StopWordsFile = ``stop words`` }
evtManager.Publish ("run", args)