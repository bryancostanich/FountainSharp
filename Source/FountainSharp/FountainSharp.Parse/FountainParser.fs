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
open System.Text
open System.Text.RegularExpressions

open FSharp.Collections
open FountainSharp.Parse.Collections
open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Patterns.List
open FountainSharp.Parse.Patterns.String

let printDebug fmt par =
    let s = FSharp.Core.Printf.sprintf fmt par
    System.Diagnostics.Debug.WriteLine s

[<Literal>]
let EmptyLine = ""

[<Literal>]
let NewLine = "\n"

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
  let endl = delimiterBracket // end limiter
  // Like List.partitionUntilEquals, but skip over escaped characters
  let rec loop acc found input =
    match input with
    | EscapedChar(x, xs) -> loop (x::'\\'::acc) found xs // skip the escaped char
    | x::xs -> 
      if List.startsWith endl input && Char.IsWhiteSpace acc.Head = false then
        loop (x::acc) true xs // found a delimiter, but we have to found the furthermost
      else
        if found then // now not matching, but in the previous iteration it was ok
          Some(List.rev acc.Tail, List.last acc :: input)
        else
          loop (x::acc) found xs // search further
    | _ when found -> Some(List.rev acc.Tail, List.last acc :: input) // input has 1 or zero elements and previous iteration matched
    | [] -> None // not found the delimiter and traversed the list
  // If it starts with 'delimiterBracket', let's search for 'endl'
  if List.startsWith delimiterBracket input then
    match loop [] false (List.skip delimiterBracket.Length input) with
    | Some(pre, post) -> Some(pre, List.skip delimiterBracket.Length post)
    | None -> None
  else None

/// recognizes emphasized text of Italic, Bold, etc.
/// take something like "*some text* some more text" and return a sequence of TextSpans: italic<"some text">::rest
let (|Emphasized|_|) span =
  
  // Emphasis is not valid across multiple lines. this function checks for it
  let check (body: char list, empType, rest) =
    if String.containsNewLine body then
        None
    else
        Some(body, empType, rest)
  
  match span with
  // if it starts with either `_` or `*`
  //   1) the code `(('_' | '*')` :: tail)` decomposes the input into a sequence of either `'_'::tail` or `'*'::tail`
  //   2) `as input` binds that sequence to a variable
  | ('_' :: tail) as input -> // Underline
    match input with
    | DelimitedText ['_'] (body, rest) ->
      check(body, Underline, rest)
    | _ -> None
  | ('*' :: '*' :: tail) as input -> // Bold
    match input with
    | DelimitedText ['*'; '*'] (body, rest) -> 
      check(body, Strong, rest)
    | _ -> None
  | ('*' :: tail) as input -> // Italic
    match input with
    | DelimitedText ['*'] (body, rest) -> 
      check(body, Italic, rest)
    | _ -> None
  | _ -> None

/// recognizes notes which start with "[[" and end with "]]"
let (|Note|_|) input =
  // replace double space with new line
  let rec transform chars = 
      // TODO: new lines should be uniform through the whole library.
      // Action active pattern places '\n' as new line, that's what we expect here
      match chars with
      | '\n' :: ' ' :: ' ' :: '\n' :: tail -> '\n' :: '\n' :: (transform tail)
      | head :: tail -> head :: (transform tail)
      | [] -> []

  match input with
  | DelimitedWith ['['; '['] [']';']'] (body, rest) -> 
      Some (transform body, rest)
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
  | '\r'::'\n'::rest
  | ('\n' | '\r')::rest ->
    //System.Diagnostics.Debug.WriteLine("found a hardlinebreak")
    yield! accLiterals.Value
    yield HardLineBreak(Range.empty)
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
  | Note (body, rest) ->
      yield! accLiterals.Value
      let body = parseChars [] body |> List.ofSeq
      yield Note(body, Range.empty)
      yield! parseChars [] rest

  // This calls itself recursively on the rest of the list
  | x::xs -> 
      yield! parseChars (x::acc) xs 
  | [] ->
      yield! accLiterals.Value }

