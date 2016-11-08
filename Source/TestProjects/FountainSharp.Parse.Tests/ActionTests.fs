module FountainSharp.Parse.Tests.Action

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper


//===== Action
[<Test>]
let ``Action - Simple`` () =
   let doc = properNewLines "Some Action\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Action (false, [ Literal ("Some Action", new Range(0, 11)); HardLineBreak(new Range(11, NewLineLength)) ], new Range(0, 11 + NewLineLength)) ]

[<Test>]
let ``Action - Forced`` () =
   let doc = "!Some Action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Action (true, [ Literal ("Some Action", new Range(1, 11)) ], new Range(0, 12)) ]

[<Test>]
let ``Action - With line breaks`` () =
   let action = properNewLines "Some Action\r\n\r\nSome More Action"
   let sceneHeading = properNewLines "\r\nEXT. BRICK'S PATIO - DAY\r\n\r\n"
   let doc = sceneHeading + action |> Fountain.Parse
   doc.Blocks
   |> should equal   [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24))], new Range(0, 24 + NewLineLength * 3)); Action (false, [Literal ("Some Action", new Range(24 + NewLineLength * 3, 11)); HardLineBreak(new Range(35 + NewLineLength * 3, NewLineLength)); HardLineBreak(new Range(35 + NewLineLength * 4, NewLineLength)); Literal ("Some More Action", new Range(35 + NewLineLength * 5, 16))], new Range(24 + NewLineLength * 3, action.Length))]

[<Test>]
let ``Action - With line breaks and no heading`` () =
   let text = "Natalie looks around at the group, TIM, ROGER, NATE, and VEEK." + Environment.NewLine + Environment.NewLine + "TIM, is smiling broadly."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal ("Natalie looks around at the group, TIM, ROGER, NATE, and VEEK.", new Range(0, 62)); HardLineBreak(new Range(62, NewLineLength)); HardLineBreak(new Range(62 + NewLineLength, NewLineLength)); Literal ("TIM, is smiling broadly.", new Range(62 + 2 * NewLineLength, 24))], new Range(0, text.Length))]

[<Test>]
let ``Action - Trailing whitespace`` () =
    let text = "Some action. "
    let doc = text |> Fountain.Parse
    doc.Blocks
    |> should equal [Action (false, [Literal(text, new Range(0, text.Length))], new Range(0, text.Length))]

[<Test>]
let ``Action - Trailing newline`` () =
    let text = "Some action. " + NewLine(2)
    let doc = text |> Fountain.Parse
    doc.Blocks
    |> should equal [Action (false, [Literal("Some action. ", new Range(0, 13)); HardLineBreak(new Range(13, NewLineLength)); HardLineBreak(new Range(13 + NewLineLength, NewLineLength))], new Range(0, 13 + NewLineLength * 2))]

