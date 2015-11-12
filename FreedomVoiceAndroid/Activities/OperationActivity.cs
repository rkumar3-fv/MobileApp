using Android.Content;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Telephony;
#if DEBUG
using Android.Util;
using FreedomVoice.Core.Utils;
#endif
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Java.Lang;

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
                if (AppHelper.Instance(this).IsAirplaneModeOn())
                {
                    var airplaneDialog = new AirplaneDialogFragment();
                    airplaneDialog.DialogEvent += AirplaneDialogOnDialogEvent;
                    airplaneDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgAirplane_content));
                }
                else
                {
                    if (AppHelper.Instance(this).IsCallerIdHides())
                    {
                        var callerDialog = new CallerIdDialogFragment();
                        callerDialog.DialogEvent += CallerDialogOnDialogEvent;
                        callerDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgCallerId_content));
                    }
                    else
                    {
                        if (phone.Length > 1)
                        {
                            var normalizedNumber = PhoneNumberUtils.NormalizeNumber(phone);
#if DEBUG
                            Log.Debug(App.AppPackage, $"DIAL TO {DataFormatUtils.ToPhoneNumber(phone)}");
#endif
                            Helper.Call(normalizedNumber);
                            JavaSystem.Gc();
                        }
#if DEBUG
                        else
                        {
                            Log.Debug(App.AppPackage, "DIAL TO EMPTY PHONE UNAVAILABLE");
                        }
#endif
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