module FountainSharp.Parse.Tests.PartialParsing

open System
open FsUnit
open NUnit.Framework
open FountainSharp
open FountainSharp.Parse
open FountainSharp.Parse.Helper
open ResourceUtils

//===== Partial parsing

[<Test>]
let ``Deleting when simple Action is present`` () =
   let doc = "Some action" |> FountainDocument.Parse
   doc.ReplaceText(0, 1, "")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("ome action", new Range(0, 10))], new Range(0, 10)) ]

[<Test>]
let ``Deleting when more blocks are present`` () =
   let doc = properNewLines "Some action\r\n\r\nINT DOGHOUSE - DAY\r\n" |> FountainDocument.Parse
   doc.ReplaceText(0, 1, "")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("ome action", new Range(0, 10)); HardLineBreak(new Range(10, NewLineLength))], new Range(0, 10 + NewLineLength)); SceneHeading(false, [ Literal("INT DOGHOUSE - DAY", new Range(10 + NewLineLength * 2, 18)) ], new Range(10 + NewLineLength, 18 + NewLineLength * 3)) ]

[<Test>]
let ``Inserting when empty`` () =
   let doc = "" |> FountainDocument.Parse
   doc.ReplaceText(0, 0, "Action")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("Action", new Range(0, 6))], new Range(0, 6)) ]

[<Test>]
let ``Replacing non empty`` () =
   let doc = "Ac" |> FountainDocument.Parse
   doc.ReplaceText(1, 1, "ction")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("Action", new Range(0, 6))], new Range(0, 6)) ]

[<Test>]
let ``Appending`` () =
   let doc = "Ac" |> FountainDocument.Parse
   doc.ReplaceText(2, 0, "tion")
   doc.Blocks
   |> should equal [ Action (false, [Literal ("Action", new Range(0, 6))], new Range(0, 6)) ]

[<Test>]
let ``Appending to Scene Heading`` () =
   let doc = properNewLines "\r\nEXT. BRICK'S PATIO - DAY" |> FountainDocument.Parse
   doc.ReplaceText(24 + NewLineLength, 0, properNewLines "\r\n")
   doc.Blocks 
   |> should equal [ SceneHeading(false, [ Literal("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24)) ], new Range(0, 24 + NewLineLength * 3)) ]

[<Test>]
let ``Appending Dialogue after Character`` () =
   let doc = properNewLines "\r\nLINDSEY\r\n" |> FountainDocument.Parse
   doc.ReplaceText(7 + NewLineLength * 2, 0, "Hello, friend.")
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Dialogue ([Literal ("Hello, friend.", new Range(7 + NewLineLength * 2, 14))], new Range(7 + NewLineLength * 2, 14))]

[<Test>]
let ``Dual Dialogue`` () =
   let doc = properNewLines "\r\nBRICK\r\nScrew retirement.\r\n" |> FountainDocument.Parse
   doc.AppendText(properNewLines "\r\nSTEEL ^\r\nScrew retirement.")
   doc.Blocks
   |> should equal [DualDialogue([Character (false, true, [Literal ("BRICK", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue ([Literal ("Screw retirement.", new Range(5 + NewLineLength * 2, 17))], new Range(5 + NewLineLength * 2, 17 + NewLineLength)); Character (false, false, [Literal ("STEEL", new Range(22 + NewLineLength * 4, 5))], new Range(22 + NewLineLength * 3, 7 + NewLineLength * 2)); Dialogue ([Literal ("Screw retirement.", new Range(29 + NewLineLength * 5, 17))], new Range(29 + NewLineLength * 5, 17))], new Range(0, 46 + NewLineLength * 5))]


[<Test>]
let ``#Bugfix - Appending new line to Character`` () =
   let doc = properNewLines "\r\nCUT TO:\r\n\r\nLINDSEYHello, friend." |> FountainDocument.Parse
   doc.ReplaceText(14 + NewLineLength * 3, 0, properNewLines "\r\n")
   doc.Blocks
   |> should equal [ Transition(false, [ Literal("CUT TO:", new Range(NewLineLength, 7)) ], new Range(0, 7 + NewLineLength * 3)); Character (false, true, [Literal ("LINDSEY", new Range(7 + NewLineLength * 3, 7))], new Range(7 + NewLineLength * 3, 7 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(14 + NewLineLength * 4, 14))], new Range(14 + NewLineLength * 4, 14))]

[<Test>]
let ``#Bugfix - Appending to Character after Transition`` () =
   let doc = properNewLines "\r\nCUT TO:\r\n\r\nL" |> FountainDocument.Parse
   doc.ReplaceText(8 + NewLineLength * 3, 0, "I")
   doc.Blocks
   |> should equal [ Transition(false, [ Literal("CUT TO:", new Range(NewLineLength, 7)) ], new Range(0, 7 + NewLineLength * 3)); Character (false, true, [Literal ("LI", new Range(7 + NewLineLength * 3, 2))], new Range(7 + NewLineLength * 3, 2)) ]

[<Test>]
let ``#Bugfix - Inserting into Character`` () =
   let doc = properNewLines "\r\nC\r\nSome action\r\n" |> FountainDocument.Parse
   doc.ReplaceText(1 + NewLineLength, 0, "H")
   doc.Blocks
   |> should equal [ Character(false, true, [ Literal("CH", new Range(NewLineLength, 2)) ], new Range(0, 2 + NewLineLength * 2)); Dialogue ( [Literal ("Some action", new Range(2 + NewLineLength * 2, 11)); HardLineBreak(new Range(13 + NewLineLength * 2, NewLineLength)) ], new Range(2 + NewLineLength * 2, 11 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Character transforms into Transition`` () =
   let doc = properNewLines "\r\nCUT TO\r\nSome action\r\n" |> FountainDocument.Parse
   doc.ReplaceText(6 + NewLineLength, 0, ":" + Environment.NewLine)
   doc.Blocks
   |> should equal [ Transition(false, [ Literal("CUT TO:", new Range(NewLineLength, 7)) ], new Range(0, 7 + NewLineLength * 3)); Action (false, [Literal ("Some action", new Range(7 + NewLineLength * 3, 11)); HardLineBreak(new Range(18 + NewLineLength * 3, NewLineLength)) ], new Range(7 + NewLineLength * 3, 11 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Starting dialogue`` () =
   let doc = properNewLines "\r\nSTEEL\r\n\r\n" |> FountainDocument.Parse
   doc.ReplaceText(5 + 2 * NewLineLength, 0, "T")
   doc.Blocks
   |> should equal [ Character(false, true, [Literal("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue([ Literal("T", new Range(5 + NewLineLength * 2, 1)); createHardLineBreak(6 + NewLineLength * 2); ], new Range(5 + NewLineLength * 2, 1 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Brick & Steel blocks disappear`` () =
    let script = readFromResource "Brick_and_Steel.fountain"
    let doc = script |> FountainDocument.Parse
    let numOfBlocks = doc.Blocks.Length
    numOfBlocks |> should greaterThan 80
    doc.ReplaceText(30, 0, "very") // insert some text into the first Action block
    // we must not lose blocks, however a lot of them disappeared
    doc.Blocks.Length |> should equal numOfBlocks
