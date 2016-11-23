module FountainSharp.Parse.Tests.Centered

open System
open FsUnit
open NUnit.Framework
open FountainSharp.Parse
open FountainSharp.Parse.Helper

//===== Centered

[<Test>]
let ``Centered `` () =
   let doc = ">The End<" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(1, 7))], new Range(0, 9))]

[<Test>]
let ``Centered - with spaces`` () =
   let doc = "> The End <" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(2, 7))], new Range(0, 11))]

[<Test>]
let ``Centered - indenting`` () =
   let doc = "\t   \t>The End <" |> Fountain.Parse
   doc.Blocks
   |> should equal [Centered ([Literal ("The End", new Range(6, 7))], new Range(0, 15))]

[<Test>]
let ``Centered - followed by line break`` () =
   let doc = properNewLines ">The End<\r\n" |> Fountain.Parse
   doc.Blocks
   |> should equal [ Centered ([Literal ("The End", new Range(1, 7)) ], new Range(0, 9 + NewLineLength)) ]
