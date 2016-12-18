module FountainSharp.Parse.Tests.Character

open System
open FsUnit
open NUnit.Framework
open FountainSharp
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Character

[<Test>]
let ``Character - Normal`` () =
   let doc = properNewLines "\r\nLINDSEY" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength))]

[<Test>]
let ``Character - With parenthetical extension`` () =
   let text = properNewLines "\r\nLINDSEY (on the radio)"
   let doc = text |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY (on the radio)", new Range(NewLineLength, 22))], new Range(0, 22 + NewLineLength))]

[<Test>]
let ``Character - With invalid parenthetical extension`` () =
   let character = "LINDSEY (on the Radio)"
   let text = Environment.NewLine + character
   let doc = text |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Action (false, [HardLineBreak(new Range(0, NewLineLength)); Literal (character, new Range(NewLineLength, character.Length))], new Range(0, text.Length))]

[<Test>]
let ``Character - With whitespace``() = 
    let character = "THIS IS ALL UPPERCASE BUT HAS WHITESPACE"
    let doc = properNewLines "\r\n" + character |> FountainDocument.Parse
    doc.Blocks 
    |> should equal 
           [ Character(false, true, [ Literal(character, new Range(NewLineLength, character.Length)) ], new Range(0, character.Length + NewLineLength)) ]
    
[<Test>]
let ``Character - With Numbers`` () =
   let doc = properNewLines "\r\nR2D2" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("R2D2", new Range(NewLineLength, 4))], new Range(0, 4 + NewLineLength))]

[<Test>]
let ``Character - Number first`` () =
   let doc = properNewLines "\r\n25D2" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Action (false, [HardLineBreak(new Range(0, NewLineLength)); Literal ("25D2", new Range(NewLineLength, 4))], new Range(0, 4 + NewLineLength))]

[<Test>]
let ``Character - Forced with at sign`` () =
   let doc = properNewLines "\r\n@McAvoy" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character (true, true, [Literal ("McAvoy", new Range(NewLineLength + 1, 6))], new Range(0, 7 + NewLineLength))]

[<Test>]
let ``Character - With forced at and parenthetical extension`` () =
   let doc = properNewLines "\r\n@McAvoy (OS)" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character (true, true, [Literal ("McAvoy (OS)", new Range(NewLineLength + 1, 11))], new Range(0, 12 + NewLineLength))]

[<Test>]
let ``Character - Whitespace after`` () =
   let doc = properNewLines "\r\nLINDSEY " |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY ", new Range(NewLineLength, 8))], new Range(0, 8 + NewLineLength))]
