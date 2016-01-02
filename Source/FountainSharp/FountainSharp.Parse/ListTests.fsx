#load "Collections.fs"
#load "StringParsing.fs"

open FSharp.Patterns.List

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