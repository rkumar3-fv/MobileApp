using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.App;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Base viewpager fragment
    /// </summary>
    public abstract class BasePagerFragment : Fragment
    {
        protected ActionsHelper _helper;
        protected List<long> _waitingActions;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var app = App.GetApplication(Activity);
            _waitingActions = new List<long>();
            _helper = app.Helper;
        }

        public override void OnPause()
        {
            base.OnPause();
            _helper.HelperEvent -= OnHelperEvent;
        }

        public override void OnResume()
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