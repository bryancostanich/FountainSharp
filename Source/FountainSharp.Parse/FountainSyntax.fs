namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic

[<Struct>]
type Range(location:int,length:int) = 
  member this.Location = location
  member this.Length = length

  member this.Offset(offset) =
    new Range(this.Location + offset, this.Length)

  member this.Contains(position) =
    if position >= this.Location && position < this.Location + this.Length then true
    else false
  
  member this.HasIntersectionWith(range:Range) =
      this.Contains(range.Location) || this.Contains(range.Location + range.Length)

  override this.ToString() =
    sprintf "Location: %d; Length: %d" this.Location this.Length

  static member empty = new Range(0, 0)

/// Represents inline formatting inside a block. This can be literal (with text), various
/// formattings (string, emphasis, etc.), hyperlinks, etc.

// TODO: implement a base class that has a mutable range, a la: 
// http://stackoverflow.com/questions/10959335/how-add-setter-to-to-discriminated-unions-in-f
// http://stackoverflow.com/questions/1332299/discriminated-union-let-binding

type FountainSpanElement =
  | Literal of string * Range // some text
  | Bold of FountainSpans * Range // **some bold text**
  | Italic of FountainSpans * Range // *some italicized text*
  | Underline of FountainSpans * Range// _some underlined text_
  | Note of FountainSpans * Range// [[this is my note]]
  | HardLineBreak of Range

  // TODO: make Range available for all types without pattern matching
  member fs.GetLength() : int =
    match fs with
    | Bold(spans, r)
    | Italic(spans, r)
    | Underline(spans, r)
    | Note(spans, r) -> r.Length
    | HardLineBreak(r) -> r.Length
    | Literal(str, r) -> r.Length
  
  member fs.GetRange(start:int):Range =
    new Range(start, fs.GetLength() + start)

/// A type alias for a list of `FountainSpan` values
and FountainSpans = list<FountainSpanElement>

/// A block represents a (possibly) multi-line element of a fountain document.
/// Blocks are headings, action blocks, dialogue blocks, etc. 
type FountainBlockElement = 
  | Action of bool * FountainSpans * Range
  | Character of bool * bool * FountainSpans * Range
  | Dialogue of FountainSpans * Range
  | Parenthetical of FountainSpans * Range
  | Section of int * FountainSpans * Range
  | Synopses of FountainSpans * Range
  | Lyrics of FountainSpans * Range
  | SceneHeading of bool * FountainSpans * Range //TODO: Should this really just be a single span? i mean, you shouldn't be able to style/inline a scene heading, right?
  | PageBreak of Range
  | Transition of bool * FountainSpans * Range
  | Centered of FountainSpans * Range
  | Boneyard of string * Range
  | DualDialogue of FountainBlocks * Range
  | TitlePage of (TitlePageKey * FountainSpans) list * Range

  member fb.GetLength() : int =
    match fb with
    | Character(_, _, _, r) -> r.Length
    | Action(_, _, r)
    | SceneHeading(_, _, r)
    | Section(_, _, r)
    | Transition(_, _, r) -> r.Length
    | Dialogue(_, r)
    | Parenthetical(_, r)
    | Synopses (_, r)
    | Lyrics(_, r)
    | Boneyard(_, r)
    | TitlePage(_, r)
    | DualDialogue(_, r)
    | Centered(_, r) -> r.Length
    | PageBreak(r) -> r.Length

/// A type alias for a list of blocks
and FountainBlocks = list<FountainBlockElement>
and TitlePageKey = string * Range // range contains the trailing ":", but the string not
and TitlePageBlock = FountainSpans * Range 

(*
// Document as a tree
/// This module provides an easy way of processing Markdown documents.
/// It lets you decompose documents into leafs and nodes with nested paragraphs.
module Matching =

  // TODO: Question: both SL and SN have the same definition here, are tehy just marker types or something?

  // represents a Leaf in the tree; that is a node that doesn't have any children
  type SpanLeafInfo = 
    private SL of FountainSpanElement

  // represents a node that has children
  type SpanNodeInfo = 
    private SN of FountainSpanElement 

  // Active Pattern that returns either a SpanLeaf or SpanNode from a span
  let (|SpanLeaf|SpanNode|) span = 
    match span with
    | Literal _
    | Note _ //TODO: not sure what this should be
    | HardLineBreak ->
        SpanLeaf(SL span) //SpanLeafInfo or (SpanNodeInfo * FountainSpans)
    | Strong spans 
    | Italic spans ->
        SpanNode(SN span, spans)
    | Underline spans ->
        SpanNode(SN span, spans)
  
  //TODO: Question: what is happening here?
    // 1) isn't it backwards that the type is defined here? or is the active pattern itself
    // the definition of types?
    // 2) what is the syntax actually doing? why is it in parentheses?
  let SpanLeaf (SL(span)) = span
    // 3) so are SpanNodes only these formatted bits? what about non formatted bits?
  let SpanNode (SN(span), spans) =
    match span with
    | Strong _ -> Strong spans 
    | Italic _ -> Italic spans
    | Underline _ -> Underline spans
    | _ -> invalidArg "" "Incorrect SpanNodeInfo"

  // TODO: Question: marker types again?
  type ParagraphSpansInfo = private PS of FountainBlockElement
  type ParagraphLeafInfo = private PL of FountainBlockElement
  type ParagraphNestedInfo = private PN of FountainBlockElement

  let (|ParagraphSpans|) par =
    match par with  
    | Section(_, spans)
    | Block(spans)
    | Lyric(spans)
    | Span(spans) ->
        ParagraphSpans(PS par, spans)

  let ParagraphSpans (PS(par), spans) = 
    match par with 
    | Section(a, _) -> Section(a, spans)
    | Block(_) -> Block(spans)
    | Span(_) -> Span(spans)
    | Lyric(_) -> Lyric(spans)
    | SceneHeading(_) -> SceneHeading(spans)
    //| _ -> invalidArg "" "Incorrect ParagraphSpansInfo." //commented out because it says the rule will never be matched.

  let ParagraphLeaf (PL(par)) = par
*)