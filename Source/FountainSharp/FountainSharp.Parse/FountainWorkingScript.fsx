#load "Collections.fs"
#load "StringParsing.fs"

open System
open System.IO
open System.Collections.Generic

open FountainSharp.Collections
open FSharp.Patterns
open FSharp.Patterns.List



//===============================================================================================
//===============================================================================================
//===============================================================================================
//===============================================================================================
//===============================================================================================
//===============================================================================================
//===============================================================================================
//===============================================================================================
//===============================================================================================
// This is where the fun really begins
module FountainTestParser =

  //====== Fountain Schema/Syntax Definition

  /// Represents inline formatting inside a block. This can be literal (with text), various
  /// formattings (string, emphasis, etc.), hyperlinks, etc.
  type FountainSpanElement =
    | Literal of string // some text
    | Strong of FountainSpans // **some bold text**
    | Italic of FountainSpans // *some italicized text*
    | Underline of FountainSpans // _some underlined text_
    | Note of FountainSpans // [[this is my note]]
    | HardLineBreak

  /// A type alias for a list of `FountainSpan` values
  and FountainSpans = list<FountainSpanElement>

  /// A block represents a (possibly) multi-line element of a fountain document.
  /// Blocks are headings, action blocks, dialogue blocks, etc. 
  type FountainBlockElement = 
    | Block of FountainSpans
    | Section of int * FountainSpans
    | Span of FountainSpans
    | Lyric of FountainSpans
    | SceneHeading of FountainSpans //TODO: Should this really just be a single span? i mean, you shouldn't be able to style/inline a scene heading, right?

  /// A type alias for a list of blocks
  and FountainBlocks = list<FountainBlockElement>

  //====== Parser
  // Part 1: Inline Formatting

  /// Succeeds when the specified character list starts with an escaped 
  /// character - in that case, returns the character and the tail of the list
  let inline (|EscapedChar|_|) input = 
    match input with
    | '\\'::( ( '*' | '\\' | '`' | '_' | '{' | '}' | '[' | ']' 
              | '(' | ')' | '>' | '#' | '.' | '!' | '+' | '-' | '$') as c) ::rest -> Some(c, rest)
    | _ -> None

  /// matches a delimited list of characters that starts with some sub list, called the 
  /// delimiter bracket, (like ['*'; '*'] for bold) that occurs at the beginning and the end of the text.
  /// returns a wrapped list and then the rest of the characters after the delimited text.
  let (|DelimitedText|_|) delimiterBracket input =
    let startl, endl = delimiterBracket, delimiterBracket
    // Like List.partitionUntilEquals, but skip over escaped characters
    let rec loop acc = function
      | EscapedChar(x, xs) -> loop (x::'\\'::acc) xs
      | input when List.startsWith endl input -> Some(List.rev acc, input)
      | x::xs -> loop (x::acc) xs
      | [] -> None
    // If it starts with 'startl', let's search for 'endl'
    if List.startsWith delimiterBracket input then
      match loop [] (List.skip delimiterBracket.Length input) with
      | Some(pre, post) -> Some(pre, List.skip delimiterBracket.Length post)
      | None -> None
    else None

  /// recognizes emphasized text of Italic, Bold, etc.
  /// take somethign like "*some text* some more text" and return a sequence of TextSpans: italic<"some text">::rest
  let (|Emphasized|_|) = function
    // if it starts with either `_` or `*`
    //   1) the code `(('_' | '*')` :: tail)` decomposes the input into a sequence of either `'_'::tail` or `'*'::tail`
    //   2) `as input` binds that sequence to a variable
    | (('_' | '*') :: tail) as input ->
      match input with
      // the *** case in which it is both italic and strong
      | DelimitedText ['*'; '*'; '*'] (body, rest) -> 
          Some(body, Italic >> List.singleton >> Strong, rest)
      | DelimitedText['*'; '*'] (body, rest) -> 
          Some(body, Strong, rest)
      | DelimitedText['_'] (body, rest) ->
          Some(body, Underline, rest)
      | DelimitedText['*'] (body, rest) -> 
          Some(body, Italic, rest)
      | _ -> None
    | _ -> None

  /// recognizes notes which start with "[[" and end with "]]"
  let (|Note|_|) = function
    // the *** case in which it is both italic and strong
    | FSharp.Patterns.List.DelimitedWith ['['; '['] [']';']'] (body, rest) -> 
        Some (body, Note, rest)
    | _ -> None


  /// Parses a body of a block and recognizes all inline tags.
  /// returns a sequence of FountainSpan
  let rec parseChars acc input = seq {

    // Zero or one literals, depending whether there is some accumulated input
    let accLiterals = Lazy.Create(fun () ->
      if List.isEmpty acc then [] 
      else [Literal(String(List.rev acc |> Array.ofList))] )

    match input with 

    // Recognizes explicit line-break at the end of line
    | ' '::' '::('\n' | '\r')::rest
    | ' '::' '::'\r'::'\n'::rest ->
        yield! accLiterals.Value
        yield HardLineBreak
        yield! parseChars [] rest

    // Encode & as an HTML entity
    | '&'::'a'::'m'::'p'::';'::rest 
    | '&'::rest ->
        yield! parseChars (';'::'p'::'m'::'a'::'&'::acc) rest      

    // Ignore escaped characters that might mean something else
    | EscapedChar(c, rest) ->
        yield! parseChars (c::acc) rest

    // Handle emphasised text
    | Emphasized (body, f, rest) ->
        yield! accLiterals.Value
        let body = parseChars [] body |> List.ofSeq
        yield f(body)
        yield! parseChars [] rest

    // Notes
    | Note (body, f, rest) ->
        yield! accLiterals.Value
        let body = parseChars [] body |> List.ofSeq
        //TODO: why won't it accept this?
        //yield f(body)
        yield! parseChars [] rest

    // This calls itself recursively on the rest of the list
    | x::xs -> 
        yield! parseChars (x::acc) xs 
    | [] ->
        yield! accLiterals.Value }

  /// Parse body of a block into a list of Markdown inline spans      
  let parseSpans (String.TrimBoth s) = 
    parseChars [] (s.ToCharArray() |> List.ofArray) |> List.ofSeq

  //====== Parser
  // Part 2: Block Formatting

  /// Recognizes a Section (# Some section, ## another section), prefixed with '#'s
  let (|Section|_|) = function
    | String.StartsWithRepeated "#" (n, header) :: rest ->
        let header = 
          // Drop "##" at the end, but only when it is preceded by some whitespace
          // (For example "## Hello F#" should be "Hello F#")
          if header.EndsWith "#" then
            let noHash = header.TrimEnd [| '#' |]
            if noHash.Length > 0 && Char.IsWhiteSpace(noHash.Chars(noHash.Length - 1)) 
            then noHash else header
          else header        
        Some(n, header.Trim(), rest)
    | rest ->
        None

  /// Recognizes a SceneHeading (prefixed with INT/EXT, etc. or a single period)
  let (|SceneHeading|_|) = function
    //TODO: why doesn't this work? 
