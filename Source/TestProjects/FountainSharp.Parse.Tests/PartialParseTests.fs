module FountainSharp.Parse.Tests.PartialParsing

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Partial parsing

[<Test>]
let ``Deleting when simple Action is present`` () =
   let doc = "Some action" |> Fountain.Parse
   doc.ReplaceText(0, 1, "")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("ome action", new Range(0, 10))], new Range(0, 10)) ]

[<Test>]
let ``Deleting when more blocks are present`` () =
   let doc = properNewLines "Some action\r\n\r\nINT DOGHOUSE - DAY\r\n" |> Fountain.Parse
   doc.ReplaceText(0, 1, "")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("ome action", new Range(0, 10)); HardLineBreak(new Range(10, NewLineLength))], new Range(0, 10 + NewLineLength)); SceneHeading(false, [ Literal("INT DOGHOUSE - DAY", new Range(10 + NewLineLength * 2, 18)) ], new Range(10 + NewLineLength, 18 + NewLineLength * 3)) ]

[<Test>]
let ``Inserting when empty`` () =
   let doc = "" |> Fountain.Parse
   doc.ReplaceText(0, 0, "Action")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("Action", new Range(0, 6))], new Range(0, 6)) ]

[<Test>]
let ``Replacing non empty`` () =
   let doc = "Ac" |> Fountain.Parse
   doc.ReplaceText(1, 1, "ction")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("Action", new Range(0, 6))], new Range(0, 6)) ]

[<Test>]
let ``Appending`` () =
   let doc = "Ac" |> Fountain.Parse
   doc.ReplaceText(2, 0, "tion")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("Action", new Range(0, 6))], new Range(0, 6)) ]

[<Test>]
let ``Appending to Scene Heading`` () =
   let doc = properNewLines "\r\nEXT. BRICK'S PATIO - DAY" |> Fountain.Parse
   doc.ReplaceText(24 + NewLineLength, 0, properNewLines "\r\n")
   doc.Blocks 
   |> should equal [ SceneHeading(false, [ Literal("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24)) ], new Range(0, 24 + NewLineLength * 3)) ]

[<Test>]
let ``Appending Dialogue after Character`` () =
   let doc = properNewLines "\r\nLINDSEY\r\n" |> Fountain.Parse
   doc.ReplaceText(7 + NewLineLength * 2, 0, "Hello, friend.")
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Dialogue ([Literal ("Hello, friend.", new Range(7 + NewLineLength * 2, 14))], new Range(7 + NewLineLength * 2, 14))]

[<Test>]
let ``#Bugfix - Appending new line to Character`` () =
   let doc = properNewLines "\r\nCUT TO:\r\n\r\nLINDSEYHello, friend." |> Fountain.Parse
   doc.ReplaceText(14 + NewLineLength * 3, 0, properNewLines "\r\n")
   doc.Blocks
   |> should equal [ Transition(false, [ Literal("CUT TO:", new Range(NewLineLength, 7)) ], new Range(0, 7 + NewLineLength * 3)); Character (false, true, [Literal ("LINDSEY", new Range(7 + NewLineLength * 3, 7))], new Range(7 + NewLineLength * 3, 7 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(14 + NewLineLength * 4, 14))], new Range(14 + NewLineLength * 4, 14))]

[<Test>]
let ``#Bugfix - Appending to Character after Transition`` () =
   let doc = properNewLines "\r\nCUT TO:\r\n\r\nL" |> Fountain.Parse
   doc.ReplaceText(8 + NewLineLength * 3, 0, "I")
   doc.Blocks
   |> should equal [ Transition(false, [ Literal("CUT TO:", new Range(NewLineLength, 7)) ], new Range(0, 7 + NewLineLength * 3)); Character (false, true, [Literal ("LI", new Range(7 + NewLineLength * 3, 2))], new Range(7 + NewLineLength * 3, 2)) ]
