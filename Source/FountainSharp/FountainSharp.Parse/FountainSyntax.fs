namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic



/// Represents inline formatting inside a paragraph. This can be literal (with text), various
/// formattings (string, emphasis, etc.), hyperlinks, etc.
type FountainSpan =
  | Literal of string
  | Strong of FountainSpans
  | Emphasis of FountainSpans
  | Underline of FountainSpans
  | HardLineBreak

/// A type alias for a list of `MarkdownSpan` values
and FountainSpans = list<FountainSpan>

/// A paragraph represents a (possibly) multi-line element of a fountain document.
/// Paragraphs are headings, action blocks, dialogue blocks, etc. 
type FountainParagraph = 
  | Section of int * FountainSpans
  | Paragraph of FountainSpans


// Document as a tree
/// This module provides an easy way of processing Markdown documents.
/// It lets you decompose documents into leafs and nodes with nested paragraphs.
module Matching =

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
    | Emphasis spans -> 
        SpanNode(SN span, spans)
    | Underline spans ->
        SpanNode(SN span, spans)

  let SpanLeaf (SL(span)) = span
  let SpanNode (SN(span), spans) =
    match span with
    | Strong _ -> Strong spans 
    | Emphasis _ -> Emphasis spans
    | _ -> invalidArg "" "Incorrect SpanNodeInfo"

  type ParagraphSpansInfo = private PS of FountainParagraph
  type ParagraphLeafInfo = private PL of FountainParagraph
  type ParagraphNestedInfo = private PN of FountainParagraph

// TODO: really wish i understood what was happenign here.
//  let (|ParagraphLeaf|ParagraphNested|ParagraphSpans|) par =
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
    | _ -> invalidArg "" "Incorrect ParagraphSpansInfo."

  let ParagraphLeaf (PL(par)) = par

