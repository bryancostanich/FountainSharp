namespace FountainSharp.Editor

open System
open Foundation
open AppKit

[<Register ("MainWindow")>]
type MainWindow =
    inherit NSWindow

    new () = { inherit NSWindow () }
    new (handle : IntPtr) = { inherit NSWindow (handle) }

    [<Export ("initWithCoder:")>]
    new (coder : NSCoder) = { inherit NSWindow (coder) }

    override x.AwakeFromNib () =
        base.AwakeFromNib ()
