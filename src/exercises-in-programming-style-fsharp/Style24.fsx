open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

type IO<'a> = IOAction of (unit -> 'a)

[<Sealed>]
type IOBuilder () =
    member __.Bind (IOAction f, cont : 'a -> IO<'b>) : IO<'b> = 
        IOAction (fun () -> 
            let (IOAction g) = cont <| f()
            g())

    member __.Zero () = IOAction (fun () -> ())

    member __.Return x = IOAction (fun () -> x)

let do' = IOBuilder()

type File =
    static member ReadAllText path = 
        IOAction (fun () -> System.IO.File.ReadAllText path)

type Console =
    static member Print str =
        IOAction (fun () -> printfn "%s" str)

[<RequireQualifiedAccess>]
module IO =
    let execute (IOAction f) = f()

let extractWords inputFile = do' {
    let! data = File.ReadAllText inputFile
    let regex = new Regex("[\W_]+")
    return regex.Replace(data, " ").ToLower().Split()
}

let removeStopWords words = do' {
    let! stopWords = File.ReadAllText ``stop words``

    let stopWords = 
        stopWords.Split(',')
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray

    return words |> Array.filter (not << stopWords.Contains)
}

let frequencies words =
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray

let top25 freqs = 
    freqs
    |> Array.sortByDescending snd
    |> Seq.take 25
    |> Seq.map (fun (word, len) -> 
        sprintf "%s - %d\n" word len)
    |> Seq.reduce (+)

let mainProgram = 
    do' {
        let! words = extractWords ``p & p``
        let! filteredWords = removeStopWords words
        let top25 = frequencies filteredWords |> top25
        do! Console.Print top25
    }

IO.execute mainProgram