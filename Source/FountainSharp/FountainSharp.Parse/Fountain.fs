namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic

open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Patterns.Lines
open FountainSharp.Parse
open FountainSharp.Parse.Parser

/// Representation of a Fountain document - the representation of Blocks
/// uses an F# discriminated union type and so is best used from F#.
// TODO: this doesn't really need a full blown type for one member (i removed the Links that was part of the markdown doc)
// maybe do a dictionary of character names though. that could be useful.
type FountainDocument(blocks) =
  /// Returns a list of blocks in the document
  member x.Blocks : FountainBlocks = blocks


/// Static class that provides methods for formatting 
/// and transforming Markdown documents.
type Fountain =
  /// Parse the specified text into a MarkdownDocument. Line breaks in the
  /// inline HTML (etc.) will be stored using the specified string.
  static member Parse(text, newline) =
    use reader = new StringReader(text)
    let lines = 
      [ let line = ref ""
        while (line := reader.ReadLine(); line.Value <> null) do
          yield line.Value ]
    let (Lines.TrimBlank lines) = lines
    let ctx : ParsingContext = { Newline = newline }
    let blocks = lines |> parseBlocks ctx |> List.ofSeq
    FountainDocument(blocks)

  /// Parse the specified text into a MarkdownDocument.
  static member Parse(text) =
    Fountain.Parse(text, Environment.NewLine)  