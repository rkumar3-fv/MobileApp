using System;
using CoreGraphics;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using UIKit;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Appearance
    {
        public static UIBarButtonItem GetPlainBarButton(string title, EventHandler handler)
        {
            return new UIBarButtonItem(title, UIBarButtonItemStyle.Plain, handler);
        }

        public static UIBarButtonItem[] GetBarButtonWithArrow(EventHandler handler, string title, bool wideLabel = false)
        {
            var negativeSpacer = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = -8 };

            var buttonImage = UIImage.FromFile("back.png");
            var highlightedButtonImage = UIImage.FromFile("back_transparent.png");
            var button = new UIButton(UIButtonType.Custom)
            {
                Frame = new CGRect(0, 0, Theme.BackButtonWidth(wideLabel), 33),
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left,
                TitleEdgeInsets = new UIEdgeInsets(0, 5, 0, 0)
            };
            button.SetImage(buttonImage, UIControlState.Normal);
            button.SetImage(highlightedButtonImage, UIControlState.Highlighted); 
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.FromRGBA(255, 255, 255, 51), UIControlState.Highlighted);
            button.TouchUpInside += handler;

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

        public static void ShowOkAlertWithMessage(UIViewController viewController, AlertMessageType alertMessageType)
        {
            var alertMessageText = string.Empty;

            switch (alertMessageType)
            {
                case AlertMessageType.CallsUnsuported:
                    alertMessageText = "Your device does not appear to support making cellular voice calls";
                    break;
                case AlertMessageType.NetworkUnreachable:
                    alertMessageText = "No Internet Connection";
                    break;
                case AlertMessageType.NoSimCardInstalled:
                    alertMessageText = "No SIM Card Installed";
                    break;
                case AlertMessageType.EmptyFileDownload:
                    alertMessageText = "Error loading file";
                    break;
            }

            var alertController = UIAlertController.Create(null, alertMessageText, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
            viewController.PresentViewController(alertController, true, null);
        }

        public static UIImageView GetMessageImageView(int cellHeight)
        {
            return new UIImageView(new CGRect(15, (cellHeight - 25)/2, 25, 25));
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

        public static UIImage GetImageFromColor(UIColor color, CGSize size)
        {
            var imageFrame = new CGRect(0, 0, size.Width, size.Height);
            UIGraphics.BeginImageContext(imageFrame.Size);

            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(imageFrame);

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }

        public static string GetFormattedMessageLength(int length, bool faxMessageType)
        {
            return faxMessageType ? DataFormatUtils.PagesToFormattedString(length) : DataFormatUtils.ToDuration(length);
        }

        public enum AlertMessageType
        {
            NetworkUnreachable,
            CallsUnsuported,
            NoSimCardInstalled,
            EmptyFileDownload
        }
    }
}