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

let properNewLines (text: string) = text.Replace("\r\n", System.Environment.NewLine)

//===== Block Elements ==============================================================

//===== Boneyard

// TODO: should we support boneyards in a single line? Also in the middle of the line?
[<Test>]
let ``Boneyard`` () =
   let doc = "/*\r\nThis is a simple comment\r\n*/" |> Fountain.Parse
   doc.Blocks
   |> should equal [Boneyard ("This is a simple comment", Range.empty)]

//===== Scene Headings
[<Test>]
let ``Basic Scene Heading`` () =
   let doc = "EXT. BRICK'S PATIO - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Forced (".") Scene Heading`` () =
   let doc = ".BINOCULARS A FORCED SCENE HEADING - LATER" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (true, [Literal ("BINOCULARS A FORCED SCENE HEADING - LATER", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Forced (".") Scene Heading with line breaks and action`` () =
   let heading = "BRICK'S PATIO - DAY";
   let doc = "." + heading + "\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (true, [Literal (heading, new Range(0,0))], new Range(0,0)); Action (false, [HardLineBreak(new Range(0,0)); Literal ("Some Action", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Forced (".") Scene Heading with more line breaks and action`` () =
   let heading = "BRICK'S PATIO - DAY";
   let doc = "." + heading + "\r\n\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (true, [Literal (heading, new Range(0,0))], new Range(0,0)); Action(false, [HardLineBreak(new Range(0,0)); HardLineBreak(new Range(0,0)); Literal("Some Action", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Forced (".") Scene Heading - No empty line after`` () =
   let doc = ".BINOCULARS A FORCED SCENE HEADING - LATER\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal (".BINOCULARS A FORCED SCENE HEADING - LATER", Range.empty); HardLineBreak(Range.empty); Literal ("Some Action", Range.empty)], Range.empty)]

[<Test>]
let ``Lowercase known scene heading`` () =
   let doc = "ext. brick's pool - day" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("ext. brick's pool - day", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Known INT Scene Head`` () =
   let doc = "INT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT DOGHOUSE - DAY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Known EXT Scene Head`` () =
   let doc = "EXT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("EXT DOGHOUSE - DAY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Known EST Scene Head`` () =
   let doc = "EST DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("EST DOGHOUSE - DAY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Known INT./EXT Scene Head`` () =
   let doc = "INT./EXT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT./EXT DOGHOUSE - DAY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Known INT/EXT Scene Head`` () =
   let doc = "INT/EXT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("INT/EXT DOGHOUSE - DAY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Known I/E Scene Head`` () =
   let doc = "I/E DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading (false, [Literal ("I/E DOGHOUSE - DAY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Scene Heading with line breaks and action`` () =
   let doc = "EXT. BRICK'S PATIO - DAY\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0,0))], new Range(0,0)); Action (false, [HardLineBreak(new Range(0,0)); Literal ("Some Action", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Scene Heading with more line breaks and action`` () =
   let doc = "EXT. BRICK'S PATIO - DAY\r\n\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0,0))], new Range(0,0)); Action(false, [HardLineBreak(new Range(0,0)); HardLineBreak(new Range(0,0)); Literal("Some Action", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Scene Heading - No empty line after`` () =
   let doc = "EXT. BRICK'S PATIO - DAY\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal ("EXT. BRICK'S PATIO - DAY", Range.empty); HardLineBreak(Range.empty); Literal ("Some Action", Range.empty)], Range.empty)]

//===== Action
[<Test>]
let ``Action - With line breaks`` () =
   let doc = "EXT. BRICK'S PATIO - DAY\r\n\r\nSome Action\r\n\r\nSome More Action" |> Fountain.Parse
   doc.Blocks
   |> should equal   [SceneHeading (false, [Literal ("EXT. BRICK'S PATIO - DAY", new Range(0,0))], new Range(0,0)); Action (false, [HardLineBreak(new Range(0,0)); Literal ("Some Action", new Range(0,0)); HardLineBreak(new Range(0,0)); HardLineBreak(new Range(0,0)); Literal ("Some More Action", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Action - With line breaks and no heading`` () =
   let doc = "Natalie looks around at the group, TIM, ROGER, NATE, and VEEK.\n
TIM, is smiling broadly." |> Fountain.Parse
   doc.Blocks
   |> should equal  [Action (false, [Literal ("Natalie looks around at the group, TIM, ROGER, NATE, and VEEK.", new Range(0,0)); 
                    HardLineBreak(new Range(0,0)); HardLineBreak(new Range(0,0)); Literal ("TIM, is smiling broadly.", new Range(0,0))], new Range(0,0))]




//===== Synopses

[<Test>]
let ``Basic Synopses`` () =
   let doc = "= Here is a synopses of this fascinating scene." |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses ([Literal (" Here is a synopses of this fascinating scene.", new Range(0,0))], new Range(0,0))]


//===== Character

[<Test>]
let ``Character - Normal`` () =
   let doc = "LINDSEY" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("LINDSEY", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Character - With parenthetical extension`` () =
   let character = "LINDSEY (on the radio)"
   let doc = character |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal (character, new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Character - With invalid parenthetical extension`` () =
   let character = "LINDSEY (on the Radio)"
   let doc = character |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal (character, new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Character - With whitespace`` () =
   let doc = "THIS IS ALL UPPERCASE BUT HAS WHITESPACE" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("THIS IS ALL UPPERCASE BUT HAS WHITESPACE", new Range(0,0))], new Range(0,0))]
    
[<Test>]
let ``Character - With Numbers`` () =
   let doc = "R2D2" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("R2D2", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Character - Number first`` () =
   let doc = "25D2" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("25D2", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Character - Forced with at sign`` () =
   let doc = "@McAvoy" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (true, [Literal ("McAvoy", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Character - with forced at and parenthetical extension`` () =
   let doc = "@McAvoy (OS)" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (true, [Literal ("McAvoy (OS)", new Range(0,0))], new Range(0,0))]


//===== Parenthetical

[<Test>]
let ``Parenthetical `` () =
   let doc = "LINDSEY\r\n(quietly)" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("LINDSEY", new Range(0,0))], new Range(0,0)); Parenthetical ([Literal ("quietly", new Range(0,0))], new Range(0,0))];

[<Test>]
let ``Parenthetical - After Dialogue`` () =
   let doc = "LINDSEY\r\n(quietly)\r\nHello, friend.\r\n(loudly)\r\nFriendo!" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("LINDSEY", Range.empty)], Range.empty); Parenthetical ([Literal ("quietly", Range.empty)], Range.empty); Dialogue ([Literal ("Hello, friend.", Range.empty)], Range.empty); Parenthetical ([Literal ("loudly", Range.empty)], Range.empty); Dialogue ([Literal ("Friendo!", Range.empty)], Range.empty)];


//===== Dialogue

[<Test>]
let ``Dialogue - Normal`` () =
   let doc = "LINDSEY\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("LINDSEY", new Range(0,0))], new Range(0,0)); Dialogue ([Literal ("Hello, friend.", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Dialogue - After Parenthetical`` () =
   let doc = "LINDSEY\r\n(quietly)\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("LINDSEY", new Range(0,0))], new Range(0,0)); Parenthetical ([Literal ("quietly", new Range(0,0))], new Range(0,0)); Dialogue ([Literal ("Hello, friend.", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Dialogue - With line break`` () =
   let doc = "DEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n\r\n  Hit or stand sir?" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("DEALER", Range.empty)], Range.empty);  Dialogue ([Literal ("Ten.", Range.empty); HardLineBreak(Range.empty); Literal ("Four.", Range.empty); HardLineBreak(Range.empty); Literal ("Dealer gets a seven.", Range.empty); HardLineBreak(Range.empty); HardLineBreak(Range.empty); Literal ("Hit or stand sir?", Range.empty)], Range.empty)]

[<Test>]
let ``Dialogue - With invalid line break`` () =
   let doc = "DEALER\r\nTen.\r\nFour.\r\nDealer gets a seven.\r\n\r\nHit or stand sir?" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character (false, [Literal ("DEALER", Range.empty)], Range.empty);  Dialogue ([Literal ("Ten.", Range.empty); HardLineBreak(Range.empty); Literal ("Four.", Range.empty); HardLineBreak(Range.empty); Literal ("Dealer gets a seven.", Range.empty)], Range.empty); Action (false, [Literal ("Hit or stand sir?", Range.empty)], Range.empty)]
   // this test now fails: parsing places line break be after last Action

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
   |> should equal [Synopses ([Literal ("=", new Range(0,0))], new Range(0,0))]

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
   |> should equal [Synopses ([Literal ("====== blah", new Range(0,0))], new Range(0,0))]


//===== Lyrics

[<Test>]
let ``Lyric - normal`` () =
   let doc = "~Birdy hop, he do. He hop a long." |> Fountain.Parse
   doc.Blocks
   |> should equal [Lyric ([Literal ("Birdy hop, he do. He hop a long.", new Range(0,0))], new Range(0,0))]


//===== Transition

[<Test>]
let ``Transition - normal`` () =
   let doc = "CUT TO:" |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition (false, [Literal ("CUT TO:", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Transition - forced`` () =
   let doc = "> Burn to White." |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition (true, [Literal ("Burn to White.", new Range(0,0))], new Range(0,0))]

//===== Centered

[<Test>]
let ``Centered `` () =
   let doc = ">The End<" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", Range.empty)], Range.empty)]

[<Test>]
let ``Centered - with spaces`` () =
   let doc = "> The End <" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", Range.empty)], Range.empty)]

//===== Line Breaks

[<Test>]
let ``Line Breaks`` () =
   let doc = "Murtaugh, springing...\n\nAn explosion of sound...\nAs it rises like an avenging angel ...\nHovers, shattering the air \n\nScreaming, chaos, frenzy.\nThree words that apply to this scene." |> Fountain.Parse
   doc.Blocks
   |> should equal ([Action (false, [Literal( "Murtaugh, springing...", Range.empty); HardLineBreak(Range.empty); HardLineBreak(Range.empty); Literal("An explosion of sound...", Range.empty); HardLineBreak(Range.empty); Literal("As it rises like an avenging angel ...", Range.empty); HardLineBreak(Range.empty); Literal("Hovers, shattering the air ", Range.empty); HardLineBreak(Range.empty); HardLineBreak(Range.empty); Literal ("Screaming, chaos, frenzy.", Range.empty); HardLineBreak(Range.empty); Literal( "Three words that apply to this scene.", Range.empty)], Range.empty)])

//===== Notes
[<Test>]
let ``Notes - Inline`` () =
   let doc = "Some text and then a [[bit of a note]]. And some more text." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("Some text and then a ", Range.empty); Note ([Literal( "bit of a note", Range.empty)], Range.empty); Literal( ". And some more text.", Range.empty)], Range.empty)]

