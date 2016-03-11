open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type WordFrequenciesModel (filepath) =
    let stopWords = 
        File.ReadAllText(``stop words``).Split ','
        |> Set.ofArray
    let regex = new Regex("[a-z]{2,}")

    let mutable freqs = [||]

    let update filepath =
        let words = 
            File.ReadAllText(filepath).ToLower()
            |> regex.Matches
            |> Seq.cast<Match>
            |> Seq.map (fun m -> m.Value)

        freqs <-
            words
            |> Seq.filter (not << stopWords.Contains)
            |> Seq.groupBy id
            |> Seq.map (fun (w, ws) -> 
                w, Seq.length ws)
            |> Seq.toArray

    do update filepath

    member __.Freqs = freqs
    member __.Update filepath = update filepath

type WordFrequenciesView (model : WordFrequenciesModel) =
    member __.Render () =
        model.Freqs
        |> Seq.sortByDescending snd
        |> Seq.take 25
        |> Seq.iter (fun (w, n) ->
            printfn "%s - %d" w n)

type WordFrequenciesController 
        (model : WordFrequenciesModel, 
         view : WordFrequenciesView) =
    do view.Render()

    member __.Run () =
        while true do
            printf "Next file :"
            let filepath = Console.ReadLine()
            model.Update filepath
            view.Render()

let model = WordFrequenciesModel(``p & p``)
let view  = WordFrequenciesView model
let controller = WordFrequenciesController(model, view)

controller.Run()