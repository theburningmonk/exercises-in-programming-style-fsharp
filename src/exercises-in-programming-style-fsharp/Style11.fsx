open System
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type DataStorageManager () =
    let mutable data = ""

    let init filePath =
        let regex = new Regex("[\W_]+")
        data <- regex.Replace(File.ReadAllText(filePath), " ").ToLower()

    let words () = data.Split()

    member __.Dispatch (message : string[]) : obj =
       match message with
       | [| "init"; filePath |] -> init filePath; null
       | [| "words" |] -> words () :> obj

type StopWordsManager () =
    let mutable stopWords = [||]

    let init () =
        stopWords <- File.ReadAllText(``stop words``).Split(',')
        stopWords <- Array.append stopWords ([|'a'..'z'|] |> Array.map string)
        |> Set.ofArray