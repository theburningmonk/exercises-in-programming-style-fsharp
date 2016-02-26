open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

type ColumnRef = string
type Formula = ColumnRef[] * (string[][] -> string[])
type DisplayValue = string[]

type Column = 
    | Formula of Formula * DisplayValue
    | Value   of DisplayValue

    member this.DisplayValue =
        match this with
        | Formula (_, v)
        | Value v -> v

type Spreadsheet () =
    let columns = Dictionary<ColumnRef, Column>()

    let rec valueOf = function
        | Value x -> x
        | Formula ((refs, map), _) ->
            refs
            |> Array.map (fun ref -> valueOf columns.[ref])
            |> map

    let refresh () =
        columns
        |> Seq.map (fun (KeyValue(ref, col)) -> ref, col)
        |> Seq.toArray
        |> Array.iter (fun (ref, col) ->
            match col with
            | Value _   -> ()
            | Formula (f, _) -> 
                columns.[ref] <- Formula (f, valueOf col))              

    member __.Item
        with get ref = columns.[ref].DisplayValue
        and set ref input = 
            match input with
            | Choice1Of2 data ->
                columns.[ref] <- Value data
                refresh()
            | Choice2Of2 formula ->
                columns.[ref] <- Formula(formula, [||])
                refresh()

let value data = Choice1Of2 data
let formula refs (map : string[][] -> string[]) = 
    Choice2Of2 (refs, map)

let spreadsheet = Spreadsheet()

// col A - the raw input
spreadsheet.["A"] <- value [||]

// col B - stop words
spreadsheet.["B"] <- value [||]

// col C - non-stop words
let nonStopWords [| allWords; stopWords |] =
    let stopWords = stopWords |> Set.ofArray
    allWords |> Array.filter (not << stopWords.Contains)
spreadsheet.["C"] <- formula [|"A";"B"|] nonStopWords

// col D - unique words
let uniqueWords [| nonStopWords |] =
    nonStopWords |> Seq.distinct |> Seq.toArray
spreadsheet.["D"] <- formula [|"C"|] uniqueWords

// col E - counts
let counts [| nonStopWords; uniqueWords |] =
    let wordCounts =
        nonStopWords
        |> Seq.groupBy id
        |> Seq.map (fun (word, lst) -> 
            word, Seq.length lst)
        |> Map.ofSeq

    uniqueWords
    |> Array.map (fun word ->
        wordCounts.[word] |> string)
spreadsheet.["E"] <- formula [|"C";"D"|] counts

// col F - sorted data
let sorted [| uniqueWords; counts |] =
    Array.zip uniqueWords counts
    |> Array.sortByDescending (snd >> int)
    |> Array.map (fun (word, count) ->
        sprintf "%s - %s" word count)
spreadsheet.["F"] <- formula [|"D";"E"|] sorted

spreadsheet.["A"] <-  
    Regex.Matches(
        File.ReadAllText(``p & p``).ToLower(), 
        "[a-z]{2,}") 
    |> Seq.cast<Match> 
    |> Seq.map (fun m -> m.Value)
    |> Seq.toArray
    |> value
spreadsheet.["B"] <- 
    value <| File.ReadAllText(``stop words``).Split(',')

spreadsheet.["F"]
|> Seq.take 25
|> Seq.iter (printfn "%s")