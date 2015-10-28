using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Base app activity with helper subscription
    /// </summary>
    public abstract class BaseActivity : AppCompatActivity
    {
        public ActionsHelper Helper;
        protected App Appl;
        protected View RootLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Appl = App.GetApplication(this);
            Helper = Appl.Helper;
        }

        protected override void OnPause()
        {
            base.OnPause();
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} paused");
            Helper.HelperEvent -= OnHelperEvent;
        }

        protected override void OnResume()
        {
            base.OnResume();
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} resumed");
            Helper.HelperEvent += OnHelperEvent;
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="sender">ActionsHelper</param>
        /// <param name="args">Result args</param>
        private void OnHelperEvent(object sender, EventArgs args)
        {
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} handle event {args.GetType().Name}");
            var arg = args as ActionsHelperIntentArgs;
            if (arg != null)
            {
                var intentArg = arg;
                StartActivity(intentArg.IntentData);
            }
            else
                OnHelperEvent(args as ActionsHelperEventArgs);
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
                    case ActionsHelperEventArgs.ConnectionLostError:
                        Snackbar.Make(RootLayout, Resource.String.Snack_connectionLost, Snackbar.LengthLong).Show();
                        return;
                }
            }
        }
    }
}