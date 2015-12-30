open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type RunArgs = 
    {
        DataFile      : string
        StopWordsFile : string
    }

type DataStorage (src : IObservable<RunArgs>) =
    let onLoad = new Event<RunArgs * string[]>()

    do src
       |> Observable.subscribe (fun args -> 
            let text = File.ReadAllText args.DataFile
            let words =
                Regex("[\W_]+")
                    .Replace(text, " ")
                    .ToLower()
                    .Split()
            onLoad.Trigger(args, words))
       |> ignore

    member __.OnLoad = onLoad.Publish

type StopWordFilter (src : IObservable<RunArgs * string[]>) =
    let onFiltered = new Event<string[]>()

    let loadStopWords { StopWordsFile = path } =
        File.ReadAllText path
        |> (fun s -> s.Split(','))
        |> Array.append ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray

    do src
       |> Observable.subscribe (fun (args, words) ->
            let stopWords = loadStopWords args
            words 
            |> Array.filter (stopWords.Contains >> not)
            |> onFiltered.Trigger)
       |> ignore

    member __.OnFiltered = onFiltered.Publish

type WordFrequencyCounter (src : IObservable<string[]>) =
    let sortByFreq words =
        let wordFreqs = Dictionary<string, int>()

        words
        |> Seq.iter (fun w ->
            match wordFreqs.TryGetValue w with
            | true, n -> wordFreqs.[w] <- n + 1
            | _       -> wordFreqs.[w] <- 1)

        wordFreqs
        |> Seq.map (fun (KeyValue(word, n)) -> word, n)
        |> Seq.sortByDescending snd

    do src
       |> Observable.subscribe (fun words ->
            words
            |> sortByFreq
            |> Seq.take 25
            |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n))
       |> ignore

let runCmd = new Event<RunArgs>()

let dataStorage     = DataStorage(runCmd.Publish)
let stopWordFilter  = StopWordFilter(dataStorage.OnLoad)
let wordFreqCounter = WordFrequencyCounter(stopWordFilter.OnFiltered)

{ DataFile = ``p & p``; StopWordsFile = ``stop words`` }
|> runCmd.Trigger