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
	[Register ("AccountUnavailableViewController")]
	partial class AccountUnavailableViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton CallCustomerCareButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel CustomerCareLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel NotAvailableLabel { get; set; }

		[Action ("CallCustomerCare_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void CallCustomerCare_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (CallCustomerCareButton != null) {
				CallCustomerCareButton.Dispose ();
				CallCustomerCareButton = null;
			}
			if (CustomerCareLabel != null) {
				CustomerCareLabel.Dispose ();
				CustomerCareLabel = null;
			}
			if (NotAvailableLabel != null) {
				NotAvailableLabel.Dispose ();
				NotAvailableLabel = null;
			}
		}
	}
}
