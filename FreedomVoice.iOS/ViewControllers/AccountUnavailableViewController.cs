using Foundation;
using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;

namespace FreedomVoice.iOS
{
	partial class AccountUnavailableViewController : UIViewController
	{
        public Account SelectedAccount { private get; set; }

        public AccountUnavailableViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);

            CallCustomerCareButton.Layer.CornerRadius = 5;
            CallCustomerCareButton.ClipsToBounds = true;
        }

	    partial void CallCustomerCare_TouchUpInside(UIButton sender)
	    {
            var url = new NSUrl("tel:" + SelectedAccount.PhoneNumber);

            if (!UIApplication.SharedApplication.OpenUrl(url))
            {
                var alert = new UIAlertView("Not supported", "Scheme 'tel:' is not supported on this device", null, "OK", null);
                alert.Show();
            };
        }
	}
}