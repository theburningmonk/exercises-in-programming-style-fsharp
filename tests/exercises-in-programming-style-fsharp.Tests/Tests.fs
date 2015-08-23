module exercises-in-programming-style-fsharp.Tests

open exercises-in-programming-style-fsharp
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = Library.hello 42
  printfn "%i" result
  Assert.AreEqual(42,result)
