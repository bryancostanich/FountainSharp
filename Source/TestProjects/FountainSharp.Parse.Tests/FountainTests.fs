//TODO: these aren't right.
#if INTERACTIVE
#r "../../bin/FountainSharp.Parse.dll"
#r "../../packages/NUnit/lib/nunit.framework.dll"
#else
module FountainSharp.Tests.Parsing
#endif

open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper
open System

let properNewLines (text: string) = text.Replace("\r\n", System.Environment.NewLine)

//===== Block Elements ==============================================================

[<Test>]
let ``Empty lines`` () =
   let doc = NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal  [ Action(false, [ HardLineBreak(new Range(0, NewLineLength)); HardLineBreak(new Range(NewLineLength, NewLineLength)) ], new Range(0, NewLineLength * 2)) ]

//===== Boneyard

// TODO: should we support boneyards in a single line? Also in the middle of the line?
[<Test>]
let ``Boneyard`` () =
   let text = properNewLines "/*\r\nThis is a simple comment\r\n*/"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Boneyard ("This is a simple comment", new Range(0, text.Length))]

//===== Scene Headings
[<Test>]
let ``Basic Scene Heading`` () =
   let doc = properNewLines "\r\nEXT. BRICK'S PATIO - DAY\r\n" |> Fountain.Parse
   doc.Blocks 
   |> should equal [ SceneHeading(false, [ Literal("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24)) ], new Range(0, 24 + NewLineLength * 3)) ]

[<Test>]
let ``Forced (".") Scene Heading`` () =
   let doc = properNewLines "\r\n.BINOCULARS A FORCED SCENE HEADING - LATER\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading (true, [Literal ("BINOCULARS A FORCED SCENE HEADING - LATER", new Range(1 + NewLineLength, 41))], new Range(0, 42 + NewLineLength * 3)) ]

[<Test>]
let ``Forced (".") Scene Heading with line breaks and action`` () =
   let text = properNewLines "\r\n.BRICK'S PATIO - DAY\r\n\r\nSome Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [ SceneHeading (true, [Literal ("BRICK'S PATIO - DAY", new Range(1 + NewLineLength, 19))], new Range(0, 20 + NewLineLength * 3)); Action (false, [Literal ("Some Action", new Range(20 + NewLineLength * 3, 11))], new Range(20 + NewLineLength * 3, 11))]

[<Test>]
let ``Forced (".") Scene Heading with more line breaks and action`` () =
   let doc = properNewLines "\r\n.BRICK'S PATIO - DAY\r\n\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (true, [Literal ("BRICK'S PATIO - DAY", new Range(1 + NewLineLength, 19))], new Range(0, 20 + 3 * NewLineLength)); Action(false, [HardLineBreak(new Range(20 + NewLineLength * 3, NewLineLength)); Literal("Some Action", new Range(20 + NewLineLength * 4, 11))], new Range(20 + NewLineLength * 3, 11 + NewLineLength))]

[<Test>]
let ``Forced (".") Scene Heading - No empty line after`` () =
   let text = properNewLines ".BINOCULARS A FORCED SCENE HEADING - LATER\r\nSome Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal (".BINOCULARS A FORCED SCENE HEADING - LATER", new Range(0, 42)); HardLineBreak(new Range(42, NewLineLength)); Literal ("Some Action", new Range(42 + NewLineLength, 11))], new Range(0, text.Length))]

[<Test>]
let ``Lowercase known scene heading`` () =
   let doc = properNewLines "\r\next. brick's pool - day\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading (false, [Literal ("ext. brick's pool - day", new Range(NewLineLength, 23))], new Range(0, 23 + NewLineLength * 3)) ]

[<Test>]
let ``Known INT Scene Head`` () =
   let doc = properNewLines "\r\nINT DOGHOUSE - DAY\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading (false, [Literal ("INT DOGHOUSE - DAY", new Range(NewLineLength, 18))], new Range(0, 18 + NewLineLength * 3))]

[<Test>]
let ``Known EXT Scene Head`` () =
   let doc = "\r\nEXT DOGHOUSE - DAY\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading (false, [Literal ("EXT DOGHOUSE - DAY", new Range(NewLineLength, 18))], new Range(0, 18 + NewLineLength * 3)) ]

[<Test>]
let ``Known EST Scene Head`` () =
   let doc = properNewLines "\r\nEST DOGHOUSE - DAY\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading (false, [Literal ("EST DOGHOUSE - DAY", new Range(NewLineLength, 18))], new Range(0, 18 + NewLineLength * 3)) ]

[<Test>]
let ``Known INT./EXT Scene Head`` () =
   let doc = properNewLines "\r\nINT./EXT DOGHOUSE - DAY\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT./EXT DOGHOUSE - DAY", new Range(NewLineLength, 23))], new Range(0, 23 + NewLineLength * 3))]