/// Parse body of a block into a list of Markdown inline spans
// trimming off \r\n?
//let parseSpans (s) = 
let parseSpans ((*String.TrimBoth*) s:string) = 
  //System.Diagnostics.Debug.WriteLine(s);
  // why List.ofArray |> List.ofSeq?
  // printDebug "parseSpans %s" s
  parseChars [] (s.ToCharArray() |> List.ofArray) |> List.ofSeq

  // we get multiple lines as a match, so for blank lines we return a hard line break, otherwise we 
  // call parse spans
let mapFunc bodyLines =
    let mapFuncInternal bodyLine : FountainSpans = 
      if (bodyLine = "") then
        [HardLineBreak(Range.empty)]
      else
        let kung = parseSpans bodyLine
        let fu = [HardLineBreak(Range.empty)]
        List.concat ([kung;fu])
    let foo = List.collect mapFuncInternal bodyLines
    foo.GetSlice(Some(0), Some(foo.Length - 2))

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

let (|Boneyard|_|) input =
    // recongizing lines inside of the boneyard
    // TODO: now /* and */ should stand alone in a line. if it's not appropriate, Boneyard should be implemented as a span (it would be weird though :))
    let rec addLines (acc : string list) = 
        function 
        | [] -> None // no beginning or ending of comment found
        | String.StartsWith "/*" head:string :: tail ->
            addLines acc tail // beginning of comment
        | String.StartsWith "*/" head:string :: tail -> 
            Some(List.rev acc, tail) // end of comment
        | head :: tail ->
            addLines (head :: acc) tail // inside or outside of comment

    match addLines [] input with
    | Some([], rest) -> None // no comment found
    | Some(body, rest) -> Some(body, rest)
    | _ -> None

// TODO: Should we also look for a line break before? 
let (|SceneHeading|_|) (input:string list) =
  let hasNewLineAfterFirstElement (forced, list) = 
    match list with
    | [] -> None
    | head::rest ->
       // look for a line break after the first element (first of the rest)
       if rest.Length = 0 || String.IsNullOrWhiteSpace rest.[0] then
          Some(forced, head, rest)
       else
          None
  match input with
  | head :: tail ->
    let head = head.Trim()
    match head with
    // look for normal heading
    | String.StartsWithAnyCaseInsensitive [ "INT"; "EXT"; "EST"; "INT./EXT."; "INT/EXT"; "I/E" ] matching ->
        hasNewLineAfterFirstElement (false, matching :: tail)
    // look for forced heading
    | String.StartsWith "." matching ->
        hasNewLineAfterFirstElement (true, matching :: tail)
    | _ -> None
  | _ -> None

let (|Character|_|) (list:string list) =
  match list with
  | [] -> None
  | EmptyLine :: head :: rest ->
    // trim white spaces as Character ignores indenting
    let head = head.Trim()
    // Character has to be preceded by empty line
    if (head.Length = 0) then
        None
    // matches "@McAVOY"
    else if (head.StartsWith "@") then
      if head.EndsWith(" ^") then
        Some(true, false, head.Substring(1), rest)
      else
        Some(true, true, head.Substring(1), rest)
    // matches "BOB" or "BOB JOHNSON" or "BOB (on the radio)" or "R2D2" but not "25D2"
    else
      let pattern = @"^\p{Lu}[\p{Lu}\d\s]*(\(.*\))?(\s+\^)?$"
      let m = Regex.Match(head, pattern)
      if m.Value = head then
        if m.Groups.Count > 1 && String.IsNullOrEmpty(m.Groups.[1].Value) = false then
          // check parenthetical extension for lowercase or uppercase
          // TODO: Do we really need to do this? The specification is not crystal clear about this.
          // If the extension can consist of mixed letters, than this block can be discarded
          let extension = m.Groups.[1].Value.ToCharArray() |> Seq.where(fun c -> Char.IsLetter(c))
          let allUpper = extension |> Seq.forall(fun c -> Char.IsUpper(c)) // all uppercase
          let allLower = extension |> Seq.forall(fun c -> Char.IsLower(c)) // all lowercase
          if allUpper || allLower then
            if m.Value.EndsWith("^") then
              Some(false, false, head, rest)
            else
              Some(false, true, head, rest)
          else
            None
        else // no parenthetical extension found
          if m.Value.EndsWith("^") then
            // character for dual dialogue
            Some(false, false, m.Value.Remove(m.Value.Length - 1).Trim(), rest)
          else
            Some(false, true, head, rest)
      // does not match Character rules
      else
        None
  | _ -> None

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
let (|Centered|_|) (input:string list) =
  match input with
  | head :: rest ->
    // Centered ignores indenting
    let head = head.Trim()
    if head.StartsWith(">") && head.EndsWith("<") then
        Some(head.Substring(1, head.Length - 2).Trim(), rest) // strip '>' and '<'
    else
        None
  | _ -> None 

