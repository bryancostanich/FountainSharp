module FountainSharp.Parse.Tests.Section

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Section

[<Test>]
let ``Section - simple without space`` () =
   let doc = "#Section" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Section (1, [Literal ("Section", new Range(1, 7))], new Range(0, 8)) ]

[<Test>]
let ``Section - simple`` () =
   let doc = "# Section" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Section (1, [Literal ("Section", new Range(2, 7))], new Range(0, 9)) ]

[<Test>]
let ``Section - with new line`` () =
   let doc = properNewLines "# Section\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Section (1, [Literal ("Section", new Range(2, 7))], new Range(0, 9 + NewLineLength)); Action(false, [ HardLineBreak(new Range(9 + NewLineLength, NewLineLength)) ], new Range(9 + NewLineLength, NewLineLength)) ]

[<Test>]
let ``Section - nested`` () =
   let doc = "### Section" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Section (3, [Literal ("Section", new Range(4, 7))], new Range(0, 11)) ]
