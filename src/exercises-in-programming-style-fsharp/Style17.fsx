#r "../../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"

open System.IO
open System.Text
open Microsoft.FSharp.Compiler.Interactive.Shell

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

// see https://fsharp.github.io/FSharp.Compiler.Service/interactive.html
// for more information about hosting a F# interactive
let sbOut     = new StringBuilder()
let sbErr     = new StringBuilder()
let inStream  = new StringReader("")
let outStream = new StringWriter(sbOut)
let errStream = new StringWriter(sbErr)

let fsiPath = "C:\Program Files (x86)\Microsoft SDKs\F#\4.0\Framework\v4.0\fsi.exe"
let allArgs = [| fsiPath; "--noninteractive"|]

let fsiConfig  = FsiEvaluationSession.GetDefaultConfiguration()
let fsiSession = 
    FsiEvaluationSession.Create(
        fsiConfig, allArgs, inStream, outStream, errStream)

let evalExpression<'a> text =
  match fsiSession.EvalExpression(text) with
  | Some value -> value.ReflectionValue :?> 'a
  | None -> failwith "eval failed"

let extractWords = 
    sprintf """
let loadStopWords = 
    System.IO.File.ReadAllText
    >> (fun s -> s.Split ',')
    >> Array.append ([| 'a'..'z' |] |> Array.map string)
    >> Set.ofArray

let filterCharsAndNormalize data =
    let regex = new System.Text.RegularExpressions.Regex("[\W_]+")
    regex.Replace(data, " ").ToLower()

let loadWords = 
    System.IO.File.ReadAllText
    >> filterCharsAndNormalize
    >> (fun s -> s.Split())

let extractWords stopWordsFile inputFile =
    let stopWords = loadStopWords stopWordsFile
    loadWords inputFile
    |> Array.filter (not << stopWords.Contains)

extractWords
"""

let frequencies = """
let frequencies (words : string[]) = 
    words 
    |> Seq.groupBy id 
    |> Seq.map (fun (word, gr) -> word, Seq.length gr)
    |> Seq.toArray

frequencies
"""

let printTop25 = """
let printTop25 (wordFreqs : (string * int)[]) =
    wordFreqs
    |> Seq.sortByDescending snd
    |> Seq.take 25
    |> Seq.iter (fun (word, n) -> printfn "%s - %d" word n)

printTop25
"""

let extract = 
    evalExpression<string -> string -> string[]> extractWords
let freqs = 
    evalExpression<string[] -> (string * int)[]> frequencies
let print = 
    evalExpression<(string * int)[] -> unit> printTop25

extract ``stop words`` ``p & p``
|> freqs
|> print