open System
open System.IO
open System.Linq
open System.Text.RegularExpressions

#time

let ``stop words`` = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p&p`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

let stops = (File.ReadAllText ``stop words``).Split(',') |> Set.ofArray
let words = Regex.Matches((File.ReadAllText ``p&p``).ToLower(), "[a-z]{2,}") |> Seq.cast<Match> |> Seq.map (fun m -> m.Value)
let counts = words.Where(stops.Contains >> not).GroupBy(fun x -> x).Select(fun gr -> gr.Key, gr |> Seq.length)
counts |> Seq.sortByDescending snd |> Seq.take 25 |> Seq.iter (fun (w, n) -> printfn "%s - %d" w n)