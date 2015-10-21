using System;
using System.Collections.Generic;
using System.Linq;

#if __UNIFIED__
using ObjCRuntime;
using Foundation;
using UIKit;
#else
using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using CGRect = global::System.Drawing.RectangleF;
using CGSize = global::System.Drawing.SizeF;
using CGPoint = global::System.Drawing.PointF;
using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#endif

using MRoundedButton;

namespace MRoundedButtonSample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		ViewController controller;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow(UIScreen.MainScreen.Bounds);
			window.BackgroundColor = UIColor.White;

			NSDictionary appearanceProxy1 = GetNSDictionary (new Dictionary<string, object> {
				{ RoundedButtonAppearanceKeys.CornerRadius , 40 },
				{ RoundedButtonAppearanceKeys.BorderWidth  , 2 },
				{ RoundedButtonAppearanceKeys.BorderColor  , UIColor.Clear },
				{ RoundedButtonAppearanceKeys.ContentColor , UIColor.Black },
				{ RoundedButtonAppearanceKeys.ContentAnimateToColor , UIColor.White },
				{ RoundedButtonAppearanceKeys.ForegroundColor , UIColor.White },
				{ RoundedButtonAppearanceKeys.ForegroundAnimateToColor , UIColor.Clear }
			});

			NSDictionary appearanceProxy2 = GetNSDictionary (new Dictionary<string, object> { 
				{ RoundedButtonAppearanceKeys.CornerRadius , 25 },
				{ RoundedButtonAppearanceKeys.BorderWidth  ,1.5 },
				{ RoundedButtonAppearanceKeys.RestoreSelectedState ,false },
				{ RoundedButtonAppearanceKeys.BorderColor , UIColor.Black.ColorWithAlpha (0.5F) },
				{ RoundedButtonAppearanceKeys.BorderAnimateToColor , UIColor.White },
				{ RoundedButtonAppearanceKeys.ContentColor , UIColor.Black.ColorWithAlpha (0.5F) },
				{ RoundedButtonAppearanceKeys.ContentAnimateToColor , UIColor.White },
				{ RoundedButtonAppearanceKeys.ForegroundColor , UIColor.White.ColorWithAlpha (0.5F) }
			});

			NSDictionary appearanceProxy3 = GetNSDictionary (new Dictionary<string, object> { 
				{ RoundedButtonAppearanceKeys.CornerRadius , 40 },
				{ RoundedButtonAppearanceKeys.BorderWidth  ,2 },
				{ RoundedButtonAppearanceKeys.RestoreSelectedState ,false },
				{ RoundedButtonAppearanceKeys.BorderColor  , UIColor.Clear },
				{ RoundedButtonAppearanceKeys.BorderAnimateToColor , UIColor.White },
				{ RoundedButtonAppearanceKeys.ContentColor , UIColor.White },
				{ RoundedButtonAppearanceKeys.ContentAnimateToColor , UIColor.Black },
				{ RoundedButtonAppearanceKeys.ForegroundColor , UIColor.Black.ColorWithAlpha (0.5F) },
				{ RoundedButtonAppearanceKeys.ForegroundAnimateToColor , UIColor.White }
			});

			RoundedButtonAppearanceManager.RegisterAppearanceProxy (appearanceProxy1, "1");
			RoundedButtonAppearanceManager.RegisterAppearanceProxy (appearanceProxy2, "2");
			RoundedButtonAppearanceManager.RegisterAppearanceProxy (appearanceProxy3, "3");

			controller = new ViewController ();
			window.RootViewController = controller;
			window.MakeKeyAndVisible();

			return true;
		}

		static NSDictionary GetNSDictionary (Dictionary<string,object> source)
		{
			return NSDictionary.FromObjectsAndKeys (
				source.Values.ToArray (), 
				source.Keys.ToArray ());
		}
	}
}

