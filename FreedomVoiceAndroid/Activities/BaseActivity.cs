using System;
using System.Collections.Generic;
using Android.OS;
using Android.Support.V7.App;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class BaseActivity : AppCompatActivity
    {
        protected ActionsHelper _helper;
        protected List<long> _waitingActions;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var app = App.GetApplication(this);
            _waitingActions = new List<long>();
            _helper = app.Helper;
        }

        protected override void OnPause()
        {
            base.OnPause();
            _helper.HelperEvent -= OnHelperEvent;
        }

        protected override void OnResume()
        {
            base.OnResume();
            _helper.HelperEvent += OnHelperEvent;
            foreach (var waitingAction in _waitingActions)
                _helper.GetRusultById(waitingAction);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="sender">ActionsHelper</param>
        /// <param name="args">Result args</param>
        private void OnHelperEvent(object sender, ActionsHelperEventArgs args)
        {
            if (!_waitingActions.Contains(args.RequestId) || args.DataBundle == null) return;
            OnHelperEvent(args);
            _helper.RemoveResult(args.RequestId);
            _waitingActions.Remove(args.RequestId);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected abstract void OnHelperEvent(ActionsHelperEventArgs args);
    }
}