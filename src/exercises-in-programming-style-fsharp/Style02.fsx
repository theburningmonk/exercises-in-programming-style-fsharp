open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Text.RegularExpressions

#time
let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"
let stopWords = __SOURCE_DIRECTORY__ + "../stop_words.txt"

[<AutoOpen>]
module Env =
    let mutable private stack : obj list = []
    let mutable private heap = Map.empty<string, obj>

    /// Pops the HEAD of the stack
    let private pop<'a> () =
        let hd::tl = stack
        stack <- tl
        hd :?> 'a

    /// Pushes a new item to the HEAD position of the stack
    let push x = stack <- x::stack

    /// Return the current size of the stack
    let len () = stack.Length

    /// Stores a new value against the specified name 
    /// in the heap
    let store name value = heap <- heap.Add(name, value)

    /// Loads the value associated with the name onto 
    /// the stack
    let load<'a> name = heap.[name] |> push

    /// Performs a unary operation against the HEAD of 
    /// the stack
    let unaryOp (f : 'a -> 'b) = pop<'a>() |> f |> push
    let unaryOp2 (f : 'a -> unit) = pop<'a>() |> f

    /// Performs a binary operation against the last 
    /// two items on the stack
    let binOp (f : 'a -> 'b -> 'c) = 
        f (pop<'a>()) (pop<'b>()) |> push
    let binOp2 (f : 'a -> 'b -> unit) = 
        f (pop<'a>()) (pop<'b>())

    /// Performs a ternary operation against the last 
    /// three items on the stack
    let ternaryOp (f : 'a -> 'b -> 'c -> 'd ) = 
        f (pop<'a>()) (pop<'b>()) (pop<'c>()) |> push

    let printStack() = 
        printfn "============== STACK =============="
        stack |> List.iter (printfn "%A")
        printfn "============== STACK =============="

    let printHeap() =
        printfn "============== HEAP =============="
        heap |> Map.iter (printfn "%s : %A")
        printfn "============== HEAP =============="

let split () =
    binOp (fun (separator : char) (str : string) -> 
        str.Split(
            [| separator |], 
            StringSplitOptions.RemoveEmptyEntries))

let readFile () = unaryOp File.ReadAllText

let replaceNonAlphanumeric () =
    let regex pattern = new Regex(pattern)

    let replace (regex : Regex) (replacement : string) input = 
        regex.Replace(input, replacement)

    push " "            // used as replacement string later
    push "[\W_]+"       // used for Regex creation below
    unaryOp regex       // push new Regex to head of stack
    ternaryOp replace   // consume the Regex, replacement and input string
    
/// Takes a string on the stack, scan for words, and place the individual 
/// words on the stack
let scan () = 
    push ' '; split()
    unaryOp2 (fun (arr : string[]) -> 
        for word in arr do push <| word.ToLower())

let removeStopWords () =
    push stopWords; readFile()
    push ','; split()

    unaryOp2 (fun (arr : string[]) -> 
        arr |> Set.ofArray |> store "stop_words")                
    store "words" List.empty<string>

    while len() > 0 do
        load<Set<string>> "stop_words"
        binOp (fun (stopWords : Set<string>) word -> 
            if stopWords.Contains word |> not 
               && word.Length >= 2
            then Some word
            else None)

        load<string list> "words"
        binOp (fun (words : string list) word ->
            match word with
            | Some word -> word::words
            | _ -> words)

        unaryOp2 (store "words")

    load<string list> "words"
    unaryOp2 (fun (words : string list) -> 
        words |> List.iter push)

type WordFreqs = Dictionary<string, int>
let frequencies () =
    store "word_freqs" <| new WordFreqs()

    while len() > 0 do
        load "word_freqs"
        binOp (fun (wordFreqs : WordFreqs) word ->
            if wordFreqs.ContainsKey word
            then word, wordFreqs.[word] + 1
            else word, 1)

        load "word_freqs"
        binOp (fun (wordFreqs : WordFreqs) (word, newCount) -> 
            wordFreqs.[word] <- newCount
            wordFreqs)

        unaryOp2 (store "word_freqs")

type KVP = KeyValuePair<string, int>
let sort () =
    load "word_freqs"
    unaryOp (fun (wordFreqs : WordFreqs) ->
        wordFreqs.OrderByDescending(fun kvp -> kvp.Value))

    unaryOp (fun (wordFreqs : KVP seq) ->
        wordFreqs.Take(25))
        
    unaryOp (fun (wordFreqs : KVP seq) ->
        wordFreqs.Reverse())

    unaryOp2 (fun (wordFreqs : KVP seq) ->
        wordFreqs |> Seq.iter push)

push ``pride and prejudice``

readFile()
replaceNonAlphanumeric()
scan()
removeStopWords()
frequencies()
sort()

while len() > 0 do
    unaryOp2 (fun (kvp : KVP) -> 
        printfn "%s - %d" kvp.Key kvp.Value)