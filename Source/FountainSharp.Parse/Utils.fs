namespace FountainSharp.Parse

open System

type Utils =
    static member ProperNewLines(text:string) =
        text.Replace("\r\n", Environment.NewLine)

