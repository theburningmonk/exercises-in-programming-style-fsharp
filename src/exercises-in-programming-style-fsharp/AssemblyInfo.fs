namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("exercises-in-programming-style-fsharp")>]
[<assembly: AssemblyProductAttribute("exercises-in-programming-style-fsharp")>]
[<assembly: AssemblyDescriptionAttribute("F# port of the examples in "Exercises in Programming Style"")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
