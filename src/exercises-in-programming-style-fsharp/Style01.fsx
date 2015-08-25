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
let stopwords = 
    File.ReadAllText(__SOURCE_DIRECTORY__ + "../stop_words.txt")
        .Split(',')

//let input = new StreamReader(``pride and prejudice``)
let input = new StreamReader(test)

let rec loop = function
    | null -> ()
    | line -> ()
loop <| input.ReadLine()