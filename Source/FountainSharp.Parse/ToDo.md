# Remaining Work/TODO

## Partial parser

The parser seems all right, although it could be further optimized (
see the implementation in `FountainDocument.ReplaceText`).

## Remaining Syntax Support

See [Syntax Definition](FountainSyntaxDefinition.md).
#### Known issues
 * **Boneyard/Comments** - Only recognized when /\* and \*/ stand alone in their lines.

## Custom CSS Styling

The HTML transformation works well, but it would be nice to allow developers to pass in their own custom css stylesheet that was embedded into the html output. We may even consider allowing a URL to a style sheet.

## Pagination Support

Scripts should keep a running tally of page breaks, both manual, and automatic (based on number of lines on a page and such). HTML transformation should also respect this and show pages.

## Unit Tests + Documentation

Implementing a new feature has to come with unit test(s) and has to be documented in the source code as well.

## Bugfixes

If a bug turns up and you'd fix it, please add a new test for it into **Bugfixes.fs** in the **FountainSharp.Tests** project. With this test project you can check your changes don't mess up anything.

## Developer Documentation

A tutorial on how to use the FountainSharp library should be written, as well as some basic API docs.
