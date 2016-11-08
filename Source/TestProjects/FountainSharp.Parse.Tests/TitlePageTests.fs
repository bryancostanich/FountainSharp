module FountainSharp.Parse.Tests.TitlePageTests

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Title page

[<Test>]
let ``Title page`` () =
   // This is quite a complex title page with inline and not inline values, emphasized spans.
   let text = properNewLines "Title:\r\n\t_**BRICK and STEEL**_\r\n\t_**FULL RETIRED**_\r\nCredit: Written by\r\n\r\nSome action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [ TitlePage ([(("Title", new Range(0, 6)), [ HardLineBreak(new Range(6, NewLineLength)); Literal("\t", new Range(6 + NewLineLength, 1)); Underline ([Bold ([Literal("BRICK and STEEL", new Range(10 + NewLineLength, 15))], new Range(8 + NewLineLength, 19))], new Range(7 + NewLineLength, 21)); HardLineBreak(new Range(28 + NewLineLength, NewLineLength)); Literal("\t", new Range(28 + NewLineLength * 2, 1)); Underline([ Bold ([Literal("FULL RETIRED", new Range(32 + NewLineLength * 2, 12))], new Range(30 + NewLineLength * 2, 16))], new Range(29 + NewLineLength * 2, 18)); HardLineBreak(new Range(47 + NewLineLength * 2, NewLineLength))]); (("Credit", new Range(47 + NewLineLength * 3, 7)), [ Literal(" Written by", new Range(54 + NewLineLength * 3, 11)); HardLineBreak(new Range(65 + NewLineLength * 3, NewLineLength)) ]) ], new Range(0, text.Length - 11)); Action(false, [ Literal("Some action", new Range(text.Length - 11, 11)) ], new Range(text.Length - 11, 11)) ]

[<Test>]
let ``Title page - Followed by Scene Heading`` () =
   // This is quite a complex title page with inline and not inline values, emphasized spans.
   let text = properNewLines "Title:\r\n\t_**BRICK and STEEL**_\r\n\t_**FULL RETIRED**_\r\nCredit: Written by\r\n\r\nEXT. BRICK'S PATIO - DAY\r\n"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [ TitlePage ([(("Title", new Range(0, 6)), [ HardLineBreak(new Range(6, NewLineLength)); Literal("\t", new Range(6 + NewLineLength, 1)); Underline ([Bold ([Literal("BRICK and STEEL", new Range(10 + NewLineLength, 15))], new Range(8 + NewLineLength, 19))], new Range(7 + NewLineLength, 21)); HardLineBreak(new Range(28 + NewLineLength, NewLineLength)); Literal("\t", new Range(28 + NewLineLength * 2, 1)); Underline([ Bold ([Literal("FULL RETIRED", new Range(32 + NewLineLength * 2, 12))], new Range(30 + NewLineLength * 2, 16))], new Range(29 + NewLineLength * 2, 18)); HardLineBreak(new Range(47 + NewLineLength * 2, NewLineLength))]); (("Credit", new Range(47 + NewLineLength * 3, 7)), [ Literal(" Written by", new Range(54 + NewLineLength * 3, 11)); HardLineBreak(new Range(65 + NewLineLength * 3, NewLineLength)) ]) ], new Range(0, text.Length - 24 - NewLineLength)); SceneHeading(false, [Literal("EXT. BRICK'S PATIO - DAY", new Range(text.Length - 24 - NewLineLength, 24))], new Range(text.Length - 24 - NewLineLength, 24 + NewLineLength * 2))]
