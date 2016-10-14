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

//===== Boneyard

// TODO: should we support boneyards in a single line? Also in the middle of the line?
[<Test>]
let ``Boneyard`` () =
   let doc = "/*\r\nThis is a simple comment\r\n*/" |> Fountain.Parse
   doc.Blocks
   |> should equal [Boneyard ("This is a simple comment", new Range(0, 0))]

//===== Scene Headings
[<Test>]
let ``Basic Scene Heading`` () =
   let doc = "EXT. BRICK'S PATIO - DAY" + NewLine(2) |> Fountain.Parse
   doc.Blocks 
   |> should equal [ SceneHeading(false, [ Literal("EXT. BRICK'S PATIO - DAY", new Range(0, 24)) ], new Range(0, 0)) ]

[<Test>]
let ``Forced (".") Scene Heading`` () =
   let doc = ".BINOCULARS A FORCED SCENE HEADING - LATER" + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (true, [Literal ("BINOCULARS A FORCED SCENE HEADING - LATER", new Range(0, 41))], new Range(0,0))]

[<Test>]
let ``Forced (".") Scene Heading with line breaks and action`` () =
   let text = ".BRICK'S PATIO - DAY" + NewLine(2) + "Some Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (true, [Literal ("BRICK'S PATIO - DAY", new Range(0, 19))], new Range(0,0)); Action (false, [Literal ("Some Action", new Range(0, 11))], new Range(0, 11))]

[<Test>]
let ``Forced (".") Scene Heading with more line breaks and action`` () =
   let text = ".BRICK'S PATIO - DAY" + NewLine(3) + "Some Action";
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (true, [Literal ("BRICK'S PATIO - DAY", new Range(0, 19))], new Range(0,0)); Action(false, [HardLineBreak(new Range(0, NewLineLength)); Literal("Some Action", new Range(NewLineLength, 11))], new Range(0, 11 + NewLineLength))]

[<Test>]
let ``Forced (".") Scene Heading - No empty line after`` () =
   let text = properNewLines ".BINOCULARS A FORCED SCENE HEADING - LATER\r\nSome Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal (".BINOCULARS A FORCED SCENE HEADING - LATER", new Range(0, 42)); HardLineBreak(new Range(42, NewLineLength)); Literal ("Some Action", new Range(42 + NewLineLength, 11))], new Range(0, text.Length))]

[<Test>]
let ``Lowercase known scene heading`` () =
   let doc = "ext. brick's pool - day" + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("ext. brick's pool - day", new Range(0, 23))], new Range(0,0))]

[<Test>]
let ``Known INT Scene Head`` () =
   let doc = "INT DOGHOUSE - DAY" + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT DOGHOUSE - DAY", new Range(0, 18))], new Range(0,0))]

[<Test>]
let ``Known EXT Scene Head`` () =
   let doc = "EXT DOGHOUSE - DAY" + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("EXT DOGHOUSE - DAY", new Range(0, 18))], new Range(0,0))]

[<Test>]
let ``Known EST Scene Head`` () =
   let doc = "EST DOGHOUSE - DAY"  + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("EST DOGHOUSE - DAY", new Range(0, 18))], new Range(0,0))]

[<Test>]
let ``Known INT./EXT Scene Head`` () =
   let doc = "INT./EXT DOGHOUSE - DAY" + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT./EXT DOGHOUSE - DAY", new Range(0, 23))], new Range(0,0))]

[<Test>]
let ``Known INT/EXT Scene Head`` () =
   let doc = "INT/EXT DOGHOUSE - DAY" + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT/EXT DOGHOUSE - DAY", new Range(0, 22))], new Range(0,0))]

[<Test>]
let ``Known I/E Scene Head`` () =
   let doc = "I/E DOGHOUSE - DAY" + NewLine(2) |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("I/E DOGHOUSE - DAY", new Range(0, 18))], new Range(0,0))]

[<Test>]
let ``Scene Heading with line breaks and action`` () =
   let text = "EXT. BRICK'S PATIO - DAY" + NewLine(2) + "Some Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0, 24))], new Range(0,0)); Action (false, [Literal ("Some Action", new Range(0, 11))], new Range(0, 11))]

