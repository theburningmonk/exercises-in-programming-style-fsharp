namespace Style19Plugins

open System
open System.IO
open System.Text.RegularExpressions

open Style19

[<AutoOpen>]
module Utils =
    let split (splitChar : char) (input : string) =
        input.Split(
            [|splitChar|],
            StringSplitOptions.RemoveEmptyEntries)
    
    let filterCharsAndNormalize data =
        let regex = new Regex("[\W_]+")
        regex.Replace(data, " ").ToLower()

    let loadStopWords path = 
        File.ReadAllText path
        |> split ','
        |> Array.append ([| 'a'..'z' |] |> Array.map string)
        |> Set.ofArray

    let frequencies words = 
        words 
        |> Seq.groupBy id 
        |> Seq.map (fun (word, gr) -> word, Seq.length gr)
        |> Seq.toArray

type WordsPlugIn() =
    interface IWords with
        member __.ExtractWords(inputFile, stopWordsFile) =
            let data = 
                File.ReadAllText inputFile
                |> filterCharsAndNormalize
                |> split ' '
            let sw = loadStopWords stopWordsFile
            data |> Array.filter (not << sw.Contains)

type FrequenciesPlugIn() =
    interface IFrequencies with
        member __.Top25(words) =
            words
            |> frequencies
            |> Array.sortByDescending snd
            |> Seq.take 25
            |> Seq.toArray