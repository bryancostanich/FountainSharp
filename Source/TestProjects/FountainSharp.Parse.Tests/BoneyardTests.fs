module FountainSharp.Parse.Tests.Boneyard

open System
open FsUnit
open NUnit.Framework
open FountainSharp
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Boneyard

// TODO: should we support boneyards in a single line? Also in the middle of the line?
[<Test>]
let ``Boneyard - simple`` () =
   let text = properNewLines "/*\r\nThis is a simple comment\r\n*/"
   let doc = text |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Boneyard ("This is a simple comment", new Range(0, text.Length))]

[<Test>]
let ``Boneyard - After Action`` () =
   let doc = properNewLines "Some action\r\n/*\r\nThis is a simple comment\r\n*/" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Action(false, [ Literal("Some action", new Range(0, 11)); HardLineBreak(new Range(11, NewLineLength)) ], new Range(0, 11 + NewLineLength)); Boneyard ("This is a simple comment", new Range(11 + NewLineLength, 28 + NewLineLength * 2))]

