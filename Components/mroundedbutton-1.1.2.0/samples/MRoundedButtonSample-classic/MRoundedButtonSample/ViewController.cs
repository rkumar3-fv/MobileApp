using System;

using MRoundedButton;

#if __UNIFIED__
using ObjCRuntime;
using Foundation;
using UIKit;
using CoreGraphics;
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

namespace MRoundedButtonSample
{
	public class ViewController : UIViewController
	{
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			UIImageView imageView = new UIImageView (View.Bounds);
			imageView.ContentMode = UIViewContentMode.ScaleAspectFill;
			imageView.Image = UIImage.FromBundle ("pic");
			View.AddSubview (imageView);

			nfloat backgroundViewHeight = (float)Math.Ceiling (UIScreen.MainScreen.Bounds.Height / 3.0F);
			nfloat backgroundViewWidth = UIScreen.MainScreen.Bounds.Width;

			UIColor[] foregroundColorArray = new [] {
				UIColor.White,
				UIColor.White.ColorWithAlpha (0.5F),
				UIColor.Black.ColorWithAlpha (0.5F)
			};
			RoundedButtonStyle[] buttonStyleArray = new [] {
				RoundedButtonStyle.Subtitle,
				RoundedButtonStyle.CentralImage,
				RoundedButtonStyle.CentralImage
			};

			for (int i = 0; i < 3; i++) {
				CGRect backgroundRect = new CGRect (
                    0,
                    backgroundViewHeight * i,
                    backgroundViewWidth,
                    backgroundViewHeight
				);
				HollowBackgroundView backgroundView = new HollowBackgroundView (backgroundRect);
				backgroundView.ForegroundColor = foregroundColorArray [i];
				View.AddSubview (backgroundView);

				nfloat buttonSize = i == 1 ? 50F : 80F;
				CGRect buttonRect = new CGRect (
                    (backgroundViewWidth - buttonSize) / 2.0F,
                    (backgroundViewHeight - buttonSize) / 2.0F,
                    buttonSize,
                    buttonSize
				);
				RoundedButton button = new RoundedButton (
                   buttonRect,
                   buttonStyleArray [i],
                   (i + 1).ToString ()
               );
				button.BackgroundColor = UIColor.Clear;

				if (i == 0) {
					button.TextLabel.Text = "7";
					button.TextLabel.Font = UIFont.BoldSystemFontOfSize (50);
					button.DetailTextLabel.Text = "P Q R S";
					button.DetailTextLabel.Font = UIFont.SystemFontOfSize (10);
				} else {
					button.ImageView.Image = UIImage.FromBundle ("twitter");
				}
				backgroundView.AddSubview (button);
			}
		}
	}
}

