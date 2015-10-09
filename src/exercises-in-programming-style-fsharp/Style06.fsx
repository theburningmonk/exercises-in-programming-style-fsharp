open System.IO
open System.Linq
open System.Text.RegularExpressions

#time

let s = (File.ReadAllText <| __SOURCE_DIRECTORY__ + "../stop_words.txt").Split(',') |> Set.ofArray
let w = Regex.Matches((File.ReadAllText <| __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt").ToLower(), "[a-z]{2,}") |> Seq.cast<Match> |> Seq.map (fun m -> m.Value)
w.Where(s.Contains >> not).GroupBy(fun x -> x).Select(fun gr -> gr.Key, gr |> Seq.length) |> Seq.sortByDescending snd |> Seq.take 25 |> Seq.iter (fun (w, n) -> printfn "%s - %d" w n)