open System.IO
open System.Linq
open System.Text.RegularExpressions

#time

let stops = (File.ReadAllText <| __SOURCE_DIRECTORY__ + "../stop_words.txt").Split(',') |> Set.ofArray
let words = Regex.Matches((File.ReadAllText <| __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt").ToLower(), "[a-z]{2,}") 
            |> Seq.cast<Match> |> Seq.map (fun m -> m.Value)
let counts = words.Where(stops.Contains >> not).GroupBy(fun x -> x).Select(fun gr -> gr.Key, gr |> Seq.length)
counts |> Seq.sortByDescending snd |> Seq.take 25 |> Seq.iter (fun (w, n) -> printfn "%s - %d" w n)