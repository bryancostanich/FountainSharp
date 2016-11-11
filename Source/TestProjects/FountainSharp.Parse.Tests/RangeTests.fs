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

[<Test>]
let ``Range - contains`` () =
   let range = new Range(10, 5)
   range.Contains(8) |> should be False

[<Test>]
let ``Range - contains start`` () =
   let range = new Range(10, 5)
   range.Contains(10) |> should be True

[<Test>]
let ``Range - contains end`` () =
   let range = new Range(10, 5)
   range.Contains(14) |> should be True

[<Test>]
let ``Range - intersection`` () =
   let range = new Range(10, 5)
   range.HasIntersectionWith(new Range(8, 3)) |> should be True

[<Test>]
let ``Range - no intersection`` () =
   let range = new Range(10, 5)
   range.HasIntersectionWith(new Range(8, 1)) |> should be False
