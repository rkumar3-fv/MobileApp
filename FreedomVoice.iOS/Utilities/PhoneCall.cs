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
            var destinationNumber = FormatPhoneNumber(destinationNumberFormatted);
            var expectedCallerIdNumber = FormatPhoneNumber(UserDefault.AccountPhoneNumber);

            if (!PhoneCapability.IsSimCardInstalled)
            {
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.NoSimCardInstalled);
                return false;
            }

            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.NetworkUnreachable);
                return false;
            }

            if (IsEmergencyNumber(destinationNumber))
            {
                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString($"tel:{destinationNumber}"));
                return false;
            }

            if (destinationNumber.Length < 5)
            {
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.IncorrectNumber);
                return true;
            }

            //if (string.IsNullOrEmpty(expectedCallerIdNumber) || expectedCallerIdNumber.Length != 10)
            //TODO: Only for test purposes, replace later
            if (string.IsNullOrEmpty(expectedCallerIdNumber) || expectedCallerIdNumber.Length != 10 && expectedCallerIdNumber.Length != 12)
            {
                var alertController = UIAlertController.Create(null, "To make calls with this app, please enter your device's phone number.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString))));
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                viewController.PresentViewController(alertController, true, null);

                return true;
            }

            var callReservationViewModel = new CallReservationViewModel(systemNumber, expectedCallerIdNumber, presentationNumber, destinationNumber, viewController);
            await callReservationViewModel.CreateCallReservationAsync();

            if (callReservationViewModel.IsErrorResponseReceived)
            {
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.CallFailed);
                return true;
            }

            var switchboardNumber = callReservationViewModel.Reservation.SwitchboardNumber;
            switchboardNumber = switchboardNumber.StartsWith("+1") ? switchboardNumber : string.Concat("+1", switchboardNumber);

            var phoneNumber = NSUrl.FromString($"tel:{switchboardNumber}");
            if (!UIApplication.SharedApplication.OpenUrl(phoneNumber))
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.CallsUnsuported);

            return true;
        }

        private static bool IsEmergencyNumber(string number) => number == "911"; 

        private static string FormatPhoneNumber(string unformattedPhoneNumber)
        {
            return string.IsNullOrEmpty(unformattedPhoneNumber) ? string.Empty : DataFormatUtils.NormalizePhone(unformattedPhoneNumber);
        }
    }
}