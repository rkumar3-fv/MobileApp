using Android.App;
using Android.OS;
using Android.Support.V7.Internal.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Phone number selection activity
    /// </summary>
    [Activity
        (Label = "@string/ActivitySelect_title",
        Theme = "@style/AppThemeActionBar")]
    public class SelectAccountActivity : BaseActivity
    {
        private RecyclerView _selectView;
        private AccountsRecyclerAdapter _adapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_select);
            _selectView = FindViewById<RecyclerView>(Resource.Id.selectAccountActivity_accountsList);
            _selectView.SetLayoutManager(new LinearLayoutManager(this));
            _selectView.AddItemDecoration(new DividerItemDecorator(this, Resource.Drawable.divider));
            _adapter = new AccountsRecyclerAdapter(Helper.AccountsList);
            _selectView.SetAdapter(_adapter);
            _adapter.ItemClick += SelectViewOnClick;
        }

        /// <summary>
        /// Account list click
        /// </summary>
        /// <param name="sender">adapter-sender</param>
        /// <param name="position">selected element</param>
        private void SelectViewOnClick(object sender, int position)
        {
            if (position >= Helper.AccountsList.Count) return;
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name}: select account #{DataFormatUtils.ToPhoneNumber(_adapter.AccountName(position))}");
            Helper.SelectedAccount = Helper.AccountsList[position];
            Helper.GetPresentationNumbers();
            //var intent = (Helper.IsFirstRun)? new Intent(this, typeof(DisclaimerActivity)): new Intent(this, typeof(ContentActivity));
            //StartActivity(intent);
        }

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
                    var logoutDialog = new LogoutDialogFragment();
                    logoutDialog.DialogEvent += OnDialogEvent;
                    logoutDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgLogout_title));
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}