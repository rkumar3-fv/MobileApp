using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Fragments;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Main aplication screen
    /// </summary>
    [Activity(
        Label = "@string/ApplicationTitle",
        Icon = "@drawable/ic_launcher")]
    public class ContentActivity : BaseActivity
    {
        private ContentPagerAdapter _pagerAdapter;
        private ContentPager _viewPager;
        private Toolbar _toolbar;
        private TabLayout _tabLayout;
        private Spinner _toolbarSpinner;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_content);
            _tabLayout = FindViewById<TabLayout>(Resource.Id.contentActivity_tabs);
            _viewPager = FindViewById<ContentPager>(Resource.Id.contentActivity_contentPager);
            _toolbar = FindViewById<Toolbar>(Resource.Id.contentActivity_toolbar);
            _toolbarSpinner = FindViewById<Spinner>(Resource.Id.contentActivity_toolbarSpinner);
            SetSupportActionBar(_toolbar);
            _pagerAdapter = new ContentPagerAdapter(SupportFragmentManager, this);
            var contactsFragment = new ContactsFragment();
            var keypadFragment = new KeypadFragment();
            var messagesFragment = new MessagesFragment();
            var recentsFragment = new RecentsFragment();
            if (_viewPager != null)
            {
                _viewPager.AllowSwipe = false;
                _viewPager.OffscreenPageLimit = 4;
                _pagerAdapter.AddFragment(recentsFragment, Resource.String.FragmentRecents_title, Resource.Drawable.ic_tab_history);
                _pagerAdapter.AddFragment(contactsFragment, Resource.String.FragmentContacts_title, Resource.Drawable.ic_tab_contacts);
                _pagerAdapter.AddFragment(keypadFragment, Resource.String.FragmentKeypad_title, Resource.Drawable.ic_tab_keypad);
                _pagerAdapter.AddFragment(messagesFragment, Resource.String.FragmentMessages_title, Resource.Drawable.ic_tab_messages);
                _viewPager.Adapter = _pagerAdapter;
                _viewPager.CurrentItem = 3;
            }
            _tabLayout.SetupWithViewPager(_viewPager);
        }

        public Spinner GetToolbarSpinner()
        {
            return _toolbarSpinner;
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
                    Helper.Logout();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
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