//    | String.StartsWithAny [ "INT"; "EXT"; "EST"; "INT./EXT."; "INT/EXT"; "I/E" ] heading:string :: rest ->
//       Some(heading.Trim(), rest)
    | String.StartsWith "." heading:string :: rest ->
       Some(heading.Trim(), rest) 
    | rest ->
       None

  /// Recognizes a Lyric (prefixed with ~)
  let (|Lyric|_|) = function
    | String.StartsWith "~" lyric:string :: rest ->
        Some(lyric.Trim(), rest)
    | rest ->
        None

  /// Splits input into lines until whitespace
  let (|LinesUntilListOrWhite|) = 
    List.partitionUntil (function
      | String.WhiteSpace -> true 
      | _ -> false
    )

  /// Splits input into lines until not-indented line and the rest.
  let (|LinesUntilListOrUnindented|) =
    List.partitionUntilLookahead (function 
      | String.Unindented::_ 
      | String.WhiteSpace::String.WhiteSpace::_ -> true | _ -> false)

  /// Takes lines that belong to a continuing block until 
  /// a white line or start of other block-item is found
  let (|TakeBlockLines|_|) input = 
    match List.partitionWhileLookahead (function
      | Section _ -> false
      | String.WhiteSpace::_ -> false
      | _ -> true) input with
    | matching, rest when matching <> [] -> Some(matching, rest)
    | _ -> None

  /// Defines a context for the main `parseBlocks` function
  // TODO: Question: what is the Links part supposed to represent?
  type ParsingContext = 
    { 
      Newline : string 
    }

  /// Parse a list of lines into a sequence of fountain blocks
  let rec parseBlocks (ctx:ParsingContext) lines = seq {
    match lines with

    // Recognize remaining types of blocks/paragraphs
    | SceneHeading(body, Lines.TrimBlankStart lines) ->
       yield SceneHeading(parseSpans body)
       yield! parseBlocks ctx lines
    | Section(n, body, Lines.TrimBlankStart lines) ->
       yield Section(n, parseSpans body)
       yield! parseBlocks ctx lines
    | Lyric(body, Lines.TrimBlankStart lines) ->
       yield Lyric(parseSpans body)
       yield! parseBlocks ctx lines
    | TakeBlockLines(lines, Lines.TrimBlankStart rest) ->      
       yield Block (parseSpans (String.concat ctx.Newline lines))
       yield! parseBlocks ctx rest 

    | Lines.TrimBlankStart [] -> () 
    | _ -> failwithf "Unexpectedly stopped!\n%A" lines }



open FountainTestParser

/// Representation of a Fountain document - the representation of Blocks
/// uses an F# discriminated union type and so is best used from F#.
// TODO: this doesn't really need a full blown type for one member (i removed the Links that was part of the markdown doc)
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
open FountainTestParser

let string1 = "*some italic text*"
let string2 = "**some bold text**"
let string3 = "***some bold italic text***"
let string4 = "_some underlined text_"
let string5 = "some text that's not emphasized"
let string6 = "pretty sure this will fail *some italic text **with some bold** in the middle*"
let string7 = @"EXT. BRICK'S PATIO - DAY

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

They drink *long* and _well_ from the beers.

.BINOCULARS A FORCED SCENE HEADING - LATER

# This is a section with level 1
## This is a section with level 2

And then there's a long beat.  
Longer than is funny.  
Long enough to be depressing.

~Some Lyrics
~Some more lyrics
"


let testString string = 
  match string with
  | Emphasized s -> 
    printfn "it's emphasized: %A" s
    //couldn't make any of this work. the middle part expects a function of `FountainSpans -> FountainSpan`
//    match s with
//    | (body,Strong(spans),rest) -> printfn "it's bold"
//    | (body,Italic(spans),rest) -> printfn "it's italic"
//    | (body,Underline(spans),rest) -> printfn "it's underlined"
//    | _ -> printfn "not sure how we got here!"
  | _ -> printfn "it's not emphasized."


// returns a sequence of characters from a string
let explode (s:string) =
  [for c in s -> c]

explode string1 |> testString
explode string2 |> testString
explode string3 |> testString
explode string4 |> testString
explode string5 |> testString
explode string6 |> testString
explode string7 |> testString

Fountain.Parse string7