[<Test>]
let ``Known INT/EXT Scene Head`` () =
   let doc = properNewLines "\r\nINT/EXT DOGHOUSE - DAY\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT/EXT DOGHOUSE - DAY", new Range(NewLineLength, 22))], new Range(0, 22 + NewLineLength * 3))]

[<Test>]
let ``Known I/E Scene Head`` () =
   let doc = properNewLines "\r\nI/E DOGHOUSE - DAY\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading (false, [Literal ("I/E DOGHOUSE - DAY", new Range(NewLineLength, 18))], new Range(0, 18 + NewLineLength * 3)) ]

[<Test>]
let ``Scene Heading with line breaks and action`` () =
   let doc = properNewLines "\r\nEXT. BRICK'S PATIO - DAY\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24))], new Range(0, 24 + NewLineLength * 3)); Action (false, [Literal ("Some Action", new Range(24 + NewLineLength * 3, 11))], new Range(24 + NewLineLength * 3, 11))]

[<Test>]
let ``Scene Heading with more line breaks and action`` () =
   let doc = "\r\nEXT. BRICK'S PATIO - DAY\r\n\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24))], new Range(0, 24 + NewLineLength * 3)); Action(false, [HardLineBreak(new Range(24 + NewLineLength * 3, NewLineLength)); Literal("Some Action", new Range(24 + NewLineLength * 4, 11))], new Range(24 + NewLineLength * 3, NewLineLength + 11))]

[<Test>]
let ``Scene Heading - No empty line after`` () =
   // this must not be recognized as scene heading
   let text = "EXT. BRICK'S PATIO - DAY" + NewLine(1) + "Some Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0, 24)); HardLineBreak(new Range(24, NewLineLength)); Literal ("Some Action", new Range(24 + NewLineLength, 11))], new Range(0, text.Length))]

[<Test>]
let ``Scene Heading - Character after`` () =
   let doc = properNewLines "\r\nEXT. BRICK'S PATIO - DAY\r\n\r\nLINDSEY" |> Fountain.Parse
   doc.Blocks 
   |> should equal [ SceneHeading(false, [ Literal("EXT. BRICK'S PATIO - DAY", new Range(NewLineLength, 24)) ], new Range(0, 24 + NewLineLength * 3)); Character(false, true, [ Literal("LINDSEY", new Range(24 + NewLineLength * 3, 7)) ], new Range(24 + NewLineLength * 3, 7)) ]


//===== Action
[<Test>]
let ``Action - Simple`` () =
   let doc = "Some Action" + NewLine(1) |> Fountain.Parse
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

//===== Synopses

[<Test>]
let ``Basic Synopses`` () =
   let doc = "= Here is a synopses of this fascinating scene." |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses ([Literal ("Here is a synopses of this fascinating scene.", new Range(2, 45))], new Range(0, 47))]


//===== Character

[<Test>]
let ``Character - Normal`` () =
   let doc = "\r\nLINDSEY" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength))]

[<Test>]
let ``Character - With parenthetical extension`` () =
   let text = properNewLines "\r\nLINDSEY (on the radio)"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY (on the radio)", new Range(NewLineLength, 22))], new Range(0, 22 + NewLineLength))]

