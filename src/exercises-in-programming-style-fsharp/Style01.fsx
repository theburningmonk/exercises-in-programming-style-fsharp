open System.IO
#time
let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"

(* 
Possible Names
--------------
* Good old times
* Early 50s style

See original Python implementation:
https://github.com/crista/exercises-in-programming-style/blob/master/01-good-old-times/tf-01.py

CONSTRAINTS
-----------
* very small amount of primary memory, typically order of magnitude smaller than
  the data that needs to be processed/generated
* no identifiers - i.e. no variable names or tagged memory addresses.
  All we have is memory that is addressable with numbers.

STRATEGY
--------
(Part 1)
1. read the input file one line at a time
2. filter the characters, normalize to lower case
3. identify words, increment corresponding counts in secondary memory (a file)
(Part 2)
4. find the 25 most frequent words in secondary memory (file)
*)

// PART 1
let stopWords = 
    File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt").Split(',')
    |> Set.ofArray

let wordFreqs = File.Open(__SOURCE_DIRECTORY__ + "../word_freqs.txt", 
                          FileMode.Create, 
                          FileAccess.ReadWrite)

let input = new StreamReader(``pride and prejudice``)
//let input = new StreamReader(test)
let next  = input.ReadLine
let (|IsAlnum|_|) c = if System.Char.IsLetterOrDigit c then Some () else None
let (|Int|) input = System.Int32.Parse input

// very imperative, but the goal is to work with limited amount of memory
let findWords (line : string) =
    seq {
        let mutable startIdx, len = -1, 0
        for i = 0 to line.Length-1 do
            match line.[i], startIdx with
            | IsAlnum, -1 -> startIdx <- i; len <- 1   // start of word
            | IsAlnum, _  -> len <- len + 1            // another char for word
            | _, -1 -> () // no word, not started, ignore
            | _ -> // end of word
                yield line.Substring(startIdx, len)
                startIdx <- -1; len <- 0

        if len > 0 then yield line.Substring(startIdx, len)
    }
    |> Seq.map (fun x -> x.ToLower())
    |> Seq.filter (fun x -> x.Length >= 2 && stopWords.Contains x |> not)    

let write (stream : Stream) word count = 
    let content = sprintf "%s,%6d\n" word count
    let bytes = System.Text.Encoding.ASCII.GetBytes(content)
    stream.Write(bytes, 0, bytes.Length)
    stream.Flush()

let incrFreq (word : string) =
    // since the reader reads 128 chars at a time, so use this to track where we are
    let mutable readPosition = 0L
    wordFreqs.Seek(0L, SeekOrigin.Begin) |> ignore
    use reader = new StreamReader(wordFreqs, System.Text.Encoding.ASCII, false, 128, true)

    let rec loop : string -> bool = function
        | null -> false // end of file, word not found
        | line -> 
            let [| word'; (Int count) |] = line.Split(',')
            let lineLen = int64 line.Length + 1L // +1 for newline
            readPosition <- readPosition + lineLen
            if word' = word then
                wordFreqs.Seek(readPosition - lineLen, SeekOrigin.Begin) |> ignore
                write wordFreqs word' (count + 1)
                true
            else
                loop <| reader.ReadLine()

    let found = loop <| reader.ReadLine()
    if not found then write wordFreqs word 1

let rec proc (next : unit -> string) =
    match next() with
    | null -> () // end of file
    | line -> 
        findWords line |> Seq.iter incrFreq
        proc next

proc next

// PART 2
wordFreqs.Seek(0L, SeekOrigin.Begin)
let reader = new StreamReader(wordFreqs)

// make the sorted list 1 bigger to prevent resizing
let list = new System.Collections.Generic.SortedList<string, int>(25 + 1)
let rec loop : string -> unit = function
    | null -> () // end of file
    | line -> 
        let [| word; (Int count) |] = line.Split(',')
        if list.Count < 25 then list.Add(word, count)
        else // swap if greater than the min
            let min = list |> Seq.minBy (fun kvp -> kvp.Value)
            if min.Value < count then
                list.Remove(min.Key) |> ignore
                list.Add(word, count)
                    
        loop <| reader.ReadLine()
loop <| reader.ReadLine()    

list 
|> Seq.sortByDescending (fun kvp -> kvp.Value)
|> Seq.iter (fun (KeyValue(word, count)) -> printfn "%s - %d" word count)