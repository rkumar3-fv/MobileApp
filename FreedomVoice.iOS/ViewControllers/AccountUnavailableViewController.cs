using System;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class AccountUnavailableViewController : UIViewController
	{
        public Account SelectedAccount { private get; set; }

        public AccountUnavailableViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = SelectedAccount.FormattedPhoneNumber;

            NavigationItem.SetLeftBarButtonItem(Appearance.GetBackBarButton(NavigationController), true);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);

            CallCustomerCareButton.Layer.CornerRadius = 5;
            CallCustomerCareButton.ClipsToBounds = true;
        }

	    partial void CallCustomerCare_TouchUpInside(UIButton sender)
	    {
            var url = new NSUrl("tel:" + SelectedAccount.PhoneNumber);

            if (!UIApplication.SharedApplication.OpenUrl(url))
            {
                var alert = new UIAlertView("Not supported", "Your device does not appear to support making cellular voice calls.", null, "OK", null);
                alert.Show();
            };
        }
	}
}