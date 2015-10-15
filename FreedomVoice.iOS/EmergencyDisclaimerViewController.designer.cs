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

namespace FreedomVoice.iOS
{
	[Register ("EmergencyDisclaimerViewController")]
	partial class EmergencyDisclaimerViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel EmergencyDisclaimerLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton UnderstandButton { get; set; }

		[Action ("UnderstandButton_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void UnderstandButton_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (EmergencyDisclaimerLabel != null) {
				EmergencyDisclaimerLabel.Dispose ();
				EmergencyDisclaimerLabel = null;
			}
			if (UnderstandButton != null) {
				UnderstandButton.Dispose ();
				UnderstandButton = null;
			}
		}
	}
}
