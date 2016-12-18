module FountainSharp.Parse.Helper

open System
open System.Text

// Returns count Environment.NewLines in a String
let NewLine(count) = 
    let sb = new StringBuilder()
    for i = 1 to count do
        sb.Append(Environment.NewLine) |> ignore
    sb.ToString()

let NewLineLength = Environment.NewLine.Length

let (|BlockWithTrailingEmptyLine|_|) = function
    | Some(FountainSharp.Transition(_, _, _))
    | Some(FountainSharp.SceneHeading(_, _, _))
    | Some(FountainSharp.TitlePage(_, _))
      -> Some(true)
    | _ -> None

let properNewLines (text: string) = text.Replace("\r\n", System.Environment.NewLine)

let splitIntoLines(text: string) = text.Split([|Environment.NewLine|], StringSplitOptions.None) |> List.ofArray
