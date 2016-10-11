// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace FountainSharp.Editor
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSTextView MainTextView { get; set; }

		[Outlet]
		WebKit.WebView MainWebView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MainWebView != null) {
				MainWebView.Dispose ();
				MainWebView = null;
			}

			if (MainTextView != null) {
				MainTextView.Dispose ();
				MainTextView = null;
			}
		}
	}
}
