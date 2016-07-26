# Understanding the Parser

## A note about idealogy, and a caveat on code quality.

This parser is based on Petricek's 
[FSharp.Formatting library](https://github.com/tpetricek/FSharp.Formatting). Which,
in turn, is based on the FSharp Compiler. As such, you may find it to be incredibly 
dense, and certainly, in most places, idiomatic.

In some ways, this is good; it certainly illustrates the power and elegance of F#. 
In other ways, it is not as good. The density of the functions makes much of it 
difficult to understand on cursory inspection, and the use of advanced features 
means that novice F#'ers will find a steep learning curve in understanding, 
maintaining, and extending the library.

Personally, I find myself allergic to religion in all its forms, so while much of 
the underlying approaches have been maintained, I've biased towards practicality 
and, as much as possible, simplicity, in my own implementation.

It's also important to point out that I am not a great, or probably even good, F# 
programmer. Indeed, creating this library was my very first attempt at F#. As such, 
there are probably places in which my own implementations are barbaric in their 
approach. I am open to suggestions.

## Files and Structure

### Projects

* **PortableProjects Folder** - Contains the consumable PCL libraries. Today, 
  there is only one project, the **MobilePCL** project, intended to be consumed
  via Xamarin (and nearly any other platform).

* **TestProjects Folder** - Contains the unit tests (**FountainSharp.Parse.Tests**),
  and the **FountainSharp.Editor** project (a Xamarin.Mac test app that visualizes
  the parsing).

* **FountanSharp.Parse** - The actual source files that are consumed by the PCL 
  projects.

### FountainSharp.Parse Files

* **Fountain.fs** - Contains a .NET/C# consumable `FountainSyntax` document and 
 associated parse/load/transformation methods. 

* **HtmlFormatting.fs** - A module that transforms a Fountain document 
 into a formatted HTML document.

* **FountainParser.fs** - The module that's responsible for actually parsing 
 string input into an in memory Fountain doc.

* **FountainSyntax.fs** - Represents an actual fountain doc that consists of
 a `List<FountainBlockElement>`. Defines the syntax of the doc itself.

* **StringParsing.fs** - A utility module that contains methods for parsing strings. 
 Largely unmodified from Petricek's implementation.

* **Collections.fs** - Similar to `StringParsing.fs`, this module contains utility
 methods for working with lists that are relevant to parsing text.

## General Parser Methodology

Generally, the parser works as follows:

 * In `Fountain.fs`, take a string input, break it into lines and pass
   a list of those lines in to `FountainParser.fs`.

 * In `FountainParser.fs`, use active pattern matching to match each of those 
   lines into an appropriate `FountainBlockElement`, and then further parse each 
   of those into `FountainSpan` elements.

A `FountainBlockElement` generally represents a piece of the Fountain doc that ends with 
a line break, but may actually have more blocks within it. For instance, a 
`Dialogue` block contains the *Character Name*, an optional *Parenthetical*, and
then the actual dialogue text. Each of these things are separated by line breaks, 
but each element only makes sense within the context of the entire `Dialogue` 
`FountainBlockElement`.

Within a block are one or more `FountainSpanElement`s which represent formatting 
within a block such as `Underline` or `Strong`. Of particular importance is 
`Literal`, which is a span element that actually contains text.

## HTML Transformation
TODO: Write this documentation