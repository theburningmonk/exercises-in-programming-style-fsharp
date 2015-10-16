open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type Map = Dictionary<string, obj>

let extractWords (obj : Map) path =
    let regex = Regex("[\W_]+")
    obj.["data"] <- regex.Replace(File.ReadAllText path, " ").ToLower().Split()

let loadStopWords (obj : Map) =
    let stopWords = 
        File.ReadAllText(``stop words``).Split(',')
        |> Array.append ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray
    obj.["stop_words"] <- stopWords

let incrementCount (obj : Map) word =
    let freqs = obj.["freqs"] :?> Dictionary<string, int>
    match freqs.TryGetValue word with
    | true, n -> freqs.[word] <- n+1
    | _ -> freqs.[word] <- 1

let dataStorageObj = Map()
dataStorageObj.["data"] <- [||]
dataStorageObj.["init"] <- fun path -> extractWords dataStorageObj path
dataStorageObj.["words"] <- fun () -> dataStorageObj.["data"]

let stopWordsObj = Map()
stopWordsObj.["stop_words"] <- Set.empty<string>
stopWordsObj.["init"] <- fun () -> loadStopWords(stopWordsObj)
stopWordsObj.["is_stop_word"] <- 
    fun word -> (stopWordsObj.["stop_words"] :?> Set<string>).Contains word

let wordFreqsObj = Map()
wordFreqsObj.["freqs"] <- Dictionary<string, int>()
wordFreqsObj.["increment_count"] <- fun word -> incrementCount wordFreqsObj word
wordFreqsObj.["sorted"] <- fun 