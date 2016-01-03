#load "Collections.fs"
#load "StringParsing.fs"

open FSharp.Collections
open FSharp.Collections.Array
open FountainSharp.Parse.Collections
open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Patterns.List
open FountainSharp.Parse.Patterns.String

let string1 = @"EXT. BRICK'S PATIO - DAY

A gorgeous day.  The sun is shining.  But BRICK BRADDOCK, retired police detective, is sitting quietly, contemplating -- something.

[[Some notes about the scene]]

And then there's a long beat.  
Longer than is funny.  
Long enough to be depressing.
"

let string2 = "[[a note]]"

let foo = ["["; "["] // gets interpreted as a string list
let goo = ["[", "["] // gets intperpreted as a tuple in a list: [("[", "[")]
let hoo = ['['; '['] // gets interpreted as a char list


let testString string = 
  match string with
  | DelimitedWith ['['; '['] [']';']'] s -> 
    printfn "found: %A" s
  | _ -> printfn "not found"

//let testString2 string = 
//  match string with
//  | Emphasized s -> 
//    printfn "it's emphasized: %A" s
//  | _ -> printfn "it's not emphasized."


// returns a sequence of characters from a string
let explode (s:string) =
  [for c in s -> c]

explode string1 |> testString
explode string2 |> testString



///// Matches when a string starts with any of the specified sub-strings
//let (|StartsWithAnyBetter|_|) (starts:seq<string>) (text:string) =
//  for testSeq in starts do
//    if text.StartsWith(testSeq) then
//      Some(text.Trim())
//  None



//======

/// Matches when a string starts with any of the specified sub-strings
let (|StartsWithAny|_|) (starts:seq<string>) (text:string) = 
  if starts |> Seq.exists (text.StartsWith) then Some(text) else None

let testString2 = "EXT. APARTMENT - DAY"

let testStartsWithAny testSequence testString =
  match testString with
  | StartsWithAny testSequence s ->
    printfn "Yes. it does."
  | _ -> printfn "No. It doesn't."

testStartsWithAny [ "INT"; "EXT"; "EST"; "INT./EXT."; "INT/EXT"; "I/E" ] testString2


//=====
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

let testPageBreak testString =
  match testString with
  | PageBreak s ->
    printfn "Yes. it does."
  | _ -> printfn "No. It doesn't."

let pageBreakText1 = "=========="
let pageBreakText2 = "==="
let pageBreakText3 = "=="
let pageBreakText4 = "========== "
let pageBreakText5 = "======= blah "

testPageBreak [pageBreakText1]
testPageBreak [pageBreakText2]
testPageBreak [pageBreakText3]
testPageBreak [pageBreakText4]
testPageBreak [pageBreakText5]


//========
let (|IsUppercaseOrWhiteSpace|_|) (text:string) =
  if (text |> Seq.forall (fun c -> (System.Char.IsUpper c|| System.Char.IsWhiteSpace c))) then
    Some(text)
  else
    None

let testIsAllUppercase testString =
  match testString with
  | IsUppercaseOrWhiteSpace s ->
    printfn "Yes. it is."
  | _ -> printfn "No. It's not."
      
let upperCaseTest1 = "This is not all uppercase."
let upperCaseTest2 = "THIS IS ALL UPPERCASE BUT HAS WHITESPACE"
let upperCaseTest3 = "UPPER"

testIsAllUppercase upperCaseTest1
testIsAllUppercase upperCaseTest2
testIsAllUppercase upperCaseTest3
