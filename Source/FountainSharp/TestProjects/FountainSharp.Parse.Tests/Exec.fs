namespace FountainSharp.Parse.Tests

open System

open FountainSharp.Parse
open FountainSharp

type Exec (args) = 
  let waitForKey () =
    printf "\nPress any key to continue ..."
    System.Console.ReadKey() |> ignore
 
  let exit x = 
    //waitForKey()
    x

  member x.Run () =
    printf "test."

    let string1 = "*some italic text*"
    let string2 = "**some bold text**"
    let string3 = "***some bold italic text***"
    let string4 = "_some underlined text_"
    let string5 = "some text that's not emphasized"
    let string6 = "pretty sure this will fail *some italic text **with some bold** in the middle*"
    let string7 = @"EXT. BRICK'S PATIO - DAY

    = Here is a synopses of this fascinating scene.

    A gorgeous day.  The sun is shining.  But BRICK BRADDOCK, retired police detective, is sitting quietly, contemplating -- something.

    The SCREEN DOOR slides open and DICK STEEL, his former partner and fellow retiree, emerges with two cold beers.

    STEEL
    Does a bear crap in the woods?

    Steel sits.  They laugh at the dumb joke.

    [[Some notes about the scene]]

    STEEL
    (beer raised)
    To retirement.

    BRICK
    To retirement.

    @McAVOY
    Oy, vay.

    They drink *long* and _well_ from the beers.

    .BINOCULARS A FORCED SCENE HEADING - LATER

    # This is a section with level 1
    ## This is a section with level 2

    And then there's a long beat.  
    Longer than is funny.  
    Long enough to be depressing.

    ~Some Lyrics
    ~Some more lyrics

    Here comes a page break!

    ===

    > ACT II <

    "

    let doc = FountainSharp.Parse.Fountain.Parse string7


    exit 1