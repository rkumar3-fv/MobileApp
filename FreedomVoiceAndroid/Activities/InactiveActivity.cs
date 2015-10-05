using System;
using Android.App;
using Android.OS;
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
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
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