// --------------------------------------------------------------------------------------
// F# Markdown (MarkdownParser.fs)
// (c) Tomas Petricek, 2012, Available under Apache 2.0 license.
// --------------------------------------------------------------------------------------
// Modifications (c) by Bryan Costanich, 2015, 2016
// --------------------------------------------------------------------------------------

module internal FountainSharp.Parse.Parser

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
// TODO: some of these should be stripped out.
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
    //| DelimitedText ['*'; '*'; '*'] (body, rest) ->
    //  Some(body, Italic >> List.singleton >> Strong, rest)
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
  // 2015.01.07 Bryan Costanich - change Lazy.Create to Lazy<char []> because of some ambiguation err when building as a PCL
  let accLiterals = Lazy<char []>.Create(fun () ->
    if List.isEmpty acc then [] 
    else [Literal(String(List.rev acc |> Array.ofList),(new Range(0,0)))] )

  match input with 

  // TODO: will need this for dialogue hard line breaks
  //// Recognizes explicit line-break at the end of line
  //| ' '::' '::('\n' | '\r')::rest
  //| ' '::' '::'\r'::'\n'::rest ->
  //    yield! accLiterals.Value
  //    yield HardLineBreak
  //    yield! parseChars [] rest

  // markdown requires two spaces and then \r or \n, but fountain 
  // recognizes without
  // Recognizes explicit line-break at the end of line
  | ('\n' | '\r')::rest
  | '\r'::'\n'::rest ->
    //System.Diagnostics.Debug.WriteLine("found a hardlinebreak")
    yield! accLiterals.Value
    yield HardLineBreak(new Range(0,0))
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
      yield f(body, (new Range(0,0)))
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
// trimming off \r\n?
//let parseSpans (s) = 
let parseSpans ((*String.TrimBoth*) s) = 
  //System.Diagnostics.Debug.WriteLine(s);
  // why List.ofArray |> List.ofSeq?
  parseChars [] (s.ToCharArray() |> List.ofArray) |> List.ofSeq

//======================================================================================
// Part 2: Block Formatting

/// Recognizes a Section (# Some section, ## another section), prefixed with '#'s
let (|Section|_|) input = //function
// conceptually, this is what is happening
//  let head :: rest = input
//  match head with
//  // ## Some Heading or # Some heading
//  | String.StartsWithRepeated "#" head -> { n, matchedText -> {
//       Some (n, matchedText.Trim(), rest)
//      }

// another way to write this:
//  match input with
//  | String.StartsWithRepeated "#" s :: rest ->
//      let (n, header) = s
//      let header = 
  match input with
  | String.StartsWithRepeated "#" (n, header) :: rest -> // step 1: decouple head :: tail. step 2: pass head to startsWith and it returns the n, header
      //TODO: at some point, re visit the header variable name because we're actually hiding in this next line
      let header = 
        // Drop "##" at the end, but only when it is preceded by some whitespace
        // (For example "## Hello F#" should be "Hello F#")
        if header.EndsWith "#" then
          let noHash = header.TrimEnd [| '#' |]
          if noHash.Length > 0 && Char.IsWhiteSpace(noHash.Chars(noHash.Length - 1)) 
          then noHash else header
        else header        
      Some(n, header, rest)
  | rest ->
      None

// TODO: Scene heading should also look for a line break before and after. 
/// Recognizes a SceneHeading (prefixed with INT/EXT, etc. or a single period)
let (|SceneHeading|_|) = function
  // TODO: Make this StartsWithAnyCaseInsensitive
  | String.StartsWithAny [ "INT"; "EXT"; "EST"; "INT./EXT."; "INT/EXT"; "I/E" ] heading:string :: rest ->
     Some(false, heading, rest)
  | String.StartsWith "." heading:string :: rest ->
     Some(true, heading, rest) 
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
      Some(true, head.Substring(1), rest)
    // matches "BOB" or "BOB JOHNSON" or "R2D2" but not "25D2"
#if _MOBILEPCL_
    else if (System.Char.IsUpper (head.[0]) && head.ToCharArray() |> Seq.forall (fun c -> (System.Char.IsUpper c|| System.Char.IsWhiteSpace c || System.Char.IsNumber c))) then
#else
    else if (System.Char.IsUpper (head.[0]) && head |> Seq.forall (fun c -> (System.Char.IsUpper c|| System.Char.IsWhiteSpace c || System.Char.IsNumber c))) then
#endif
      Some(false, head, rest)
    // matches "BOB (*)"
    //else if (
    else
      None


/// Recognizes a PageBreak (3 or more consecutive equals and nothign more)
let (|PageBreak|_|) input = //function
  match input with
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
      Some(lyric, rest)
  | rest ->
      None

/// Recognizes centered text (> The End <)
let (|Centered|_|) = function
  | String.StartsWith ">" text:string :: rest ->
     if text.EndsWith "<" then //TODO: i'm sure an F# ninja can find a way to combine this with previous line
       Some(text.Trim().TrimEnd [|'<'|], rest)
     else
       None
  | rest ->
      None

// Parenthetical
let (|Parenthetical|_|) (lastParsedBlock:FountainSharp.Parse.FountainBlockElement option) (input:string list) =
  match lastParsedBlock with
  // parenthetical can come after character OR dialogue
  | Some (FountainSharp.Parse.Character(_)) 
  | Some (FountainSharp.Parse.Dialogue(_)) ->
     match input with
     | blockContent :: rest ->
        if (blockContent.Trim().StartsWith "(" && blockContent.EndsWith ")") then
          Some(blockContent.Trim().TrimStart([|'('|]).TrimEnd([|')'|]), rest)
        else
          None
     | [] -> None
  | _ -> None

