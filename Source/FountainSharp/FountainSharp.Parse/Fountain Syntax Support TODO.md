

#[x] Scene Heading

A Scene Heading is any line that has a blank line following it, and either begins with INT or EXT or similar (full list below). 
A Scene Heading always has at least one blank line preceding it.

Power user: You can "force" a Scene Heading by starting the line with a single period.

List of known prefixes:
INT
EXT
EST
INT./EXT
INT/EXT
I/E

[] Action

# [] Character
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
Power User: You can force a Character element by preceding it with the "at" symbol @.

The ability to force a Character element is helpful for names that require lower-case letters, and for non-Roman languages.

```
@McCLANE
Yippie ki-yay! I got my lower-case C back!
```
Fountain will remove the @ and interpret McCLANE as Character, preserving its mixed case.

# [] Dialogue

# [] Parenthetical

# [] Dual Dialogue

# [X] Lyrics

You create a Lyric by starting with a line with a tilde ~.

~Willy Wonka! Willy Wonka! The amazing chocolatier!
~Willy Wonka! Willy Wonka! Everybody give a cheer!
Fountain will remove the '~' and leave it up to the app to style the Lyric appropriately. Lyrics are always forced. There is no "automatic" way to get them.

# [] Transition

# [] Centered Text

# [X] Emphasis - Supported, but needs some testing. i doubt nesting works correctly.

# [] Page Breaks

# [] Punctuation

# [] Line Breaks

# [] Indenting

# [~] Notes - some support, but not the double space thing

# [] Boneyard (Comments) /* stuff */

# [X] Sections

# [] Synopses

# [] Title Page
