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
//TODO: Question: why is this inlined?
let inline (|EscapedChar|_|) input = 
  match input with
  | '\\'::( ( '*' | '\\' | '`' | '_' | '{' | '}' | '[' | ']' 
            | '(' | ')' | '>' | '#' | '.' | '!' | '+' | '-' | '$') as c) ::rest -> Some(c, rest)
  | _ -> None

// TODO: I shouldn't need this. there are no lists in fountain, but need to re-write
// the next function to not use it.
/// Matches a list if it starts with a sub-list that is delimited
/// using the specified delimiters. Returns a wrapped list and the rest.
///
/// This is similar to `List.Delimited`, but it skips over escaped characters.
let (|DelimitedMarkdown|_|) bracket input = 
  let startl, endl = bracket, bracket
  // Like List.partitionUntilEquals, but skip over escaped characters
  let rec loop acc = function
    | EscapedChar(x, xs) -> loop (x::'\\'::acc) xs

    //TODO: wtf, autocomplete tells me that there is a Pascal case version (StartsWith)
    // but isn't finding that one, or the `startsWith` version in Collections.fs
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
// TODO: this calls DelimitedMarkdown, but i don't think i need that.
// so how do i rewrite this?
// TODO: underscores should now be underline. fountain != markdown
// TODO; rename "Emphasised" to "Decorated," because Emphasised is too
// easy to confuse with Emphasis
let (|Emphasised|_|) = function
  | (('_' | '*') :: tail) as input ->
    match input with
    // the *** case in which it is both italic and strong
    | DelimitedMarkdown ['*'; '*'; '*'] (body, rest) -> 
        Some(body, Emphasis >> List.singleton >> Strong, rest)
    // is this a fall-through case?? e.g. does it go to the next line??
    // TOOD: get rid of "__" anyway
    | DelimitedMarkdown ['_'; '_'] (body, rest) 
    | DelimitedMarkdown ['*'; '*'] (body, rest) -> 
        Some(body, Strong, rest)
    | DelimitedMarkdown ['_'] (body, rest) ->
        Some(body, Underline, rest)
    | DelimitedMarkdown ['*'] (body, rest) -> 
        Some(body, Emphasis, rest)
    | _ -> None
  | _ -> None

