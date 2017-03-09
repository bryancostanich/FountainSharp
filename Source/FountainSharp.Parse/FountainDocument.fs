namespace FountainSharp

open System
open System.Text
open System.Diagnostics

open FountainSharp.Parse
open FountainSharp.Parse.Parser
open FountainSharp.Parse.Helper

/// Representation of a Fountain document - the representation of Blocks
/// uses an F# discriminated union type and so is best used from F#.
// TODO: this doesn't really need a full blown type for one member (i removed the Links that was part of the markdown doc)
// maybe do a dictionary of character names though. that could be useful.
[<AllowNullLiteral>]
type FountainDocument(blocks : FountainBlocks, ?text : string) =
  /// Parse the specified text into a MarkdownDocument. Line breaks in the
  /// inline HTML (etc.) will be stored using the specified string.
  static let parse(text : string, ctx:ParsingContext) =
    let text = properNewLines text
    let lines = text.Split([|Environment.NewLine|], StringSplitOptions.None) |> List.ofArray

    let blocks = 
      lines 
      |> parseBlocks ctx 
      |> List.ofSeq
      |> List.where(fun block -> block.Range.Location < ctx.LastParsedLocation + text.Length)
    FountainDocument(blocks, text)

  static let parseAsync(text, newline) = async {
      return parse(text, new ParsingContext(newline))
  }

  let mutable _blocks = blocks
  let mutable _text = defaultArg text null
  let mutable _classDebug = false

  let rec hasBlockIntersectionWith (block:FountainBlockElement) (blocks:FountainBlocks) =
    match blocks with
    | [] -> false
    | head :: tail -> 
        if head.Equals(block) = false && block.Range.HasIntersectionWith(head.Range) then
          true
        else
          hasBlockIntersectionWith block tail

  /// Returns a list of blocks in the document
  member doc.Blocks with get() = _blocks
  /// Returns the original text of the whole document
  member doc.Text with get() = _text

  /// Returns the original text of a block
  member doc.GetText(range:Range) =
    if doc.Text.Length < range.Location then
        ""
    else
        let length = min range.Length (doc.Text.Length - range.Location)
        doc.Text.Substring(range.Location, length)

  /// Check whether blocks intersecting each other
  member doc.CheckIntersections() =
      let rec hasIntersectionWith (blocks:FountainBlocks) (blocks2:FountainBlocks) =
          match blocks with
          | [] -> false   
          | head :: tail ->
              if hasBlockIntersectionWith head blocks2 then true
              else hasIntersectionWith tail blocks2
      
      hasIntersectionWith doc.Blocks doc.Blocks

  static member Parse(text, newline) =
    parse(text, new ParsingContext(newline))

  static member ParseAsync(text, newline) =
      Async.StartAsTask(parseAsync(text, newline))

  /// Parse the specified text into a MarkdownDocument.
  static member Parse(text) =
    FountainDocument.Parse(text, Environment.NewLine)

  static member ParseAsync(text) =
      Async.StartAsTask(parseAsync(text, Environment.NewLine))

  member doc.ReplaceText(location, length, replaceText:string) =
       let predicateContains (block:FountainBlockElement) = 
           if block.Range.HasIntersectionWith(new Range(location, length)) then true
           else false
       let predicateDoesNotContain (block:FountainBlockElement) =
           if predicateContains block then false
           else true
       let getLength startLocation endLocation =
           if endLocation < startLocation then 0
           else endLocation - startLocation + 1

       let rec getTouchedBlocks (accTouchedBlocks, accNotTouchedBlocks) (blocks: FountainBlocks) =
           match blocks with
           | [] -> (List.rev accTouchedBlocks, List.rev accNotTouchedBlocks)
           | head :: tail ->
             if head.Range.EndLocation < location then
               // haven't reached the modified range yet
               getTouchedBlocks (accTouchedBlocks, head :: accNotTouchedBlocks) tail
             else if predicateContains(head) then
               // inside the modified range
               getTouchedBlocks (head :: accTouchedBlocks, accNotTouchedBlocks) tail
             else
               // after the modified range we consider blocks touched until an Action block
               match head with
               | FountainSharp.Action(_, _, _) -> (List.rev accTouchedBlocks, List.rev accNotTouchedBlocks)
               | _ -> getTouchedBlocks (head :: accTouchedBlocks, accNotTouchedBlocks) tail

       /// determine the min, max position and the length of range occupied by the blocks       
       let lengthOfBlocks (blocks:FountainBlocks) =
         let mutable minLocation =
           if blocks.IsEmpty then location
           else blocks |> List.map(fun block -> block.Range.Location) |> List.min
         let maxLocation =
           if blocks.IsEmpty then location
           else blocks |> List.map(fun block -> block.Range.EndLocation) |> List.max
         (minLocation, maxLocation, getLength minLocation maxLocation)

       // build the new text
       let newTextBuilder = new StringBuilder(doc.Text.Remove(location, length))
       newTextBuilder.Insert(location, replaceText) |> ignore
       let newText = newTextBuilder.ToString()

       let lengthChange = replaceText.Length - length
       // determine blocks touched and not touched by the text modification
       let (touchedBlocks, notTouchedBlocks) = getTouchedBlocks ([],[]) doc.Blocks
       let (minTouchLocation, maxTouchLocation, touchLength) = lengthOfBlocks touchedBlocks
       
       // idea: let's parse from the last non touched block
       let isSimpleBlock (block:FountainBlockElement) =
         // true if the block type is not affected by surrounding change
         // false, if the surrounding change can change the block itself
         // e.g.: Dialogue, DualDialogue
         match block with
         | FountainSharp.Character(_, _, _, _)
         | FountainSharp.Dialogue(_, _) -> false
         | _ -> true

       let prevBlocks = doc.Blocks |> List.map (fun block -> (block, block.Range.EndLocation)) |> List.where (fun (block, l) -> l < location && isSimpleBlock block) |> List.sortBy(fun (block, location) -> -location)
       // let's determine the last valid block to look back - the second to last one it is.
       let lastParsedBlock =
         if prevBlocks.Length < 2 then None
         else Some(fst(prevBlocks |> List.skip 1 |> List.head))
       let lookBackLocation = // from here we parse now
         if lastParsedBlock.IsSome then lastParsedBlock.Value.Range.EndLocation + 1
         else 0 // not enough preceding blocks (at most 1), start from the beginning
       let lookBackLength = minTouchLocation - lookBackLocation
       
       let parseRange = new Range(lookBackLocation, min (max (lookBackLength + touchLength + lengthChange) 0) (newText.Length - lookBackLocation) )
       let textToParse = newText.Substring(parseRange.Location, parseRange.Length)
       Debug.WriteLineIf(_classDebug, String.Format("Parsing text: '{0}'", textToParse))
       // parse the text
       let ctx = new ParsingContext(Environment.NewLine, lookBackLocation, lastParsedBlock)
       let reparsedDoc = parse(textToParse, ctx)
       let reparsedBlocks = reparsedDoc.Blocks
       Debug.WriteLineIf(_classDebug && reparsedBlocks.IsEmpty, "No block has been recognized.")
       // offset the range of blocks after the change
       notTouchedBlocks |> List.iter(fun block -> if block.Range.Location > maxTouchLocation then block.OffsetRange(lengthChange))
       let finalNotTouchedBlocks = notTouchedBlocks |> List.choose (fun block -> if parseRange.Contains(block.Range) then None else Some(block))

       let finalBlocks = 
           finalNotTouchedBlocks
           |> List.append (reparsedBlocks |> List.where(fun block -> hasBlockIntersectionWith block finalNotTouchedBlocks = false))
           |> List.sortBy (fun block -> block.Range.Location)
       _blocks <- finalBlocks
       _text <- newText // finalize text change
    
    /// Append text to the end of the document
    member doc.AppendText(text) =
        doc.ReplaceText(_text.Length, 0, text)
