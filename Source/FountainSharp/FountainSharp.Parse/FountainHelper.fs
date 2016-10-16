module FountainSharp.Parse.Helper

open System
open System.Text

// Returns count Environment.NewLines in a String
let NewLine(count) = 
    let sb = new StringBuilder()
    for i = 1 to count do
        sb.AppendLine() |> ignore
    sb.ToString()

let NewLineLength = Environment.NewLine.Length