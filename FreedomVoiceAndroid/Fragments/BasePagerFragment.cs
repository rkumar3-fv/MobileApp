using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Base viewpager fragment
    /// </summary>
    public abstract class BasePagerFragment : Fragment
    {
        protected ActionsHelper Helper;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var app = App.GetApplication(Activity);
            Helper = app.Helper;
        }

        public override void OnPause()
        {
            base.OnPause();
            Log.Debug(App.AppPackage, $"FRAGMENT {GetType().Name} paused");
            Helper.HelperEvent -= OnHelperEvent;
        }

        public override void OnResume()
        {
            base.OnResume();
            Log.Debug(App.AppPackage, $"FRAGMENT {GetType().Name} resumed");
            Helper.HelperEvent += OnHelperEvent;
        }

        /// <summary>
        /// Get current content activity
        /// </summary>
        /// <returns></returns>
        public ContentActivity GetContentActivity()
        {
            return Activity as ContentActivity;
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="sender">ActionsHelper</param>
        /// <param name="args">Result args</param>
        private void OnHelperEvent(object sender, EventArgs args)
        {
            
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected abstract void OnHelperEvent(ActionsHelperEventArgs args);
    }
}