//==== DIALOGUE

// Dialogue
let (|Dialogue|_|) (lastParsedBlock:FountainSharp.Parse.FountainBlockElement option) (input:string list) =
  match lastParsedBlock with
  | Some (FountainSharp.Parse.Character(_)) 
  | Some (FountainSharp.Parse.Parenthetical(_)) ->
     match input with
     | blockContent :: rest ->
        if blockContent.StartsWith "!" then // guard against forced action
          None
        else
          Some(blockContent, rest)
     | [] -> None
  | _ -> None

//==== /DIALOGUE

// Transition
let (|Transition|_|) (input:string list) =
   match input with
   | blockContent :: rest ->
      if blockContent.StartsWith "!" then // guard against forced action
        None
      elif blockContent.EndsWith "TO:" || blockContent.StartsWith ">" then // TODO: need to check for a hard linebreak after
        if blockContent.StartsWith ">" then
          Some(true, blockContent.Substring(1), rest) // true for forced
        else
          Some(false, blockContent.Trim(), rest)
      else
       None
   | [] -> None

//==== ACTION

let (|Action|_|) input =
  // look ahead and keep matching while it's none of these.
  match List.partitionWhileLookahead (function
    | SceneHeading _ -> false //note: it's decomposing the match and the rest and discarding the rest: `SceneHeading _` 
    | Character _ -> false
    | Lyric _ -> false
    | Transition _ -> false // ugh. need to pass the last parsed block.
    | Centered _ -> false
    | Section _ -> false
    //| Comments _ -> false
    //| Title _ -> false
    | Synopses _ -> false
    | PageBreak _ -> false
    | _ -> true) input with // if we found a match, and it's not empty, return the Action and the rest
      | matching, rest ->
        match input with
        | [] -> None
        | hd::tail ->
          if (hd.StartsWith "!") then
            Some(true, hd.Substring(1)::tail, rest) // trim off the '!' and smash the list back together
          else
            Some(false, matching, rest)
      | _ -> None

//==== /ACTION


/// Defines a context for the main `parseBlocks` function
// TODO: Question: what is the Links part supposed to represent? Answer: for some reason he was creating a 
// dictionary of known links. probably for additional processing. but fountain doesn't have links, so i got 
// rid of it. but now we need to simplify this ParsingContext, probably.
type ParsingContext = 
  { 
    Newline : string 
  }

/// Parse a list of lines into a sequence of fountain blocks
/// note, we pass the lastParsedBlock because some blocks are dependent on what came before. dialogue, for 
/// instance, comes after Character
/// 
let rec parseBlocks (ctx:ParsingContext) (lastParsedBlock:FountainBlockElement option) (lines: _ list) = seq {

  // NOTE: Order of matching is important here. for instance, if you matched dialogue before 
  // parenthetical, you'd never get parenthetical

  //let foo =
  //  match lines with 
  //   | (line::rest) ->
  //     match line with
  //     | "" -> "Empty line"
  //     | _ -> ""
  //   | _ -> ""

  //if foo = "Empty line" then
  //  System.Diagnostics.Debug.WriteLine("empty line")

  match lines with
  // Recognize remaining types of blocks/paragraphs
  | SceneHeading(forced, body, rest) ->
     let item = SceneHeading(forced, parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  | Section(n, body, rest) ->
     let item = Section(n, parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  | Character(forced, body, rest) ->
     let item = Character(forced, parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest

  | PageBreak(body, rest) ->
     let item = PageBreak
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  | Synopses(body, rest) ->
     let item = Synopses(parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  | Lyric(body, rest) ->
     let item = Lyric(parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest

  | Centered(body, rest) ->
     let item = Centered(parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest

  | Transition(forced, body, rest) ->
     let item = Transition(forced, parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  
  | Parenthetical lastParsedBlock (body, rest) ->
     let item = Parenthetical(parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest

  | Dialogue lastParsedBlock (body, rest) ->
     let item = Dialogue(parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest

  | Action(forced, bodyLines, rest) ->
    // we get multiple lines as a match, so for blank lines we return a hard line break, otherwise we 
    // call parse spans
    let mapFunc bodyLine : FountainSpans = 
      if (bodyLine = "") then
        [HardLineBreak(new Range(0,0))]
      else
        let kung = parseSpans bodyLine
        let fu = [HardLineBreak(new Range(0,0))]
        List.concat ([kung;fu])

    //HACK: Action is a block element, so we want ot pull the last HardLineBreak off.
    // we have to do this here because we're adding it on above to every line.
    let foo = List.collect mapFunc bodyLines
    let goo = foo.GetSlice(Some(0), Some(foo.Length - 2))

    // collect is a magic function that concatenates lists
    //let item = Action(List.collect mapFunc bodyLines)
    let item = Action(forced, goo, new Range(0,0))

    yield item

    // go on to parse the rest
    yield! parseBlocks ctx (Some(item)) rest

  //| Lines.TrimBlankStart [] ->
  //   yield Action([HardLineBreak])
  ////   System.Diagnostics.Debug.WriteLine("Trimming blank line. ")
  //   () 
  | _ as line -> 
    //System.Diagnostics.Debug.WriteLine("Not really sure what's here.")
    ()
  }

  //| _ -> failwithf "Unexpectedly stopped!\n%A" lines }