[<Test>]
let ``Scene Heading with more line breaks and action`` () =
   let doc = "EXT. BRICK'S PATIO - DAY" + NewLine(3) + "Some Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0, 24))], new Range(0,0)); Action(false, [HardLineBreak(new Range(0, NewLineLength)); Literal("Some Action", new Range(NewLineLength, 11))], new Range(0, NewLineLength + 11))]

[<Test>]
let ``Scene Heading - No empty line after`` () =
   // this must not be recognized as scene heading
   let text = "EXT. BRICK'S PATIO - DAY" + NewLine(1) + "Some Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0, 24)); HardLineBreak(new Range(24, NewLineLength)); Literal ("Some Action", new Range(24 + NewLineLength, 11))], new Range(0, text.Length))]

//===== Action
[<Test>]
let ``Action - With line breaks`` () =
   let action = "Some Action" + NewLine(2) + "Some More Action"
   let sceneHeading = "EXT. BRICK'S PATIO - DAY" + NewLine(2)
   let doc = sceneHeading + action |> Fountain.Parse
   doc.Blocks
   |> should equal   [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0,24))], new Range(0,0)); Action (false, [Literal ("Some Action", new Range(0,11)); HardLineBreak(new Range(11, NewLineLength)); HardLineBreak(new Range(11 + NewLineLength, NewLineLength)); Literal ("Some More Action", new Range(11 + 2 * NewLineLength, 16))], new Range(0, action.Length))]

[<Test>]
let ``Action - With line breaks and no heading`` () =
   let text = "Natalie looks around at the group, TIM, ROGER, NATE, and VEEK." + Environment.NewLine + Environment.NewLine + "TIM, is smiling broadly."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal ("Natalie looks around at the group, TIM, ROGER, NATE, and VEEK.", new Range(0, 62)); HardLineBreak(new Range(62, NewLineLength)); HardLineBreak(new Range(62 + NewLineLength, NewLineLength)); Literal ("TIM, is smiling broadly.", new Range(62 + 2 * NewLineLength, 24))], new Range(0, text.Length))]

//===== Synopses

[<Test>]
let ``Basic Synopses`` () =
   let doc = "= Here is a synopses of this fascinating scene." |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses ([Literal (" Here is a synopses of this fascinating scene.", new Range(0, 46))], new Range(0,0))]


//===== Character

[<Test>]
let ``Character - Normal`` () =
   let doc = "\r\nLINDSEY" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(0, 7))], new Range(0, 7 + NewLineLength))]

[<Test>]
let ``Character - With parenthetical extension`` () =
   let character = "LINDSEY (on the radio)"
   let text = NewLine(1) + character
   let doc =text |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal (character, new Range(0, character.Length))], new Range(0, text.Length))]

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
           [ Character(false, true, [ Literal(character, new Range(0, character.Length)) ], new Range(0, character.Length + NewLineLength)) ]
    
[<Test>]
let ``Character - With Numbers`` () =
   let doc = "\r\nR2D2" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("R2D2", new Range(0, 4))], new Range(0, 4 + NewLineLength))]

[<Test>]
let ``Character - Number first`` () =
   let text = Environment.NewLine + "25D2"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [HardLineBreak(new Range(0, NewLineLength)); Literal ("25D2", new Range(NewLineLength, 4))], new Range(0, text.Length))]

[<Test>]
let ``Character - Forced with at sign`` () =
   let text = NewLine(1) + "@McAvoy"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (true, true, [Literal ("McAvoy", new Range(0, 6))], new Range(0, text.Length))]

[<Test>]
let ``Character - With forced at and parenthetical extension`` () =
   let doc = "\r\n@McAvoy (OS)" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (true, true, [Literal ("McAvoy (OS)", new Range(0, 11))], new Range(0, 12 + NewLineLength))]


//===== Parenthetical