[<Test>]
let ``Character - With invalid parenthetical extension`` () =
   let character = "LINDSEY (on the Radio)"
   let text = Environment.NewLine + character
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [HardLineBreak(new Range(0, NewLineLength)); Literal (character, new Range(NewLineLength, character.Length))], new Range(0, text.Length))]

[<Test>]
let ``Character - With whitespace``() = 
    let character = "THIS IS ALL UPPERCASE BUT HAS WHITESPACE"
    let doc = "\r\n" + character |> Fountain.Parse
    doc.Blocks 
    |> should equal 
           [ Character(false, true, [ Literal(character, new Range(NewLineLength, character.Length)) ], new Range(0, character.Length + NewLineLength)) ]
    
[<Test>]
let ``Character - With Numbers`` () =
   let doc = "\r\nR2D2" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("R2D2", new Range(NewLineLength, 4))], new Range(0, 4 + NewLineLength))]

[<Test>]
let ``Character - Number first`` () =
   let text = properNewLines "\r\n25D2"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [HardLineBreak(new Range(0, NewLineLength)); Literal ("25D2", new Range(NewLineLength, 4))], new Range(0, text.Length))]

[<Test>]
let ``Character - Forced with at sign`` () =
   let doc = properNewLines "\r\n@McAvoy" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (true, true, [Literal ("McAvoy", new Range(NewLineLength + 1, 6))], new Range(0, 7 + NewLineLength))]

[<Test>]
let ``Character - With forced at and parenthetical extension`` () =
   let doc = "\r\n@McAvoy (OS)" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (true, true, [Literal ("McAvoy (OS)", new Range(NewLineLength + 1, 11))], new Range(0, 12 + NewLineLength))]

[<Test>]
let ``Character - Whitespace after`` () =
   let doc = "\r\nLINDSEY " |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY ", new Range(NewLineLength, 8))], new Range(0, 8 + NewLineLength))]


//===== Parenthetical

[<Test>]
let ``Parenthetical `` () =
   let doc = properNewLines "\r\nLINDSEY\r\n(quietly)" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Parenthetical ([Literal ("quietly", new Range(8 + NewLineLength * 2, 7))], new Range(7 + NewLineLength * 2, 9))];

[<Test>]
let ``Parenthetical - After Dialogue`` () =
   let text = "\r\nLINDSEY\r\n(quietly)\r\nHello, friend.\r\n(loudly)\r\nFriendo!"
   let doc = properNewLines text |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Parenthetical ([Literal ("quietly", new Range(8 + NewLineLength * 2, 7))], new Range(7 + NewLineLength * 2, 9 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(16 + NewLineLength * 3, 14))], new Range(16 + NewLineLength * 3, 14)); Parenthetical ([Literal ("loudly", new Range(31 + NewLineLength * 3, 6))], new Range(30 + NewLineLength * 3, 8 + NewLineLength)); Dialogue ([Literal ("Friendo!", new Range(38 + NewLineLength * 4, 8))], new Range(38 + NewLineLength * 4, 8))];


//===== Dialogue

[<Test>]
let ``Dialogue - Normal`` () =
   let doc = "\r\nLINDSEY\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Dialogue ([Literal ("Hello, friend.", new Range(7 + NewLineLength * 2, 14))], new Range(7 + NewLineLength * 2, 14))]

[<Test>]
let ``Dialogue - After Parenthetical`` () =
   let doc = "\r\nLINDSEY\r\n(quietly)\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 2)); Parenthetical ([Literal ("quietly", new Range(8 + NewLineLength * 2, 7))], new Range(7 + NewLineLength * 2, 9 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(16 + NewLineLength * 3, 14))], new Range(16 + NewLineLength * 3, 14))]

