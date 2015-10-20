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
	[Register ("KeypadController")]
	partial class KeypadController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIPickerView CallerList { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		RoundedButton KeypadButton1 { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (CallerList != null) {
				CallerList.Dispose ();
				CallerList = null;
			}
			if (KeypadButton1 != null) {
				KeypadButton1.Dispose ();
				KeypadButton1 = null;
			}
		}
	}
}
