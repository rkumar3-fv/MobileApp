using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.CustomControls.Callbacks;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Fragments;
using SearchView = Android.Support.V7.Widget.SearchView;
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
        WindowSoftInputMode = SoftInput.AdjustPan)]
    public class ContentActivity : OperationActivity
    {
        private Color _whiteColor;
        private Color _grayColor;
        private CoordinatorLayout _rootLayout;
        private AppBarLayout _appBar;
        private ContentPagerAdapter _pagerAdapter;
        private ContentPager _viewPager;
        private Toolbar _toolbar;
        private TabLayout _tabLayout;
        private string _request;

        /// <summary>
        /// Contacts search listener
        /// </summary>
        public SearchViewListener SearchListener { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_content);
            _rootLayout = FindViewById<CoordinatorLayout>(Resource.Id.contentActivity_rootBar);
            RootLayout = _rootLayout;
            _whiteColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorActionBarText));
            _grayColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorTabIndicatorInactive));
            SearchListener = new SearchViewListener();
            SearchListener.OnCollapse += SearchListenerOnCollapse;
            SearchListener.OnExpand += SearchListenerOnExpand;
            _appBar = FindViewById<AppBarLayout>(Resource.Id.contentActivity_contentAppBar);
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

        protected override void OnStart()
        {
            base.OnStart();
            Window.SetSoftInputMode(SoftInput.StateHidden | SoftInput.AdjustPan);
        }

        protected override void OnResumeFragments()
        {
            base.OnResumeFragments();
            SetToolbarContent();
            if (_request != null)
            {
                var menu = _toolbar.Menu;
                var item = menu?.FindItem(Resource.Id.menu_action_search);
                if (item != null)
                {
                    var view = MenuItemCompat.GetActionView(item);
                    var searchView = view?.JavaCast<SearchView>();
                    if (searchView != null)
                    {
                        item.ExpandActionView();
                        searchView.SetQuery(_request, false);
                        return;
                    }
                }
            }
            var param = _toolbar.LayoutParameters.JavaCast<AppBarLayout.LayoutParams>();
            param.ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll | AppBarLayout.LayoutParams.ScrollFlagEnterAlways;
            _tabLayout.Visibility = ViewStates.Visible;
        }

        protected override void OnPause()
        {
            base.OnPause();
            var menu = _toolbar.Menu;
            var item = menu?.FindItem(Resource.Id.menu_action_search);
            if (item != null)
            {
                var view = MenuItemCompat.GetActionView(item);
                var searchView = view?.JavaCast<SearchView>();
                if ((searchView != null) && (!searchView.Iconified))
                {
                    _request = searchView.Query;
                    return;
                }
            }
            _request = null;
        }

        private void ViewPagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs pageSelectedEventArgs)
        {
            SetToolbarContent();
            if (!Appl.ApplicationHelper.IsGoogleAnalyticsOn) return;
            Appl.ApplicationHelper.AnalyticsTracker.SetScreenName(
                    $"Activity {GetType().Name}, Screen {_pagerAdapter.GetItem(_viewPager.CurrentItem).GetType().Name}");
            Appl.ApplicationHelper.AnalyticsTracker.Send(new HitBuilders.ScreenViewBuilder().Build());
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
                    ExpandToolbar();
                    return;
                }
                if (Helper.SelectedExtension != -1)
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
                    var menu = _toolbar.Menu;
                    var searchView = MenuItemCompat.GetActionView(menu.FindItem(Resource.Id.menu_action_search)).JavaCast<SearchView>();
                    var menuItem = menu.FindItem(Resource.Id.menu_action_search);
                    if ((menuItem != null) && (searchView != null))
                    {
                        MenuItemCompat.SetOnActionExpandListener(menuItem, SearchListener);
                        MenuItemCompat.SetActionView(menuItem, searchView);
                        searchView.SetOnQueryTextListener(SearchListener);
                        var editText = searchView.FindViewById<EditText>(Resource.Id.search_src_text);
                        editText.SetTextColor(_whiteColor);
                        editText.SetHintTextColor(_grayColor);
                    }
                    break;
                case 2:
                    _toolbar.InflateMenu(Resource.Menu.menu_content);
                    ExpandToolbar();
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
                    if (Appl.ApplicationHelper.IsVoicecallsSupported() == false)
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
                    var clearDialog = new ClearRecentsDialogFragment();
                    clearDialog.DialogEvent += ClearDialogEvent;
                    clearDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgLogout_title));
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        /// <summary>
        /// Expand scrollable toolbar
        /// </summary>
        public void ExpandToolbar()
        {
            var param = _appBar.LayoutParameters.JavaCast<CoordinatorLayout.LayoutParams>();
            var behavior = param.Behavior.JavaCast<AppBarLayout.Behavior>();
            behavior?.OnNestedFling(_rootLayout, _appBar, null, 0, -10000, false);
        }

        /// <summary>
        /// Collapse scrollable toolbar
        /// </summary>
        public void CollapseToolbar()
        {
            var param = _appBar.LayoutParameters.JavaCast<CoordinatorLayout.LayoutParams>();
            var behavior = param.Behavior.JavaCast<AppBarLayout.Behavior>();
            behavior?.OnNestedFling(_rootLayout, _appBar, null, 0, 10000, false);
        }

        /// <summary>
        /// Confirm recents clearing
        /// </summary>
        private void ClearDialogEvent(object sender, DialogEventArgs args)
        {
            if (args.Result == DialogResult.Ok)
                Helper.ClearAllRecents();
        }

        /// <summary>
        /// Search bar opening event
        /// </summary>
        private void SearchListenerOnExpand(object sender, bool b)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            var param = _toolbar.LayoutParameters.JavaCast<AppBarLayout.LayoutParams>();
            param.ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagExitUntilCollapsed;
            _tabLayout.Visibility = ViewStates.Gone;
        }

        /// <summary>
        /// Search bar closing
        /// </summary>
        private void SearchListenerOnCollapse(object sender, bool b)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);
            var param = _toolbar.LayoutParameters.JavaCast<AppBarLayout.LayoutParams>();
            param.ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll | AppBarLayout.LayoutParams.ScrollFlagEnterAlways;
            _tabLayout.Visibility = ViewStates.Visible;
        }

        public override void OnBackPressed()
        {
            if ((_viewPager.CurrentItem == 3) && (Helper.SelectedExtension != -1))
            {
                Helper.GetPrevious();
                SetToolbarContent();
                return;
            }
            var menu = _toolbar.Menu;
            var item = menu?.FindItem(Resource.Id.menu_action_search);
            if (item != null)
            {
                var view = MenuItemCompat.GetActionView(item);
                var searchView = view?.JavaCast<SearchView>();
                if ((searchView != null) && (!searchView.Iconified))
                {
                    searchView.Iconified = true;
                    return;
                }
            }
            MoveTaskToBack(true);
        }
    }
}