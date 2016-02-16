module Style19Main

open System.IO
open System.Reflection

open Style19

type AssemblyName = string
type TypeName     = string

type Config =
    {
        PluginsDir  : string
        Words       : AssemblyName * TypeName
        Frequencies : AssemblyName * TypeName
        Input       : string
        StopWords   : string
    }

let private loadPlugins { PluginsDir = dir } =
    let baseType = typeof<IPlugin>

    Directory.GetFiles(dir, "*.dll")
    |> Array.map Assembly.LoadFrom
    |> Array.collect (fun a ->
        a.GetExportedTypes()
        |> Array.filter (fun t ->
            not t.IsAbstract && 
            baseType.IsAssignableFrom t)
        |> Array.map (fun t -> 
            let ctor = t.GetConstructor([||])
            let plugin = ctor.Invoke([||]) :?> IPlugin
            (a.GetName().Name, t.Name), plugin))
    |> Map.ofSeq

let runWith config =
    let plugins = loadPlugins config
    let words = plugins.[config.Words] :?> IWords
    let freqs = plugins.[config.Frequencies] :?> IFrequencies

    words.ExtractWords(config.Input, config.StopWords)
    |> freqs.Top25
    |> Array.iter (fun (word, len) -> 
        printfn "%s - %d" word len)