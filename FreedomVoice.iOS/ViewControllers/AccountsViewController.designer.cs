// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//

using System.CodeDom.Compiler;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	[Register ("AccountsViewController")]
	partial class AccountsViewController
	{
        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UITableView AccountsTableView { get; set; }

        void ReleaseDesignerOutlets ()
		{
            if (AccountsTableView != null)
            {
                AccountsTableView.Dispose();
                AccountsTableView = null;
            }
        }
	}
}
