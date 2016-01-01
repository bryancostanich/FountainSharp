namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic



/// Represents inline formatting inside a paragraph. This can be literal (with text), various
/// formattings (string, emphasis, etc.), hyperlinks, etc.
type FountainSpan =
  | Literal of string
  | Strong of FountainSpans
  | Italic of FountainSpans
  | Underline of FountainSpans
  | HardLineBreak

/// A type alias for a list of `FountainSpan` values
and FountainSpans = list<FountainSpan>

/// A paragraph represents a (possibly) multi-line element of a fountain document.
/// Paragraphs are headings, action blocks, dialogue blocks, etc. 
type FountainParagraph = 
  | Section of int * FountainSpans
  | Paragraph of FountainSpans
  | Span of FountainSpans

// Document as a tree
/// This module provides an easy way of processing Markdown documents.
/// It lets you decompose documents into leafs and nodes with nested paragraphs.
module Matching =

  // TODO: Question: both SL and SN have the same definition here, are tehy just marker types or something?

  // represents a Leaf in the tree; that is a node that doesn't have any children
  type SpanLeafInfo = 
    private SL of FountainSpan

  // represents a node that has children
  type SpanNodeInfo = 
    private SN of FountainSpan 

  // Active Pattern that returns either a SpanLeaf or SpanNode from a span
  let (|SpanLeaf|SpanNode|) span = 
    match span with
    | Literal _ 
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
  type ParagraphSpansInfo = private PS of FountainParagraph
  type ParagraphLeafInfo = private PL of FountainParagraph
  type ParagraphNestedInfo = private PN of FountainParagraph

  let (|ParagraphSpans|) par =
    match par with  
    | Section(_, spans)
    | Paragraph(spans)
    | Span(spans) ->
        ParagraphSpans(PS par, spans)

  let ParagraphSpans (PS(par), spans) = 
    match par with 
    | Section(a, _) -> Section(a, spans)
    | Paragraph(_) -> Paragraph(spans)
    | Span(_) -> Span(spans)
    //| _ -> invalidArg "" "Incorrect ParagraphSpansInfo." //commented out because it says the rule will never be matched.

  let ParagraphLeaf (PL(par)) = par

