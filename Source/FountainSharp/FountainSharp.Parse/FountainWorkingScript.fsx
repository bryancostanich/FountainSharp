#load "Collections.fs"
#load "StringParsing.fs"
#load "FountainSyntax.fs"
#load "FountainParser.fs"

open System
open System.IO
open System.Collections.Generic

open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Patterns.Lines
open FountainSharp.Parse
open FountainSharp.Parse.Parser


/// Representation of a Fountain document - the representation of Blocks
/// uses an F# discriminated union type and so is best used from F#.
// TODO: this doesn't really need a full blown type for one member (i removed the Links that was part of the markdown doc)
// maybe do a dictionary of character names though. that could be useful.
type FountainDocument(blocks) =
  /// Returns a list of blocks in the document
  member x.Blocks : FountainBlocks = blocks


/// Static class that provides methods for formatting 
/// and transforming Markdown documents.
type Fountain =
  /// Parse the specified text into a MarkdownDocument. Line breaks in the
  /// inline HTML (etc.) will be stored using the specified string.
  static member Parse(text, newline) =
    use reader = new StringReader(text)
    let lines = 
      [ let line = ref ""
        while (line := reader.ReadLine(); line.Value <> null) do
          yield line.Value ]
    let (Lines.TrimBlank lines) = lines
    let ctx : ParsingContext = { Newline = newline }
    let blocks = lines |> parseBlocks ctx |> List.ofSeq
    FountainDocument(blocks)

  /// Parse the specified text into a MarkdownDocument.
  static member Parse(text) =
    Fountain.Parse(text, Environment.NewLine)


//======= TESTING CODE

let string1 = "*some italic text*"
let string2 = "**some bold text**"
let string3 = "***some bold italic text***"
let string4 = "_some underlined text_"
let string5 = "some text that's not emphasized"
let string6 = "pretty sure this will fail *some italic text **with some bold** in the middle*"
let string7 = @"EXT. BRICK'S PATIO - DAY

= Here is a synopses of this fascinating scene.

A gorgeous day.  The sun is shining.  But BRICK BRADDOCK, retired police detective, is sitting quietly, contemplating -- something.

The SCREEN DOOR slides open and DICK STEEL, his former partner and fellow retiree, emerges with two cold beers.

STEEL
Does a bear crap in the woods?

Steel sits.  They laugh at the dumb joke.

[[Some notes about the scene]]

STEEL
(beer raised)
To retirement.

BRICK
To retirement.

@McAVOY
Oy, vay.

They drink *long* and _well_ from the beers.

.BINOCULARS A FORCED SCENE HEADING - LATER

# This is a section with level 1
## This is a section with level 2

And then there's a long beat.  
Longer than is funny.  
Long enough to be depressing.

~Some Lyrics
~Some more lyrics

Here comes a page break!

===

> ACT II <

"

Fountain.Parse string7