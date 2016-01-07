namespace FountainSharp.Editor
open System
open Foundation
open AppKit

[<Register ("MainWindowController")>]
type MainWindowController =

    inherit NSWindowController

    new () = { inherit NSWindowController ("MainWindow") }
    new (handle : IntPtr) = { inherit NSWindowController (handle) }

    [<Export ("initWithCoder:")>]
    new (coder : NSCoder) = { inherit NSWindowController (coder) }

    override x.AwakeFromNib () =
        base.AwakeFromNib ()

    member x.Window with get () = base.Window :?> MainWindow
