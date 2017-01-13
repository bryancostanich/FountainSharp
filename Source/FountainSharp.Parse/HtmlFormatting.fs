// --------------------------------------------------------------------------------------
// F# Markdown (HtmlFormatting.fs)
// (c) Tomas Petricek, 2012, Available under Apache 2.0 license.
// --------------------------------------------------------------------------------------
// Modifications (c) 2016, Bryan Costanich the Awesome.
// --------------------------------------------------------------------------------------

/// [omit]
namespace FountainSharp

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions

open FountainSharp.Parse
open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Collections

module private Html =

    // --------------------------------------------------------------------------------------
    // Formats Fountain documents as an HTML file
    // TODO: need to prepend a style sheet for underline and such. mabe even return that as a
    // seperate item, with an option of embedding it
    // --------------------------------------------------------------------------------------

    /// Basic escaping as done by Markdown
    let htmlEncode (code:string) = 
      code.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")

    /// Basic escaping as done by Markdown including quotes
    let htmlEncodeQuotes (code:string) = 
      (htmlEncode code).Replace("\"", "&quot;")

    /// Lookup a specified key in a dictionary, possibly
    /// ignoring newlines or spaces in the key.
    let (|LookupKey|_|) (dict:IDictionary<_, _>) (key:string) = 
      [ key; key.Replace("\r\n", ""); key.Replace("\r\n", " "); 
        key.Replace("\n", ""); key.Replace("\n", " ") ]
      |> Seq.tryPick (fun key ->
        match dict.TryGetValue(key) with
        | true, v -> Some v 
        | _ -> None)

    /// Generates a unique string out of given input
    type UniqueNameGenerator() =
      let generated = new System.Collections.Generic.Dictionary<string, int>()

      member __.GetName(name : string) =
        let ok, i = generated.TryGetValue name
        if ok then
          generated.[name] <- i + 1
          sprintf "%s-%d" name i
        else
          generated.[name] <- 1
          name

    // TODO: a base FormattingContext (converted to class type) could be published for clients
    /// Context passed around while formatting the HTML
    type FormattingContext =
      { LineBreak : unit -> unit // line break appending routine
        Newline : string
        Writer : TextWriter
        // don't think i'll ever need this in fountain, but perhaps. so i'll keep it commented for now.
        //GenerateHeaderAnchors : bool
        UniqueNameGenerator : UniqueNameGenerator
        ParagraphIndent : unit -> unit
        PreserveWhiteSpace : bool // whether to preserve white spaces or not
      }

    let bigBreak (ctx:FormattingContext) () =
      ctx.Writer.Write(ctx.Newline + ctx.Newline)
    let smallBreak (ctx:FormattingContext) () =
      ctx.Writer.Write(ctx.Newline)
    let noBreak (ctx:FormattingContext) () = ()

    //======== This is where all of our span element HTML tags are defined.
    /// Write FountainSpan value to a TextWriter
    let rec formatSpan (ctx:FormattingContext) = function
      | Literal(str, range) ->
          // preserve white spaces - Action possibly have those
          ctx.Writer.Write("""<span style="white-space: pre;">""")
          ctx.Writer.Write(if ctx.PreserveWhiteSpace then str else str.Trim())
          ctx.Writer.Write("</span>");
      | HardLineBreak (range) -> ctx.Writer.Write("<br />")
      | Bold(body, range) -> 
          ctx.Writer.Write("<strong>")
          formatSpans ctx body
          ctx.Writer.Write("</strong>")
      | Italic(body, range) -> 
          ctx.Writer.Write("<em>")
          formatSpans ctx body
          ctx.Writer.Write("</em>")
      | Underline(body, range) -> 
          // TODO: put this in a css named style
          ctx.Writer.Write("""<span style="text-decoration: underline;">""")
          formatSpans ctx body
          ctx.Writer.Write("</span>")
      | Note(body, range) -> 
          // TODO: put this in a css named style
          ctx.Writer.Write("""<span style="color: yellow;">""")
          formatSpans ctx body
          ctx.Writer.Write("</span>")

    /// Write list of MarkdownSpan values to a TextWriter
    and formatSpans ctx list = 
      List.iter (formatSpan ctx) list

    // Generates anchor names. Can bring this back in when needed.
    ///// generate anchor name from Markdown text
    //let formatAnchor (ctx:FormattingContext) (spans:FountainSpans) =
    //    let extractWords (text:string) =
    //        Regex.Matches(text, @"\w+")
    //        |> Seq.cast<Match>
    //        |> Seq.map (fun m -> m.Value)
    //
    //    let rec gather (span:FountainSpanElement) : seq<string> = 
    //        seq {
    //            match span with
    //            | Literal str -> yield! extractWords str
    //            | Strong body -> yield! gathers body
    //            | Italic body -> yield! gathers body
    //            | Underline body -> yield! gathers body
    //            | _ -> ()
    //        }
    //
    //    and gathers (spans:FountainSpans) = Seq.collect gather spans
    //
    //    spans 
    //    |> gathers 
    //    |> String.concat "-"
    //    |> fun name -> if String.IsNullOrWhiteSpace name then "header" else name
    //    |> ctx.UniqueNameGenerator.GetName

    let withInner ctx f =
      use sb = new StringWriter()
      let newCtx = { ctx with Writer = sb }
      f newCtx
      sb.ToString()

    //======== This is where all of our block element HTML tags are defined.
    /// Write a FountainBlockElement value to a TextWriter
    let rec formatBlockElement (ctx:FormattingContext) block =
      match block with
      | TitlePage(keyValuePairs, _) ->
          for ((key, _), spans) in keyValuePairs do
              match key with
              | "Contact"
              | "Draft date" ->
                  ctx.Writer.Write("""<div style="text-align:left;"><br/>""")
              | _ ->
                  ctx.Writer.Write("""<div style="text-align:center;"><br/>""")
              formatSpans { ctx with PreserveWhiteSpace = false } spans
              ctx.Writer.Write("</div>")
          ctx.Writer.Write("<hr>") // implicit page break after Title Page
      | Boneyard(_, _) -> ()
      | Section(n, spans, range) -> 
          ctx.Writer.Write("<h" + string n + ">")
    //      if ctx.GenerateHeaderAnchors then
    //        let anchorName = formatAnchor ctx spans
    //        ctx.Writer.Write(sprintf """<a name="%s" class="anchor" href="#%s">""" anchorName anchorName)
    //        formatSpans ctx spans
    //        ctx.Writer.Write "</a>"
    //      else
    //        formatSpans ctx spans
          formatSpans ctx spans
          ctx.Writer.Write("</h" + string n + ">")
      | SceneHeading (forced, spans, range) ->
          ctx.Writer.Write("<div><strong>")
          for span in spans do 
            formatSpan ctx span
          ctx.Writer.Write("</strong></div>")
      | PageBreak (range) ->
          ctx.Writer.Write("<hr>")
      | Synopses (spans, range) ->
          ctx.Writer.Write("""<div style="color:#6cf;">""")
          for span in spans do 
            formatSpan ctx span
          ctx.Writer.Write("</div>")
      | Lyrics (spans, range) ->
          ctx.Writer.Write("""<div style="color:#333"><em>""")
          for span in spans do 
            formatSpan ctx span
          ctx.Writer.Write("</em></div>")
      | Transition (forced, spans, range) ->
          ctx.Writer.Write("""<div style="text-align:right;"><strong>""")
          //if forced then
          //  ctx.Writer.Write("&gt;")
          for span in spans do 
            formatSpan ctx span
          ctx.Writer.Write("</strong><br/></div>")
      | DualDialogue(blocks, range) ->
          ctx.Writer.Write("""<table style="width:100%">""");

          // writes the list of blocks into a table cell
          let writeDualDialogueBlocks primary blocks = 
              if primary then ctx.Writer.Write("<tr>"); // new line for primary character
              ctx.Writer.Write("<td>");
              for block in blocks do
                formatBlockElement ctx block
              ctx.Writer.Write("</td>");
              if not primary then ctx.Writer.Write("</tr>"); // end line after the secondary character
      
          // formatting list of dual dialogue's internal blocks as chunks of (Character, Parenthetical, Dialogue, Parenthetical)
          // Parenthetical blocks are optional
          let rec formatDualDialogueBlocks blocks =
            match blocks with
            | [] -> ()
            | (Character(_, main, _, _) as c) :: (Parenthetical(_, _) as pc) :: (Dialogue(_, _) as d) :: (Parenthetical(_, _) as pd) :: tail ->
              writeDualDialogueBlocks main [c; pc; d; pd]
              formatDualDialogueBlocks tail
            | (Character(_, main, _, _) as c) :: (Dialogue(_, _) as d) :: (Parenthetical(_, _) as pd) :: tail ->
              writeDualDialogueBlocks main [c; d; pd]
              formatDualDialogueBlocks tail
            | (Character(_, main, _, _) as c) :: (Parenthetical(_, _) as pc) :: (Dialogue(_, _) as d) :: tail ->
              writeDualDialogueBlocks main [c; pc; d]
              formatDualDialogueBlocks tail
            | (Character(_, main, _, _) as c) :: (Dialogue(_, _) as d) :: tail ->
              writeDualDialogueBlocks main [c; d]
              formatDualDialogueBlocks tail
            | _ -> ()

          formatDualDialogueBlocks blocks

          ctx.Writer.Write("</table>");
      | Character (forced, primary, spans, range) ->
          ctx.Writer.Write("""<div style="text-align:center;"><br/>""")
          //if forced then
          //  ctx.Writer.Write("@")
          for span in spans do 
            formatSpan ctx span
          ctx.Writer.Write("</div>")
      | Dialogue (spans, range)
      | Centered (spans, range) ->
          ctx.Writer.Write("""<div style="text-align:center;word-wrap:break-word;">""")
          for span in spans do 
            formatSpan ctx span
          ctx.Writer.Write("</div>")
      | Parenthetical (spans, range) ->
          ctx.Writer.Write("""<div style="text-align:center;">(""")
          for span in spans do 
            formatSpan ctx span
          ctx.Writer.Write(")</div>")
      | Action (forced, spans, range) ->
          ctx.Writer.Write("""<div style="word-wrap:break-word;">""")
          formatSpans ctx spans
          ctx.Writer.Write("</div>")
      ctx.LineBreak()

    /// Write a list of MarkdownParagraph values to a TextWriter
    and formatBlocks ctx blocks = 
      ctx.Writer.Write("""<style>body{font-family:"Courier Prime","Courier New",Courier,monospace;}"</style>""")
      let length = List.length blocks
      let smallCtx = { ctx with LineBreak = smallBreak ctx }
      let bigCtx = { ctx with LineBreak = bigBreak ctx }
      for last, block in blocks |> Seq.mapi (fun i v -> (i = length - 1), v) do
        formatBlockElement (if last then smallCtx else bigCtx) block

    /// Format Markdown document and write the result to 
    /// a specified TextWriter. Parameters specify newline character
    /// and a dictionary with link keys defined in the document.
    let formatFountain writer generateAnchors newline wrap = 
      formatBlocks
        { Writer = writer
          Newline = newline
          LineBreak = ignore
          //GenerateHeaderAnchors = generateAnchors
          UniqueNameGenerator = new UniqueNameGenerator()
          ParagraphIndent = ignore
          PreserveWhiteSpace = true
        }

