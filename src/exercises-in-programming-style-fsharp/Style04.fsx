open System
open System.IO

#time

let ``pride and prejudice`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

let mutable data : char[]    = [||]
let mutable words : string[] = [||]
let mutable wordFreqs : (string*int)[] = [||]

let readFile path = 
    data <- (File.ReadAllText path).ToCharArray()

let filterCharsAndNormalize () =
    for idx = 0 to data.Length-1 do
        let c = data.[idx]
        if not <| Char.IsLetterOrDigit c then data.[idx] <- ' '
        else data.[idx] <- Char.ToLower c

let scan () = 
    let data = new String(data)
    words <- data.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)

let removeStopWords () = 
    let stopWords = 
        File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt")
            .Split(',')
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray
    
    words <- words |> Array.filter (not << stopWords.Contains)

let frequencies () = 
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray
    |> fun x -> wordFreqs <- x

let sort () = Array.sortInPlaceBy (fun (_, n) -> -n) wordFreqs

readFile ``pride and prejudice``
filterCharsAndNormalize()
scan()
removeStopWords()
frequencies()
sort()

for (word, n) in wordFreqs.[0..24] do
    printfn "%s - %d" word n