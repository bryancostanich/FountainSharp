namespace FountainSharp.Parse

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
type FountainDocument(blocks, ?text) =
  /// Returns a list of blocks in the document
  member doc.Blocks : FountainBlocks = blocks
  member doc.Text : string = defaultArg text null

  /// Returns the original text of a block
  member doc.GetText(range:Range) =
    if doc.Text.Length < range.Location then
        ""
    else
        let length = min range.Length (doc.Text.Length - range.Location)
        doc.Text.Substring(range.Location, length)

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
    FountainDocument(blocks, text)

  /// Parse the specified text into a MarkdownDocument.
  static member Parse(text) =
    Fountain.Parse(text, Environment.NewLine)

  // Parses a single line. Used for optimization when working with a large doc from an editor.
  static member ParseLine(text:string, newline) =
    let ctx = new ParsingContext(newline)
    let line = text::[]
    let blocks = 
      line 
      |> parseBlocks ctx 
      |> List.ofSeq
    FountainDocument(blocks, text)

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
