# FountainSharp
An F# based Fountain Markdown processor that's based on the [FSharp.Formatting library](https://github.com/tpetricek/FSharp.Formatting) by Tomas Petricek.

This project is a work in progress. To see where I'm at, check out the [syntax readme](Source/FountainSharp/FountainSharp.Parse/FountainSyntaxDefinition.md).

For contributing, please see the [Source Documentation](Source/FountainSharp/FountainSharp.Parse/Documentation.md)


## TODO

For a detailed list of outstanding tasks, see the [TODO](Source/FountainSharp/FountainSharp.Parse/ToDo.md), however, in general the following major items are outstanding:

 * **Finish Syntax Parsing** - Nearly all of the Fountain syntax is supported, but a few small items remain.
 * **Add Range Support** - Document elements should have an associated `Range` so that the document representation can be more effectively used in an editor.
 * **Custom HTML CSS** - HTML transformation is largely done, but custom CSS templates should be allowed.
 * **Nuget Package** - The library should be published as a Nuget package.
