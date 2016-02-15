namespace Style19

type IPlugin = interface end

type IWords = 
    inherit IPlugin

    abstract member ExtractWords : inputFile:string * stopWordsFile:string -> string[]

type IFrequencies = 
    inherit IPlugin

    abstract member Top25 : words:string[] -> (string * int)[]