[<Test>]
let ``Notes - Block`` () =
   let doc = "[[It was supposed to be Vietnamese, right?]]" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Note ([Literal ("It was supposed to be Vietnamese, right?", Range.empty)], Range.empty)], Range.empty)]

[<Test>]
let ``Notes - Line breaks`` () =
   let doc = "His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.[[This section needs work.\r\nEither that, or I need coffee.\r\nDefinitely coffee.]] He looks around. Phone ringing." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Literal ("His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.", Range.empty); Note([Literal("This section needs work.", Range.empty); HardLineBreak(Range.empty); Literal("Either that, or I need coffee.", Range.empty); HardLineBreak(Range.empty); Literal("Definitely coffee.", Range.empty);], Range.empty); Literal(" He looks around. Phone ringing.", Range.empty)], Range.empty)]

[<Test>]
let ``Notes - Line breaks with empty line`` () =
   let doc = "His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.[[This section needs work.\r\nEither that, or I need coffee.\r\n  \r\nDefinitely coffee.]] He looks around. Phone ringing." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action(false, [Literal ("His hand is an inch from the receiver when the phone RINGS. Scott pauses for a moment, suspicious for some reason.", Range.empty); Note([Literal("This section needs work.", Range.empty); HardLineBreak(Range.empty); Literal("Either that, or I need coffee.", Range.empty); HardLineBreak(Range.empty); HardLineBreak(Range.empty); Literal("Definitely coffee.", Range.empty);], Range.empty); Literal(" He looks around. Phone ringing.", Range.empty)], Range.empty)]