/// Static class that provides methods for formatting Fountain document as HTML
type HtmlFormatter =
  /// Transform Fountain document into HTML format. The result
  /// will be written to the provided TextWriter.
  static member TransformHtml(text, writer:TextWriter, newline) = 
    let doc = FountainDocument.Parse(text, newline)
    Html.formatFountain writer false newline false doc.Blocks

  /// Transform Foutnain document into HTML format. The result
  /// will be written to the provided TextWriter.
  static member TransformHtml(text, writer:TextWriter) = 
    HtmlFormatter.TransformHtml(text, writer, Environment.NewLine)

  /// Transform Fountain document into HTML format. 
  /// The result will be returned as a string.
  static member TransformHtml(text, newline) =
    let sb = new System.Text.StringBuilder()
    use wr = new StringWriter(sb)
    HtmlFormatter.TransformHtml(text, wr, newline)
    sb.ToString()

  /// Transform Markdown document into HTML format. 
  /// The result will be returned as a string.
  static member TransformHtml(text) =
    HtmlFormatter.TransformHtml(text, Environment.NewLine)
  
  /// Transform the provided MarkdownDocument into HTML
  /// format and write the result to a given writer.
  static member WriteHtml(doc:FountainDocument, writer, newline) = 
    Html.formatFountain writer false newline false doc.Blocks

  /// Transform the provided MarkdownDocument into HTML
  /// format and return the result as a string.
  static member WriteHtml(doc:FountainDocument, newline) = 
    let sb = new System.Text.StringBuilder()
    use wr = new StringWriter(sb)
    HtmlFormatter.WriteHtml(doc, wr, newline)
    sb.ToString()

  /// Transform the provided MarkdownDocument into HTML
  /// format and return the result as a string.
  static member WriteHtml(doc:FountainDocument) = 
    HtmlFormatter.WriteHtml(doc, Environment.NewLine)

  /// Transform the provided MarkdownDocument into HTML
  /// format and write the result to a given writer.
  static member WriteHtml(doc:FountainDocument, writer) = 
    HtmlFormatter.WriteHtml(doc, writer, Environment.NewLine)
