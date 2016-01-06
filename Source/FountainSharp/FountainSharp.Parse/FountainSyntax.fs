namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic


/// Represents inline formatting inside a block. This can be literal (with text), various
/// formattings (string, emphasis, etc.), hyperlinks, etc.
type FountainSpanElement =
  | Literal of string // some text
  | Strong of FountainSpans // **some bold text**
  | Italic of FountainSpans // *some italicized text*
  | Underline of FountainSpans // _some underlined text_
  | Note of FountainSpans // [[this is my note]]
  | HardLineBreak

/// A type alias for a list of `FountainSpan` values
and FountainSpans = list<FountainSpanElement>

/// A block represents a (possibly) multi-line element of a fountain document.
/// Blocks are headings, action blocks, dialogue blocks, etc. 
type FountainBlockElement = 
  | Block of FountainSpans
  | Character of FountainSpans //TODO: maybe just FountainSpanElement? or just string?
  | Dialogue of FountainSpans
  | Parenthetical of FountainSpans
  | Section of int * FountainSpans
  | Synopses of FountainSpans 
  | Span of FountainSpans
  | Lyric of FountainSpans
  | SceneHeading of FountainSpans //TODO: Should this really just be a single span? i mean, you shouldn't be able to style/inline a scene heading, right?
  | PageBreak
  | Transition of FountainSpans
  | Centered of FountainSpans

/// A type alias for a list of blocks
and FountainBlocks = list<FountainBlockElement>

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