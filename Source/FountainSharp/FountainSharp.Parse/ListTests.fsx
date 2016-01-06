#load "Collections.fs"
#load "StringParsing.fs"

open FSharp.Collections
//open FSharp.Collections.Array
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
let (|Character|_|) (text:string) =
  if (text.Length = 0) then 
    None
  // matches "@McAVOY"
  else if (text.StartsWith "@") then
    Some(text)
  // matches "BOB" or "BOB JOHNSON" or "R2D2" but not "25D2"
  else if (System.Char.IsUpper (text.[0]) && text |> Seq.forall (fun c -> (System.Char.IsUpper c|| System.Char.IsWhiteSpace c || System.Char.IsNumber c))) then
    Some(text)
  // matches "BOB (*)"
  //else if (
  else
    None

let testIsCharacter testString =
  match testString with
  | Character s ->
    printfn "Yes. it is."
  | _ -> printfn "No. It's not."
      
let characterTest1 = "This is not all uppercase." // needs to fail
let characterTest2 = "THIS IS ALL UPPERCASE BUT HAS WHITESPACE" // needs to succeed
let characterTest3 = "UPPER" // needs to succeed
let characterTest4 = "R2D2" // needs to succeed
let characterTest5 = "25D2" // needs to fail
let characterTest6 = "@McAvoy" // needs to succeed


testIsCharacter characterTest1
testIsCharacter characterTest2
testIsCharacter characterTest3
testIsCharacter characterTest4
testIsCharacter characterTest5
testIsCharacter characterTest6



//let (|SomeSheeit|_|) (text:string) =
//  match text with
//  | fun text -> (
//      if text.Length > 1 then Some(input)
//    )
//  | _ -> None
//
//let (|SomeSheeit|_|) =
//  function text when text.Length > 1 -> Some text | _ -> None


#load "FountainSyntax.fs"


let (|Dialogue|_|) (lastParsedBlock:FountainSharp.Parse.FountainBlockElement option) input =
  match lastParsedBlock with
  | Some (FountainSharp.Parse.Character(_)) ->
  //| :? FountainSharp.Parse.Character as c ->
     printfn "Last item was a dialogue"
     match input with 
     | _ -> None
  | _ -> None










let (|MultipleOf|_|) x input = 
  if input % x = 0 then 
    Some(input / x) else 
  None
 
let factorize x =
    let rec factorizeRec n i =
        let sqrt = int (System.Math.Sqrt(float n))
        if i > sqrt then
            []
        else
            match n with
            | MultipleOf i timesXdividesIntoI
                -> i :: timesXdividesIntoI :: (factorizeRec n (i + 1))
            | _ -> factorizeRec n (i + 1)
    factorizeRec x 1
   
assert ([1; 10; 2; 5] = (factorize 10))