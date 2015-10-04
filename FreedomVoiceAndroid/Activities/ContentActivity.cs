using Android.App;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Internal.View;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Main pplication screen
    /// </summary>
    [Activity(
        MainLauncher = true,
        Label = "@string/ApplicationTitle",
        Icon = "@drawable/ic_launcher")]
    class ContentActivity : BaseActivity
    {
        private ViewPager _viewPager;
        private Toolbar _toolbar;
        private TabLayout _tabLayout;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_content);
            _tabLayout = FindViewById<TabLayout>(Resource.Id.contentActivity_tabs);
            _viewPager = FindViewById<ViewPager>(Resource.Id.contentActivity_contentPager);
            _toolbar = FindViewById<Toolbar>(Resource.Id.contentActivity_toolbar);
            SetSupportActionBar(_toolbar);
    }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            var inflater = new SupportMenuInflater(this);
            inflater.Inflate(Resource.Menu.menu_logout, menu);
            return true;
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}