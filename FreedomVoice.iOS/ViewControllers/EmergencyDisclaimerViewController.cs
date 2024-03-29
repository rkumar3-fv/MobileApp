using System;
using CoreGraphics;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class EmergencyDisclaimerViewController : BaseViewController
	{
        protected override string PageName => "Emergency Disclaimer Screen";

        public Account SelectedAccount { private get; set; }
        public UIViewController ParentController { private get; set; }

        private UIImageView _emergencyDisclaimerImage;
        private UILabel _emergencyDisclaimerLabel;
	    private UIButton _understandButton;

        public EmergencyDisclaimerViewController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
        {
	        InitializeEmergencyDisclaimerImage();
            InitializeEmergencyDisclaimerLabel();
	        InitializeUnderstandButton();

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);

            base.ViewDidLoad();
        }

        private async void OnUnderstandButtonTouchUpInside(object sender, EventArgs args)
	    {
	        UserDefault.IsLaunchedBefore = true;

            _understandButton.Hidden = true;

            var navigationBarHeight = Theme.StatusBarHeight + NavigationController.NavigationBarHeight();

            AppDelegate.ActivityIndicator.SetActivityIndicatorCenter(new CGPoint(_understandButton.Center.X, _understandButton.Center.Y + navigationBarHeight));

            var mainTabBarController = await AppDelegate.GetMainTabBarController(SelectedAccount, ParentController, true);
            var controller = (UINavigationController)ParentController;
            if (mainTabBarController == null)
            {
                _understandButton.Hidden = false;
                return;
            }

            if (controller != null)
            {
                controller.PushViewController(mainTabBarController, false);
                Theme.TransitionController(controller);
            }
            else
            {
                var navigationController = new UINavigationController(mainTabBarController);
                Theme.TransitionController(navigationController);
            }
            ServiceContainer.Resolve<IAppNavigator>()?.UpdateMainTabBarController(mainTabBarController);
	    }

        #region Controls Initialization

        private void InitializeEmergencyDisclaimerImage()
        {
            _emergencyDisclaimerImage = new UIImageView(UIImage.FromFile("warning.png"));
            _emergencyDisclaimerImage.Frame = new CGRect(0, 102, _emergencyDisclaimerImage.Image.CGImage.Width / 2, _emergencyDisclaimerImage.Image.CGImage.Height / 2);
            _emergencyDisclaimerImage.Center = new CGPoint(View.Center.X, _emergencyDisclaimerImage.Center.Y);

            View.AddSubview(_emergencyDisclaimerImage);
        }

        private void InitializeEmergencyDisclaimerLabel()
        {
            var labelFrame = new CGRect(0, _emergencyDisclaimerImage.Frame.Y + _emergencyDisclaimerImage.Frame.Height + 25, Theme.ScreenBounds.Width, 40);
            _emergencyDisclaimerLabel = new UILabel(labelFrame)
            {
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                Text = $"Emergency 911 services are provided{Environment.NewLine}by your cellular phone carrier.",
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Lines = 2
            };
            _emergencyDisclaimerLabel.Center = new CGPoint(View.Center.X, _emergencyDisclaimerLabel.Center.Y);

            View.Add(_emergencyDisclaimerLabel);
        }

        private void InitializeUnderstandButton()
        {
            _understandButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Theme.ButtonColor,
                Frame = new CGRect(15, _emergencyDisclaimerLabel.Frame.Y + _emergencyDisclaimerLabel.Frame.Height + 40, Theme.ScreenBounds.Width - 30, 44),
                Font = UIFont.SystemFontOfSize(21, UIFontWeight.Medium),
                ClipsToBounds = true
            };
            _understandButton.Layer.CornerRadius = 5;
            _understandButton.Center = new CGPoint(View.Center.X, _understandButton.Center.Y);
            _understandButton.SetTitle("I Understand", UIControlState.Normal);
            _understandButton.SetTitleColor(Theme.WhiteColor, UIControlState.Normal);
            _understandButton.TouchUpInside += OnUnderstandButtonTouchUpInside;

            View.AddSubview(_understandButton);
        }

        #endregion
    }
}