using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Telephony;
#if DEBUG
using Android.Util;
using FreedomVoice.Core.Utils;
#endif
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class OperationActivity : LogoutActivity
    {
        /// <summary>
        /// Call action
        /// </summary>
        /// <param name="phone">Destination phone</param>
        public void Call(string phone)
        {
            if ((Helper.PhoneNumber == null) || (Helper.PhoneNumber.Length == 0))
            {
                var noCellularDialog = new NoCellularDialogFragment();
                noCellularDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgCellular_title));
            }
            else
            {
                if (IsAirplaneModeOn())
                {
                    var airplaneDialog = new AirplaneDialogFragment();
                    airplaneDialog.DialogEvent += AirplaneDialogOnDialogEvent;
                    airplaneDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgAirplane_content));
                }
                else
                {
                    if (IsCallerIdHides())
                    {
                        var callerDialog = new CallerIdDialogFragment();
                        callerDialog.DialogEvent += CallerDialogOnDialogEvent;
                        callerDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgCallerId_content));
                    }
                    else
                    {
                        var normalizedNumber = PhoneNumberUtils.NormalizeNumber(phone);
#if DEBUG
                    Log.Debug(App.AppPackage, $"DIAL TO {DataFormatUtils.ToPhoneNumber(phone)}");
#endif
                        Helper.Call(normalizedNumber);
                    }
                }
            }
        }

        private void CallerDialogOnDialogEvent(object sender, DialogEventArgs args)
        {
            if (args.Result == DialogResult.Ok)
                StartActivityForResult(new Intent(Settings.ActionSettings), 0);
        }

        private void AirplaneDialogOnDialogEvent(object sender, DialogEventArgs args)
        {
            if (args.Result == DialogResult.Ok)
                StartActivityForResult(new Intent(Settings.ActionAirplaneModeSettings), 0);
        }

        /// <summary>
        /// Check is airplane mode ON for different API levels
        /// </summary>
        private bool IsAirplaneModeOn()
        {
            return (int) Build.VERSION.SdkInt < 17 ? IsAirplaneOldApi() : IsAirplaneNewApi();
        }

        //@SuppressLint("NewApi")
        private bool IsAirplaneNewApi()
        {
            return Settings.Global.GetInt(ContentResolver, Settings.Global.AirplaneModeOn, 0) != 0;
        }

        //@SuppressWarnings("deprecation")
        private bool IsAirplaneOldApi()
        {
#pragma warning disable 618
            return Settings.System.GetInt(ContentResolver, Settings.System.AirplaneModeOn, 0) != 0;
#pragma warning restore 618
        }

        /// <summary>
        /// Get caller ID state
        /// <b>No API method available for getting caller ID state</b>
        /// </summary>
        private bool IsCallerIdHides()
        {
            return false;
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.CallReservationFail:
                        Snackbar.Make(RootLayout, Resource.String.Snack_callFailed, Snackbar.LengthLong).Show();
                        break;
                    case ActionsHelperEventArgs.CallReservationWrong:
                        Snackbar.Make(RootLayout, Resource.String.Snack_callWrong, Snackbar.LengthLong).Show();
                        break;
                }
            }
        }
    }
}