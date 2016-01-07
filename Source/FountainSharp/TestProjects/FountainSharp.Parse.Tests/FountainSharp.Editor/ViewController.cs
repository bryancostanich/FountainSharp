using System;

using AppKit;
using Foundation;

using FountainSharp.Parse;

namespace FountainSharp.Editor
{
	public partial class ViewController : NSViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();


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

			this.MainTextView.TextDidChange += async (object sender, EventArgs e) => {
				this.UpdateHtml();
			};


		}

		protected async void UpdateHtml ()
		{
			await System.Threading.Tasks.Task.Run (() => {
				
				InvokeOnMainThread (()=>{
					this.MainWebView.MainFrame.LoadHtmlString (FountainSharp.Parse.Fountain.TransformHtml (this.MainTextView.Value), NSUrl.FromString (""));
				});
			});
		}


		public override NSObject RepresentedObject {
			get {
				return base.RepresentedObject;
			}
			set {
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
