open System
open System.IO
open System.Text.RegularExpressions

#time
let ``pride and prejudice`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"
let test = __SOURCE_DIRECTORY__ + "../test.txt"
let stopWords = __SOURCE_DIRECTORY__ + "../stop_words.txt"

let mutable stack : obj list = []
let mutable heap  = Map.empty<string, obj>

let pop<'a> () =
    let hd::tl = stack
    stack <- tl
    hd :?> 'a

let push x = stack <- x::stack

let store name value = heap <- heap.Add(name, value)
let fetch<'a> name = heap.[name] :?> 'a

let unaryOp (f : 'a -> 'b) = pop<'a>() |> f
let binOp (f : 'a -> 'b -> 'c) = f (pop<'a>()) (pop<'b>())
let ternaryOp (f : 'a -> 'b -> 'c -> 'd ) = 
    f (pop<'a>()) (pop<'b>()) (pop<'c>())

let readFile () = unaryOp File.ReadAllText |> push

let split (separator : char) =
    unaryOp (fun (str : string) -> 
        str.Split([| separator |], StringSplitOptions.RemoveEmptyEntries))

let replaceNonAlphanumeric () =
    let regex pattern = new Regex(pattern)

    let replace (regex : Regex) (replacement : string) input = 
        regex.Replace(input, replacement)

    push " "                  // used as replacement string later
    push "[\W_]+"             // used for Regex creation below
    unaryOp regex |> push     // push new Regex to head of stack
    ternaryOp replace |> push // consume the Regex, replacement and input string
    
/// Takes a string on the stack, scan for words, and place the
/// words back on the stack
let scan () = split ' ' |> Array.iter push

let removeStopWords () =
    push stopWords
    readFile()
    pop<string>().Split(',')

push test
readFile(); replaceNonAlphanumeric(); scan()
