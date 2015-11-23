using Android.Content;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class OperationActivity : LogoutActivity
    {
        private const int ContactsPermissionRequestId = 2045;
        private const string CallerDlgTag = "CALLER_DLG_TAG";
        private const string CellularDlgTag = "CELLULAR_DLG_TAG";
        private const string PhoneDlgTag = "PHONE_DLG_TAG";

        /// <summary>
        /// Call action
        /// </summary>
        /// <param name="phone">Destination phone</param>
        public void Call(string phone)
        {
            if (Appl.ApplicationHelper.CheckCallsPermission() == false)
            {
                var snackPerm = Snackbar.Make(RootLayout, Resource.String.Snack_noPhonePermission, Snackbar.LengthLong);
                snackPerm.SetAction(Resource.String.Snack_noPhonePermissionAction, OnSetPermission);
                snackPerm.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                snackPerm.Show();
            }
            else if (!Appl.ApplicationHelper.IsVoicecallsSupported()||(Appl.ApplicationHelper.GetMyPhoneNumber() == null))
            {
                NoCellularDialog();
            }
            else if (Appl.ApplicationHelper.GetMyPhoneNumber().Length < 10)
            {
                CreatePhoneDialog();
            }
            else
            {
                if (Appl.ApplicationHelper.IsAirplaneModeOn())
                {
                    AirplaneDialog();
                }
                else
                {
                    if (Appl.ApplicationHelper.IsCallerIdHides())
                    {
                        CallerIdDialog();
                    }
                    else
                    {
                        if (phone.Length > 4)
                        {
                            var normalizedNumber = DataFormatUtils.NormalizePhone(phone);
#if DEBUG
                            Log.Debug(App.AppPackage, $"DIAL TO {DataFormatUtils.ToPhoneNumber(phone)}");
#endif
                            Helper.Call(normalizedNumber);
                            JavaSystem.Gc();
                        }

                        else
                        {
                            Snackbar.Make(RootLayout, Resource.String.Snack_incorrectDest, Snackbar.LengthLong).Show();
#if DEBUG
                            Log.Debug(App.AppPackage, "DIAL TO EMPTY PHONE UNAVAILABLE");
#endif
                        }
                    }
                }
            }
        }

        private void NoPhoneDialogOnDialogEvent(object sender, DialogEventArgs args)
        {
            if (args.Result == DialogResult.Ok)
                StartActivity(new Intent(this, typeof(SetNumberActivityWithBack)));
        }

        private void CallerDialogOnDialogEvent(object sender, DialogEventArgs args)
        {
            if (args.Result == DialogResult.Ok)
                StartActivity(new Intent(Settings.ActionSettings));
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.CallReservationNotSupports:
                        NoCellularDialog();
                        break;
                    case ActionsHelperEventArgs.PhoneNumberNotSets:
                        CreatePhoneDialog();
                        break;
                    case ActionsHelperEventArgs.CallPermissionDenied:
                        var snackPerm = Snackbar.Make(RootLayout, Resource.String.Snack_noPhonePermission, Snackbar.LengthLong);
                        snackPerm.SetAction(Resource.String.Snack_noPhonePermissionAction, OnSetPermission);
                        snackPerm.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                        snackPerm.Show();
                        break;

                    case ActionsHelperEventArgs.CallReservationFail:
                        Snackbar.Make(RootLayout, Resource.String.Snack_callFailed, Snackbar.LengthLong).Show();
                        break;
                    case ActionsHelperEventArgs.CallReservationWrong:
                        Snackbar.Make(RootLayout, Resource.String.Snack_callWrong, Snackbar.LengthLong).Show();
                        break;
                }
            }
        }

        private void OnSetPermission(View view)
        {
            RequestPermissions(new[] { AppHelper.MakeCallsPermission }, ContactsPermissionRequestId);
        }

        private void CallerIdDialog()
        {
            if (SupportFragmentManager.FindFragmentByTag(CallerDlgTag) != null)
                return;
            var callerDialog = new CallerIdDialogFragment();
            callerDialog.DialogEvent += CallerDialogOnDialogEvent;
            callerDialog.Show(SupportFragmentManager, CallerDlgTag);
        }

        private void NoCellularDialog()
        {
            if (SupportFragmentManager.FindFragmentByTag(CellularDlgTag) != null)
                return;
            var noCellularDialog = new NoCellularDialogFragment();
            noCellularDialog.Show(SupportFragmentManager, CellularDlgTag);
        }

        private void CreatePhoneDialog()
        {
            if (SupportFragmentManager.FindFragmentByTag(PhoneDlgTag) != null)
                return;
            var noPhoneDialog = new NoPhoneDialogFragment();
            noPhoneDialog.DialogEvent += NoPhoneDialogOnDialogEvent;
            noPhoneDialog.Show(SupportFragmentManager, PhoneDlgTag);
        }
    }
}