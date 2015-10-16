using Android.Support.V7.Internal.View;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Dialogs;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class LogoutActivity : BaseActivity
    {
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
                case Resource.Id.menu_action_logout:
                    LogoutAction();
                    return true;
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
            var logoutDialog = new LogoutDialogFragment();
            logoutDialog.DialogEvent += OnDialogEvent;
            logoutDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgLogout_title));
        }
    }
}