//===== Boneyard (Comments)
//TODO: not implemented yet.

//===== Span Elements ==============================================================

//===== Emphasis

[<Test>]
let ``Emphasis - Bold`` () =
   let doc = "**This is bold Text**" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Strong ([Literal ("This is bold Text", new Range(0,0))], new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Emphasis - Italics`` () =
   let doc = "*This is italic Text*" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Italic ([Literal ("This is italic Text", new Range(0,0))], new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Emphasis - Bold Italic`` () =
   let doc = "***This is bold Text***" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Strong ([Italic ([Literal ("This is bold Text", new Range(0,0))], new Range(0,0))], new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Emphasis - Nested Bold Italic`` () =
 let doc = "**This is bold *and Italic Text***" |> Fountain.Parse
 doc.Blocks
   |> should equal [Action (false, [Strong ([Literal ("This is bold ", new Range(0,0)); Italic ([Literal ("and Italic Text", new Range(0,0)) ], new Range(0,0)) ], new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Emphasis - Nested Underline Italic`` () =
   let doc = "From what seems like only INCHES AWAY.  _Steel's face FILLS the *Leupold Mark 4* scope_." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("From what seems like only INCHES AWAY.  ", new Range(0,0)); Underline ([Literal ("Steel's face FILLS the ", new Range(0,0)); Italic ([Literal ("Leupold Mark 4", new Range(0,0))], new Range(0,0)); Literal (" scope", new Range(0,0))], new Range(0,0)); Literal (".", new Range(0,0))], new Range(0,0))]

[<Test>]
let ``Emphasis - with escapes`` () =
   let doc = "Steel enters the code on the keypad: **\*9765\***" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("Steel enters the code on the keypad: ", new Range(0,0)); Strong ([Literal("*9765*", Range.empty)], Range.empty)], Range.empty)]

[<Test>]
let ``Emphasis - italics with spaces to left`` () =
   // this is not italic, as there is a space on the left of the second one
   let doc = "He dialed *69 and then *23, and then hung up." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal ("He dialed *69 and then *23, and then hung up.", Range.empty)], Range.empty)]

[<Test>]
let ``Emphasis - italics with spaces to left but escaped`` () =
   let doc = "He dialed *69 and then 23\*, and then hung up.." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal( "He dialed *69 and then 23*, and then hung up..", Range.empty)], Range.empty)]

[<Test>]
let ``Emphasis - between line breaks`` () =
   let doc = "As he rattles off the long list, Brick and Steel *share a look.\r\n\
             \r\n\
             This is going to be BAD.*" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action (false, [Literal( "As he rattles off the long list, Brick and Steel *share a look.", Range.empty); HardLineBreak(Range.empty); HardLineBreak(Range.empty); Literal ("This is going to be BAD.*", Range.empty)], Range.empty)]

//===== Indenting
// TODO

