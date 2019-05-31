using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
#if DEBUG
using Android.Util;
using FreedomVoice.Core.Utils.Interfaces;
#endif
using FreedomVoice.Core.Utils;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Phone number selection activity
    /// </summary>
    [Activity
        (Label = "@string/ActivitySelect_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden, 
        Theme = "@style/AppThemeActionBar")]
    public class SelectAccountActivity : LogoutActivity
    {
        private RelativeLayout _progressLayout;
        private ProgressBar _progressBar;
        private RecyclerView _selectView;
        private AccountsRecyclerAdapter _adapter;

        protected override void OnCreate(Bundle bundle)
        {
            if ((Intent.Flags & ActivityFlags.BroughtToFront) != 0)
            {
                Finish();
                return;
            }
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_select);
            RootLayout = FindViewById(Resource.Id.selectAccountActivity_root);
            _progressLayout = FindViewById<RelativeLayout>(Resource.Id.selectAccountActivity_progressLayout);
            _progressBar = FindViewById<ProgressBar>(Resource.Id.selectAccountActivity_progress);
            var progressColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressBlue));
            _progressBar.IndeterminateDrawable?.SetColorFilter(progressColor, PorterDuff.Mode.SrcIn);
            _progressBar.ProgressDrawable?.SetColorFilter(progressColor, PorterDuff.Mode.SrcIn);
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
#if DEBUG
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name}: select account #{ServiceContainer.Resolve<IPhoneFormatter>().Format(_adapter.AccountName(position))}");
#else
            Appl.ApplicationHelper.Reports?.Log($"ACTIVITY {GetType().Name}: select account #{DataFormatUtils.ToPhoneNumber(_adapter.AccountName(position))}");
#endif
            if (_progressLayout.Visibility == ViewStates.Gone)
                _progressLayout.Visibility = ViewStates.Visible;
            Helper.SelectedAccount = Helper.AccountsList[position];
            Helper.GetPresentationNumbers();
            Helper.RegisterFcm();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_progressLayout.Visibility == ViewStates.Visible)
                _progressLayout.Visibility = ViewStates.Gone;
            if (Helper.SelectedAccount == null) return;
            var intent = (Helper.IsFirstRun)
                ? new Intent(this, typeof (DisclaimerActivity))
                : new Intent(this, typeof (ContentActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            if (_progressLayout.Visibility == ViewStates.Visible)
                _progressLayout.Visibility = ViewStates.Gone;
        }
    }
}