[<Test>]
let ``Parenthetical `` () =
   let doc = NewLine(1) + "LINDSEY" + NewLine(1) + "(quietly)" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(0, 7))], new Range(0,7 + Environment.NewLine.Length)); Parenthetical ([Literal ("quietly", new Range(0, 7))], new Range(0,0))];

[<Test>]
let ``Parenthetical - After Dialogue`` () =
   let text = "\r\nLINDSEY\r\n(quietly)\r\nHello, friend.\r\n(loudly)\r\nFriendo!"
   let doc = properNewLines text |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(0, 7))], new Range(0, 7 + NewLineLength)); Parenthetical ([Literal ("quietly", new Range(0, 7))], new Range(0, 0)); Dialogue ([Literal ("Hello, friend.", new Range(0, 14))], new Range(0, 0)); Parenthetical ([Literal ("loudly", new Range(0, 6))], new Range(0, 0)); Dialogue ([Literal ("Friendo!", new Range(0, 8))], new Range(0, 0))];


//===== Dialogue

[<Test>]
let ``Dialogue - Normal`` () =
   let doc = "\r\nLINDSEY\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(0, 7))], new Range(0, 7 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(0, 14))], new Range(0,0))]

[<Test>]
let ``Dialogue - After Parenthetical`` () =
   let doc = "\r\nLINDSEY\r\n(quietly)\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(0, 7))], new Range(0, 7 + NewLineLength)); Parenthetical ([Literal ("quietly", new Range(0, 7))], new Range(0,0)); Dialogue ([Literal ("Hello, friend.", new Range(0, 14))], new Range(0,0))]

[<Test>]
let ``Dialogue - With line break`` () =
   let doc = "\r\nDEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n\r\n  Hit or stand sir?" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("DEALER", new Range(0, 6))], new Range(0, 6 + NewLineLength));  Dialogue ([Literal ("Ten.", new Range(0, 4)); HardLineBreak(new Range(0, 0)); Literal ("Four.", new Range(0, 5)); HardLineBreak(new Range(0, 0)); Literal ("Dealer gets a seven.", new Range(0, 20)); HardLineBreak(new Range(0, 0)); HardLineBreak(new Range(0, 0)); Literal ("Hit or stand sir?", new Range(0, 17))], new Range(0, 0))]

[<Test>]
let ``Dialogue - With invalid line break`` () =
   let action = "Hit or stand sir?"
   let doc = "\r\nDEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n\r\n" + action |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("DEALER", new Range(0, 6))], new Range(0, 6 + NewLineLength));  Dialogue ([Literal ("Ten.", new Range(0, 4)); HardLineBreak(new Range(0, 0)); Literal ("Four.", new Range(0, 5)); HardLineBreak(new Range(0, 0)); Literal ("Dealer gets a seven.", new Range(0, 20))], new Range(0, 0)); Action (false, [Literal ("Hit or stand sir?", new Range(0, 17))], new Range(0, action.Length))]
   // this test now fails: parsing places line break be after last Action

[<Test>]
let ``Dual Dialogue`` () =
   let firstCharacter = "\r\nBRICK\r\nScrew retirement.\r\n"
   let secondCharacter = "\r\nSTEEL ^\r\nScrew retirement."
   let doc = properNewLines firstCharacter + secondCharacter |> Fountain.Parse
   doc.Blocks
   |> should equal [DualDialogue([Character (false, true, [Literal ("BRICK", new Range(0, 5))], new Range(0, 5 + NewLineLength)); Dialogue ([Literal ("Screw retirement.", new Range(5 + NewLineLength, 17))], new Range(0, 0)); Character (false, false, [Literal ("STEEL", new Range(5 + NewLineLength, 5))], new Range(5 + NewLineLength, 7 + NewLineLength)); Dialogue ([Literal ("Screw retirement.", new Range(12 + 2 * NewLineLength, 17))], new Range(0, 0))], new Range(0, 0))]

[<Test>]
let ``Dual Dialogue - Second character`` () =
   let doc = "\r\nLINDSEY     ^\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, false, [Literal ("LINDSEY", new Range(0, 7))], new Range(0, 13 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(0, 14))], new Range(0, 0))]

