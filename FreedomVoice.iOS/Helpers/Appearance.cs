using System;
using CoreGraphics;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.Helpers
{
    public static class Appearance
    {
        public static UIBarButtonItem GetBackButton(UINavigationController controller, string title = "Back")
        {
            return new UIBarButtonItem(title, UIBarButtonItemStyle.Plain, (s, args) => { controller.PopViewController(true); });
        }

        public static UIBarButtonItem[] GetBackButtonWithArrow(UIViewController controller, bool isModalController, string title = "Back")
        {
            var negativeSpacer = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = -8 };

            var buttonImage = UIImage.FromFile("back.png");
            var highlightedButtonImage = UIImage.FromFile("back_transparent.png");
            var button = new UIButton(UIButtonType.Custom)
            {
                Frame = new CGRect(0, 0, 95, 33),
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left,
                TitleEdgeInsets = new UIEdgeInsets(0, 5, 0, 0)
            };
            button.SetImage(buttonImage, UIControlState.Normal);
            button.SetImage(highlightedButtonImage, UIControlState.Highlighted); 
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.FromRGBA(255, 255, 255, 51), UIControlState.Highlighted);
            if (isModalController)
                button.TouchUpInside += (s, args) => { Theme.TransitionController(controller); };
            else
                button.TouchUpInside += (s, args) => { (controller as UINavigationController)?.PopViewController(true); };

            return new[] { negativeSpacer, new UIBarButtonItem(button) };
        }

        public static UIBarButtonItem GetLogoutBarButton(UIViewController controller)
        {
            return new UIBarButtonItem(UIImage.FromFile("logout.png"), UIBarButtonItemStyle.Plain, (s, args) => {
                var alertController = UIAlertController.Create("Confirm logout?", "Your Recents list will be cleared.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, null));
                alertController.AddAction(UIAlertAction.Create("Log Out", UIAlertActionStyle.Cancel, a => { (UIApplication.SharedApplication.Delegate as AppDelegate)?.GoToLoginScreen(); }));
                controller.PresentViewController(alertController, true, null);
            });
        }

        public static UIBarButtonItem GetSkipBarButton(EventHandler handler)
        {
            return new UIBarButtonItem("Skip", UIBarButtonItemStyle.Plain, handler);
        }

        public static UIImageView GetMessageImageView(MessageType messageType, bool unread, bool active)
        {
            var messageImageView = new UIImageView(new CGRect(15, 15, 25, 25))
            {
                Image = GetMessageImage(messageType, unread, active)
            };

            return messageImageView;
        }

        public static UIImage GetMessageImage(MessageType messageType, bool unread, bool active)
        {
            var imagePostfix = active ? "_active" : unread ? "_unread" : string.Empty;

            switch (messageType)
            {
                case MessageType.Recording:
                    return UIImage.FromFile($"callrecord{imagePostfix}.png");
                case MessageType.Fax:
                    return UIImage.FromFile($"fax{imagePostfix}.png");
                case MessageType.Voicemail:
                    return UIImage.FromFile($"voicemail{imagePostfix}.png");
                default:
                    return null;
            }
        }

        public static UIBarButtonItem[] GetBackButtonWithArrow(UIViewController controller, Action onTouchInside = null)
        {
            var negativeSpacer = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = -8 };

            var buttonImage = UIImage.FromFile("back.png");
            var highlightedButtonImage = UIImage.FromFile("back_transparent.png");
            var button = new UIButton(UIButtonType.Custom)
            {
                Frame = new CGRect(0, 0, 95, 33),
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left,
                TitleEdgeInsets = new UIEdgeInsets(0, 5, 0, 0)
            };
            button.SetImage(buttonImage, UIControlState.Normal);
            button.SetImage(highlightedButtonImage, UIControlState.Highlighted);
            button.SetTitle("Back", UIControlState.Normal);
            button.SetTitleColor(UIColor.FromRGBA(255, 255, 255, 51), UIControlState.Highlighted);            
            button.TouchUpInside += (s, args) => { onTouchInside.Invoke(); };
            

            return new[] { negativeSpacer, new UIBarButtonItem(button) };
        }
    }
}