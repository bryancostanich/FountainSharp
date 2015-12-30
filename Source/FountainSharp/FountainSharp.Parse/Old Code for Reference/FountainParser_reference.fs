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

open FSharp.Patterns
open FSharp.Collections

// --------------------------------------------------------------------------------------
// Parsing of Markdown - first part handles inline formatting
// --------------------------------------------------------------------------------------

/// Succeeds when the specified character list starts with an escaped 
/// character - in that case, returns the character and the tail of the list
let inline (|EscapedChar|_|) input = 
  match input with
  | '\\'::( ( '*' | '\\' | '`' | '_' | '{' | '}' | '[' | ']' 
            | '(' | ')' | '>' | '#' | '.' | '!' | '+' | '-' | '$') as c) ::rest -> Some(c, rest)
  | _ -> None

/// Matches a list if it starts with a sub-list that is delimited
/// using the specified delimiters. Returns a wrapped list and the rest.
///
/// This is similar to `List.Delimited`, but it skips over escaped characters.
let (|DelimitedMarkdown|_|) bracket input = 
  let startl, endl = bracket, bracket
  // Like List.partitionUntilEquals, but skip over escaped characters
  let rec loop acc = function
    | EscapedChar(x, xs) -> loop (x::'\\'::acc) xs
    | input when List.startsWith endl input -> Some(List.rev acc, input)
    | x::xs -> loop (x::acc) xs
    | [] -> None
  // If it starts with 'startl', let's search for 'endl'
  if List.startsWith bracket input then
    match loop [] (List.skip bracket.Length input) with 
    | Some(pre, post) -> Some(pre, List.skip bracket.Length post)
    | None -> None
  else None

/// Recognizes some form of emphasis using `**bold**` or `*italic*`
/// (both can be also marked using underscore).
/// TODO: This does not handle nested emphasis well.
let (|Emphasised|_|) = function
  | (('_' | '*') :: tail) as input ->
    match input with
    | DelimitedMarkdown ['_'; '_'; '_'] (body, rest) 
    | DelimitedMarkdown ['*'; '*'; '*'] (body, rest) -> 
        Some(body, Emphasis >> List.singleton >> Strong, rest)
    | DelimitedMarkdown ['_'; '_'] (body, rest) 
    | DelimitedMarkdown ['*'; '*'] (body, rest) -> 
        Some(body, Strong, rest)
    | DelimitedMarkdown ['_'] (body, rest) 
    | DelimitedMarkdown ['*'] (body, rest) -> 
        Some(body, Emphasis, rest)
    | _ -> None
  | _ -> None

/// Parses a body of a paragraph and recognizes all inline tags.
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
  | Emphasised (body, f, rest) ->
      yield! accLiterals.Value
      let body = parseChars [] body |> List.ofSeq
      yield f(body)
      yield! parseChars [] rest
  // Encode '<' char if it is not link or inline HTML
  | '<'::rest -> 
      yield! parseChars (';'::'t'::'l'::'&'::acc) rest      
  | '>'::rest -> 
      yield! parseChars (';'::'t'::'g'::'&'::acc) rest      
  | x::xs -> 
      yield! parseChars (x::acc) xs 
  | [] ->
      yield! accLiterals.Value }

/// Parse body of a paragraph into a list of Markdown inline spans      
let parseSpans (String.TrimBoth s) = 
  parseChars [] (s.ToCharArray() |> List.ofArray) |> List.ofSeq


// --------------------------------------------------------------------------------------
// Parsing of Markdown - second part handles paragraph-level formatting (headings, etc.)
// --------------------------------------------------------------------------------------

/// Recognizes heading, either prefixed with #s or followed by === or --- line
let (|Heading|_|) = function
  | (String.TrimBoth header) :: (String.TrimEnd (String.EqualsRepeated "=")) :: rest ->
      Some(1, header, rest)
  | (String.TrimBoth header) :: (String.TrimEnd (String.EqualsRepeated "-")) :: rest ->
      Some(2, header, rest)
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


/// Splits input into lines until whitespace or starting of a list and the rest.
let (|LinesUntilListOrWhite|) = 
  List.partitionUntil (function
    | ListStart _ | String.WhiteSpace -> true | _ -> false)

/// Splits input into lines until not-indented line or starting of a list and the rest.
let (|LinesUntilListOrUnindented|) =
  List.partitionUntilLookahead (function 
    | (ListStart _ | String.Unindented)::_ 
    | String.WhiteSpace::String.WhiteSpace::_ -> true | _ -> false)

/// Takes lines that belong to a continuing paragraph until 
/// a white line or start of other paragraph-item is found
let (|TakeParagraphLines|_|) input = 
  match List.partitionWhileLookahead (function
    | Heading _ -> false
    | FencedCodeBlock _ -> false
    | BlockquoteStart _::_ -> false
    | String.WhiteSpace::_ -> false
    | _ -> true) input with
  | matching, rest when matching <> [] -> Some(matching, rest)
  | _ -> None

/// Continues taking lines until a whitespace line or start of a blockquote
let (|LinesUntilBlockquoteOrWhite|) =
  List.partitionUntil (function 
    | BlockquoteStart _ | String.WhiteSpace -> true | _ -> false)

/// Defines a context for the main `parseParagraphs` function
// Q: why "Links" name? and why the weird type definition?
type ParsingContext = 
  { Links : Dictionary<string, string * option<string>> 
    Newline : string }

/// Parse a list of lines into a sequence of markdown paragraphs
let rec parseParagraphs (ctx:ParsingContext) lines = seq {
  match lines with

  // Recognize remaining types of paragraphs
  | Heading(n, body, Lines.TrimBlankStart lines) ->
      yield Section(n, parseSpans body)
      yield! parseParagraphs ctx lines 
  | TakeParagraphLines(lines, Lines.TrimBlankStart rest) ->      
      yield Paragraph (parseSpans (String.concat ctx.Newline lines))
      yield! parseParagraphs ctx rest 

  | Lines.TrimBlankStart [] -> () 
  | _ -> failwithf "Unexpectedly stopped!\n%A" lines }    