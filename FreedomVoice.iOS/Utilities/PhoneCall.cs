using System;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.iOS.Core;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public static class PhoneCall
    {
        private static readonly ILogger _logger = ServiceContainer.Resolve<ILogger>();
        public static async Task<bool> CreateCallReservation(string systemNumber, string presentationNumber, string destinationNumberFormatted, UIViewController viewController, Action onSuccessAction)
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

            onSuccessAction?.Invoke();

            if (string.IsNullOrEmpty(expectedCallerIdNumber))
            {
                viewController.View.UserInteractionEnabled = true;

                var alertController = UIAlertController.Create(null, "To make calls with this app, please enter your device's phone number.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString))));
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                viewController.PresentViewController(alertController, true, null);

                return true;
            }

            var callReservationViewModel = new CallReservationViewModel(systemNumber, expectedCallerIdNumber, presentationNumber, destinationNumber);
            
            try
            {
                await callReservationViewModel.CreateCallReservationAsync();
            }
            catch (Exception ex)
            {
                _logger.Debug(nameof(PhoneCall), nameof(CreateCallReservation), ex.ToString());
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.CallFailed);
                viewController.View.UserInteractionEnabled = true;
                return false;
            }
            
            if (callReservationViewModel.IsErrorResponseReceived)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.CallFailed);
                viewController.View.UserInteractionEnabled = true;
                return true;
            }

            var switchboardNumber = callReservationViewModel.Reservation.SwitchboardNumber;

            var phoneNumber = NSUrl.FromString($"tel:{switchboardNumber}");
            if (!UIApplication.SharedApplication.OpenUrl(phoneNumber))
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.CallsUnsuported);
                viewController.View.UserInteractionEnabled = true;
            }

            return true;
        }

        private static bool IsEmergencyNumber(string number) => number == "911"; 

        private static string FormatPhoneNumber(string unformattedPhoneNumber)
        {
            return string.IsNullOrEmpty(unformattedPhoneNumber) ? string.Empty : ServiceContainer.Resolve<IPhoneFormatter>().NormalizeNational(unformattedPhoneNumber);
        }
    }
}