[<Test>]
let ``Dual Dialogue - invalid`` () =
   // BRICK must not be recognized as character as there is no new line before
   let doc = "\r\nSTEEL\r\nBeer's ready!\r\nBRICK\r\nAre they cold?" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("STEEL", new Range(0, 5))], new Range(0, NewLineLength + 5)); Dialogue ([Literal ("Beer's ready!", new Range(0, 13)); HardLineBreak(new Range(0, 0)); Literal("BRICK", new Range(0, 5)); HardLineBreak(new Range(0, 0)); Literal("Are they cold?", new Range(0, 14))], new Range(0, 0))]

[<Test>]
let ``Dual Dialogue - Parenthetical`` () =
   let doc = "\r\nSTEEL\r\n(beer raised)\r\nTo retirement.\r\n\r\nBRICK ^\r\nTo retirement." |> Fountain.Parse
   let expected = [DualDialogue([Character (false, true, [Literal ("STEEL", new Range(0, 5))], new Range(0, 5 + NewLineLength)); Parenthetical([Literal("beer raised", new Range(0, 11))], new Range(0, 0)); Dialogue ([Literal ("To retirement.", new Range(5 + NewLineLength, 14))], new Range(0, 0)); Character (false, false, [Literal ("BRICK", new Range(5 + NewLineLength, 5))], new Range(5 + NewLineLength, 7 + NewLineLength)); Dialogue ([Literal ("To retirement.", new Range(12 + 2 * NewLineLength, 14))], new Range(0, 0))], new Range(0, 0))]
   doc.Blocks
   |> should equal expected

//===== Page Break

[<Test>]
let ``PageBreak - ===`` () =
   let doc = "===" |> Fountain.Parse
   doc.Blocks
   |> should equal [PageBreak]

[<Test>]
// TODO: should this be a synopses? probably, yeah? need clarification from the spec
let ``PageBreak - == (not enough =)`` () =
   let doc = "==" |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses ([Literal ("=", new Range(0,1))], new Range(0,0))]

[<Test>]
let ``PageBreak - ==========`` () =
   let doc = "==========" |> Fountain.Parse
   doc.Blocks
   |> should equal [PageBreak]

[<Test>]
let ``PageBreak - ======= (with space at end)`` () =
   let doc = "======= " |> Fountain.Parse
   doc.Blocks
   |> should equal [PageBreak]


[<Test>]
let ``PageBreak - ======= blah (fail with other chars)`` () =
   let doc = "======= blah" |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses ([Literal ("====== blah", new Range(0, 11))], new Range(0,0))]


//===== Lyrics

[<Test>]
let ``Lyric - normal`` () =
   let doc = "~Birdy hop, he do. He hop a long." |> Fountain.Parse
   doc.Blocks
   |> should equal [Lyric ([Literal ("Birdy hop, he do. He hop a long.", new Range(0, 32))], new Range(0,0))]


//===== Transition

[<Test>]
let ``Transition - normal`` () =
   let doc = "CUT TO:" |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition (false, [Literal ("CUT TO:", new Range(0,7))], new Range(0,0))]

[<Test>]
let ``Transition - forced`` () =
   let doc = "> Burn to White." |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition (true, [Literal ("Burn to White.", new Range(0,14))], new Range(0,0))]

//===== Centered

[<Test>]
let ``Centered `` () =
   let doc = ">The End<" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(0, 7))], new Range(0, 0))]

[<Test>]
let ``Centered - with spaces`` () =
   let doc = "> The End <" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(0, 7))], new Range(0, 0))]

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
   |> should equal [Action (false, [Literal ("Some text and then a ", new Range(0, 21)); Note ([Literal( "bit of a note", new Range(0, 13))], new Range(0, 0)); Literal( ". And some more text.", new Range(0, 21))], new Range(0, text.Length))]

[<Test>]
let ``Notes - Block`` () =
   let text = "[[It was supposed to be Vietnamese, right?]]"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Note ([Literal ("It was supposed to be Vietnamese, right?", new Range(0, 40))], new Range(0, 0))], new Range(0, text.Length))]

