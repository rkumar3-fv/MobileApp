using Android.Content;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Widget;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class OperationActivity : LogoutActivity
    {
        private const string CallerDlgTag = "CALLER_DLG_TAG";
        private const string CellularDlgTag = "CELLULAR_DLG_TAG";
        private const string PhoneDlgTag = "PHONE_DLG_TAG";

        /// <summary>
        /// Call action
        /// </summary>
        /// <param name="phone">Destination phone</param>
        public void Call(string phone)
        {
            if (Appl?.ApplicationHelper == null) return;
            if (Appl.ApplicationHelper.CheckCallsPermission() == false || Appl.ApplicationHelper.CheckReadPhoneState() == false)
            {
                var snackPerm = Snackbar.Make(RootLayout, Resource.String.Snack_noPhonePermission, Snackbar.LengthLong);
                snackPerm.SetAction(Resource.String.Snack_noPhonePermissionAction, OnSetCallsPermission);
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
                            var normalizedNumber = ServiceContainer.Resolve<IPhoneFormatter>().Format(phone);
#if DEBUG
                            Log.Debug(App.AppPackage, $"DIAL TO {ServiceContainer.Resolve<IPhoneFormatter>().Format(phone)}");
#else
                            Appl.ApplicationHelper.Reports?.Log($"DIAL TO {DataFormatUtils.ToPhoneNumber(phone)}");
#endif
                            Helper.Call(normalizedNumber);
                            JavaSystem.Gc();
                        }

                        else
                        {
                            try
                            {
                                Snackbar.Make(RootLayout, Resource.String.Snack_incorrectDest, Snackbar.LengthLong).Show();
                            }
                            catch (RuntimeException)
                            {
#if DEBUG
                                Log.Debug(App.AppPackage, "SNACKBAR creation failed. Please, REBUILD APP.");
#else
                                Appl.ApplicationHelper.Reports?.Log("SNACKBAR creation failed. Please, REBUILD APP.");
#endif
                                Toast.MakeText(this, Resource.String.Snack_incorrectDest, ToastLength.Short).Show();
                            }
                            
#if DEBUG
                            Log.Debug(App.AppPackage, "DIAL TO EMPTY PHONE UNAVAILABLE");
#else
                            Appl.ApplicationHelper.Reports?.Log("DIAL TO EMPTY PHONE UNAVAILABLE");
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
                        snackPerm.SetAction(Resource.String.Snack_noPhonePermissionAction, OnSetCallsPermission);
                        snackPerm.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                        snackPerm.Show();
                        break;

                    case ActionsHelperEventArgs.CallReservationFail:
                        try
                        {
                            Snackbar.Make(RootLayout, Resource.String.Snack_callFailed, Snackbar.LengthLong).Show();
                        }
                        catch (RuntimeException)
                        {
#if DEBUG
                            Log.Debug(App.AppPackage, "SNACKBAR creation failed. Please, REBUILD APP.");
#else
                            Appl.ApplicationHelper.Reports?.Log("SNACKBAR creation failed. Please, REBUILD APP.");
#endif
                            Toast.MakeText(this, Resource.String.Snack_callFailed, ToastLength.Short).Show();
                        }        
                        break;
                    case ActionsHelperEventArgs.CallReservationWrong:
                        try
                        {
                            Snackbar.Make(RootLayout, Resource.String.Snack_callWrong, Snackbar.LengthLong).Show();
                        }
                        catch (RuntimeException)
                        {
#if DEBUG
                            Log.Debug(App.AppPackage, "SNACKBAR creation failed. Please, REBUILD APP.");
#else
                            Appl.ApplicationHelper.Reports?.Log("SNACKBAR creation failed. Please, REBUILD APP.");
#endif
                            Toast.MakeText(this, Resource.String.Snack_callWrong, ToastLength.Short).Show();
                        }
                        
                        break;
                }
            }
        }

        private void CallerIdDialog()
        {
            if ((SupportFragmentManager.FindFragmentByTag(CallerDlgTag) != null)||(IsFinishing))
                return;
            var callerDialog = new CallerIdDialogFragment();
            callerDialog.DialogEvent += CallerDialogOnDialogEvent;
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(callerDialog, CallerDlgTag);
            transaction.CommitAllowingStateLoss();
        }

        private void NoCellularDialog()
        {
            if ((SupportFragmentManager.FindFragmentByTag(CellularDlgTag) != null)||(IsFinishing))
                return;
            var noCellularDialog = new NoCellularDialogFragment();
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(noCellularDialog, CellularDlgTag);
            transaction.CommitAllowingStateLoss();
        }

        private void CreatePhoneDialog()
        {
            if ((SupportFragmentManager.FindFragmentByTag(PhoneDlgTag) != null)||(IsFinishing))
                return;
            var noPhoneDialog = new NoPhoneDialogFragment();
            noPhoneDialog.DialogEvent += NoPhoneDialogOnDialogEvent;
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(noPhoneDialog, PhoneDlgTag);
            transaction.CommitAllowingStateLoss();
        }
    }
}