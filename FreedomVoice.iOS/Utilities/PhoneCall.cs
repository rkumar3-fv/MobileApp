using System.Text.RegularExpressions;
using Foundation;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public static class PhoneCall
    {
        public static async void CreateCallReservation(string systemNumber, string presentationNumber, string destinationNumberFormatted, UIViewController viewController)
        {
            var destinationNumber = FormatPhoneNumber(destinationNumberFormatted);
            var expectedCallerIdNumber = FormatPhoneNumber(UserDefault.AccountPhoneNumber);

            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            if (!PhoneCapability.IsSimCardInstalled())
            {
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.NoSimCardInstalled);
                return;
            }

            if (string.IsNullOrEmpty(expectedCallerIdNumber))
            {
                var alertController = UIAlertController.Create(null, "To make calls with this app, please enter your device's phone number.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString))));
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                viewController.PresentViewController(alertController, true, null);

                return;
            }

            var callReservationViewModel = new CallReservationViewModel(systemNumber, expectedCallerIdNumber, presentationNumber, destinationNumber, viewController);
            await callReservationViewModel.CreateCallReservationAsync();

            if (callReservationViewModel.IsErrorResponseReceived)
            {
                var alertController = UIAlertController.Create(null, "Call Failed", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                viewController.PresentViewController(alertController, true, null);

                return;
            }

            var switchboardNumber = callReservationViewModel.Reservation.SwitchboardNumber;
            switchboardNumber = switchboardNumber.StartsWith("+1") ? switchboardNumber : string.Concat("+1", switchboardNumber);

            var phoneNumber = NSUrl.FromString($"tel:{switchboardNumber}");
            if (!UIApplication.SharedApplication.OpenUrl(phoneNumber))
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.CallsUnsuported);
        }

        private static string FormatPhoneNumber(string unformattedPhoneNumber)
        {
            if (string.IsNullOrEmpty(unformattedPhoneNumber))
                return string.Empty;

            var phoneNumber = Regex.Replace(unformattedPhoneNumber.Replace(" ", ""), @"(?<!^)\+|[^\d+]+", "");

            if (phoneNumber.Length == 11 && phoneNumber.StartsWith("1"))
                return string.Concat("+", phoneNumber);

            return phoneNumber.Length == 10 ? string.Concat("+1", phoneNumber) : phoneNumber;
        }
    }
}