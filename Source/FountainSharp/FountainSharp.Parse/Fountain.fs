// --------------------------------------------------------------------------------------
// F# Markdown (Collections.fs)
// original (c) Tomas Petricek, 2012, Available under Apache 2.0 license.
//
// Modifications (c) by Bryan Costanich, 2015, 2016
// --------------------------------------------------------------------------------------

// --------------------------------------------------------------------------------------
// f# Notes:
//  'type Name = | x | y | z' - is a discriminated union. http://fsharpforfunandprofit.com/posts/discriminated-unions/
//  'and' - is needed when you can't forward declare a type, but you have a cyclic dependency (needed because f#'s compiler sucks
//  '| Section of int * FountainSpans' - a tuple! so in this case, a section/heading has an integer level and the actual text
// --------------------------------------------------------------------------------------


namespace FountainSharp.Parse

open System
open System.IO
open System.Collections.Generic

// --------------------------------------------------------------------------------------
// Definition of the Fountain format
// --------------------------------------------------------------------------------------

///// Column in a table can be aligned to left, right, center or using the default alignment
//type FountainColumnAlignment =
//  | AlignLeft
//  | AlignRight
//  | AlignCenter
//  | AlignDefault

/// Represents inline formatting inside a paragraph. This can be literal (with text), various
/// formattings (string, emphasis, etc.), hyperlinks, images, inline maths etc.
type FountainSpan =
  | Literal of string
  | Strong of FountainSpans
  | Emphasis of FountainSpans
  | Underline of FountainSpans
  | HardLineBreak

/// A type alias for a list of `MarkdownSpan` values
and FountainSpans = list<FountainSpan>

/// Provides an extensibility point for adding custom kinds of spans into a document
/// (`MarkdownEmbedSpans` values can be embedded using `MarkdownSpan.EmbedSpans`)

//// TODO: i don't understand this. learn it. :D is it this? https://msdn.microsoft.com/en-us/library/dd233242.aspx
//and FountainEmbedSpans =
//  abstract Render : unit -> FountainSpans

/// A paragraph represents a (possibly) multi-line element of a Markdown document.
/// Paragraphs are headings, inline paragraphs, code blocks, lists, quotations, tables and
/// also embedded LaTeX blocks.
type FountainParagraph = 
  | Section of int * FountainSpans
  | Character of string
  | Dialogue of FountainSpans
  | Paragraph of FountainSpans
  | Span of FountainSpans
//  | EmbedParagraphs of FountainEmbedParagraphs

/// A type alias for a list of paragraphs
and FountainParagraphs = list<FountainParagraph>

///// Provides an extensibility point for adding custom kinds of paragraphs into a document
///// (`MarkdownEmbedParagraphs` values can be embedded using `MarkdownParagraph.EmbedParagraphs`)
//and FountainEmbedParagraphs =
//  abstract Render : unit -> FountainParagraphs

// --------------------------------------------------------------------------------------
// Patterns that make recursive Markdown processing easier
// --------------------------------------------------------------------------------------

/// This module provides an easy way of processing Markdown documents.
/// It lets you decompose documents into leafs and nodes with nested paragraphs.
module Matching =
  type SpanLeafInfo = private SL of FountainSpan
  type SpanNodeInfo = private SN of FountainSpan

  let (|SpanLeaf|SpanNode|) span = 
    match span with
    | Literal _ 
    | HardLineBreak -> 
        SpanLeaf(SL span)
    | Strong spans 
    | Emphasis spans -> 
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

  let (|ParagraphLeaf|ParagraphNested|ParagraphSpans|) par =
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

  // TODO: figure out what this is doing and comment the shit out of it. :D
  // also, might not need some of it, because it deals with tables.
//  let ParagraphNested (PN(par), pars) =
//    let splitEach n list =
//      let rec loop n left ansList curList items =
//        if List.isEmpty items && List.isEmpty curList then List.rev ansList
//        elif left = 0 || List.isEmpty items then loop n n ((List.rev curList) :: ansList) [] items
//        else loop n (left - 1) ansList ((List.head items) :: curList) (List.tail items)
//      loop n n [] [] list
//
//    match par with 
//(*    | ListBlock(a, _) -> ListBlock(a, pars) *)
//    | TableBlock(headers, alignments, _) ->
//        let rows = splitEach (alignments.Length) pars
//        if List.isEmpty rows || headers.IsNone then TableBlock(None, alignments, rows)
//        else TableBlock(Some(List.head rows), alignments, List.tail rows)
//    | _ -> invalidArg "" "Incorrect ParagraphNestedInfo."
