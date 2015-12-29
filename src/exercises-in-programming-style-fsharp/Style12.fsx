open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type Map = Dictionary<string, obj>

let dataStorageObj = Map()
dataStorageObj.["data"] <- [||]

let extractWords (obj : Map) path =
    let regex = Regex("[\W_]+")
    obj.["data"] <- 
        regex.Replace(File.ReadAllText path, " ")
             .ToLower()
             .Split()

dataStorageObj.["init"]  <- (extractWords dataStorageObj)
dataStorageObj.["words"] <- fun () -> dataStorageObj.["data"]

let stopWordsObj = Map()
stopWordsObj.["stop_words"] <- Set.empty<string>

let loadStopWords (obj : Map) =
    let stopWords = 
        File.ReadAllText(``stop words``).Split(',')
        |> Array.append ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray
    obj.["stop_words"] <- stopWords

stopWordsObj.["init"] <- fun () -> loadStopWords stopWordsObj
stopWordsObj.["is_stop_word"] <- 
    fun word -> 
        (stopWordsObj.["stop_words"] :?> Set<string>)
            .Contains word

type Freqs = Dictionary<string, int>
let wordFreqsObj = Map()
wordFreqsObj.["freqs"] <- Freqs()

let incrementCount (obj : Map) word =
    let freqs = obj.["freqs"] :?> Dictionary<string, int>
    match freqs.TryGetValue word with
    | true, n -> freqs.[word] <- n+1
    | _ -> freqs.[word] <- 1

wordFreqsObj.["increment_count"] <- incrementCount wordFreqsObj
wordFreqsObj.["sorted"] <- 
    fun () -> 
        (wordFreqsObj.["freqs"] :?> Freqs)
        |> Seq.map (fun (KeyValue(word, n)) -> word, n)
        |> Seq.sortByDescending snd

(dataStorageObj.["init"] :?> string -> unit) ``p & p``
(stopWordsObj.["init"] :?> unit -> unit) ()

for w in (dataStorageObj.["data"] :?> string[]) do
    if (not << (stopWordsObj.["is_stop_word"] :?> string -> bool)) w
    then (wordFreqsObj.["increment_count"] :?> string -> unit) w

let wordFreqs = 
    (wordFreqsObj.["sorted"] :?> unit -> seq<string * int>) ()
for (w, c) in wordFreqs |> Seq.take 25 do
    printfn "%s - %d" w c