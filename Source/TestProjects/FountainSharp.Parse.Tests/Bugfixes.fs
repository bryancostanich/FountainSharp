module FountainSharp.Parse.Tests.Bugfixes

open FsUnit
open NUnit.Framework
open FountainSharp
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Recognized bugs

[<Test>]
let ``#Bugfix - Character after Action`` () =
   let doc = properNewLines "Some action\r\n\r\nSTEEL" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Action (false, [ Literal ("Some action", new Range(0, 11)); HardLineBreak(new Range(11, NewLineLength)) ], new Range(0, 11 + NewLineLength)); Character(false, true, [Literal("STEEL", new Range(11 + NewLineLength * 2, 5))], new Range(11 + NewLineLength, 5 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Scene Heading, Action, Character`` () =
   let doc = properNewLines "\r\nINT DOGHOUSE - DAY\r\n\r\nSome action\r\n\r\nSTEEL" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ SceneHeading(false, [ Literal("INT DOGHOUSE - DAY", new Range(NewLineLength, 18)) ], new Range(0, 18 + NewLineLength * 3)); Action (false, [ Literal ("Some action", new Range(18 + NewLineLength * 3, 11)); HardLineBreak(new Range(29 + NewLineLength * 3, NewLineLength)) ], new Range(18 + NewLineLength * 3, 11 + NewLineLength)); Character(false, true, [Literal("STEEL", new Range(29 + NewLineLength * 5, 5))], new Range(29 + NewLineLength * 4, 5 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Dialogue with trailing new lines`` () =
   let doc = properNewLines "\r\nSTEEL\r\nTo retirement.\r\n\r\n" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character(false, true, [Literal("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue([ Literal ("To retirement.", new Range(5 + NewLineLength * 2, 14)); ], new Range(5 + NewLineLength * 2, 14 + NewLineLength)); Action(false, [ HardLineBreak(new Range(19 + NewLineLength * 3, NewLineLength)) ], new Range(19 + NewLineLength * 3, NewLineLength)) ]

[<Test>]
let ``#Bugfix - Action after Dialogue`` () =
   let doc = properNewLines "\r\nSTEEL\r\nTo retirement.\r\n\r\nSome action" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character(false, true, [Literal("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue([ Literal ("To retirement.", new Range(5 + NewLineLength * 2, 14)); ], new Range(5 + NewLineLength * 2, 14 + NewLineLength)); Action(false, [ HardLineBreak(new Range(19 + NewLineLength * 3, NewLineLength)); Literal("Some action", new Range(19 + NewLineLength * 4, 11)) ], new Range(19 + NewLineLength * 3, 11 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Transition after Scene Heading`` () =
   let doc = properNewLines "\r\nINT DOGHOUSE - DAY\r\n\r\nCUT TO:\r\n" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ SceneHeading(false, [ Literal("INT DOGHOUSE - DAY", new Range(NewLineLength, 18)) ], new Range(0, 18 + NewLineLength * 3)); Transition(false, [ Literal("CUT TO:", new Range(18 + NewLineLength * 3, 7)) ], new Range(18 + NewLineLength * 3, 7 + NewLineLength * 2)) ]

[<Test>]
let ``#Bugfix - Character after Dialogue`` () =
   // Dialogue's range got incorrect if followed by a Character block
   let doc = properNewLines "\r\nSTEEL\r\nDoes a bear crap in the woods?\r\n\r\nBRICK" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character(false, true, [ Literal("STEEL", new Range(NewLineLength, 5)) ], new Range(0, 5 + NewLineLength * 2)); Dialogue( [ Literal("Does a bear crap in the woods?", new Range(5 + NewLineLength * 2, 30)) ], new Range(5 + NewLineLength * 2, 30 + NewLineLength)); Character(false, true, [ Literal("BRICK", new Range(35 + NewLineLength * 4, 5)) ], new Range(35 + NewLineLength * 3, 5 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Centered recognized as Transition`` () =
   let doc = properNewLines "\r\n> Brick & Steel <\r\n\r\nSome action" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Action(false, [ HardLineBreak(new Range(0, NewLineLength)) ], new Range(0, NewLineLength)); Centered ([Literal ("Brick & Steel", new Range(2 + NewLineLength, 13))], new Range(NewLineLength, 17 + NewLineLength)); Action(false, [ HardLineBreak(new Range(17 + NewLineLength * 2, NewLineLength)); Literal("Some action", new Range(17 + NewLineLength * 3, 11)) ], new Range(17 + NewLineLength * 2, 11 + NewLineLength))]

[<Test>]
let ``#Bugfix - Title recognized as Action without empty line`` () =
   let text = properNewLines "Title:\r\n\t_**BRICK & STEEL**_\r\n\t_**FULL RETIRED**_\r\nCredit: Written by\r\nAuthor: Stu Maschwitz\r\nSource: Story by KTM\r\nDraft date: 1/27/2012\r\nContact:\r\n\tNext Level Productions\r\n\t1588 Mission Dr.\r\n\tSolvang, CA 93463\r\n"
   let doc = text |> FountainDocument.Parse
   // 1 title page block has to be recognized
   // contents of the block itself can be checked also, but similar is checked in TitlePageTests.fs
   doc.Blocks.Length |> should equal 1
   match doc.Blocks.Head with
   | FountainSharp.TitlePage(_, _) -> ()
   | _ -> fail()
//   doc.Blocks
//   |> should equal [TitlePage ([("Title", [Underline ([Bold ([Literal("BRICK and STEEL", new Range(3, 15))], new Range(1, 19))], new Range(0, 21)); HardLineBreak(new Range(21, NewLineLength)); Underline([ Bold ([Literal("FULL RETIRED", new Range(24 + NewLineLength, 12))], new Range(22 + NewLineLength, 16))], new Range(21 + NewLineLength, 18))]); ("Credit", [Literal("Written by", new Range(0, 10))])], new Range(0, text.Length - 24 - NewLineLength)); SceneHeading(false, [Literal("EXT. BRICK'S PATIO - DAY", new Range(text.Length - 24 - NewLineLength, 24))], new Range(text.Length - 24 - NewLineLength, 24 + NewLineLength * 2))]

[<Test>]
let ``#Bugfix - Scene Heading at the end`` () =
   let doc = properNewLines "\r\nEXT. BRICK'S PATIO - DAY\r\n" |> FountainDocument.Parse
   doc.Blocks
   |> should equal  [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24))], new Range(0, 24 + NewLineLength * 2)) ]
