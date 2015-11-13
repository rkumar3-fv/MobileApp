using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using FreedomVoice.Core.Utils;
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
        public const string InactiveAccontTag = "InactiveTag";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var extra = Intent.GetStringExtra(InactiveAccontTag);
            SetContentView(Resource.Layout.act_inactive);
            RootLayout = FindViewById(Resource.Id.inactiveActivity_root);
            ActionButton = FindViewById<CardView>(Resource.Id.inactiveActivity_dialButton);
            if (extra != null)
                SupportActionBar.Title = DataFormatUtils.ToPhoneNumber(extra);
            else if (Helper.SelectedAccount != null)
                SupportActionBar.Title = DataFormatUtils.ToPhoneNumber(Helper.SelectedAccount.AccountName);
            else
                SupportActionBar.SetTitle(Resource.String.ActivityInactive_title);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_action_logout:
                    var logoutDialog = new LogoutDialogFragment(false);
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
#if DEBUG
                var callIntent = new Intent(Intent.ActionCall, Uri.Parse("tel:+1" + GetString(Resource.String.ActivityInactive_customerNumber)));
                Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} CREATES CALL to +1{GetString(Resource.String.ActivityInactive_customerNumber)}");
#else
                var callIntent = new Intent(Intent.ActionCall, Uri.Parse("tel:" + GetString(Resource.String.ActivityInactive_customerNumber)));
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