[<Test>]
let ``Dialogue - With line break`` () =
   let doc = "\r\nDEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n  \r\nHit or stand sir?" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("DEALER", new Range(NewLineLength, 6))], new Range(0, 6 + NewLineLength * 2));  Dialogue ([Literal ("Ten.", new Range(6 + NewLineLength * 2, 4)); HardLineBreak(new Range(10 + NewLineLength * 2, NewLineLength)); Literal ("Four.", new Range(10 + NewLineLength * 3, 5)); HardLineBreak(new Range(15 + NewLineLength * 3, NewLineLength)); Literal ("Dealer gets a seven.", new Range(15 + NewLineLength * 4, 20)); HardLineBreak(new Range(35 + NewLineLength * 4, NewLineLength)); HardLineBreak(new Range(35 + NewLineLength * 5, 2 + NewLineLength)); Literal ("Hit or stand sir?", new Range(37 + NewLineLength * 6, 17))], new Range(6 + NewLineLength * 2, 48 + NewLineLength * 4))]

[<Test>]
let ``Dialogue - With invalid line break`` () =
   let doc = properNewLines "\r\nDEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n\r\nHit or stand sir?" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("DEALER", new Range(NewLineLength, 6))], new Range(0, 6 + NewLineLength * 2));  Dialogue ([Literal ("Ten.", new Range(6 + NewLineLength * 2, 4)); HardLineBreak(new Range(10 + NewLineLength * 2, NewLineLength)); Literal ("Four.", new Range(10 + NewLineLength * 3, 5)); HardLineBreak(new Range(15 + NewLineLength * 3, NewLineLength)); Literal ("Dealer gets a seven.", new Range(15 + NewLineLength * 4, 20))], new Range(6 + NewLineLength * 2, 29 + NewLineLength * 3)); Action (false, [ HardLineBreak(new Range(35 + NewLineLength * 5, NewLineLength)); Literal ("Hit or stand sir?", new Range(35 + NewLineLength * 6, 17))], new Range(35 + NewLineLength * 5, 17 + NewLineLength))]
   // this test now fails: parsing places line break be after last Action

[<Test>]
let ``Dual Dialogue`` () =
   let firstCharacter = "\r\nBRICK\r\nScrew retirement.\r\n"
   let secondCharacter = "\r\nSTEEL ^\r\nScrew retirement."
   let doc = properNewLines firstCharacter + secondCharacter |> Fountain.Parse
   doc.Blocks
   |> should equal [DualDialogue([Character (false, true, [Literal ("BRICK", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue ([Literal ("Screw retirement.", new Range(5 + NewLineLength * 2, 17))], new Range(5 + NewLineLength * 2, 17)); Character (false, false, [Literal ("STEEL", new Range(22 + NewLineLength * 3, 5))], new Range(22 + NewLineLength * 2, 7 + NewLineLength * 2)); Dialogue ([Literal ("Screw retirement.", new Range(29 + NewLineLength * 4, 17))], new Range(29 + NewLineLength * 4, 17))], new Range(0, 46 + NewLineLength * 4))]

[<Test>]
let ``Dual Dialogue - Second character`` () =
   let doc = "\r\nLINDSEY     ^\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, false, [Literal ("LINDSEY", new Range(NewLineLength, 7))], new Range(0, 13 + NewLineLength * 2)); Dialogue ([Literal ("Hello, friend.", new Range(13 + NewLineLength * 2, 14))], new Range(13 + NewLineLength * 2, 14))]

[<Test>]
let ``Dual Dialogue - invalid`` () =
   // BRICK must not be recognized as character as there is no new line before
   let doc = "\r\nSTEEL\r\nBeer's ready!\r\nBRICK\r\nAre they cold?" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Dialogue ([Literal ("Beer's ready!", new Range(5 + NewLineLength * 2, 13)); HardLineBreak(new Range(18 + NewLineLength * 2, NewLineLength)); Literal("BRICK", new Range(18 + NewLineLength * 3, 5)); HardLineBreak(new Range(23 + NewLineLength * 3, NewLineLength)); Literal("Are they cold?", new Range(23 + NewLineLength * 4, 14))], new Range(5 + NewLineLength * 2, 32 + NewLineLength * 2))]

[<Test>]
let ``Dual Dialogue - Parenthetical`` () =
   let doc = "\r\nSTEEL\r\n(beer raised)\r\nTo retirement.\r\n\r\nBRICK ^\r\nTo retirement." |> Fountain.Parse
   let expected = [DualDialogue([Character (false, true, [Literal ("STEEL", new Range(NewLineLength, 5))], new Range(0, 5 + NewLineLength * 2)); Parenthetical([Literal("beer raised", new Range(6 + NewLineLength * 2, 11))], new Range(5 + NewLineLength * 2, 13 + NewLineLength)); Dialogue ([Literal ("To retirement.", new Range(18 + NewLineLength * 3, 14))], new Range(18 + NewLineLength * 3, 14)); Character (false, false, [Literal ("BRICK", new Range(32 + NewLineLength * 4, 5))], new Range(32 + NewLineLength * 3, 7 + NewLineLength * 2)); Dialogue ([Literal ("To retirement.", new Range(39 + NewLineLength * 5, 14))], new Range(39 + NewLineLength * 5, 14))], new Range(0, 53 + NewLineLength * 5))]
   doc.Blocks
   |> should equal expected

//===== Page Break

[<Test>]
let ``PageBreak - ===`` () =
   let doc = "===" |> Fountain.Parse
   doc.Blocks
   |> should equal [PageBreak(new Range(0, 3))]

[<Test>]
// TODO: should this be a synopses? probably, yeah? need clarification from the spec
let ``PageBreak - == (not enough =)`` () =
   let doc = "==" |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses ([Literal ("=", new Range(1, 1))], new Range(0, 2))]

[<Test>]
let ``PageBreak - ==========`` () =
   let doc = "==========" |> Fountain.Parse
   doc.Blocks
   |> should equal [PageBreak(new Range(0, 10))]

[<Test>]
let ``PageBreak - ======= (with space at end)`` () =
   let doc = "======= " |> Fountain.Parse
   doc.Blocks
   |> should equal [PageBreak(new Range(0, 8))]


[<Test>]
let ``PageBreak - ======= blah (fail with other chars)`` () =
   let doc = "======= blah" |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses ([Literal ("====== blah", new Range(1, 11))], new Range(0, 12))]


