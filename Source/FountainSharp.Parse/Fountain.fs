﻿namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic
open System.Text

open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Patterns.Lines
open FountainSharp.Parse
open FountainSharp.Parse.Parser
open FountainSharp.Fountain.Html
open FountainSharp.Parse.Helper

/// Representation of a Fountain document - the representation of Blocks
/// uses an F# discriminated union type and so is best used from F#.
// TODO: this doesn't really need a full blown type for one member (i removed the Links that was part of the markdown doc)
// maybe do a dictionary of character names though. that could be useful.
[<AllowNullLiteral>]
type FountainDocument =
  val mutable private _blocks : FountainBlocks
  val mutable private _text : string

  new (blocks, ?text) = { _blocks = blocks; _text = defaultArg text null }

  /// Returns a list of blocks in the document
  member doc.Blocks with get() = doc._blocks
  /// Returns the original text of the whole document
  member doc.Text with get() = doc._text

  /// Returns the original text of a block
  member doc.GetText(range:Range) =
    if doc.Text.Length < range.Location then
        ""
    else
        let length = min range.Length (doc.Text.Length - range.Location)
        doc.Text.Substring(range.Location, length)

  member doc.ReplaceText(location, length, newText:string) =
       let predicateContains (block:FountainBlockElement) = 
           if block.Range.HasIntersectionWith(new Range(location, length)) then true
           else false
       let predicateDoesNotContain (block:FountainBlockElement) =
           if predicateContains block then false
           else true

       // replace the text
       let newTextBuilder = new StringBuilder(doc._text.Remove(location, length))
       newTextBuilder.Insert(location, newText) |> ignore

       let lengthChange = newText.Length - length
       // blocks being dropped: they are touched by the deletion
       let touchedBlocks = List.where predicateContains doc.Blocks
       // blocks to be preserved: they are not touched by the deletion
       // TODO: this is not always true (some blocks rely on last parsed block)
       let notTouchedBlocks = List.where predicateDoesNotContain doc.Blocks
       // determine the first and last position of touched blocks
       let minTouchedPosition =
         if touchedBlocks.IsEmpty then location
         else touchedBlocks |> List.map(fun block -> block.Range.Location) |> List.min
       let maxTouchedPosition =
         if touchedBlocks.IsEmpty then location
         else touchedBlocks |> List.map(fun block -> block.Range.EndLocation) |> List.max
       let textToParse =
         if minTouchedPosition = maxTouchedPosition then
           newTextBuilder.ToString().Substring(minTouchedPosition, max lengthChange 0)
         else
           newTextBuilder.ToString().Substring(minTouchedPosition, maxTouchedPosition - minTouchedPosition + 1 + lengthChange)
       // parse the text
       let lines = textToParse.Split([|Environment.NewLine|], StringSplitOptions.None) |> List.ofArray
       let ctx = new ParsingContext(Environment.NewLine, minTouchedPosition)
       let newBlocks = lines |> parseBlocks ctx |> List.ofSeq
       // offset the range of blocks after the change
       notTouchedBlocks |> List.iter(fun block -> if block.Range.Location > maxTouchedPosition then block.OffsetRange(lengthChange))
       let finalBlocks = 
           notTouchedBlocks
           |> List.append newBlocks
           |> List.sortBy (fun block -> block.Range.Location)
       doc._blocks <- finalBlocks
       doc._text <- newTextBuilder.ToString() // finalize text change

/// Static class that provides methods for formatting 
/// and transforming Markdown documents.
type Fountain =
  /// Parse the specified text into a MarkdownDocument. Line breaks in the
  /// inline HTML (etc.) will be stored using the specified string.
  static member Parse(text : string, newline) =
    //System.Diagnostics.Debug.WriteLine("Parsing: " + text)
    let lines = text.Split([|Environment.NewLine|], StringSplitOptions.None) |> List.ofArray

    let ctx = new ParsingContext(newline)
    let blocks = 
      lines 
      |> parseBlocks ctx 
      |> List.ofSeq
      |> List.where(fun block -> block.Range.Location < text.Length)
    FountainDocument(blocks, text)

  /// Parse the specified text into a MarkdownDocument.
  static member Parse(text) =
    Fountain.Parse(text, Environment.NewLine)

  /// Transform Fountain document into HTML format. The result
  /// will be written to the provided TextWriter.
  static member TransformHtml(text, writer:TextWriter, newline) = 
    let doc = Fountain.Parse(text, newline)
    formatFountain writer false newline false doc.Blocks

  /// Transform Foutnain document into HTML format. The result
  /// will be written to the provided TextWriter.
  static member TransformHtml(text, writer:TextWriter) = 
    Fountain.TransformHtml(text, writer, Environment.NewLine)

  /// Transform Fountain document into HTML format. 
  /// The result will be returned as a string.
  static member TransformHtml(text, newline) =
    let sb = new System.Text.StringBuilder()
    use wr = new StringWriter(sb)
    Fountain.TransformHtml(text, wr, newline)
    sb.ToString()

  /// Transform Markdown document into HTML format. 
  /// The result will be returned as a string.
  static member TransformHtml(text) =
    Fountain.TransformHtml(text, Environment.NewLine)
  
  /// Transform the provided MarkdownDocument into HTML
  /// format and write the result to a given writer.
  static member WriteHtml(doc:FountainDocument, writer, newline) = 
    formatFountain writer false newline false doc.Blocks

  /// Transform the provided MarkdownDocument into HTML
  /// format and return the result as a string.
  static member WriteHtml(doc:FountainDocument, newline) = 
    let sb = new System.Text.StringBuilder()
    use wr = new StringWriter(sb)
    Fountain.WriteHtml(doc, wr, newline)
    sb.ToString()

  /// Transform the provided MarkdownDocument into HTML
  /// format and return the result as a string.
  static member WriteHtml(doc:FountainDocument) = 
    Fountain.WriteHtml(doc, Environment.NewLine)

  /// Transform the provided MarkdownDocument into HTML
  /// format and write the result to a given writer.
  static member WriteHtml(doc:FountainDocument, writer) = 
    Fountain.WriteHtml(doc, writer, Environment.NewLine)