let isForcedAction (input:string) =
    input.StartsWith("!")

// Parenthetical
let (|Parenthetical|_|) (lastParsedBlock:FountainBlockElement option) (input:string list) =
  match lastParsedBlock with
  // parenthetical can come after character OR dialogue
  | Some (FountainSharp.Parse.Character(_)) 
  | Some (FountainSharp.Parse.Parenthetical(_)) // parenthetical can occur in a dialog which preceded by a character-parenthetical duo (in this case parenthetical is the last parsed block)
  | Some (FountainSharp.Parse.Dialogue(_)) ->
     match input with
     | blockContent :: rest ->
        if (blockContent.Trim().StartsWith "(" && blockContent.EndsWith ")") then
          Some(blockContent.Trim().TrimStart([|'('|]).TrimEnd([|')'|]), rest)
        else
          None
     | [] -> None
  | _ -> None

//==== Transition

// Transition
let (|Transition|_|) (input:string list) =
   match input with
   | blockContent :: rest ->
      let blockContent = blockContent.Trim() // Transition ignores indenting
      if blockContent.StartsWith "!" then // guard against forced action
        None
      elif blockContent.EndsWith "TO:" || blockContent.StartsWith ">" then // TODO: need to check for a hard linebreak after
        if blockContent.StartsWith ">" then
          Some(true, blockContent.Substring(1).Trim(), rest) // true for forced
        else
          Some(false, blockContent.Trim(), rest)
      else
       None
   | [] -> None

//==== Dialogue

// Dialogue
let (|Dialogue|_|) (lastParsedBlock:FountainBlockElement option) (input:string list) =
  match lastParsedBlock with
  | Some (FountainSharp.Parse.Character(_)) 
  | Some (FountainSharp.Parse.Parenthetical(_)) ->
     // Dialogue starts after Character or Character (parenthetical)
     // look ahead and keep matching while it's none of these.
     match List.partitionWhileLookahead (function
     | SceneHeading _ -> false //note: it's decomposing the match and the rest and discarding the rest: `SceneHeading _` 
     | Character _ -> false
     | Lyric _ -> false
     | Transition _ -> false // ugh. need to pass the last parsed block.
     | Centered _ -> false
     | Section _ -> false
     | Synopses _ -> false
     | PageBreak _ -> false
     | Parenthetical lastParsedBlock _ -> false
     | _ -> true) input with // if we found a match, and it's not empty, return the Action and the rest
        | [], _ -> None
        | matching, rest ->
          // parsing dialogue's lines
          let rec addLines (acc: string list) = function
            // TODO: the following matches could be simpler, I think
            | first :: second :: tail as input ->
                if first = EmptyLine then
                    if second.StartsWith("  ") then // dialogue continues
                        addLines (second.Substring(2) :: first :: acc) tail
                    else
                        Some(List.rev acc, List.append(second :: tail) rest)
                elif isForcedAction(first) then // stop at forced Action
                    Some(List.rev acc, List.append input rest)
                else if second = EmptyLine then
                    addLines (first :: acc) (second :: tail)
                else
                    addLines (second :: first :: acc) tail
            | [head] ->
                if isForcedAction(head) then // stop at forced Action
                    Some(List.rev acc, rest)
                else
                    addLines (head :: acc) []
            | [] ->
                Some(List.rev acc, rest) // traversed all the lines

          let lines = addLines [] matching
          match lines with
          | Some([], rest) -> None // no lines found
          | Some(body, rest) ->
              Some(body |> List.map(fun line -> line.Trim()), rest)
          | _ -> None
  | _ -> None

