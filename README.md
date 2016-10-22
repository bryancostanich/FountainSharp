# FountainSharp

[![Build Status](https://www.bitrise.io/app/45c89db89673e862.svg?token=HAU9M6A-HNZGe6rCJ4lnIw)](https://www.bitrise.io/app/45c89db89673e862)

An F# based [Fountain Markdown](http://fountain.io) processor that's based on the [FSharp.Formatting library](https://github.com/tpetricek/FSharp.Formatting) by Tomas Petricek.

FoutainSharp parses Fountain-formatted scripts and loads them into model that can be transformed or used for WYSIWYG editing. It ships with a sample transformation engine that transforms Fountain markdown into HTML:

![Image of a parsed script formatted in HTML](ParsedOutput.png)

FountainSharp fully supports the entirety of the Fountain syntax and includes unit tests for all elements.

## Usage

FountainSharp can operate on an entire script, or just a portion and expects a string as an input and will output an in-memory representation as a `MarkdownDocument`. A convenience. that takes a Fountain-formatted string and outputs an HTML string is also provided.

### To parse an entire script:

```CSharp
var markdown = FountainSharp.Parse.Fountain.Parse(script);

```


```CSharp

var scriptResource = "FountainSharp.Editor.Scripts.SimpleTest.fountain";
string script;

using (var stream = Resource.FromPath (scriptResource)) {
	if (stream != null) {
		using (var reader = new System.IO.StreamReader (stream)) {
			script = reader.ReadToEnd ();

			this.MainTextView.Value = script;
			this.UpdateHtml ();

		}
	} else {
		Console.WriteLine ("Couldn't load " + scriptResource);
	}

}

protected async void UpdateHtml ()
{
	await System.Threading.Tasks.Task.Run (() => {
		
		InvokeOnMainThread (()=>{
			this.MainWebView.MainFrame.LoadHtmlString (FountainSharp.Parse.Fountain.TransformHtml (this.MainTextView.Value), NSUrl.FromString (""));
		});
	});
}
```


## TODO

This project is a work in progress. For a detailed list of outstanding tasks, see the [TODO](Source/FountainSharp/FountainSharp.Parse/ToDo.md), however, in general the following major items are outstanding:

 * **Custom HTML CSS** - HTML transformation is largely done, but custom CSS templates should be allowed.
 * **Usage Documentation** - The library should be documented from a consumer perspective.
 

## Contributing

For contributing, please see the [Source Documentation](Source/FountainSharp/FountainSharp.Parse/Documentation.md). I <3 well documented pull requests. :)

# People

**Author**

[Bryan Costanich](https://twitter.com/bryancostanich)


**Contributors**

Gabor Nemeth
