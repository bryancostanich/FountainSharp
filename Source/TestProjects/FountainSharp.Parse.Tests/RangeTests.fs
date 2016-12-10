module FountainSharp.Parse.Tests.Range

open System
open FsUnit
open NUnit.Framework
open FountainSharp

//===== Range

[<Test>]
let ``Range - empty`` () =
   let range = Range.empty
   range |> should equal (new Range(0, 0))

[<Test>]
let ``Range - offset`` () =
   let range = new Range(10, 5)
   range.Offset(8)
   range |> should equal (new Range(18, 5))

[<Test>]
let ``Range - static offset`` () =
   let range = new Range(10, 5)
   Range.Offset(range, 8) |> should equal (new Range(18, 5))

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
let ``Range - contains range`` () =
   let range = new Range(10, 5)
   range.Contains(new Range(10, 2)) |> should be True

[<Test>]
let ``Range - intersection`` () =
   let range = new Range(10, 5)
   range.HasIntersectionWith(new Range(8, 3)) |> should be True

[<Test>]
let ``Range - intersection containing`` () =
   let range = new Range(10, 5)
   range.HasIntersectionWith(new Range(8, 20)) |> should be True

[<Test>]
let ``Range - no intersection`` () =
   let range = new Range(10, 5)
   range.HasIntersectionWith(new Range(8, 1)) |> should be False

[<Test>]
let ``Range - end`` () =
   let range = new Range(10, 5)
   range.EndLocation |> should equal 14
