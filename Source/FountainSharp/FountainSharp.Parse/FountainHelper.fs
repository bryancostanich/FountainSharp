module FountainSharp.Parse.Helper

open System.Text

// Returns count Environment.NewLines in a String
let NewLine(count) = 
    let sb = new StringBuilder()
    for i = 1 to count do
        sb.AppendLine() |> ignore
    sb.ToString()