//==== /Dialogue

//==== Dual Dialogue

let (|DualDialogue|_|) (lastParsedBlock:FountainBlockElement option) (input:string list) =
  // parse input for Character or Character, Parenthetical and return the list of them  
  let rec parseCharacter (input:string list, acc, lastParsedBlock:FountainBlockElement option) = 
    match input with
    | Character(forced, primary, body, rest) as item -> 
        let characterItem = Character(forced, primary, parseSpans body, Range.empty)
        let lastParsedBlock = Some(characterItem)
        match rest with
        | Parenthetical lastParsedBlock (body, rest) ->
            let parentheticalItem = Parenthetical(parseSpans body, Range.empty)
            parseCharacter (rest, parentheticalItem :: characterItem :: acc, Some(characterItem))
        | _ ->
            parseCharacter (rest, characterItem :: acc, Some(characterItem))
    | _ -> if acc.Length = 0 then None else Some(acc, input, lastParsedBlock)

  // parse input for Dialogue or Dialogue, Parenthetical and return the list of them  
  let rec parseDialogue (input:string list, acc, lastParsedBlock:FountainBlockElement option) = 
    match input with
    | Dialogue lastParsedBlock (dialogBody, rest) -> 
        let dialogueItem = Dialogue(mapFunc dialogBody, Range.empty)
        let lastParsedBlock = Some(dialogueItem)
        match rest with
        | Parenthetical lastParsedBlock (body, rest) ->
            let parentheticalItem = Parenthetical(parseSpans body, Range.empty)
            parseDialogue (rest, parentheticalItem :: dialogueItem :: acc, Some(dialogueItem))
        | _ ->
            parseDialogue (rest, dialogueItem :: acc, Some(dialogueItem))
    | _ -> if acc.Length = 0 then None else Some(acc, input, lastParsedBlock)

  // parse input for (Character, Dialogue) pairs and return the list of them  
  let rec parse (input:string list, acc, lastParsedBlock:FountainBlockElement option) = 
    if input.Length = 0 then
        Some(List.rev acc, input)
    else
      match parseCharacter(input, [], lastParsedBlock) with
      | Some(characterBlocks, rest, lastParsedBlock) -> 
          match parseDialogue(rest, [], lastParsedBlock) with
          | Some(dialogueBlocks, rest, lastParsedBlock) ->
              parse (rest, List.concat [dialogueBlocks; characterBlocks; acc], lastParsedBlock)
          | _ -> Some(List.rev acc, input)
      | _ -> Some(List.rev acc, input)

  let isSecondary block =
    match block with
    | FountainBlockElement.Character(forced, primary, spans, r) ->
      not primary
    | _ -> false

  let isPrimary block = 
    match block with
    | FountainBlockElement.Character(forced, primary, spans, r) ->
      primary
    | _ -> false

  // at least 2 (Character, Dialogue) blocks have to be found and at least one of them should be secondary character (marked by a caret)
    
  match parse (input, [], lastParsedBlock) with
  | Some([], _) -> None // no (Character, Dialogue) blocks found
  | Some(list, rest) ->
    if List.tryFind isPrimary list <> None &&  List.tryFind isSecondary list <> None then
       Some(list, rest)
    else
       None
  | None -> None

//==== /Dual Dialogue

//==== Action

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
        match matching with
        | [] -> None
        | hd::tail ->
          let sb = new StringBuilder()
          List.iter (fun x -> sb.Append(x:string) |> ignore; sb.Append('\n') |> ignore) (hd::tail)
          if (hd.StartsWith "!") then // forced Action, trim off the '!'
            Some(true, sb.ToString().Substring(1).TrimEnd(), rest)
          else
            Some(false, sb.ToString().TrimEnd(), rest)
      //| _ -> None

//==== /Action

//==== TitlePage

