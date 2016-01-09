// --------------------------------------------------------------------------------------
// F# Markdown (HtmlFormatting.fs)
// (c) Tomas Petricek, 2012, Available under Apache 2.0 license.
// --------------------------------------------------------------------------------------
// Modifications (c) 2016, Bryan Costanich the Awesome.
// --------------------------------------------------------------------------------------

/// [omit]
module FountainSharp.Fountain.Html

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions

open FountainSharp.Parse
open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Collections

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

/// Context passed around while formatting the HTML
type FormattingContext =
  { LineBreak : unit -> unit
    Newline : string
    Writer : TextWriter
    // don't think i'll ever need this in fountain, but perhaps. so i'll keep it commented for now.
    //GenerateHeaderAnchors : bool
    UniqueNameGenerator : UniqueNameGenerator
    ParagraphIndent : unit -> unit }

let bigBreak (ctx:FormattingContext) () =
  ctx.Writer.Write(ctx.Newline + ctx.Newline)
let smallBreak (ctx:FormattingContext) () =
  ctx.Writer.Write(ctx.Newline)
let noBreak (ctx:FormattingContext) () = ()

//======== This is where all of our span element HTML tags are defined.
/// Write FountainSpan value to a TextWriter
let rec formatSpan (ctx:FormattingContext) = function
  | Literal(str) -> ctx.Writer.Write(str)
  | HardLineBreak -> ctx.Writer.Write("<br />")
  | Strong(body) -> 
      ctx.Writer.Write("<strong>")
      formatSpans ctx body
      ctx.Writer.Write("</strong>")
  | Italic(body) -> 
      ctx.Writer.Write("<em>")
      formatSpans ctx body
      ctx.Writer.Write("</em>")
  | Underline(body) -> 
      // TODO: put this in a css named style
      ctx.Writer.Write("""<span style="text-decoration: underline;">""")
      formatSpans ctx body
      ctx.Writer.Write("</span>")
  | Note(body) -> 
      // TODO: put this in a css named style
      ctx.Writer.Write("""<span style="color: yellow;">""")
      formatSpans ctx body
      ctx.Writer.Write("</span>")

/// Write list of MarkdownSpan values to a TextWriter
and formatSpans ctx = List.iter (formatSpan ctx)

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
  | Section(n, spans) -> 
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
  | SceneHeading (spans) ->
      ctx.Writer.Write("<div><strong>")
      for span in spans do 
        formatSpan ctx span
      ctx.Writer.Write("</strong></div>")
  | PageBreak ->
      ctx.Writer.Write("<hr>")
  | Synopses (spans) ->
      ctx.Writer.Write("""<div style="color:#6cf;">""")
      for span in spans do 
        formatSpan ctx span
      ctx.Writer.Write("</div>")
  | Lyric (spans) ->
      ctx.Writer.Write("""<div style="color:#333"><em>""")
      for span in spans do 
        formatSpan ctx span
      ctx.Writer.Write("</em></div>")
  | Transition spans ->
      ctx.Writer.Write("""<div style="text-align:right;"><strong>""")
      for span in spans do 
        formatSpan ctx span
      ctx.Writer.Write("</strong><br/></div>")
  | Character spans ->
      ctx.Writer.Write("""<div style="text-align:center;"><br/>""")
      for span in spans do 
        formatSpan ctx span
      ctx.Writer.Write("</div>")
  | Dialogue spans
  | Centered spans ->
      ctx.Writer.Write("""<div style="text-align:center;">""")
      for span in spans do 
        formatSpan ctx span
      ctx.Writer.Write("</div>")
  | Parenthetical spans ->
      ctx.Writer.Write("""<div style="text-align:center;">(""")
      for span in spans do 
        formatSpan ctx span
      ctx.Writer.Write(")</div>")
  | Action spans
  | Span spans -> 
      formatSpans ctx spans
  //| Block(spans) ->
  //    ctx.ParagraphIndent()
  //    ctx.Writer.Write("<p>")
  //    for span in spans do 
  //      formatSpan ctx span
  //    ctx.Writer.Write("</p>")
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
      ParagraphIndent = ignore }

