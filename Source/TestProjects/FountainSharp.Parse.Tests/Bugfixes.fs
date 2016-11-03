module FountainSharp.Parse.Tests.Bugfixes

open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper
open FountainSharp.Parse.Tests.Helper

//===== Recognized bugs

[<Test>]
let ``#Bugfix - Character after Action`` () =
   let doc = properNewLines "Some action\r\n\r\nSTEEL" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Action (false, [ Literal ("Some action", new Range(0, 11)); HardLineBreak(new Range(11, NewLineLength)) ], new Range(0, 11 + NewLineLength)); Character(false, true, [Literal("STEEL", new Range(11 + NewLineLength * 2, 5))], new Range(11 + NewLineLength, 5 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Scene Heading, Action, Character`` () =
   let doc = properNewLines "\r\nINT DOGHOUSE - DAY\r\n\r\nSome action\r\n\r\nSTEEL" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading(false, [ Literal("INT DOGHOUSE - DAY", new Range(NewLineLength, 18)) ], new Range(0, 18 + NewLineLength * 3)); Action (false, [ Literal ("Some action", new Range(18 + NewLineLength * 3, 11)); HardLineBreak(new Range(29 + NewLineLength * 3, NewLineLength)) ], new Range(18 + NewLineLength * 3, 11 + NewLineLength)); Character(false, true, [Literal("STEEL", new Range(29 + NewLineLength * 5, 5))], new Range(29 + NewLineLength * 4, 5 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Dialogue with trailing new lines`` () =
   let doc = properNewLines "\r\nSTEEL\r\nTo retirement.\r\n\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character(false, true, [Literal("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue([ Literal ("To retirement.", new Range(5 + NewLineLength * 2, 14)); ], new Range(5 + NewLineLength * 2, 14 + NewLineLength)); Action(false, [ HardLineBreak(new Range(19 + NewLineLength * 3, NewLineLength)) ], new Range(19 + NewLineLength * 3, NewLineLength)) ]

[<Test>]
let ``#Bugfix - Action after Dialogue`` () =
   let doc = properNewLines "\r\nSTEEL\r\nTo retirement.\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character(false, true, [Literal("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue([ Literal ("To retirement.", new Range(5 + NewLineLength * 2, 14)); ], new Range(5 + NewLineLength * 2, 14 + NewLineLength)); Action(false, [ HardLineBreak(new Range(19 + NewLineLength * 3, NewLineLength)); Literal("Some action", new Range(19 + NewLineLength * 4, 11)) ], new Range(19 + NewLineLength * 3, 11 + NewLineLength)) ]

[<Test>]
let ``#Bugfix - Transition after Scene Heading`` () =
   let doc = properNewLines "\r\nINT DOGHOUSE - DAY\r\n\r\nCUT TO:\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading(false, [ Literal("INT DOGHOUSE - DAY", new Range(NewLineLength, 18)) ], new Range(0, 18 + NewLineLength * 3)); Transition(false, [ Literal("CUT TO:", new Range(18 + NewLineLength * 3, 7)) ], new Range(18 + NewLineLength * 3, 7 + NewLineLength * 2)) ]