//===== Lyrics

[<Test>]
let ``Lyrics - normal`` () =
   let doc = "~Birdy hop, he do. He hop a long." |> Fountain.Parse
   doc.Blocks
   |> should equal [Lyrics ([Literal ("Birdy hop, he do. He hop a long.", new Range(1, 32))], new Range(0, 33))]

[<Test>]
let ``Lyrics - Line break at the end`` () =
   let doc = properNewLines "~Birdy hop, he do. He hop a long.\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Lyrics ([Literal ("Birdy hop, he do. He hop a long.", new Range(1, 32)); HardLineBreak(new Range(33, NewLineLength)) ], new Range(0, 33 + NewLineLength)); Action(false, [ HardLineBreak(new Range(33 + NewLineLength, NewLineLength))], new Range(33 + NewLineLength, NewLineLength)) ]


//===== Transition

[<Test>]
let ``Transition - normal`` () =
   let doc = "\r\nCUT TO:\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (false, [Literal ("CUT TO:", new Range(NewLineLength, 7))], new Range(0, 7 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(7 + NewLineLength * 3, 11))], new Range(7 + NewLineLength * 3, 11))]

[<Test>]
let ``Transition - Non uppercase`` () =
   // This is not a transition as 'Cut' is not all uppercase
   let doc = "\r\nCut TO:\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Action (false, [HardLineBreak(new Range(0, NewLineLength)); Literal("Cut TO:", new Range(NewLineLength, 7)); HardLineBreak(new Range(7 + NewLineLength, NewLineLength)); HardLineBreak(new Range(7 + NewLineLength * 2, NewLineLength)); Literal("Some action", new Range(7 + NewLineLength * 3, 11))], new Range(0, 18 + NewLineLength * 3))]

[<Test>]
let ``Transition - forced`` () =
   let doc = "\r\n> Burn to White.\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (true, [Literal ("Burn to White.", new Range(2 + NewLineLength, 14))], new Range(0, 16 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(16 + NewLineLength * 3, 11))], new Range(16 + NewLineLength * 3, 11)) ]

