// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	[Register ("ExtensionsViewController")]
	partial class ExtensionsViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView ExtensionsTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (ExtensionsTableView != null) {
				ExtensionsTableView.Dispose ();
				ExtensionsTableView = null;
			}
		}
	}
}
