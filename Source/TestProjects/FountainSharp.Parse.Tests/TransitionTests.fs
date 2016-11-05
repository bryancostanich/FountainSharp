module FountainSharp.Parse.Tests.Transition

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Transition

[<Test>]
let ``Transition - normal`` () =
   let doc = properNewLines "\r\nCUT TO:\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (false, [Literal ("CUT TO:", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(7 + NewLineLength * 3, 11))], new Range(7 + NewLineLength * 3, 11))]

[<Test>]
let ``Transition - Non uppercase`` () =
   // This is not a transition as 'Cut' is not all uppercase
   let doc = properNewLines "\r\nCut TO:\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Action (false, [HardLineBreak(new Range(0, NewLineLength)); Literal("Cut TO:", new Range(NewLineLength, 7)); HardLineBreak(new Range(7 + NewLineLength, NewLineLength)); HardLineBreak(new Range(7 + NewLineLength * 2, NewLineLength)); Literal("Some action", new Range(7 + NewLineLength * 3, 11))], new Range(0, 18 + NewLineLength * 3))]

[<Test>]
let ``Transition - forced`` () =
   let doc = properNewLines "\r\n> Burn to White.\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (true, [Literal ("Burn to White.", new Range(2 + NewLineLength, 14))], new Range(0, 16 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(16 + NewLineLength * 3, 11))], new Range(16 + NewLineLength * 3, 11)) ]

[<Test>]
let ``Transition - indenting`` () =
   let doc = properNewLines "\r\n  \t  CUT TO:\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (false, [Literal ("CUT TO:", new Range(NewLineLength + 5, 7))], new Range(0, 12 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(12 + NewLineLength * 3, 11))], new Range(12 + NewLineLength * 3, 11))]

[<Test>]
let ``Transition - forced indenting`` () =
   let doc = properNewLines "\r\n\t > Burn to White.\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (true, [Literal ("Burn to White.", new Range(NewLineLength + 4, 14))], new Range(0, 18 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(18 + NewLineLength * 3, 11))], new Range(18 + NewLineLength * 3, 11))]

