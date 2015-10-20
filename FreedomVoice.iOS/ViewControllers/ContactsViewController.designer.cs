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
	[Register ("ContactsViewController")]
	partial class ContactsViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView ContactsTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (ContactsTableView != null) {
				ContactsTableView.Dispose ();
				ContactsTableView = null;
			}
		}
	}
}
