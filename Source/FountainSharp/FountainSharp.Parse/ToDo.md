# Remaining Work/TODO

## Add positional support to the parser.

The parser is all right on its own, but to be best used from an editor, each formatted
document element (`FountainSpanElement` and `FountainBlockElement`) should also contain
its positional/range information, i.e.: `start` and `length` of the element with in the
doc.

Range can be:

```FSharp
[<Struct>]
type Range(location:int,length:int) =
  member this.Location = location
  member this.Length = length
```

Both `FountainSpanElement` and `FountainBlockElement` should probably inherit from a
base class that is the Range, or has the Range or something. We can't put it directly
on those classes because they're discriminated unions. See:

* [SO Article 1](http://stackoverflow.com/questions/10959335/how-add-setter-to-to-discriminated-unions-in-f)
* [SO 2](http://stackoverflow.com/questions/1332299/discriminated-union-let-binding)

However, I have some temporary working code checked in right now that has the Range
directly on them, but it should be removed, see:

`FountainSyntax.fs`
```FSharp
type FountainSpanElement =
  | Literal of string * Range // some text
  | Strong of FountainSpans * Range // **some bold text**
...

type FountainBlockElement =
  | Action of bool * FountainSpans * Range
  | Character of bool * FountainSpans * Range
...
```

### Calculating the Range

Because of the nature of parsing routine, it would be very difficult to calculate the
range during each element's parsing. Therefore, it probably needs to be calculated at
the end of the parsing. This means that the Range necessarily needs to be mutable.

I'm open to ideas here, though. Maybe someone smarter than me can do this during
parsing.

### Unit Tests + Documentation

Along with the actual range calculation and class modifications, unit tests should be
created that validate the ranges for all elements. Additionally, the class
modifications themselves should be documented at the class level.

## Nuget Package Creation

Probably both the FountainSharp.Parse and the FountainSharp.Parse.MobilePCL should be packaged up in a NuGet and published.

## Remaining Syntax Support

See [Syntax Definition](FountainSyntaxDefinition.md). Several outstanding syntax
items need to be added and/or modified:

 * **Dual Dialogue** - Not supported at all yet.

 * **Boneyard/Comments** - Not supported at all yet.

 * **Notes** - No support yet for double-space continuation.

 * **Indenting** - Unknown support.

Each one of these also need associated unit tests for parsing, as well as HTML
transformation.

## Custom CSS Styling

The HTML transformation works well, but it would be nice to allow developers to pass in their own custom css stylesheet that was embedded into the html output. We may even consider allowing a URL to a style sheet.


## Pagination Support

Scripts should keep a running tally of page breaks, both manual, and automatic (based on number of lines on a page and such). HTML transformation should also respect this and show pages.

## Developer Documentation

A tutorial on how to use the FountainSharp library should be written, as well as some basic API docs.
