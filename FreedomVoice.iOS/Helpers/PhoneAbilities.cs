using CoreFoundation;
using Foundation;
using System;
using System.Net;
using System.Text.RegularExpressions;
using SystemConfiguration;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.Helpers
{
    public enum NetworkStatus
    {
        NotReachable,
        ReachableViaCarrierDataNetwork,
        ReachableViaWiFiNetwork
    }

    public static class PhoneCapability
    {
        public const string REMOTE_HOST_STATUS = "remoteHostStatus";
        public const string INTERNET_STATUS = "internetStatus";
        public const string LOCAL_WIFI_STATUS = "localWifiStatus";

        public static event EventHandler ReachabilityChanged;

        private static void OnChange(NetworkReachabilityFlags flags)
        {
            ReachabilityChanged?.Invoke(null, EventArgs.Empty);
        }

        public static bool IsReachableWithoutRequiringConnection(NetworkReachabilityFlags flags)
        {
            var isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

            var noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0 || (flags & NetworkReachabilityFlags.IsWWAN) != 0;

            return isReachable && noConnectionRequired;
        }

        private static NetworkReachability _remoteHostReachability;

        public static NetworkStatus RemoteHostStatus()
        {
            NetworkReachabilityFlags flags;
            bool reachable;

            if (_remoteHostReachability == null)
            {
                _remoteHostReachability = new NetworkReachability(IPAddress.Any);

                reachable = _remoteHostReachability.TryGetFlags(out flags);

                _remoteHostReachability.SetNotification(OnChange);
                _remoteHostReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            }
            else
            {
                reachable = _remoteHostReachability.TryGetFlags(out flags);
            }

            if (!reachable)
                return NetworkStatus.NotReachable;

            if (!IsReachableWithoutRequiringConnection(flags))
                return NetworkStatus.NotReachable;

            return (flags & NetworkReachabilityFlags.IsWWAN) != 0 ? NetworkStatus.ReachableViaCarrierDataNetwork : NetworkStatus.ReachableViaWiFiNetwork;
        }

        public static NetworkStatus InternetConnectionStatus()
        {
            NetworkReachabilityFlags flags;
            bool defaultNetworkAvailable = IsNetworkAvailable(out flags);
            if (defaultNetworkAvailable && ((flags & NetworkReachabilityFlags.IsDirect) != 0))
                return NetworkStatus.NotReachable;

            if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                return NetworkStatus.ReachableViaCarrierDataNetwork;

            if (flags == 0)
                return NetworkStatus.NotReachable;

            return NetworkStatus.ReachableViaWiFiNetwork;
        }

        private static NetworkReachability _defaultRouteReachability;

        private static bool IsNetworkAvailable(out NetworkReachabilityFlags flags)
        {
            if (_defaultRouteReachability == null)
            {
                _defaultRouteReachability = new NetworkReachability(new IPAddress(0));
                _defaultRouteReachability.SetNotification(OnChange);
                _defaultRouteReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            }
            return _defaultRouteReachability.TryGetFlags(out flags) && IsReachableWithoutRequiringConnection(flags);
        }

        public static NetworkStatus LocalWifiConnectionStatus()
        {
            NetworkReachabilityFlags flags;
            if (IsAdHocWiFiNetworkAvailable(out flags))
                if ((flags & NetworkReachabilityFlags.IsDirect) != 0)
                    return NetworkStatus.ReachableViaWiFiNetwork;

            return NetworkStatus.NotReachable;
        }

        private static NetworkReachability _adHocWiFiNetworkReachability;

        public static bool IsAdHocWiFiNetworkAvailable(out NetworkReachabilityFlags flags)
        {
            if (_adHocWiFiNetworkReachability == null)
            {
                _adHocWiFiNetworkReachability = new NetworkReachability(new IPAddress(new byte[] {169, 254, 0, 0}));
                _adHocWiFiNetworkReachability.SetNotification(OnChange);
                _adHocWiFiNetworkReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            }

            return _adHocWiFiNetworkReachability.TryGetFlags(out flags) && IsReachableWithoutRequiringConnection(flags);
        }

        public static bool IsAirplaneMode()
        {
            return InternetConnectionStatus() == NetworkStatus.NotReachable;
        }
    }

    public static class PhoneCall
    {
        public static async void CreateCallReservation(string systemNumber, string presentationNumber, string destinationNumberFormatted, UIViewController viewController)
        {
            var expectedCallerIdNumber = UserDefault.AccountPhoneNumber;

            if (string.IsNullOrEmpty(expectedCallerIdNumber))
            {
                var alertController = UIAlertController.Create(null, "To make calls with this app, please enter your device's phone number.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, a => UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString))));
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                viewController.PresentViewController(alertController, true, null);

                return;
            }

            var destinationNumber = Regex.Replace(destinationNumberFormatted.Replace(" ", ""), @"[^\d]", "");

            var callReservationViewModel = new CallReservationViewModel(systemNumber, expectedCallerIdNumber, presentationNumber, destinationNumber, viewController);
            await callReservationViewModel.CreateCallReservationAsync();

            if (callReservationViewModel.IsErrorResponseReceived) return;

            var switchboardNumber = callReservationViewModel.Reservation.SwitchboardNumber;

            var phoneNumber = NSUrl.FromString("tel:" + "+1" + switchboardNumber);
            if (!UIApplication.SharedApplication.OpenUrl(phoneNumber))
            {
                var alertController = UIAlertController.Create(null, "Your device does not appear to support making cellular voice calls.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                viewController.PresentViewController(alertController, true, null);
            }
        }
    }
}