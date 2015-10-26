using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Dialogs;
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
        Icon = "@drawable/ic_launcher",
        Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Portrait, 
        WindowSoftInputMode = SoftInput.StateHidden)]
    public class ContentActivity : LogoutActivity
    {
        private ContentPagerAdapter _pagerAdapter;
        private ContentPager _viewPager;
        private Toolbar _toolbar;
        private TabLayout _tabLayout;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_content);
            _tabLayout = FindViewById<TabLayout>(Resource.Id.contentActivity_tabs);
            _viewPager = FindViewById<ContentPager>(Resource.Id.contentActivity_contentPager);
            _toolbar = FindViewById<Toolbar>(Resource.Id.contentActivity_toolbar);
            SetSupportActionBar(_toolbar);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            _pagerAdapter = new ContentPagerAdapter(SupportFragmentManager, this);
            var contactsFragment = new ContactsFragment();
            var keypadFragment = new KeypadFragment();
            var messagesFragment = new MessagesFragment();
            var recentsFragment = new RecentsFragment();
            if (_viewPager != null)
            {
                _viewPager.AllowSwipe = false;
                _viewPager.OffscreenPageLimit = 1;
                _pagerAdapter.AddFragment(recentsFragment, Resource.String.FragmentRecents_title, Resource.Drawable.ic_tab_history);
                _pagerAdapter.AddFragment(contactsFragment, Resource.String.FragmentContacts_title, Resource.Drawable.ic_tab_contacts);
                _pagerAdapter.AddFragment(keypadFragment, Resource.String.FragmentKeypad_title, Resource.Drawable.ic_tab_keypad);
                _pagerAdapter.AddFragment(messagesFragment, Resource.String.FragmentMessages_title, Resource.Drawable.ic_tab_messages);
                _viewPager.Adapter = _pagerAdapter;
                _viewPager.CurrentItem = 2;
                _viewPager.PageSelected += ViewPagerOnPageSelected;
            }
            _tabLayout.SetupWithViewPager(_viewPager);
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetToolbarContent();
        }

        private void ViewPagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs pageSelectedEventArgs)
        {
            SetToolbarContent();
            Appl.AnalyticsTracker.SetScreenName($"Activity {GetType().Name}, Screen {_pagerAdapter.GetItem(_viewPager.CurrentItem).GetType().Name}");
            Appl.AnalyticsTracker.Send(new HitBuilders.ScreenViewBuilder().Build());
        }

        public void SetToolbarContent()
        {
            _toolbar.Menu.Clear();
            if (_viewPager.CurrentItem == 3)
            {
                _toolbar.InflateMenu(Resource.Menu.menu_content);
                if (Helper.SelectedFolder != -1)
                {
                    SupportActionBar.Title =
                        Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].FolderName;
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    return;
                }
                else if (Helper.SelectedExtension != -1)
                {
                    SupportActionBar.Title =
                        $"{Helper.ExtensionsList[Helper.SelectedExtension].Id} - {Helper.ExtensionsList[Helper.SelectedExtension].ExtensionName}";
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    return;
                }
            }
            SupportActionBar.Title = _pagerAdapter.GetTabName(_viewPager.CurrentItem);
            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);
            switch (_viewPager.CurrentItem)
            {
                case 0:
                    _toolbar.InflateMenu(Resource.Menu.menu_recents);
                    break;
                case 1:
                    _toolbar.InflateMenu(Resource.Menu.menu_contacts);
                    break;
                case 2:
                    _toolbar.InflateMenu(Resource.Menu.menu_content);
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = new SupportMenuInflater(this);
            inflater.Inflate(Resource.Menu.menu_content, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case global::Android.Resource.Id.Home:
                    Helper.GetPrevious();
                    SetToolbarContent();
                    return true;
                case Resource.Id.menu_action_phone:
                    if (Helper.PhoneNumber == null)
                    {
                        var noCellularDialog = new NoCellularDialogFragment();
                        noCellularDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgCellular_title));
                    }
                    else
                    {
                        var intent = new Intent(this, typeof(SetNumberActivityWithBack));
                        StartActivity(intent);
                    }
                    return true;
                case Resource.Id.menu_action_logout:
                    LogoutAction();
                    return true;
                case Resource.Id.menu_action_clear:
                    Helper.ClearAllRecents();
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
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.ConnectionLostError:
                        Toast.MakeText(this, Resource.String.Snack_connectionLost, ToastLength.Long).Show();
                        return;
                }
            }
        }
    }
}