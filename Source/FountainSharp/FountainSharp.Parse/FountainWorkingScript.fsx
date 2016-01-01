open System
open System.IO
open System.Collections.Generic

module List = 
  /// Returns a singleton list containing a specified value
  let singleton v = [v]

  /// Skips the specified number of elements. Fails if the list is smaller.
  let rec skip count = function
    | xs when count = 0 -> xs
    | _::xs when count > 0 -> skip (count - 1) xs
    | _ -> invalidArg "" "Insufficient length"

  /// Skips elements while the predicate returns 'true' and then 
  /// returns the rest of the list as a result.
  let rec skipWhile p = function
    | hd::tl when p hd -> skipWhile p tl
    | rest -> rest

  /// Partitions list into an initial sequence (while the 
  /// specified predicate returns true) and a rest of the list.
  let partitionWhile p input = 
    let rec loop acc = function
      | hd::tl when p hd -> loop (hd::acc) tl
      | rest -> List.rev acc, rest
    loop [] input

  /// Partitions list into an initial sequence (while the specified predicate 
  /// returns true) and a rest of the list. The predicate gets the entire 
  /// tail of the list and can perform lookahead.
  let partitionWhileLookahead p input = 
    let rec loop acc = function
      | hd::tl when p (hd::tl) -> loop (hd::acc) tl
      | rest -> List.rev acc, rest
    loop [] input

  /// Partitions list into an initial sequence (while the 
  /// specified predicate returns 'false') and a rest of the list.
  let partitionUntil p input = partitionWhile (p >> not) input

  /// Partitions list into an initial sequence (while the 
  /// specified predicate returns 'false') and a rest of the list.
  let partitionUntilLookahead p input = partitionWhileLookahead (p >> not) input

  /// Iterates over the elements of the list and calls the first function for 
  /// every element. Between each two elements, the second function is called.
  let rec iterInterleaved f g input =
    match input with 
    | x::y::tl -> f x; g (); iterInterleaved f g (y::tl)
    | x::[] -> f x
    | [] -> ()

  /// Tests whether a list starts with the elements of another
  /// list (specified as the first parameter)
  let inline startsWith start (list:'T list) = 
    let rec loop start (list:'T list) = 
      match start, list with
      | x::xs, y::ys when x = y -> loop xs ys
      | [], _ -> true
      | _ -> false
    loop start list

  /// Partitions the input list into two parts - the break is added 
  /// at a point where the list starts with the specified sub-list.
  let partitionUntilEquals endl input = 
    let rec loop acc = function
      | input when startsWith endl input -> Some(List.rev acc, input)
      | x::xs -> loop (x::acc) xs
      | [] -> None
    loop [] input    

  /// A function that nests items of the input sequence 
  /// that do not match a specified predicate under the 
  /// last item that matches the predicate. 
  let nestUnderLastMatching f input = 
    let rec loop input = seq {
      let normal, other = partitionUntil f input
      match List.rev normal with
      | last::prev ->
          for p in List.rev prev do yield p, []
          let other, rest = partitionUntil (f >> not) other
          yield last, other 
          yield! loop rest
      | [] when other = [] -> ()
      | _ -> invalidArg "" "Should start with true" }
    loop input |> List.ofSeq


module String =
  /// Matches when a string is a whitespace or null
  let (|WhiteSpace|_|) s = 
    if String.IsNullOrWhiteSpace(s) then Some() else None

  /// Matches when a string does starts with non-whitespace
  let (|Unindented|_|) (s:string) = 
    if not (String.IsNullOrWhiteSpace(s)) && s.TrimStart() = s then Some() else None

  /// Returns a string trimmed from both start and end
  let (|TrimBoth|) (text:string) = text.Trim()
  /// Returns a string trimmed from the end
  let (|TrimEnd|) (text:string) = text.TrimEnd()
  /// Returns a string trimmed from the start
  let (|TrimStart|) (text:string) = text.TrimStart()

  /// Retrusn a string trimmed from the end using characters given as a parameter
  let (|TrimEndUsing|) chars (text:string) = text.TrimEnd(Array.ofSeq chars)

  /// Returns a string trimmed from the start together with 
  /// the number of skipped whitespace characters
  let (|TrimStartAndCount|) (text:string) = 
    let trimmed = text.TrimStart()
    text.Length - trimmed.Length, trimmed

  /// Matches when a string starts with any of the specified sub-strings
  let (|StartsWithAny|_|) (starts:seq<string>) (text:string) = 
    if starts |> Seq.exists (text.StartsWith) then Some() else None
  /// Matches when a string starts with the specified sub-string
  let (|StartsWith|_|) (start:string) (text:string) = 
    if text.StartsWith(start) then Some(text.Substring(start.Length)) else None
  /// Matches when a string starts with the specified sub-string
  /// The matched string is trimmed from all whitespace.
  let (|StartsWithTrim|_|) (start:string) (text:string) = 
    if text.StartsWith(start) then Some(text.Substring(start.Length).Trim()) else None

  /// Matches when a string starts with the given value and ends 
  /// with a given value (and returns the rest of it)
  let (|StartsAndEndsWith|_|) (starts, ends) (s:string) =
    if s.StartsWith(starts) && s.EndsWith(ends) && 
       s.Length >= starts.Length + ends.Length then 
      Some(s.Substring(starts.Length, s.Length - starts.Length - ends.Length))
    else None

  /// Matches when a string starts with the given value and ends 
  /// with a given value (and returns trimmed body)
  let (|StartsAndEndsWithTrim|_|) args = function
    | StartsAndEndsWith args (TrimBoth res) -> Some res
    | _ -> None

  /// Matches when a string starts with a non-zero number of complete
  /// repetitions of the specified parameter (and returns the number
  /// of repetitions, together with the rest of the string)
  ///
  ///    let (StartsWithRepeated "/\" (2, " abc")) = "/\/\ abc"
  ///
  let (|StartsWithRepeated|_|) (repeated:string) (text:string) = 
    let rec loop i = 
      if i = text.Length then i
      elif text.[i] <> repeated.[i % repeated.Length] then i
      else loop (i + 1)

    let n = loop 0 
    if n = 0 || n % repeated.Length <> 0 then None
    else Some(n/repeated.Length, text.Substring(n, text.Length - n)) 

  /// Matches when a string starts with a sub-string wrapped using the 
  /// opening and closing sub-string specified in the parameter.
  /// For example "[aa]bc" is wrapped in [ and ] pair. Returns the wrapped
  /// text together with the rest.
  let (|StartsWithWrapped|_|) (starts:string, ends:string) (text:string) = 
    if text.StartsWith(starts) then 
      let id = text.IndexOf(ends, starts.Length)
      if id >= 0 then 
        let wrapped = text.Substring(starts.Length, id - starts.Length)
        let rest = text.Substring(id + ends.Length, text.Length - id - ends.Length)
        Some(wrapped, rest)
      else None
    else None

  /// Matches when a string consists of some number of 
  /// complete repetitions of a specified sub-string.
  let (|EqualsRepeated|_|) repeated = function
    | StartsWithRepeated repeated (n, "") -> Some()
    | _ -> None 

  /// Given a list of lines indented with certan number of whitespace 
  /// characters (spaces), remove the spaces from the beginning of each line 
  /// and return the string as a list of lines
  let removeSpaces lines =
    let spaces =
      lines 
      |> Seq.filter (String.IsNullOrWhiteSpace >> not)
      |> Seq.map (fun line -> line |> Seq.takeWhile Char.IsWhiteSpace |> Seq.length)
      |> fun xs -> if Seq.isEmpty xs then 0 else Seq.min xs
    lines 
    |> Seq.map (fun line -> 
        if String.IsNullOrWhiteSpace(line) then ""
        else line.Substring(spaces))
  


//===============================================================================================
//===============================================================================================
//===============================================================================================
// This is where the fun really begins
module FountainTestParser =

  //====== Fountain Schema/Syntax Definition

  /// Represents inline formatting inside a paragraph. This can be literal (with text), various
  /// formattings (string, emphasis, etc.), hyperlinks, etc.
  type FountainSpan =
    | Literal of string
    | Strong of FountainSpans
    | Italic of FountainSpans
    | Underline of FountainSpans
    | HardLineBreak

  /// A type alias for a list of `FountainSpan` values
  and FountainSpans = list<FountainSpan>

  /// A paragraph represents a (possibly) multi-line element of a fountain document.
  /// Paragraphs are headings, action blocks, dialogue blocks, etc. 
  type FountainParagraph = 
    | Section of int * FountainSpans
    | Paragraph of FountainSpans
    | Span of FountainSpans


  //====== Parser

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
      // is this a fall-through case?? e.g. does it go to the next line??
      // TOOD: get rid of "__" anyway
      | DelimitedText['_'; '_'] (body, rest) 
      | DelimitedText['*'; '*'] (body, rest) -> 
          Some(body, Strong, rest)
      | DelimitedText['_'] (body, rest) ->
          Some(body, Underline, rest)
      | DelimitedText['*'] (body, rest) -> 
          Some(body, Italic, rest)
      | _ -> None
    | _ -> None


//======= TESTING CODE
open FountainTestParser

let string1 = "*some italic text*"
let string2 = "**some bold text**"
let string3 = "***some bold italic text***"
let string4 = "_some underlined text_"
let string5 = "some text that's not emphasized"
let string6 = "pretty sure this will fail *some italic text **with some bold** in the middle*"

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
