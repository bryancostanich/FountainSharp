using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace FountainSharp.Editor
{
	public partial class ViewController : NSViewController
	{
		public ViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var scriptResource = "FountainSharp.Editor.Scripts.SimpleTest.fountain";
			string script;

			using (var stream = Resource.FromPath(scriptResource))
			{
				if (stream != null)
				{
					using (var reader = new System.IO.StreamReader(stream))
					{
						script = reader.ReadToEnd();

						MainTextView.Value = script;
						UpdateHtml();
					}
				}
				else
				{
					Console.WriteLine("Couldn't load " + scriptResource);
				}

			}

			MainTextView.TextDidChange += (object sender, EventArgs e) =>
			{
				UpdateHtml();
			};
		}

		protected void UpdateHtml()
		{
			Task.Run(() =>
			{
				InvokeOnMainThread(() =>
			   {
				   MainWebView.MainFrame.LoadHtmlString(HtmlFormatter.TransformHtml(MainTextView.Value), NSUrl.FromString(""));
			   });
			});
		}


		public override NSObject RepresentedObject
		{
			get
			{
				return base.RepresentedObject;
			}
			set
			{
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
