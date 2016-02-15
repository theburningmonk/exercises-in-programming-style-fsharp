#time

#r "../Style19Main/bin/Style19Main.dll"
open Style19Main

let ``stop words`` = 
    __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = 
    __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

let pluginDir =
    __SOURCE_DIRECTORY__ + "/../Style19Plugins/bin"

// imagine this config had come from app.config or such
let config = 
    {
        PluginsDir  = pluginDir
        Words       = "Style19Plugins", "WordsPlugIn"
        Frequencies = "Style19Plugins", "FrequenciesPlugIn"
        Input       = ``p & p``
        StopWords   = ``stop words``
    }

runWith config