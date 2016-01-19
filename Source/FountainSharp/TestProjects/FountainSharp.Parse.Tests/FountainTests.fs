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

//===== Scene Headings
[<Test>]
let ``Basic Scene Heading`` () =
   let doc = "EXT. BRICK'S PATIO - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "EXT. BRICK'S PATIO - DAY"]]

[<Test>]
let ``Forced (".") Scene Heading`` () =
   let doc = ".BINOCULARS A FORCED SCENE HEADING - LATER" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "BINOCULARS A FORCED SCENE HEADING - LATER"]]

[<Test>]
let ``Lowercase known scene heading`` () =
   let doc = "ext. brick's pool - day" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "ext. brick's pool - day"]]

[<Test>]
let ``Known INT Scene Head`` () =
   let doc = "INT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "INT DOGHOUSE - DAY"]]

[<Test>]
let ``Known EXT Scene Head`` () =
   let doc = "EXT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "EXT DOGHOUSE - DAY"]]

[<Test>]
let ``Known EST Scene Head`` () =
   let doc = "EST DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "EST DOGHOUSE - DAY"]]

[<Test>]
let ``Known INT./EXT Scene Head`` () =
   let doc = "INT./EXT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "INT./EXT DOGHOUSE - DAY"]]

[<Test>]
let ``Known INT/EXT Scene Head`` () =
   let doc = "INT/EXT DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "INT/EXT DOGHOUSE - DAY"]]

[<Test>]
let ``Known I/E Scene Head`` () =
   let doc = "I/E DOGHOUSE - DAY" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "I/E DOGHOUSE - DAY"]]

[<Test>]
let ``Scene Heading with line breaks and action`` () =
   let doc = "EXT. BRICK'S PATIO - DAY\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "EXT. BRICK'S PATIO - DAY"]; Action [HardLineBreak; Literal "Some Action"]]


[<Test>]
let ``Scene Heading with more line breaks and action`` () =
   let doc = "EXT. BRICK'S PATIO - DAY\r\n\r\n\r\nSome Action" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal "EXT. BRICK'S PATIO - DAY"]; Action [HardLineBreak]; Action [HardLineBreak]; Action [Literal "\nSome Action"]]


//===== Synopses

[<Test>]
let ``Basic Synopses`` () =
   let doc = "= Here is a synopses of this fascinating scene." |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses [Literal "Here is a synopses of this fascinating scene."]]


//===== Character

[<Test>]
let ``Character - Normal`` () =
   let doc = "LINDSEY" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "LINDSEY"]]

[<Test>]
let ``Character - With parenthetical extension`` () =
   let doc = "LINDSEY (on the radio)" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "LINDSEY (on the radio)"]]

[<Test>]
let ``Character - With whitespace`` () =
   let doc = "THIS IS ALL UPPERCASE BUT HAS WHITESPACE" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "THIS IS ALL UPPERCASE BUT HAS WHITESPACE"]]

[<Test>]
let ``Character - With Numbers`` () =
   let doc = "R2D2" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "R2D2"]]

[<Test>]
let ``Character - Number first`` () =
   let doc = "25D2" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal "25D2"]]

[<Test>]
let ``Character - Forced with at sign`` () =
   let doc = "@McAvoy" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "McAvoy"]]

[<Test>]
let ``Character - with forced at and parenthetical extension`` () =
   let doc = "@McAvoy (OS)" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "McAvoy (OS)"]]


//===== Parenthetical

[<Test>]
let ``Parenthetical `` () =
   let doc = "LINDSEY\r\n(quietly)" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "LINDSEY"]; Parenthetical [Literal "quietly"]];

[<Test>]
let ``Parenthetical - After Dialogue`` () =
   let doc = "LINDSEY\r\n(quietly)\r\nHello, friend.\r\n(loudly)\r\nFriendo!" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "LINDSEY"]; Parenthetical [Literal "quietly"]; Dialogue [Literal "Hello, friend."]; Parenthetical [Literal "loudly"]; Dialogue [Literal "Friendo!"]];


//===== Dialogue

[<Test>]
let ``Dialogue - Normal`` () =
   let doc = "LINDSEY\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "LINDSEY"]; Dialogue [Literal "Hello, friend."]]

[<Test>]
let ``Dialogue - After Parenthetical`` () =
   let doc = "LINDSEY\r\n(quietly)\r\nHello, friend." |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal "LINDSEY"]; Parenthetical [Literal "quietly"]; Dialogue [Literal "Hello, friend."]]


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
   |> should equal [Synopses [Literal "="]]

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
   |> should equal [Synopses [Literal "====== blah"]]


//===== Lyrics

