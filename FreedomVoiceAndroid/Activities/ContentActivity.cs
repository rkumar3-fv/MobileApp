using Android.App;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Internal.View;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Fragments;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Main pplication screen
    /// </summary>
    [Activity(
        Label = "@string/ApplicationTitle",
        Icon = "@drawable/ic_launcher")]
    class ContentActivity : BaseActivity
    {
        private ContentPagerAdapter _pagerAdapter;
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
            _pagerAdapter = new ContentPagerAdapter(SupportFragmentManager);
            var contactsFragment = new ContactsFragment();
            var keypadFragment = new KeypadFragment();
            var messagesFragment = new MessagesFragment();
            var recentsFragment = new RecentsFragment();
            if (_viewPager != null)
            {
                _viewPager.OffscreenPageLimit = 4;
                _pagerAdapter.AddFragment(recentsFragment, GetString(Resource.String.FragmentRecents_title));
                _pagerAdapter.AddFragment(contactsFragment, GetString(Resource.String.FragmentContacts_title));
                _pagerAdapter.AddFragment(keypadFragment, GetString(Resource.String.FragmentKeypad_title));
                _pagerAdapter.AddFragment(messagesFragment, GetString(Resource.String.FragmentMessages_title));
                _viewPager.Adapter = _pagerAdapter;
                _viewPager.CurrentItem = 3;
            }
            _tabLayout.SetupWithViewPager(_viewPager);
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
                    //TODO: logout
                    return true;
            }
            return base.OnOptionsItemSelected(item);
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