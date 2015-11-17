using System;
using System.Net;
using CoreFoundation;
using SystemConfiguration;
using CoreTelephony;

namespace FreedomVoice.iOS.Utilities
{
    public enum NetworkStatus
    {
        NotReachable,
        ReachableViaCarrierDataNetwork,
        ReachableViaWiFiNetwork
    }

    public static class PhoneCapability
    {
        public static event EventHandler ReachabilityChanged;

        private static NetworkReachability _defaultRouteReachability;
        private static NetworkReachability _adHocWiFiNetworkReachability;

        public static bool NetworkIsUnreachable => InternetConnectionStatus() == NetworkStatus.NotReachable;

        public static bool IsSimCardInstalled()
        {
            var provider = new CTTelephonyNetworkInfo().SubscriberCellularProvider;

            return !string.IsNullOrEmpty(provider?.MobileNetworkCode);
        }

        private static NetworkStatus InternetConnectionStatus()
        {
            NetworkReachabilityFlags flags;

            bool defaultNetworkAvailable = IsNetworkAvailable(out flags);

            if (defaultNetworkAvailable && ((flags & NetworkReachabilityFlags.IsDirect) != 0))
                return NetworkStatus.NotReachable;

            if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                return NetworkStatus.ReachableViaCarrierDataNetwork;

            return flags == 0 ? NetworkStatus.NotReachable : NetworkStatus.ReachableViaWiFiNetwork;
        }

        private static NetworkStatus LocalWifiConnectionStatus()
        {
            NetworkReachabilityFlags flags;
            if (IsAdHocWiFiNetworkAvailable(out flags))
                if ((flags & NetworkReachabilityFlags.IsDirect) != 0)
                    return NetworkStatus.ReachableViaWiFiNetwork;

            return NetworkStatus.NotReachable;
        }

        private static void OnChange(NetworkReachabilityFlags flags)
        {
            ReachabilityChanged?.Invoke(null, EventArgs.Empty);
        }

        private static bool IsAdHocWiFiNetworkAvailable(out NetworkReachabilityFlags flags)
        {
            if (_adHocWiFiNetworkReachability == null)
            {
                _adHocWiFiNetworkReachability = new NetworkReachability(IPAddress.Any);
                _adHocWiFiNetworkReachability.SetNotification(OnChange);
                _adHocWiFiNetworkReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            }

            return _adHocWiFiNetworkReachability.TryGetFlags(out flags) && IsReachableWithoutRequiringConnection(flags);
        }

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

        private static bool IsReachableWithoutRequiringConnection(NetworkReachabilityFlags flags)
        {
            var isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

            var noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0 || (flags & NetworkReachabilityFlags.IsWWAN) != 0;

            return isReachable && noConnectionRequired;
        }
    }
}