//===== Centered

[<Test>]
let ``Centered `` () =
   let doc = ">The End<" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(1, 7))], new Range(0, 9))]

[<Test>]
let ``Centered - with spaces`` () =
   let doc = "> The End <" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(2, 7))], new Range(0, 11))]

//===== Line Breaks

[<Test>]
let ``Line Breaks`` () =
   let text = "Murtaugh, springing..." + NewLine(2) + "An explosion of sound..." + NewLine(1) + "As it rises like an avenging angel ..." + NewLine(1) + "Hovers, shattering the air " + NewLine(2) + "Screaming, chaos, frenzy." + NewLine(1) + "Three words that apply to this scene."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal ([Action (false, [Literal( "Murtaugh, springing...", new Range(0, 22)); HardLineBreak(new Range(22, NewLineLength)); HardLineBreak(new Range(22 + NewLineLength, NewLineLength)); Literal("An explosion of sound...", new Range(22 + 2 * NewLineLength, 24)); HardLineBreak(new Range(46 + 2 * NewLineLength, NewLineLength)); Literal("As it rises like an avenging angel ...", new Range(46 + 3 * NewLineLength, 38)); HardLineBreak(new Range(84 + 3 * NewLineLength, NewLineLength)); Literal("Hovers, shattering the air ", new Range(84 + 4 * NewLineLength, 27)); HardLineBreak(new Range(111 + 4 * NewLineLength, NewLineLength)); HardLineBreak(new Range(111 + 5 * NewLineLength, NewLineLength)); Literal ("Screaming, chaos, frenzy.", new Range(111 + 6 * NewLineLength, 25)); HardLineBreak(new Range(136 + 6 * NewLineLength, NewLineLength)); Literal( "Three words that apply to this scene.", new Range(136 + 7 * NewLineLength, 37))], new Range(0, text.Length))])

//===== Notes
[<Test>]
let ``Notes - Inline`` () =
   let text = "Some text and then a [[bit of a note]]. And some more text."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("Some text and then a ", new Range(0, 21)); Note ([Literal( "bit of a note", new Range(23, 13))], new Range(21, 17)); Literal( ". And some more text.", new Range(38, 21))], new Range(0, 59))]

[<Test>]
let ``Notes - Block`` () =
   let doc = "[[It was supposed to be Vietnamese, right?]]" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Note ([Literal ("It was supposed to be Vietnamese, right?", new Range(2, 40))], new Range(0, 44))], new Range(0, 44))]

[<Test>]
let ``Notes - Line breaks`` () =
   let text = properNewLines "His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.[[This section needs work.\r\nEither that, or I need coffee.\r\nDefinitely coffee.]] He looks around. Phone ringing."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Literal ("His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.", new Range(0, 114)); Note([Literal("This section needs work.", new Range(116, 24)); HardLineBreak(new Range(140, NewLineLength)); Literal("Either that, or I need coffee.", new Range(140 + NewLineLength, 30)); HardLineBreak(new Range(170 + NewLineLength, NewLineLength)); Literal("Definitely coffee.", new Range(170 + 2 * NewLineLength, 18))], new Range(114, 76 + NewLineLength * 2)); Literal(" He looks around. Phone ringing.", new Range(190 + NewLineLength * 2, 32))], new Range(0, text.Length))]

[<Test>]
let ``Notes - Line breaks with empty line`` () =
   let text = properNewLines "His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.[[This section needs work.\r\nEither that, or I need coffee.\r\n  \r\nDefinitely coffee.]] He looks around. Phone ringing."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Literal ("His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.", new Range(0, 114)); Note([Literal("This section needs work.", new Range(116, 24)); HardLineBreak(new Range(140, NewLineLength)); Literal("Either that, or I need coffee.", new Range(140 + NewLineLength, 30)); HardLineBreak(new Range(170 + NewLineLength, NewLineLength)); HardLineBreak(new Range(170 + 2 * NewLineLength, NewLineLength + 2)); Literal("Definitely coffee.", new Range(172 + NewLineLength * 3, 18))], new Range(114, 78 + NewLineLength * 3)); Literal(" He looks around. Phone ringing.", new Range(192 + NewLineLength * 3, 32))], new Range(0, text.Length))]

