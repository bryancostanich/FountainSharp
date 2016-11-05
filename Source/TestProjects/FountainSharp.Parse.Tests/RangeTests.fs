module FountainSharp.Parse.Tests.Range

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Range

[<Test>]
let ``Range - empty`` () =
   let range = Range.empty
   range |> should equal (new Range(0, 0))

[<Test>]
let ``Range - offset`` () =
   let range = new Range(10, 5)
   range.Offset(8) |> should equal (new Range(18, 5))