let (|TitlePage|_|) (lastParsedBlock:FountainBlockElement option) (input: string list) =
  match lastParsedBlock with
  | None ->
    // match "key: value" pattern at the beginning of the input as far as it is possible, and returns the matching values as well the remaining input
    let rec matchAndRemove acc (input:string) =
      let validCharacterClass = "[^:]"
      let pattern = String.Format(@"^(?<key>\b{0}+):(?<value>{0}+\n)", validCharacterClass)
      let m = Regex.Match(input, pattern, RegexOptions.Singleline)
      if m.Success = false then
        (List.rev acc, input) // no more match found
      else
       let key = m.Groups.["key"] // text before ':'
       let value = m.Groups.["value"] // text after ':'
       // let's parse spans - white spaces must be trimmed per line
       let spans = value.Value.Trim().Split('\n') |> List.ofArray |> List.map( fun s -> s.Trim() ) |> String.concat "\n" |> parseSpans
       matchAndRemove ((key.Value, spans) :: acc) (input.Remove(m.Index, m.Length))
    
    // TODO: spare conversion from list to string and back to list of the remaining text!
    let inputAsSingleString = String.asSingleString(input, NewLine) // treat input as one string
    // an empty line has to be present after the Title Page
    let indexOfEmptyLine = inputAsSingleString.IndexOf(NewLine + NewLine)
    if indexOfEmptyLine = -1 then
        None
    else
        let titlePageText = inputAsSingleString.Substring(0, indexOfEmptyLine + 1) // text before the empty line '\n' at the end
        match matchAndRemove [] titlePageText with
        | ([], _) -> None
        | (keyValuePairs, rest) ->
            Some(keyValuePairs, String.asStringList(inputAsSingleString.Substring(indexOfEmptyLine + 2), NewLine))
        | _ -> None
  | _ -> None // Title page must be the first block of the document 

//==== /TitlePage


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

  match lines with
  | TitlePage lastParsedBlock (keyValuePairs, rest) ->
     let item = TitlePage(keyValuePairs, Range.empty)
     yield item
     yield PageBreak // Page break is implicit after Title page
     yield! parseBlocks ctx (Some(item)) rest
  | Boneyard(body, rest) ->
     let item = Boneyard(String.asSingleString(body, "\n"), Range.empty)
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  // Recognize remaining types of blocks/paragraphs
  | SceneHeading(forced, body, rest) ->
     let item = SceneHeading(forced, parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  | Section(n, body, rest) ->
     let item = Section(n, parseSpans body, new Range(0,0))
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  | DualDialogue lastParsedBlock (blocks, rest) ->
     let item = DualDialogue(blocks, Range.empty)
     yield item
     yield! parseBlocks ctx (Some(item)) rest
  | Character(forced, primary, body, rest) ->
     let item = Character(forced, primary, parseSpans body, new Range(0,0))
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
    let spans = mapFunc body
    let item = Dialogue(spans, Range.empty)
    yield item
    yield! parseBlocks ctx (Some(item)) rest // go on to parse the rest

  | Action(forced, body, rest) ->
    // body: as a single string. this can be parsed for spans much better
    let spans = parseSpans body
    let item = Action(forced, spans, Range.empty)
    yield item
    yield! parseBlocks ctx (Some(item)) rest // go on to parse the rest

  //| Lines.TrimBlankStart [] ->
  //   yield Action([HardLineBreak])
  ////   System.Diagnostics.Debug.WriteLine("Trimming blank line. ")
  //   () 
  | _ as line -> 
    //System.Diagnostics.Debug.WriteLine("Not really sure what's here.")
    ()
  }

  //| _ -> failwithf "Unexpectedly stopped!\n%A" lines }


//let CountBlocks (blocks:FountainBlockElement list) :FountainBlockElement list =

//  let i = 0
//  let (countedBlocks:FountainBlockElement list) = [] 

//  for block in blocks do
//    match block with
//    | Action (forced, spans, range)
//    | Character (forced, spans, range)
//    | SceneHeading (forced, spans, range)
//    | Transition (forced, spans, range) -> ()

//    | Dialogue (spans, range)
//    | Parenthetical (spans, range)
//    | Section(spans, range)
//    | Synopses(spans, range)
//    | Span(spans, range)
//    | Lyric(spans, range)
//    | Centered(spans, range) ->
//      ()
//    | PageBreak ->
//      ()


//  blocks