//===== Boneyard (Comments)
//TODO: not implemented yet.

//===== Span Elements ==============================================================

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


//===== Indenting

[<Test>]
let ``Scene Heading - Indenting`` () =
   let doc = properNewLines "\r\n\t EXT. BRICK'S PATIO - DAY\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(2 + NewLineLength, 24))], new Range(0, 26 + NewLineLength * 3)); Action(false, [Literal ("Some Action", new Range(26 + NewLineLength * 3, 11))], new Range(26 + NewLineLength * 3, 11))]

[<Test>]
let ``Character - Indenting`` () =
   // white spaces have to be ignored
   let doc = "\r\n\t   LINDSEY" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength + 4, 7))], new Range(0, 11 + NewLineLength))]

[<Test>]
let ``Dialogue - Indenting`` () =
   let doc = "\r\n\t  LINDSEY\r\n   \t Hello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [ Character (false, true, [Literal ("LINDSEY", new Range(NewLineLength + 3, 7))], new Range(0, 10 + NewLineLength * 2)); Dialogue ([Literal ("Hello, friend.", new Range(15 + NewLineLength * 2, 14))], new Range(10 + NewLineLength * 2, 19))]

[<Test>]
let ``Action - indenting`` () =
   let text = "\tNatalie looks around at the group, TIM, ROGER, NATE, and VEEK." + NewLine(2) + "\t\tTIM, is smiling broadly."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal ("\tNatalie looks around at the group, TIM, ROGER, NATE, and VEEK.", new Range(0, 63)); HardLineBreak(new Range(63, NewLineLength)); HardLineBreak(new Range(63 + NewLineLength, NewLineLength)); Literal ("\t\tTIM, is smiling broadly.", new Range(63 + 2 * NewLineLength, 26))], new Range(0, text.Length))]

[<Test>]
let ``Centered - indenting`` () =
   let doc = "\t   \t>The End <" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(6, 7))], new Range(0, 15))]

[<Test>]
let ``Transition - indenting`` () =
   let doc = "\r\n  \t  CUT TO:\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (false, [Literal ("CUT TO:", new Range(NewLineLength + 5, 7))], new Range(0, 12 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(12 + NewLineLength * 3, 11))], new Range(12 + NewLineLength * 3, 11))]

[<Test>]
let ``Transition - forced indenting`` () =
   let doc = "\r\n\t > Burn to White.\r\n\r\nSome action" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Transition (true, [Literal ("Burn to White.", new Range(NewLineLength + 4, 14))], new Range(0, 18 + NewLineLength * 3)); Action(false, [Literal("Some action", new Range(18 + NewLineLength * 3, 11))], new Range(18 + NewLineLength * 3, 11))]

//===== Title page

[<Test>]
let ``Title page`` () =
   // This is quite a complex title page with inline and not inline values, emphasized spans.
   let text = "Title:" + NewLine(1) + "\t_**BRICK and STEEL**_" + NewLine(1) + "\t_**FULL RETIRED**_" + NewLine(1) + "Credit: Written by" + NewLine(2) + "Some action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [TitlePage ([("Title", [Underline ([Bold ([Literal("BRICK and STEEL", new Range(3, 15))], new Range(1, 19))], new Range(0, 21)); HardLineBreak(new Range(21, NewLineLength)); Underline([ Bold ([Literal("FULL RETIRED", new Range(24 + NewLineLength, 12))], new Range(22 + NewLineLength, 16))], new Range(21 + NewLineLength, 18))]); ("Credit", [Literal("Written by", new Range(0, 10))])], new Range(0, text.Length - 11)); PageBreak(new Range(text.Length - 11, 0)); Action(false, [Literal("Some action", new Range(text.Length - 11, 11))], new Range(text.Length - 11, 11))]

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
