open System.IO

let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride_and_prejudice.txt"
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
let stopWords = 
    File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt").Split(',')
    |> Set.ofArray

let wordFreqs = File.Open(__SOURCE_DIRECTORY__ + "../word_freqs.txt", 
                          FileMode.Create, 
                          FileAccess.ReadWrite)

//let input = new StreamReader(``pride and prejudice``)
let input = new StreamReader(test)
let next  = input.ReadLine
let (|IsAlnum|_|) c = if System.Char.IsLetterOrDigit c then Some () else None
let (|Int|) input = System.Int32.Parse input

// very imperative, but the goal is to work with min amount of memory
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
    |> Seq.filter (fun x -> x.Length >= 2 && stopWords.Contains x |> not)

let write (stream : Stream) word count = 
    use writer = new StreamWriter(stream, System.Text.Encoding.ASCII, 1024, true)
    writer.WriteLine(sprintf "%s,%d" word count)
    writer.Flush()

let incrFreq (word : string) =
    wordFreqs.Seek(0L, SeekOrigin.Begin) |> ignore
    use reader = new StreamReader(wordFreqs, System.Text.Encoding.ASCII, false, 1024, true)

    let rec loop : string -> bool = function
        | null -> false // end of file, word not found
        | line -> 
            let [| word'; (Int count) |] = line.Split(',')
            if word' = word then 
                wordFreqs.Seek(-int64 line.Length, SeekOrigin.Current) |> ignore
                write wordFreqs word' (count + 1)
                true
            else loop <| reader.ReadLine()

    let found = loop <| reader.ReadLine()
    if not found then write wordFreqs word 1

let rec proc (next : unit -> string) =
    match next() with
    | null -> () // end of file
    | line -> 
        findWords line |> Seq.iter incrFreq
        proc next

proc next