using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Inactive phone number status info w/o back button
    /// </summary>
    [Activity(
        Label = "@string/ActivityInactive_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        Theme = "@style/AppThemeActionBar")]
    public class InactiveActivity : InfoActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_inactive);
            RootLayout = FindViewById(Resource.Id.inactiveActivity_root);
            ActionButton = FindViewById<Button>(Resource.Id.inactiveActivity_dialButton);
            SupportActionBar.SetTitle(Resource.String.ActivityInactive_title);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_action_logout:
                    var logoutDialog = new LogoutDialogFragment();
                    logoutDialog.DialogEvent += OnDialogEvent;
                    logoutDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgLogout_title));
                    return true;
                case global::Android.Resource.Id.Home:
                    BaseBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        protected void BaseBackPressed()
        {
            base.OnBackPressed();
        }

        /// <summary>
        /// Dial button click action
        /// </summary>
        protected override void ActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Helper.PhoneNumber != null)
            {
                var callIntent = new Intent(Intent.ActionCall, Uri.Parse("tel:" + GetString(Resource.String.ActivityInactive_customerNumber)));
#if DEBUG
                Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} CREATES CALL to {GetString(Resource.String.ActivityInactive_customerNumber)}");
#endif
                StartActivity(callIntent);
            }
            else
            {
                var noCellularDialog = new NoCellularDialogFragment();
                noCellularDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgCellular_title));
            }
        }
    }
}