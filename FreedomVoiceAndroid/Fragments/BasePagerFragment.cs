using System;
using Android.OS;
using Android.Support.V4.App;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
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
        protected ContentActivity ContentActivity => Activity as ContentActivity;

        protected LayoutInflater Inflater;
        protected WeakReference<View> RootView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Inflater = inflater ?? LayoutInflater.From(ContentActivity);
            View view;
            if (RootView == null)
                view = null;
            else
                RootView.TryGetTarget(out view);

            if (view != null)
            {
                var parent = view.Parent;
                (parent as ViewGroup)?.RemoveView(view);
            }
            else
            {
                view = InitView();
                RootView = new WeakReference<View>(view);
            }
            return view;
        }

        protected abstract View InitView();

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var app = App.GetApplication(Activity as BaseActivity);
            Helper = app.ApplicationHelper.ActionsHelper;
        }

        public override void OnPause()
        {
            base.OnPause();
#if DEBUG
            Log.Debug(App.AppPackage, $"FRAGMENT {GetType().Name} paused");
#endif
            Helper.HelperEvent -= OnHelperEvent;
        }

        public override void OnResume()
        {
            base.OnResume();
#if DEBUG
            Log.Debug(App.AppPackage, $"FRAGMENT {GetType().Name} resumed");
#endif
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
            var eventArgs = args as ActionsHelperEventArgs;
            if (eventArgs != null)
                OnHelperEvent(eventArgs);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected abstract void OnHelperEvent(ActionsHelperEventArgs args);
    }
}