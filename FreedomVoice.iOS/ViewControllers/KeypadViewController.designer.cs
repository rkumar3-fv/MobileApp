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
	[Register ("KeypadViewController")]
	partial class KeypadViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton KeypadDial { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel PhoneLabel { get; set; }

		[Action ("ClearPhone_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void ClearPhone_TouchUpInside (UIButton sender);

		[Action ("KeypadDial_TouchUpInside:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void KeypadDial_TouchUpInside (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (KeypadDial != null) {
				KeypadDial.Dispose ();
				KeypadDial = null;
			}
			if (PhoneLabel != null) {
				PhoneLabel.Dispose ();
				PhoneLabel = null;
			}
		}
	}
}
