using System;
using Android.Gms.Analytics;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Dialogs;
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

            Appl.AnalyticsTracker.SetScreenName($"Activity {GetType().Name}");
            Appl.AnalyticsTracker.Send(new HitBuilders.ScreenViewBuilder().Build());
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
        protected abstract void OnHelperEvent(ActionsHelperEventArgs args);
    }
}