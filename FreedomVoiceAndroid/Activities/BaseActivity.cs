using System;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
//#if DEBUG
using Android.Util;
//#endif
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Base app activity with helper subscription
    /// </summary>
    public abstract class BaseActivity : AppCompatActivity
    {
        private const string AirDlgTag = "AIR_DLG_TAG";
        public ActionsHelper Helper;
        protected App Appl;
        protected View RootLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
//#if DEBUG
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} created");
//#endif
            Appl = App.GetApplication(this);
            if (Appl.ApplicationHelper == null)
                return;
            Helper = Appl.ApplicationHelper.ActionsHelper;
        }

        protected override void OnPause()
        {
            base.OnPause();
//#if DEBUG
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} paused");
//#endif
            Helper.HelperEvent -= OnHelperEvent;
        }

        protected override void OnResume()
        {
            base.OnResume();
//#if DEBUG
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} resumed");
            //#else
#if !DEBUG
            Appl.ApplicationHelper.InitInsights();
#endif
            Helper.HelperEvent += OnHelperEvent;
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="sender">ActionsHelper</param>
        /// <param name="args">Result args</param>
        private void OnHelperEvent(object sender, EventArgs args)
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} handle event {args.GetType().Name}");
#endif
            var arg = args as ActionsHelperIntentArgs;
            if (arg != null)
            {
                var intentArg = arg.IntentData;
                if (intentArg == null) return;
                if (intentArg.Action == Intent.ActionCall)
                {
                    try
                    {
                        StartActivity(intentArg);
                    }
                    catch (ActivityNotFoundException)
                    {
                        var noCellularDialog = new NoCellularDialogFragment();
                        noCellularDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgCellular_title));
                    }
                }
                else
                    StartActivity(intentArg);
            }
            else
                OnHelperEvent(args as ActionsHelperEventArgs);
        }

        protected void AirplaneDialogOnDialogEvent(object sender, DialogEventArgs args)
        {
            if (args.Result == DialogResult.Ok)
                StartActivity(new Intent(Settings.ActionAirplaneModeSettings));
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected virtual void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.NoInternetConnection:
                        Snackbar.Make(RootLayout, Resource.String.Snack_noInternet, Snackbar.LengthLong).Show();
                        return;
                    case ActionsHelperEventArgs.ConnectionLostError:
                        Snackbar.Make(RootLayout, Resource.String.Snack_connectionLost, Snackbar.LengthLong).Show();
                        return;
                    case ActionsHelperEventArgs.InternalError:
                        Snackbar.Make(RootLayout, Resource.String.Snack_serverError, Snackbar.LengthLong).Show();
                        return;
                }
            }
        }

        protected void AirplaneDialog()
        {
            if (SupportFragmentManager.FindFragmentByTag(AirDlgTag) != null)
                return;
            var airplaneDialog = new AirplaneDialogFragment();
            airplaneDialog.DialogEvent += AirplaneDialogOnDialogEvent;
            airplaneDialog.Show(SupportFragmentManager, AirDlgTag);
        }
    }
}