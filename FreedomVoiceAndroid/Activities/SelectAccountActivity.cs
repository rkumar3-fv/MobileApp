using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Adapters;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Phone number selection activity
    /// </summary>
    [Activity
        (Label = "@string/ActivitySelect_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        Theme = "@style/AppThemeActionBar")]
    public class SelectAccountActivity : LogoutActivity
    {
        private RecyclerView _selectView;
        private AccountsRecyclerAdapter _adapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_select);
            RootLayout = FindViewById(Resource.Id.selectAccountActivity_root);
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
            Log.Debug(App.AppPackage,
                $"ACTIVITY {GetType().Name}: select account #{DataFormatUtils.ToPhoneNumber(_adapter.AccountName(position))}");
            Helper.SelectedAccount = Helper.AccountsList[position];
            Helper.GetPresentationNumbers();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Helper.SelectedAccount == null) return;
            var intent = (Helper.IsFirstRun)
                ? new Intent(this, typeof (DisclaimerActivity))
                : new Intent(this, typeof (ContentActivity));
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }
    }
}