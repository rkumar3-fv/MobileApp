using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public static class PhoneCall
    {
        public static async Task<bool> CreateCallReservation(string systemNumber, string presentationNumber, string destinationNumberFormatted, UIViewController viewController)
        {
            viewController.View.UserInteractionEnabled = false;

            var destinationNumber = FormatPhoneNumber(destinationNumberFormatted);
            var expectedCallerIdNumber = FormatPhoneNumber(UserDefault.AccountPhoneNumber);

            if (!PhoneCapability.IsSimCardInstalled)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NoSimCardInstalled);
                viewController.View.UserInteractionEnabled = true;
                return false;
            }

            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                viewController.View.UserInteractionEnabled = true;
                return false;
            }

            if (IsEmergencyNumber(destinationNumber))
            {
                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"tel:{destinationNumber}"));
                viewController.View.UserInteractionEnabled = true;
                return false;
            }

            if (destinationNumber.Length < 5)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.IncorrectNumber);
                viewController.View.UserInteractionEnabled = true;
                return false;
            }

            //if (string.IsNullOrEmpty(expectedCallerIdNumber) || expectedCallerIdNumber.Length != 10)
            //TODO: Only for test purposes, replace later
            if (string.IsNullOrEmpty(expectedCallerIdNumber) || expectedCallerIdNumber.Length != 10 && expectedCallerIdNumber.Length != 12)
            {
                viewController.View.UserInteractionEnabled = true;

                var alertController = UIAlertController.Create(null, "To make calls with this app, please enter your device's phone number.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString))));
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                viewController.PresentViewController(alertController, true, null);

                return true;
            }

            var callReservationViewModel = new CallReservationViewModel(systemNumber, expectedCallerIdNumber, presentationNumber, destinationNumber);

            var watcher = Stopwatch.StartNew();
            await callReservationViewModel.CreateCallReservationAsync();
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, "CreateCallReservation", "", watcher.ElapsedMilliseconds);

            if (callReservationViewModel.IsErrorResponseReceived)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.CallFailed);
                viewController.View.UserInteractionEnabled = true;
                return true;
            }

            var switchboardNumber = callReservationViewModel.Reservation.SwitchboardNumber;
            switchboardNumber = switchboardNumber.StartsWith("+1") ? switchboardNumber : string.Concat("+1", switchboardNumber);

            var phoneNumber = NSUrl.FromString($"tel:{switchboardNumber}");
            if (!UIApplication.SharedApplication.OpenUrl(phoneNumber))
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.CallsUnsuported);

            viewController.View.UserInteractionEnabled = true;
            return true;
        }

        private static bool IsEmergencyNumber(string number) => number == "911"; 

        private static string FormatPhoneNumber(string unformattedPhoneNumber)
        {
            return string.IsNullOrEmpty(unformattedPhoneNumber) ? string.Empty : DataFormatUtils.NormalizePhone(unformattedPhoneNumber);
        }
    }
}