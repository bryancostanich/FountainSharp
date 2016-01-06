#if INTERACTIVE
#r "../../bin/FountainSharp.Parse.dll"
#r "../../packages/NUnit/lib/nunit.framework.dll"
#load "FsUnit.fs"
#else
module FountainSharp.Tests.Parsing
#endif

open FsUnit
open NUnit.Framework
open FountainSharp.Parse

let properNewLines (text: string) = text.Replace("\r\n", System.Environment.NewLine)

//===== Scene Headings
[<Test>]
let ``Basic Scene Heading`` () =
   let doc = """EXT. BRICK'S PATIO - DAY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """EXT. BRICK'S PATIO - DAY"""]]

[<Test>]
let ``Forced (".") Scene Heading`` () =
   let doc = """.BINOCULARS A FORCED SCENE HEADING - LATER""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """BINOCULARS A FORCED SCENE HEADING - LATER"""]]

[<Test>]
let ``Lowercase known scene heading`` () =
   let doc = """ext. brick's pool - day""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """ext. brick's pool - day"""]]

[<Test>]
let ``Known INT Scene Head`` () =
   let doc = """INT DOGHOUSE - DAY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """INT DOGHOUSE - DAY"""]]

[<Test>]
let ``Known EXT Scene Head`` () =
   let doc = """EXT DOGHOUSE - DAY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """EXT DOGHOUSE - DAY"""]]

[<Test>]
let ``Known EST Scene Head`` () =
   let doc = """EST DOGHOUSE - DAY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """EST DOGHOUSE - DAY"""]]

[<Test>]
let ``Known INT./EXT Scene Head`` () =
   let doc = """INT./EXT DOGHOUSE - DAY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """INT./EXT DOGHOUSE - DAY"""]]

[<Test>]
let ``Known INT/EXT Scene Head`` () =
   let doc = """INT/EXT DOGHOUSE - DAY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """INT/EXT DOGHOUSE - DAY"""]]

[<Test>]
let ``Known I/E Scene Head`` () =
   let doc = """I/E DOGHOUSE - DAY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [SceneHeading [Literal """I/E DOGHOUSE - DAY"""]]

//===== Synopses

[<Test>]
let ``Basic Synopses`` () =
   let doc = """= Here is a synopses of this fascinating scene.""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Synopses [Literal """Here is a synopses of this fascinating scene."""]]


//===== Character

[<Test>]
let ``Character - Normal`` () =
   let doc = """LINDSEY""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal """LINDSEY"""]]

[<Test>]
let ``Character - With whitespace`` () =
   let doc = """THIS IS ALL UPPERCASE BUT HAS WHITESPACE""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal """THIS IS ALL UPPERCASE BUT HAS WHITESPACE"""]]

[<Test>]
let ``Character - With Numbers`` () =
   let doc = """R2D2""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal """R2D2"""]]

[<Test>]
let ``Character - Number first`` () =
   let doc = """25D2""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Action [Literal """25D2"""]]

[<Test>]
let ``Character - Forced with at sign`` () =
   let doc = """@McAvoy""" |> Fountain.Parse
   doc.Blocks
   |> should equal [Character [Literal """McAvoy"""]]
