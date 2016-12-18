module FountainSharp.Parse.Tests.Dialogue

open System
open FsUnit
open NUnit.Framework
open FountainSharp
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Dialogue

[<Test>]
let ``Dialogue - Normal`` () =
   let doc = properNewLines "\r\nLINDSEY\r\nHello, friend." |> FountainDocument.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Dialogue ([Literal ("Hello, friend.", new Range(7 + NewLineLength * 2, 14))], new Range(7 + NewLineLength * 2, 14))]

[<Test>]
let ``Dialogue - After Parenthetical`` () =
   let doc = properNewLines "\r\nLINDSEY\r\n(quietly)\r\nHello, friend." |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Parenthetical ([Literal ("quietly", new Range(8 + NewLineLength * 2, 7))], new Range(7 + NewLineLength * 2, 9 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(16 + NewLineLength * 3, 14))], new Range(16 + NewLineLength * 3, 14))]

[<Test>]
let ``Dialogue - With line break`` () =
   let doc = properNewLines "\r\nDEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n  \r\nHit or stand sir?" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("DEALER", new Range(NewLineLength, 6))], new Range(0, 6 + NewLineLength * 2));  Dialogue ([Literal ("Ten.", new Range(6 + NewLineLength * 2, 4)); HardLineBreak(new Range(10 + NewLineLength * 2, NewLineLength)); Literal ("Four.", new Range(10 + NewLineLength * 3, 5)); HardLineBreak(new Range(15 + NewLineLength * 3, NewLineLength)); Literal ("Dealer gets a seven.", new Range(15 + NewLineLength * 4, 20)); HardLineBreak(new Range(35 + NewLineLength * 4, NewLineLength)); HardLineBreak(new Range(35 + NewLineLength * 5, 2 + NewLineLength)); Literal ("Hit or stand sir?", new Range(37 + NewLineLength * 6, 17))], new Range(6 + NewLineLength * 2, 48 + NewLineLength * 4))]

[<Test>]
let ``Dialogue - With invalid line break`` () =
   let doc = properNewLines "\r\nDEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n\r\nHit or stand sir?" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("DEALER", new Range(NewLineLength, 6))], new Range(0, 6 + NewLineLength * 2));  Dialogue ([Literal ("Ten.", new Range(6 + NewLineLength * 2, 4)); HardLineBreak(new Range(10 + NewLineLength * 2, NewLineLength)); Literal ("Four.", new Range(10 + NewLineLength * 3, 5)); HardLineBreak(new Range(15 + NewLineLength * 3, NewLineLength)); Literal ("Dealer gets a seven.", new Range(15 + NewLineLength * 4, 20))], new Range(6 + NewLineLength * 2, 29 + NewLineLength * 3)); Action (false, [ HardLineBreak(new Range(35 + NewLineLength * 5, NewLineLength)); Literal ("Hit or stand sir?", new Range(35 + NewLineLength * 6, 17))], new Range(35 + NewLineLength * 5, 17 + NewLineLength))]
   // this test now fails: parsing places line break be after last Action

[<Test>]
let ``Dual Dialogue`` () =
   let doc = properNewLines "\r\nBRICK\r\nScrew retirement.\r\n\r\nSTEEL ^\r\nScrew retirement." |> FountainDocument.Parse
   doc.Blocks
   |> should equal [DualDialogue([Character (false, true, [Literal ("BRICK", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue ([Literal ("Screw retirement.", new Range(5 + NewLineLength * 2, 17))], new Range(5 + NewLineLength * 2, 17 + NewLineLength)); Character (false, false, [Literal ("STEEL", new Range(22 + NewLineLength * 4, 5))], new Range(22 + NewLineLength * 3, 7 + NewLineLength * 2)); Dialogue ([Literal ("Screw retirement.", new Range(29 + NewLineLength * 5, 17))], new Range(29 + NewLineLength * 5, 17))], new Range(0, 46 + NewLineLength * 5))]

[<Test>]
let ``Dual Dialogue - Second character`` () =
   let doc = properNewLines "\r\nLINDSEY     ^\r\nHello, friend." |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Character (false, false, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 13 + NewLineLength * 2)); Dialogue ([Literal ("Hello, friend.", new Range(13 + NewLineLength * 2, 14))], new Range(13 + NewLineLength * 2, 14))]

[<Test>]
let ``Dual Dialogue - invalid`` () =
   // BRICK must not be recognized as character as there is no new line before
   let doc = properNewLines "\r\nSTEEL\r\nBeer's ready!\r\nBRICK\r\nAre they cold?" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue ([Literal ("Beer's ready!", new Range(5 + NewLineLength * 2, 13)); HardLineBreak(new Range(18 + NewLineLength * 2, NewLineLength)); Literal("BRICK", new Range(18 + NewLineLength * 3, 5)); HardLineBreak(new Range(23 + NewLineLength * 3, NewLineLength)); Literal("Are they cold?", new Range(23 + NewLineLength * 4, 14))], new Range(5 + NewLineLength * 2, 32 + NewLineLength * 2))]

[<Test>]
let ``Dual Dialogue - Parenthetical`` () =
   let doc = properNewLines "\r\nSTEEL\r\n(beer raised)\r\nTo retirement.\r\n\r\nBRICK ^\r\nTo retirement." |> FountainDocument.Parse
   let expected = [DualDialogue([Character (false, true, [Literal ("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Parenthetical([Literal("beer raised", new Range(6 + NewLineLength * 2, 11))], new Range(5 + NewLineLength * 2, 13 + NewLineLength)); Dialogue ([Literal ("To retirement.", new Range(18 + NewLineLength * 3, 14))], new Range(18 + NewLineLength * 3, 14 + NewLineLength)); Character (false, false, [Literal ("BRICK", new Range(32 + NewLineLength * 5, 5))], new Range(32 + NewLineLength * 4, 7 + NewLineLength * 2)); Dialogue ([Literal ("To retirement.", new Range(39 + NewLineLength * 6, 14))], new Range(39 + NewLineLength * 6, 14))], new Range(0, 53 + NewLineLength * 6))]
   doc.Blocks
   |> should equal expected

[<Test>]
let ``Dialogue - Empty after Character`` () =
   let doc = properNewLines "\r\nBRICK\r\n\r\n" |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("BRICK", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue ([HardLineBreak(new Range(5 + NewLineLength * 2, NewLineLength))], new Range(5 + NewLineLength * 2, NewLineLength)) ]

[<Test>]
let ``Dialogue - Action after empty Dialogue`` () =
   let doc = properNewLines "\r\nBRICK\r\n\r\nScrew retirement." |> FountainDocument.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("BRICK", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue ([HardLineBreak(new Range(5 + NewLineLength * 2, NewLineLength))], new Range(5 + NewLineLength * 2, NewLineLength)); Action(false, [ Literal("Screw retirement.",  new Range(5 + NewLineLength * 3, 17)) ], new Range(5 + NewLineLength * 3, 17)) ]