[<Test>]
let ``Notes - Line breaks`` () =
   let text = properNewLines "His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.[[This section needs work.\r\nEither that, or I need coffee.\r\nDefinitely coffee.]] He looks around. Phone ringing."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Literal ("His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.", new Range(0, 114)); Note([Literal("This section needs work.", new Range(0, 24)); HardLineBreak(new Range(24, NewLineLength)); Literal("Either that, or I need coffee.", new Range(24 + NewLineLength, 30)); HardLineBreak(new Range(54 + NewLineLength, NewLineLength)); Literal("Definitely coffee.", new Range(54 + 2 * NewLineLength, 18))], new Range(0, 0)); Literal(" He looks around. Phone ringing.", new Range(0, 32))], new Range(0, text.Length))]

[<Test>]
let ``Notes - Line breaks with empty line`` () =
   let text = "His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.[[This section needs work." + NewLine(1) + "Either that, or I need coffee." + NewLine(1) + "  " + NewLine(1) + "Definitely coffee.]] He looks around. Phone ringing."
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Literal ("His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.", new Range(0, 114)); Note([Literal("This section needs work.", new Range(0, 24)); HardLineBreak(new Range(24, NewLineLength)); Literal("Either that, or I need coffee.", new Range(24 + NewLineLength, 30)); HardLineBreak(new Range(54 + NewLineLength, NewLineLength)); HardLineBreak(new Range(54 + 2 * NewLineLength, NewLineLength)); Literal("Definitely coffee.", new Range(54 + 3 * NewLineLength, 18))], new Range(0, 0)); Literal(" He looks around. Phone ringing.", new Range(0, 32))], new Range(0, text.Length))]

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
   let text = "\t EXT. BRICK'S PATIO - DAY\r\n\r\nSome Action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0, 24))], new Range(0, 0)); Action(false, [Literal ("Some Action", new Range(0, 11))], new Range(0, 11))]

[<Test>]
let ``Character - Indenting`` () =
   // white spaces have to be ignored
   let doc = "\r\n\t   LINDSEY" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(0, 7))], new Range(0, 11 + NewLineLength))]

[<Test>]
let ``Dialogue - Indenting`` () =
   let doc = "\r\n\t  LINDSEY\r\n   \t Hello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, true, [Literal ("LINDSEY", new Range(0, 7))], new Range(0, 10 + NewLineLength)); Dialogue ([Literal ("Hello, friend.", new Range(0, 14))], new Range(0, 0))]

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
   |> should equal [Centered ([Literal ("The End", new Range(0, 7))], new Range(0, 0))]

[<Test>]
let ``Transition - indenting`` () =
   let doc = "  \t  CUT TO:" |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition (false, [Literal ("CUT TO:", new Range(0, 7))], new Range(0, 0))]

[<Test>]
let ``Transition - forced indenting`` () =
   let doc = "\t > Burn to White." |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition (true, [Literal ("Burn to White.", new Range(0, 14))], new Range(0, 0))]

//===== Title page

[<Test>]
let ``Title page`` () =
   // This is quite a complex title page with inline and not inline values, emphasized spans.
   let text = "Title:" + NewLine(1) + "\t_**BRICK and STEEL**_" + NewLine(1) + "\t_**FULL RETIRED**_" + NewLine(1) + "Credit: Written by" + NewLine(2) + "Some action"
   let doc = text |> Fountain.Parse
   doc.Blocks
   |> should equal [TitlePage ([("Title", [Underline ([Bold ([Literal("BRICK and STEEL", new Range(3, 15))], new Range(1, 19))], new Range(0, 21)); HardLineBreak(new Range(21, NewLineLength)); Underline([ Bold ([Literal("FULL RETIRED", new Range(24 + NewLineLength, 12))], new Range(22 + NewLineLength, 16))], new Range(21 + NewLineLength, 18))]); ("Credit", [Literal("Written by", new Range(0, 10))])], new Range(0, text.Length - 11)); PageBreak; Action(false, [Literal("Some action", new Range(text.Length - 11, 11))], new Range(text.Length - 11, 11))]
