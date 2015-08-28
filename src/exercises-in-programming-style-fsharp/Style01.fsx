open System
open System.Collections.Generic
open System.IO
open System.Text

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

// simulate the absense of identifiers, so everything goes into this array which
// acts as the 'main memory'
// 0 = stop words
// 1 = input buffer (1024 bytes)
// 2 = start index (used in findWords)
// 3 = length of word (used in findWords)
// 4 = the word 
// 5 = write buffer (word + count)
// 6 = current position in word_freqs
// 7 = current word_freqs line
// 8 = current min word freq of the top 25
let data = Array.zeroCreate<obj> 9

// PART 1
let stopWords = File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt")
data.[0] <- stopWords.Split(',') |> Set.ofArray :> obj

let wordFreqs = File.Open(__SOURCE_DIRECTORY__ + "../word_freqs.txt", 
                          FileMode.Create, 
                          FileAccess.ReadWrite)

let input = new StreamReader(``pride and prejudice``)
//let input = new StreamReader(test)

let (|IsAlnum|_|) c = if System.Char.IsLetterOrDigit c then Some () else None

// note: each line in word_freqs is "word,count", so these two funcs just make 
// the code easier to read
let getWord (x : obj) = (x :?> string).Split(',').[0]
let getFreq (x : obj) = (x :?> string).Split(',').[1] |> Int32.Parse

let findWords (line : string) =
    seq {
        data.[2] <- -1 :> obj
        data.[3] <- 0  :> obj
        for i = 0 to line.Length-1 do
            match line.[i], data.[2] :?> int with
            | IsAlnum, -1 -> // start of word
                data.[2] <- i :> obj
                data.[3] <- 1 :> obj
            | IsAlnum, _  -> // another char for word
                data.[3] <- (data.[3] :?> int + 1) :> obj
            | _, -1 -> () // no word, not started, ignore
            | _ -> // end of word
                yield line.Substring(data.[2] :?> int, data.[3] :?> int)
                data.[2] <- -1 :> obj
                data.[3] <- 0  :> obj

        if (data.[3] :?> int) > 0 then 
            yield line.Substring(data.[2] :?> int, data.[3] :?> int)
    }
    |> Seq.map (fun x -> x.ToLower())
    |> Seq.filter (fun x -> x.Length >= 2 && stopWords.Contains x |> not)

let write (stream : Stream) word count =
    data.[5] <- sprintf "%s,%6d\n" word count :> obj

    printfn "writing %s %d" word count

    stream.Write(Encoding.ASCII.GetBytes(data.[5] :?> string), 0, (data.[5] :?> string).Length)
    stream.Flush()

let incrFreq (word : string) =
    // since the reader reads 128 chars at a time, so use this to track where we are
    data.[6] <- 0L :> obj
    wordFreqs.Seek(0L, SeekOrigin.Begin) |> ignore
    use reader = new StreamReader(wordFreqs, Encoding.ASCII, false, 128, true)

    data.[7] <- reader.ReadLine() :> obj
    while data.[7] <> null && data.[6] <> null do        
        if word = getWord data.[7] then
            wordFreqs.Seek(data.[6] :?> int64, SeekOrigin.Begin) |> ignore
            write wordFreqs word (getFreq data.[7] + 1)

            // set read pos to null to indicate we've found and incremented count
            data.[6] <- null
        else
            // note: +1 here to account for the newline character
            data.[6] <- (data.[6] :?> int64 + int64 (data.[7] :?> string).Length + 1L) :> obj
            data.[7] <- reader.ReadLine() :> obj

    if data.[6] <> null then
        write wordFreqs word 1
    
data.[1] <- input.ReadLine() :> obj
while (data.[1] <> null) do
    data.[1] :?> string |> findWords |> Seq.iter incrFreq
    data.[1] <- input.ReadLine() :> obj

// PART 2
wordFreqs.Seek(0L, SeekOrigin.Begin)
let reader = new StreamReader(wordFreqs)

// make the sorted list 1 bigger to prevent resizing
let list = new System.Collections.Generic.SortedList<string, int>(25 + 1)
data.[7] <- reader.ReadLine() :> obj
while data.[7] <> null do
    if list.Count < 25 then 
        list.Add(getWord data.[7], getFreq data.[7])
    else
        data.[8] <- (list |> Seq.minBy (fun kvp -> kvp.Value)) :> obj
        if (data.[8] :?> KeyValuePair<string, int>).Value < getFreq data.[7] then
            (data.[8] :?> KeyValuePair<string, int>).Key
            |> list.Remove 
            |> ignore
            list.Add(getWord data.[7], getFreq data.[7])

    data.[7] <- reader.ReadLine() :> obj

list 
|> Seq.sortByDescending (fun kvp -> kvp.Value)
|> Seq.iter (fun (KeyValue(word, count)) -> printfn "%s - %d" word count)