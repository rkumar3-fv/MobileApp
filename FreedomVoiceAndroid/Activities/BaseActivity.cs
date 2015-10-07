using System.Collections.Generic;
using Android.OS;
using Android.Support.V7.App;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Base app activity with helper subscription
    /// </summary>
    public abstract class BaseActivity : AppCompatActivity
    {
        protected ActionsHelper Helper;
        protected List<long> WaitingActions;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var app = App.GetApplication(this);
            WaitingActions = new List<long>();
            Helper = app.Helper;
        }

        protected override void OnPause()
        {
            base.OnPause();
            Helper.HelperEvent -= OnHelperEvent;
        }

        protected override void OnResume()
        {
            base.OnResume();
            Helper.HelperEvent += OnHelperEvent;
            foreach (var waitingAction in WaitingActions)
                Helper.GetRusultById(waitingAction);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="sender">ActionsHelper</param>
        /// <param name="args">Result args</param>
        private void OnHelperEvent(object sender, ActionsHelperEventArgs args)
        {
            if (args.ResponseData == null) return;
            OnHelperEvent(args);
            if (!WaitingActions.Contains(args.RequestId)) return;
            Helper.RemoveResult(args.RequestId);
            WaitingActions.Remove(args.RequestId);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected abstract void OnHelperEvent(ActionsHelperEventArgs args);
    }
}