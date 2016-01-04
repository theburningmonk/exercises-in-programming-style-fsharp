open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

let (?) x prop =
    let propInfo = x.GetType().GetProperty(prop)
    propInfo.GetValue x

type DataStorage (filepath) =
    let regex = new Regex("[\W_]+")
    let data = 
        regex
            .Replace(File.ReadAllText(filepath), " ")
            .ToLower()

    member __.Words = data.Split()

type StopWordsFilter (filepath) =
    let stopWords = 
        File.ReadAllText filepath
        |> (fun s -> s.Split(','))
        |> Array.append 
            ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray

    member __.StopWords = stopWords

let filterWords dataStorage filter =
    let stopWords = filter?StopWords  :?> Set<string>
    let words     = dataStorage?Words :?> string[]
    
    words |> Seq.filter (stopWords.Contains >> not)

let countFrequency words = 
    words 
    |> Seq.groupBy id
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)

type WordFrequencyController (dataStorage, filter) =
    let words = filterWords dataStorage filter
    let freqs = countFrequency words

    member __.PrintTop25 () =
        freqs
        |> Seq.sortByDescending snd
        |> Seq.take 25
        |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

let dataStorage = new DataStorage(``p & p``)
let filter      = new StopWordsFilter(``stop words``)
let controller  = new WordFrequencyController(dataStorage, filter)

controller.PrintTop25()