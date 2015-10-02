using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        Label = "@string/ActivityInactive_title",
        Theme = "@style/AppThemeActionBar")]
    public class InactiveActivity : InfoActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_inactive);
            ActionButton = FindViewById<Button>(Resource.Id.inactiveActivity_dialButton);

            SupportActionBar.SetTitle(Resource.String.ActivityInactive_title);
            SupportActionBar.SetIcon(Resource.Drawable.ic_action_back);
        }

        /// <summary>
        /// Dial button click action
        /// </summary>
        protected override void ActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}