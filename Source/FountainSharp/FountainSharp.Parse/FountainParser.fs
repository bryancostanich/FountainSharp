// --------------------------------------------------------------------------------------
// F# Markdown (MarkdownParser.fs)
// (c) Tomas Petricek, 2012, Available under Apache 2.0 license.
// --------------------------------------------------------------------------------------
// Modifications (c) by Bryan Costanich, 2015, 2016
// --------------------------------------------------------------------------------------

module FountainSharp.Parse.Parser

open System
open System.IO
open System.Collections.Generic

open FSharp.Collections
open FountainSharp.Parse.Collections
open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Patterns.List
open FountainSharp.Parse.Patterns.String

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
  | DelimitedWith ['['; '['] [']';']'] (body, rest) -> 
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
      // until i figure out what's happening here, Note won't work, because it just pulls the text out entirely.
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

//======================================================================================
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
  // TODO: Make this StartsWithAnyCaseInsensitive
  | String.StartsWithAny [ "INT"; "EXT"; "EST"; "INT./EXT."; "INT/EXT"; "I/E" ] heading:string :: rest ->
     Some(heading.Trim(), rest)
  | String.StartsWith "." heading:string :: rest ->
     Some(heading.Trim(), rest) 
  | rest ->
     None

// CHARACTER TODO: "BOB (OS)"
let (|Character|_|) (list:string list) =
  match list with
  | [] -> None
  | head :: rest ->
    if (head.Length = 0) then 
      None
    // matches "@McAVOY"
    else if (head.StartsWith "@") then
      Some(head.Trim(), rest)
    // matches "BOB" or "BOB JOHNSON" or "R2D2" but not "25D2"
    else if (System.Char.IsUpper (head.[0]) && head |> Seq.forall (fun c -> (System.Char.IsUpper c|| System.Char.IsWhiteSpace c || System.Char.IsNumber c))) then
      Some(head.Trim(), rest)
    // matches "BOB (*)"
    //else if (
    else
      None


// DIALOG

/// Recognizes a PageBreak (3 or more consecutive equals and nothign more)
let (|PageBreak|_|) = function
  | String.StartsWithRepeated "=" text :: rest ->
    if (fst text) >= 3 then
      match (snd text).Trim() with
      | "" -> //after the trim, there should be nothing left.
         Some(PageBreak, rest)
      | _ -> 
         None
    else None
  | rest ->
     None

/// Recognizes a synposes (prefixed with `=` sign)
let (|Synopses|_|) = function
  | String.StartsWith "=" text :: rest ->
     Some(text, rest)
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
// TODO: Question: what is the Links part supposed to represent? Answer: for some reason he was creating a 
// dictionary of known links. probably for additional processing. but fountain doesn't have links, so i got 
// rid of it. but now we need to simplify this ParsingContext, probably.
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
  | Character(body, Lines.TrimBlankStart lines) ->
     yield Character(parseSpans body)
     yield! parseBlocks ctx lines
  | PageBreak(body, Lines.TrimBlankStart lines) ->
     yield PageBreak
     yield! parseBlocks ctx lines
  | Synopses(body, Lines.TrimBlankStart lines) ->
     yield Synopses(parseSpans body)
     yield! parseBlocks ctx lines
  | Lyric(body, Lines.TrimBlankStart lines) ->
     yield Lyric(parseSpans body)
     yield! parseBlocks ctx lines
  | TakeBlockLines(lines, Lines.TrimBlankStart rest) ->      
     yield Block (parseSpans (String.concat ctx.Newline lines))
     yield! parseBlocks ctx rest 

  | Lines.TrimBlankStart [] -> () 
  | _ -> failwithf "Unexpectedly stopped!\n%A" lines }
