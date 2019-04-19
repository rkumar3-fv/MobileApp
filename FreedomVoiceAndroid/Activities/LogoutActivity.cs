using Android.Content;
using Android.Content.PM;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.View;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class LogoutActivity : BaseActivity
    {
        private const string FeedbackDlgTag = "FEEDBACK_DLG_TAG";
        protected const int CallsPermissionRequestId = 2045;
        protected const int StatePermissionRequestId = 2046;
        protected const int ContactsPermissionRequestId = 2047;
        protected const int StoragePermissionRequestId = 2048;
        private bool _needPhoneRedirection;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            var inflater = new SupportMenuInflater(this);
            inflater.Inflate(Resource.Menu.menu_logout, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_action_phone:
                    if (Appl.ApplicationHelper.CheckCallsPermission() == false)
                    {
                        _needPhoneRedirection = true;
                        var snackPerm = Snackbar.Make(RootLayout, Resource.String.Snack_noPhonePermission, Snackbar.LengthLong);
                        snackPerm.SetAction(Resource.String.Snack_noPhonePermissionAction, OnSetCallsPermission);
                        snackPerm.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                        snackPerm.Show();
                    }
                    else
                         SetPhone();
                    return true;
                case Resource.Id.menu_action_logout:
                    LogoutAction();
                    return true;
                /*case Resource.Id.menu_action_feedback:
                    FeedbackDialog();
                    return true;*/
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        /// <summary>
        /// Dialog actions handler
        /// </summary>
        protected void OnDialogEvent(object sender, DialogEventArgs args)
        {
            switch (sender.GetType().Name)
            {
                case "LogoutDialogFragment":
                    if (args.Result == DialogResult.Ok)
                        Helper.Logout();
                    break;
            }
        }

        protected void LogoutAction()
        {
            bool hasRecents;
            if ((Helper.RecentsDictionary == null) || (Helper.RecentsDictionary.Count == 0))
                hasRecents = false;
            else
                hasRecents = true;
            if (IsFinishing)
                return;
            var logoutDialog = new LogoutDialogFragment(hasRecents);
            logoutDialog.DialogEvent += OnDialogEvent;
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(logoutDialog, GetString(Resource.String.DlgLogout_title));
            transaction.CommitAllowingStateLoss();
        }

        private void SetPhone()
        {
            if (!Appl.ApplicationHelper.IsVoicecallsSupported())
            {
                if (IsFinishing)
                    return;
                var noCellularDialog = new NoCellularDialogFragment();
                var transaction = SupportFragmentManager.BeginTransaction();
                transaction.Add(noCellularDialog, GetString(Resource.String.DlgCellular_title));
                transaction.CommitAllowingStateLoss();
            }
            else
            {
                var intent = new Intent(this, typeof(SetNumberActivityWithBack));
                StartActivity(intent);
            }
        }

        protected void OnSetCallsPermission(View view)
        {
            if (!Appl.ApplicationHelper.CheckCallsPermission())
                RequestPermissions(new[] { AppHelper.MakeCallsPermission, AppHelper.ReadPhoneStatePermission }, CallsPermissionRequestId);
        }

        protected void OnSetStatePermission(View view)
        {
            if (!Appl.ApplicationHelper.CheckReadPhoneState())
                RequestPermissions(new[] { AppHelper.ReadPhoneStatePermission }, StatePermissionRequestId);
        }

        protected void OnSetContactsPermission(View view)
        {
            if (!Appl.ApplicationHelper.CheckContactsPermission())
                RequestPermissions(new[] { AppHelper.ReadContactsPermission }, ContactsPermissionRequestId);
        }

        protected void OnSetStoragePermission(View view)
        {
            if (!Appl.ApplicationHelper.CheckFilesPermissions())
                RequestPermissions(new[] { AppHelper.WriteStoragePermission }, StoragePermissionRequestId);
        }

        private void FeedbackDialog()
        {
            if ((SupportFragmentManager.FindFragmentByTag(FeedbackDlgTag) != null)||(IsFinishing))
                return;
            var feedbackDialog = new FeedbackDialogFragment(this);
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Add(feedbackDialog, FeedbackDlgTag);
            transaction.CommitAllowingStateLoss();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case CallsPermissionRequestId:
                    if ((grantResults[0] == Permission.Granted) && _needPhoneRedirection)
                    {
                        _needPhoneRedirection = false;
                        SetPhone();
                    }
                    else
                        _needPhoneRedirection = false;
                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }
    }
}