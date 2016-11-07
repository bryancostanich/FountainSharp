module FountainSharp.Parse.Tests.Emphasis

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Emphasis

[<Test>]
let ``Emphasis - Bold`` () =
   let text = "**This is bold Text**"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Bold ([Literal ("This is bold Text", new Range(2, text.Length - 4))], new Range(0, text.Length))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - Italics`` () =
   let text = "*This is italic Text*"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Italic ([Literal ("This is italic Text", new Range(1, text.Length - 2))], new Range(0, text.Length))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - Bold Italic`` () =
   let text = "***This is bold Text***"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Bold ([Italic ([Literal ("This is bold Text", new Range(3, 17))], new Range(2, text.Length - 4))], new Range(0, text.Length))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - Nested Bold Italic`` () =
 let text = "**This is bold *and Italic Text***"
 let doc = text |> Fountain.Parse
 doc.Blocks
   |> should equal [Action (false, [Bold ([Literal ("This is bold ", new Range(2, 13)); Italic ([Literal ("and Italic Text", new Range(16 ,15)) ], new Range(15, 17)) ], new Range(0, text.Length))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - Nested Underline Italic`` () =
   let text = "From what seems like only INCHES AWAY.  _Steel's face FILLS the *Leupold Mark 4* scope_."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("From what seems like only INCHES AWAY.  ", new Range(0, 40)); Underline ([Literal ("Steel's face FILLS the ", new Range(41, 23)); Italic ([Literal ("Leupold Mark 4", new Range(65, 14))], new Range(64, 16)); Literal (" scope", new Range(80, 6))], new Range(40,47)); Literal (".", new Range(87, 1))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - with escapes`` () =
   let text = "Steel enters the code on the keypad: **\*9765\***"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("Steel enters the code on the keypad: ", new Range(0, 37)); Bold ([Literal("*9765*", new Range(39, 8))], new Range(37, 12))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - italics with spaces to left`` () =
   // this is not italic, as there is a space on the left of the second one
   let text = "He dialed *69 and then *23, and then hung up."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("He dialed *69 and then *23, and then hung up.", new Range(0, text.Length))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - italics with spaces to left but escaped`` () =
   let text = "He dialed *69 and then 23\*, and then hung up.."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal( "He dialed *69 and then 23*, and then hung up..", new Range(0, text.Length))], new Range(0, text.Length))]

[<Test>]
let ``Emphasis - between line breaks`` () =
   let text = "As he rattles off the long list, Brick and Steel *share a look." + NewLine(2) + "This is going to be BAD.*"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal( "As he rattles off the long list, Brick and Steel *share a look.", new Range(0, 63)); HardLineBreak(new Range(63, NewLineLength)); HardLineBreak(new Range(63 + NewLineLength, NewLineLength)); Literal ("This is going to be BAD.*", new Range(text.Length - 25, 25))], new Range(0, text.Length))]
