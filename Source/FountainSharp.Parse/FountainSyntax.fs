namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic

type Range =
  val mutable private _location : int
  val mutable private _length : int
  
  new(location:int,length:int) = { _location = location; _length = length }
  
  member this.Location with get() = this._location and set(l) = this._location <- l
  member this.Length with get() = this._length

  member this.EndLocation
    with get() =
      if this.Length = 0 then this.Location
      else this.Location + this.Length - 1
  
  /// Offsets the location
  member this.Offset(offset) =
    this._location <- this._location + offset
    ()
  
  /// Returns a new Range instance offset from r
  static member Offset(r:Range, offset) =
    new Range(r.Location + offset, r.Length)

  /// Returns true, position is inside
  member this.Contains(position) =
    if position >= this.Location && position < this.Location + this.Length then true
    else false

  member this.HasIntersectionWith(range:Range) =
      this.Contains(range.Location) || this.Contains(range.EndLocation) ||
      range.Contains(this.Location) || range.Contains(this.EndLocation)

  override this.ToString() =
    sprintf "Location: %d; Length: %d" this.Location this.Length
  
  override this.GetHashCode() =
    hash(this.Location, this.Length)

  // override equality check to be structural instead of reference comparison
  override this.Equals(obj) =
    match obj with
    | :? Range as r -> (this.Location, this.Length) = (r.Location, r.Length)
    | _ -> false

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
  member fs.Range
    with get() =
        match fs with
        | Bold(_, r)
        | Italic(_, r)
        | Underline(_, r)
        | Note(_, r) -> r
        | Literal(_, r) -> r
        | HardLineBreak(r) -> r
  
  /// Offsets the range of the span (including inner spans)
  member fs.OffsetRange(offset) =
    match fs with
    | Bold(spans, r)
    | Italic(spans, r)
    | Underline(spans, r)
    | Note(spans, r) ->
      for span in spans do
        span.OffsetRange(offset)
      r.Offset(offset)
    | Literal(_, r) -> r.Offset(offset)
    | HardLineBreak(r) -> r.Offset(offset)

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

  member fb.Range : Range =
    match fb with
    | Character(_, _, _, r) -> r
    | Action(_, _, r)
    | SceneHeading(_, _, r)
    | Section(_, _, r)
    | Transition(_, _, r) -> r
    | Dialogue(_, r)
    | Parenthetical(_, r)
    | Synopses (_, r)
    | Lyrics(_, r)
    | Boneyard(_, r)
    | TitlePage(_, r)
    | DualDialogue(_, r)
    | Centered(_, r) -> r
    | PageBreak(r) -> r
  
  /// Offsets the range of the block (including inner blocks and spans)
  member fb.OffsetRange(offset) =
    let offsetSpans (spans:FountainSpans) offset =
      for span in spans do
        span.OffsetRange(offset)
    match fb with
    | Character(_, _, spans, r) ->
      offsetSpans spans offset
      r.Offset(offset)
    | Action(_, spans, r)
    | SceneHeading(_, spans, r)
    | Section(_, spans, r)
    | Transition(_, spans, r) ->
      offsetSpans spans offset
      r.Offset(offset)
    | Dialogue(spans, r)
    | Parenthetical(spans, r)
    | Synopses (spans, r)
    | Lyrics(spans, r)
    | Centered(spans, r) ->
      offsetSpans spans offset
      r.Offset(offset)
    | DualDialogue(blocks, r) ->
      for block in blocks do block.OffsetRange(offset)
    | Boneyard(_, r)
    | PageBreak(r) -> r.Offset(offset)
    | TitlePage(blocks, r) ->
      for (key, spans) in blocks do
        offsetSpans spans offset
        r.Offset(offset)


/// A type alias for a list of blocks
and FountainBlocks = FountainBlockElement list
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