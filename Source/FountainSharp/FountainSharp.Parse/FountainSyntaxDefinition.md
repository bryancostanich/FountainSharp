The following syntax definition comes from the [Fountain.io](http://fountain.io/syntax) site.

# Current Syntax Support Status

  * [Scene Heading](#scene-heading) - √
  * [Action](#action) - not supported
  * [Character](#character) - √ - Missing support for Parenthetical extensions, e.g. "BOB (OS)" or "AMY (on the radio)"
  * [Dialogue](#dialogue) - √
  * [Parenthetical](#parenthetical) - √
  * [Dual Dialogue](#dual-dialogue) - not supported
  * [Lyrics](#lyrics) - √
  * [Transition](#transition) - √ - Needs a little refinement to look for hardlinebreaks before and after.
  * [Centered Text](#centered-text) - not supported
  * [Emphasis](#emphasis) - √
  * [Title Page](#title-page) - not supported
  * [Page Breaks](#page-breaks) - √
  * [Punctuation](#punctuation) - √
  * [Line Breaks](#line-breaks) - √ - Mostly, I think.
  * [Indenting](#indenting) - ?
  * [Notes](#notes) - √ - No support yet for double-space continuation.
  * [Boneyard/Comments](#boneyard-comments) - not supported
  * [Sections](#sections) - √
  * [Synposes](#synopses) - √


# Scene Heading

TODO: Allow lower-case/mixed case scene headings (make string ToUpper first and then compare)

A Scene Heading is any line that has a blank line following it, and either begins with INT or EXT or similar (full list below). A Scene Heading always has at least one blank line preceding it.

Power user: You can "force" a Scene Heading by starting the line with a single period.

Here's a regular Scene Heading followed by a forced Scene Heading.

```
EXT. BRICK'S POOL - DAY

Steel, in the middle of a heated phone call:

STEEL
They're coming out of the woodwork!
(pause)
No, everybody we've put away!
(pause)
Point Blank Sniper?

.SNIPER SCOPE POV

From what seems like only INCHES AWAY.  _Steel's face FILLS the *Leupold Mark 4* scope_.
```

`EXT. BRICK'S POOL - DAY` becomes a Scene Heading because of the `EXT`, but `SNIPER SCOPE POV` requires
the period before it to force a Scene Heading element. The period is removed from the formatted output.

Note that only a single leading period followed by an alphanumeric character will force a Scene Heading.
This allows the writer to begin Action and Dialogue elements with ellipses without worry that they'll be
interpreted as Scene Headings.

```
EXT. OLYMPIA CIRCUS - NIGHT

...where the second-rate carnival is parked for the moment in an Alabama field.
```

Although uppercase is recommended for Scene Headings to increase readability, it is not required. This will still be recognized as a Scene Heading:

```
ext. brick's pool - day
```

A line beginning with any of the following, followed by either a dot or a space, is considered a Scene Heading (unless the line is preceded by an exclamation point !). Case insensitive.

```
INT
EXT
EST
INT./EXT
INT/EXT
I/E
```

**Power user:** Scene Headings can optionally be appended with Scene Numbers. Scene numbers are any alphanumerics (plus dashes and periods), wrapped in #. All of the following are valid scene numbers:

```
INT. HOUSE - DAY #1#
INT. HOUSE - DAY #1A#
INT. HOUSE - DAY #1a#
INT. HOUSE - DAY #A1#
INT. HOUSE - DAY #I-1-A#
INT. HOUSE - DAY #1.#
INT. HOUSE - DAY - FLASHBACK (1944) #110A#
```


# Action

Action, or scene description, is any paragraph that doesn't meet criteria for another element (e.g. Scene
Heading, Character, Dialogue, etc.). Fountain respects your line-by-line decision to single or double-space,
taking every carriage return as intentional.

```
They drink long and well from the beers.

And then there's a long beat.
Longer than is funny.
Long enough to be depressing.

The men look at each other.
```

**Power User:** You can force an Action element can by preceding it with an exclamation point `!`.

This is helpful when Action is in uppercase and directly followed by another line of Action, preventing the
two from being interpreted as Character and Dialogue elements.

More on the line break logic can be found here.

Tabs and spaces are retained in Action elements, allowing writers to indent a line. Tabs are converted to
four spaces.

```
He opens the card.  A simple little number inside of which is hand written:

          Scott --

          Jacob Billups
          Palace Hotel, RM 412
          1:00 pm tomorrow

Scott exasperatedly throws down the card on the table and picks up the phone, hitting speed dial #1...
```

Here the ten spaces before the text on the card are passed through to the formatted output. All lines in this example are Action.

(Note that when an Action element is centered, leading spaces are not preserved.)

Action also respects vertical whitespace. Any number of empty lines in the Fountain file will be passed faithfully through to the formatted output as empty Action lines.

# Character
A Character element is any line entirely in uppercase, with one empty line before it and without an empty line after it.

```
STEEL
The man's a myth!
If you want to indent a Character element with tabs or spaces, you can, but it is not necessary. See Indenting.
```

"Character Extensions"--the parenthetical notations that follow a character name on the same line--may be in uppercase or lowercase:

```
MOM (O. S.)
Luke! Come down for supper!

HANS (on the radio)
What was it you said?
Character names must include at least one alphabetical character. "R2D2" works, but "23" does not.
```
Power User: You can force a Character element by preceding it with the "at" symbol `@`.

The ability to force a Character element is helpful for names that require lower-case letters, and for non-Roman languages.

```
@McCLANE
Yippie ki-yay! I got my lower-case C back!
```
Fountain will remove the `@` and interpret McCLANE as Character, preserving its mixed case.

# Dialogue

Dialogue is any text following a Character or Parenthetical element.

```
SANBORN
A good 'ole boy. You know, loves the Army, blood runs green. Country boy. Seems solid.
Manual line breaks are allowed in Dialogue, as are intentionally "empty" lines--see the Line Breaks section.

DAN
Then let's retire them.
_Permanently_.
```

# Parenthetical

Parentheticals follow a Character or Dialogue element, and are wrapped in parentheses `()`.

```
STEEL
(starting the engine)
So much for retirement!
```

# Dual Dialogue

Dual, or simultaneous, dialogue is expressed by adding a caret `^` after the second Character element.

```
BRICK
Screw retirement.

STEEL ^
Screw retirement.
```
Any number of spaces between the Character name and the caret are acceptable, and will be ignored. All that matters is that the caret is the last character on the line.

# Lyrics

You create a Lyric by starting with a line with a tilde ~.

```
~Willy Wonka! Willy Wonka! The amazing chocolatier!
~Willy Wonka! Willy Wonka! Everybody give a cheer!
```

Fountain will remove the '~' and leave it up to the app to style the Lyric appropriately. Lyrics are always forced. There is no "automatic" way to get them.

# Transition

The requirements for Transition elements are:

* Uppercase
* Preceded by and followed by an empty line
* Ending in `TO:`

Example:

```
Jack begins to argue vociferously in Vietnamese (?), But mercifully we...

CUT TO:

EXT. BRICK'S POOL - DAY
```

**Power user:** You can force any line to be a transition by beginning it with a greater-than symbol `>`.

```
Brick and Steel regard one another.  A job well done.

> Burn to White.
```

**Power user:** If a line matches the rules for Transition, but you want in interpreted as something else, you have two options:

* Precede it with a period to force a Scene Heading, or
* Add one or more spaces after the colon to cause the line to be interpreted as Action (since the line no longer ends with a colon).

# Centered Text
Centered text constitutes an Action element, and is bracketed with greater/less-than:

```
>THE END<
```

Leading spaces are usually preserved in Action, but not for centered text, so you can add spaces between the text and the >< if you like.

```
> THE END <
```

# Emphasis
Supported, but needs some testing. i doubt nesting works correctly.

Fountain follows Markdown's rules for emphasis, except that it reserves the use of underscores for underlining, which is not interchangeable with italics in a screenplay.

```
*italics*
**bold**
***bold italics***
_underline_
```
In this way the writer can mix and match and combine bold, italics and underlining, as screenwriters often do.

```
From what seems like only INCHES AWAY.  _Steel's face FILLS the *Leupold Mark 4* scope_.
```
If you need to use any of the emphasis syntaxes verbatim, you escape the characters using the Markdown convention of a backslash:

```
Steel enters the code on the keypad: **\*9765\***
```
Believe it or not, that turns into:

> Steel enters the code on the keypad: *9765*

As with Markdown, the spaces around the emphasis characters are meaningful. In this example, the asterisks would not trigger italics between them, because both have a space to the left:

```
He dialed *69 and then *23, and then hung up.
```
But in this case, the text between the asterisks would be italicized:

```
He dialed *69 and then 23*, and then hung up.
```
The writer would need to escape one or both of the asterisks to avoid the accidental italics:

```
He dialed *69 and then 23\*, and then hung up.
```
Also as with Markdown, emphasis is not carried across line breaks. So there are no italics in the formatted output of this example--just asterisks:

```
As he rattles off the long list, Brick and Steel *share a look.

This is going to be BAD.*
```

# Title Page

The optional Title Page is always the first thing in a Fountain document. Information is encoding in the
format `key: value`. Keys can have spaces (e. g. `Draft date`), but must end with a colon.

```
Title:
    _**BRICK & STEEL**_
    _**FULL RETIRED**_
Credit: Written by
Author: Stu Maschwitz
Source: Story by KTM
Draft date: 1/20/2012
Contact:
    Next Level Productions
    1588 Mission Dr.
    Solvang, CA 93463
```
The recommendation is that Title, Credit, Author (or Authors, either is a valid key syntax), and Source
will be centered on the page in formatted output. Contact and Draft date would be placed at the lower left.

Values can be inline with the key or they can be indented on a newline below the key (as shown with
Contact above). Indenting is 3 or more spaces, or a tab. The indenting pattern allows multiple values for
the same key (multiple authors, multiple address lines).

The key values may change, but those listed above comprise a minimal useful set. If you add unsupported
key values to your document, they will be ignored, but you may find them useful as metadata.

All Title Page parts are optional. So:

```
Draft date: 6/23/2012
```

...on its own is a valid Title Page.

A page break is implicit after the Title Page. Just drop down two lines and start writing your screenplay.

# Page Breaks

Page Breaks are indicated by a line containing three or more consecutive equals signs, and nothing more. Page
breaks are useful for television scripts, where act breaks are explicitly labeled, and for creating "vanity"
first-pages featuring a quotation or prologue text.

```
The General Lee flies through the air. FREEZE FRAME.

NARRATOR
Shoot, to the Dukes that's about like taking Grandma for a Sunday drive.

>**End of Act One**<

===

>**Act Two**<

The General Lee hangs in the air, right where we left it.  The NARRATOR'S voice kicks in.
```

# Punctuation
Some Markdown interpreters convert plain text shorthands for common punctuation to their typographical equivalencies.
For example, three consecutive dashes become an em-dash, three consecutive periods becomes an ellipsis, and straight
quotes become curly quotes.

Fountain doesn't do any of that, because the screenplay typographical convention is to emulate a typewriter. However
you type your apostrophes, quotes, dashes, and dots, that's how they'll wind up in the screenplay.

# Line Breaks

Unlike some markup languages, Fountain takes every carriage return as intent. This allows the writer to control the spacing between paragraphs in Action elements, as seen in this classic example:

Murtaugh, springing hell bent for leather -- and folks, grab your hats ... because just then, a BELL COBRA HELICOPTER crests the edge of the bluff.

```
An explosion of sound...
As it rises like an avenging angel ...
Hovers, shattering the air with turbo-throb, sandblasting the hillside with a roto-wash of loose dirt, tables, chairs, everything that's not nailed down ...

Screaming, chaos, frenzy.
Three words that apply to this scene.
```

All of these lines are interpreted as Action, and the writer's single and double-space decisions would be preserved.

There are some unusual cases where this will fail. If you wrote something like this:

```
INT. CASINO - NIGHT

THE DEALER eyes the new player warily.

SCANNING THE AISLES...
Where is that pit boss?

No luck. He has no choice to deal the cards.
```
...Fountain would interpret SCANNING THE AISLES... as a Character name--due to the entire line being in uppercase--and
therefore subsequently interpret Where is that pit boss? as Dialogue. To correct this, use a preceding ! to force the
uppercase line to be Action:

```
INT. CASINO - NIGHT

THE DEALER eyes the new player warily.

!SCANNING THE AISLES...
Where is that pit boss?

No luck. He has no choice to deal the cards.
```

In this case, ```INT. CASINO - NIGHT``` is a Scene Heading, and all the other lines are interpreted as Action elements.

Dialogue is a bit easier. If you typed:

```
DEALER
Ten.
Four.
Dealer gets a seven.  Hit or stand sir?

MONKEY
Dude, I'm a monkey.
```

...Fountain will know that DEALER's dialogue should be one continuous formatted block with forced line breaks. However, if you want to do the unconventional thing of leaving white space in dialogue:

```
DEALER
Ten.
Four.
Dealer gets a seven.

Hit or stand sir?

MONKEY
Dude, I'm a monkey.
...You would need to type two spaces on your "blank" line so that Fountain knows that Hit or stand sir? is not an Action element:

DEALER
Ten.
Four.
Dealer gets a seven.
{two spaces}
Hit or stand sir?

MONKEY
Dude, I'm a monkey.
```

# Indenting
Leading tabs or spaces in elements other than Action will be ignored. If you choose to use them though, your Fountain text file could
look quite a bit more like a screenplay.

In this example, Transitions are preceded by four tabs, Character names by three, Parentheticals by two, and Dialogue by one.

```
                CUT TO:

INT. GARAGE - DAY

BRICK and STEEL get into Mom's PORSCHE, Steel at the wheel.  They pause for a beat, the gravity of the situation catching up with them.

            BRICK
    This is everybody we've ever put away.

            STEEL
        (starting the engine)
    So much for retirement!

They speed off.  To destiny!
```

Tabs do not "hint" formatting to Fountain. They are ignored and the lines are interpreted as if they weren't there. The exception
is in Action, where leading tabs and spaces are respected. This allows the writer to indent lines manually. See Action for more
on this.

# Notes

A Note is created by enclosing some text with double brackets. Notes can be inserted between lines, or in the middle of a line.

```
INT. TRAILER HOME - DAY

This is the home of THE BOY BAND, AKA DAN and JACK[[Or did we think of actual names for these guys?]].  They too are drinking beer, and counting the take from their last smash-and-grab.  Money, drugs, and ridiculous props are strewn about the table.

[[It was supposed to be Vietnamese, right?]]

JACK
(in Vietnamese, subtitled)
*Did you know Brick and Steel are retired?*
```

The empty lines around the Note on its own line would be removed in parsing.

Notes can contain carriage returns, but if you wish a note to contain an empty line, you must place two spaces there to "connect" the element into one.

```
His hand is an inch from the receiver when the phone RINGS.  Scott pauses for a moment, suspicious for some reason.[[This section needs work.
Either that, or I need coffee.
{two spaces}
Definitely coffee.]] He looks around.  Phone ringing.
```

Notes are designed to be compatible with the types of inserted annotation common in screenwriting software, e.g. Final Draft's Scriptnotes. To hide, or "comment out" sections of text, use the boneyard syntax.

# Boneyard (Comments)

If you want Fountain to ignore some text, wrap it with /* some text */. In this example, an entire scene is put in the boneyard. It will be ignored completely on formatted output.

```
COGNITO
Everyone's coming after you mate!  Scorpio, The Boy Band, Sparrow, Point Blank Sniper...

As he rattles off the long list, Brick and Steel share a look.  This is going to be BAD.

CUT TO:
/*
INT. GARAGE - DAY

BRICK and STEEL get into Mom's PORSCHE, Steel at the wheel.  They pause for a beat, the gravity of the situation catching up with them.

BRICK
This is everybody we've ever put away.

STEEL
(starting the engine)
So much for retirement!

They speed off.  To destiny!

CUT TO:
*/
EXT. PALATIAL MANSION - DAY

An EXTREMELY HANDSOME MAN drinks a beer.  Shirtless, unfortunately.
```

The boneyard is the exception to the rule of syntax not carrying across line breaks. Your /* ... */ pairs can span as much of your screenplay as you like.

# Sections
Sections are optional markers for managing the structure of a story. Some screenplay applications use these like nested folders in a navigation view.

Fountain's Sections resemble Markdown's ATX-style headers, but differ in that they do not appear in formatted output.

Create a Section by preceding a line with one or more pound-sign `#` characters:

```
CUT TO:

# This is a Section

INT. PALACE HALLWAY - NIGHT
```

You can nest Sections by adding more `#` characters.

```
# Act

## Sequence

### Scene

## Another Sequence

# Another Act
```

If you use a Markdown-header-aware app (such as MultiMarkdown Composer on Mac or Writing Kit on the iPad) to write in
Fountain, you'll be able to navigate your screenplay easily, just as you would a Markdown document.

Fountain's Sections are purely tools for the writer--they are ignored completely in formatted output. In this way they
are much like the structural tools offered in Movie Magic Screenwriter.

# Synopses
Synopses are optional blocks of text to describe a Section or scene.

Synopses are single lines prefixed by an equals sign `=`. They can be located anywhere within the screenplay.

```
# ACT I

= Set up the characters and the story.

EXT. BRICK'S PATIO - DAY

= This scene sets up Brick & Steel's new life as retirees. Warm sun, cold beer, and absolutely nothing to do.

A gorgeous day.  The sun is shining.  But BRICK BRADDOCK, retired police detective, is sitting quietly, contemplating -- something.
```

Like Sections, Synopses are purely optional tools for the writer's outlining and organizational process. They are
ignored in formatted output.

Not all screenwriting applications support synopses or sections, but most support some kind of invisible markers, such
as Notes. Such apps might choose to import Fountain's Synopses and Sections as notes. Some applications support synopses
of "scenes" as defined by a single Scene Heading. Such applications might import Scene Heading synopses correctly,
but need to import Section synopses as Notes.