[<Test>]
let ``Lyric - normal`` () =
   let doc = "~Birdy hop, he do. He hop a long." |> Fountain.Parse
   doc.Blocks
   |> should equal [Lyric [Literal "Birdy hop, he do. He hop a long."]]


//===== Transition

[<Test>]
let ``Transition - normal`` () =
   let doc = "CUT TO:" |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition [Literal "CUT TO:"]]

[<Test>]
let ``Transition - forced`` () =
   let doc = "> Burn to White." |> Fountain.Parse
   doc.Blocks
   |> should equal [Transition [Literal "Burn to White."]]

//===== Centered

[<Test>]
let ``Centered `` () =
   let doc = ">The End<" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered [Literal "The End"]]

// TODO: wtf doesn't this compile either?
[<Test>]
let ``Centered - with spaces`` () =
   let doc = "> The End <" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered [Literal "The End"]]

//===== Line Breaks

[<Test>]
let ``Line Breaks`` () =
   let doc = """Murtaugh, springing...\n\nAn explosion of sound...\nAs it rises like an avenging angel ...\nHovers, shattering the air \n\nScreaming, chaos, frenzy.\nThree words that apply to this scene.""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal "Murtaugh, springing...\n\nAn explosion of sound...\nAs it rises like an avenging angel ...\nHovers, shattering the air \n\nScreaming, chaos, frenzy.\nThree words that apply to this scene."]]


//===== Notes
[<Test>]
let ``Notes - Inline`` () =
   let doc = "Some text and then a [[bit a of a note]]. And some more text." |> Fountain.Parse
   doc.Blocks
   // TODO: figure out the right output here. 
   |> should equal [Action [Literal "fails anyway, it drops the note right now."]]
   //|> should equal [Action [Literal "Some text and then a "];[Note [Literal"bit of a note"]]; Literal ". And some more text."]

[<Test>]
let ``Notes - Block`` () =
   let doc = "[[It was supposed to be Vietnamese, right?]]" |> Fountain.Parse
   doc.Blocks
   |> should equal [Note [Literal "It was supposed to be Vietnamese, right?"]]

//===== Boneyard (Comments)
//TODO: not implemented yet.

//===== Span Elements ==============================================================

//===== Emphasis

[<Test>]
let ``Emphasis - Bold`` () =
   let doc = "**This is bold Text**" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Strong [Literal "This is bold Text"]]]

[<Test>]
let ``Emphasis - Italics`` () =
   let doc = "*This is italic Text*" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Italic [Literal "This is italic Text"]]]

[<Test>]
let ``Emphasis - Bold Italic`` () =
   let doc = "***This is bold Text***" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Strong [Italic [Literal "This is bold Text"]]]]

//TODO: i don't even know how to write this test. it fails anyway. (look at next one, for clues)
[<Test>]
let ``Emphasis - Nested Bold Italic`` () =
 let doc = "**This is bold *and Italic Text***" |> Fountain.Parse
 doc.Blocks
   |> should equal [Action [Literal "need to figure out how to write the value here."]]
// |> should equal [Action [Strong [Literal "This is bold "] [Italic [Literal "and Italic Text"]]]]

[<Test>]
let ``Emphasis - Nested Italic Bold`` () =
   let doc = "From what seems like only INCHES AWAY.  _Steel's face FILLS the *Leupold Mark 4* scope_." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal "From what seems like only INCHES AWAY.  "; Underline [Literal "Steel's face FILLS the "; Italic [Literal "Leupold Mark 4"]; Literal " scope"]; Literal "."]]

[<Test>]
let ``Emphasis - with escapes`` () =
   let doc = "Steel enters the code on the keypad: **\*9765\***" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal "Steel enters the code on the keypad: "; Strong [Literal "*9765*"]]]

[<Test>]
let ``Emphasis - italics with spaces to left`` () =
   let doc = "He dialed *69 and then *23, and then hung up." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal "He dialed "; Italic [Literal "69 and then "]; Literal "23, and then hung up."]]

[<Test>]
let ``Emphasis - italics with spaces to left but escaped`` () =
   let doc = "He dialed *69 and then 23\*, and then hung up.." |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal "He dialed *69 and then 23*, and then hung up.."]]

// TODO: are line breaks being recognized properly here? i think not. i think i need more line break cases
[<Test>]
let ``Emphasis - between line breaks`` () =
   let doc = """As he rattles off the long list, Brick and Steel *share a look.\r\n\
             \r\n\
             This is going to be BAD.*""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal @"As he rattles off the long list, Brick and Steel *share a look.\r\n\"]; Action [Literal @"\r\n\"]; Action [Literal "This is going to be BAD.*"]]

//===== Indenting
// TODO

