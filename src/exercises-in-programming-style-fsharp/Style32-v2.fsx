open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type WordFrequenciesModel =
    {
        Freqs : (string * int)[]
    }

let stopWords = 
    File.ReadAllText(``stop words``).Split ','
    |> Set.ofArray

let createModel filepath =
    let regex = new Regex("[a-z]{2,}")
    let words = 
        File.ReadAllText(filepath).ToLower()
        |> regex.Matches
        |> Seq.cast<Match>
        |> Seq.map (fun m -> m.Value)

    let freqs =
        words
        |> Seq.filter (not << stopWords.Contains)
        |> Seq.groupBy id
        |> Seq.map (fun (w, ws) -> 
            w, Seq.length ws)
        |> Seq.toArray

    { Freqs = freqs }

let render { Freqs = freqs } =
    freqs
    |> Seq.sortByDescending snd
    |> Seq.take 25
    |> Seq.iter (fun (w, n) ->
        printfn "%s - %d" w n)

type WordFrequenciesController () =
    let newInput = new Event<string>()

    member __.Run () =
        do newInput.Trigger ``p & p``

        while true do
            printf "Next file :"
            let filepath = Console.ReadLine()
            newInput.Trigger filepath

    member __.OnInput = newInput.Publish

let controller = WordFrequenciesController()
controller.OnInput
|> Observable.map createModel
|> Observable.subscribe render

controller.Run()