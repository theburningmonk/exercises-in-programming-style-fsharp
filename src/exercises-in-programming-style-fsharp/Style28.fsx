open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

#time

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "/stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "/pride-and-prejudice.txt"

type DataStorageMessage =
    | LoadWords of string
    | NextWord  of AsyncReplyChannel<string option>

let dataStorageManager = 
    MailboxProcessor<DataStorageMessage>.Start(fun inbox ->
        let loadWords path =
            Regex("[\W_]+")
                .Replace(File.ReadAllText path, " ")
                .ToLower()
                .Split()
            |> Array.toList

        let rec loop data = 
            async {
                let! msg = inbox.Receive()
                match msg with
                | LoadWords path ->
                    return! loop <| loadWords path
                | NextWord reply ->
                    match data with
                    | [] -> 
                        reply.Reply None
                        return! loop data
                    | hd::tl -> 
                        reply.Reply <| Some hd
                        return! loop tl
            }

        loop [])

type StopWordsMessage =
    | LoadStopWords of string
    | IsStopWord    of string * AsyncReplyChannel<bool>

let stopWordsManager = 
    MailboxProcessor<StopWordsMessage>.Start(fun inbox ->
        let loadStopWords path =
            File.ReadAllText path
            |> (fun s -> s.Split(','))
            |> Array.append 
                ([|'a'..'z'|] |> Array.map string)
            |> Set.ofArray

        let rec loop stopWords =
            async {
                let! msg = inbox.Receive()
                match msg with
                | LoadStopWords path ->  
                    return! loop <| loadStopWords path
                | IsStopWord (word, reply) ->
                    reply.Reply <| stopWords.Contains word
                    return! loop stopWords
            }
            
        loop <| set [])

type WordFrequencyMessage =
    | Add   of string
    | TopN  of int * AsyncReplyChannel<(string * int)[]>
    | Reset

let wordFreqManager =
    MailboxProcessor<WordFrequencyMessage>.Start(fun inbox ->
        let wordFreqs = Dictionary<string, int>()
        
        async {
            while true do
                let! msg = inbox.Receive()
                match msg with
                | Reset -> wordFreqs.Clear()
                | Add word ->
                    match wordFreqs.TryGetValue word with
                    | true, n -> wordFreqs.[word] <- n+1
                    | _       -> wordFreqs.[word] <- 1
                | TopN (n, reply) ->
                    wordFreqs
                    |> Seq.map (fun (KeyValue(word, n)) -> word, n)
                    |> Seq.sortByDescending snd
                    |> Seq.take n
                    |> Seq.toArray
                    |> reply.Reply
        
        })

type ControllerMessage =
    | Run of AsyncReplyChannel<unit>

let controller =
    MailboxProcessor<ControllerMessage>.Start(fun inbox ->
        let init () =
            dataStorageManager.Post <| LoadWords ``p & p``
            stopWordsManager.Post <| LoadStopWords ``stop words``
            wordFreqManager.Post Reset

        let rec countWords () =
            async {
                let! word = 
                    dataStorageManager.PostAndAsyncReply NextWord

                match word with
                | None -> ()
                | Some word ->
                    let! ignore = 
                        stopWordsManager
                            .PostAndAsyncReply(fun reply -> 
                                IsStopWord(word, reply))
                    
                    if not ignore then
                        wordFreqManager.Post <| Add word

                    return! countWords ()
            }

        let printTop25 () =
            async {
                let! top25 = 
                    wordFreqManager
                        .PostAndAsyncReply(fun reply ->
                            TopN(25, reply))
                
                top25
                |> Array.iter (fun (word, n) -> 
                printfn "%s - %d" word n)
            }

        async {
            let! msg = inbox.Receive()
            match msg with
            | Run reply ->
                init()
                do! countWords()
                do! printTop25()

                reply.Reply ()

        })

controller.PostAndAsyncReply Run
|> Async.RunSynchronously