module FountainSharp.Parse.Tests.Document

open System
open FsUnit
open NUnit.Framework
open FountainSharp
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== FountainDocument tests

[<Test>]
let ``FountainDocument.GetText - Scene heading length`` () =
   let text = properNewLines "\r\n.BRICK'S PATIO - DAY\r\n\r\nSome Action"
   let doc = text |> FountainDocument.Parse
   doc.Blocks
   |> should equal  [ SceneHeading (true, [Literal ("BRICK'S PATIO - DAY", new Range(1 + NewLineLength, 19))], new Range(0, 20 + NewLineLength * 3)); Action (false, [Literal ("Some Action", new Range(20 + NewLineLength * 3, 11))], new Range(20 + NewLineLength * 3, 11))]
   let sceneHeadingRange = new Range(0, 20 + NewLineLength * 3)
   let sceneHeadingText = doc.GetText(sceneHeadingRange)
   sceneHeadingText.Length |> should equal sceneHeadingRange.Length
