module FountainSharp.Parse.Tests.TitlePageTests

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper
open FountainSharp.Parse.Tests.Helper

//===== Title page

[<Test>]
let ``Title page`` () =
   // This is quite a complex title page with inline and not inline values, emphasized spans.
   let text = properNewLines "Title:\r\n\t_**BRICK and STEEL**_\r\n\t_**FULL RETIRED**_\r\nCredit: Written by\r\n\r\nSome action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [TitlePage ([("Title", [Underline ([Bold ([Literal("BRICK and STEEL", new Range(3, 15))], new Range(1, 19))], new Range(0, 21)); HardLineBreak(new Range(21, NewLineLength)); Underline([ Bold ([Literal("FULL RETIRED", new Range(24 + NewLineLength, 12))], new Range(22 + NewLineLength, 16))], new Range(21 + NewLineLength, 18))]); ("Credit", [Literal("Written by", new Range(0, 10))])], new Range(0, text.Length - 11)); Action(false, [Literal("Some action", new Range(text.Length - 11, 11))], new Range(text.Length - 11, 11))]

[<Test>]
let ``Title page - Followed by Scene Heading`` () =
   // This is quite a complex title page with inline and not inline values, emphasized spans.
   let text = properNewLines "Title:\r\n\t_**BRICK and STEEL**_\r\n\t_**FULL RETIRED**_\r\nCredit: Written by\r\n\r\nEXT. BRICK'S PATIO - DAY\r\n"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [TitlePage ([("Title", [Underline ([Bold ([Literal("BRICK and STEEL", new Range(3, 15))], new Range(1, 19))], new Range(0, 21)); HardLineBreak(new Range(21, NewLineLength)); Underline([ Bold ([Literal("FULL RETIRED", new Range(24 + NewLineLength, 12))], new Range(22 + NewLineLength, 16))], new Range(21 + NewLineLength, 18))]); ("Credit", [Literal("Written by", new Range(0, 10))])], new Range(0, text.Length - 24 - NewLineLength)); SceneHeading(false, [Literal("EXT. BRICK'S PATIO - DAY", new Range(text.Length - 24 - NewLineLength, 24))], new Range(text.Length - 24 - NewLineLength, 24 + NewLineLength * 2))]
