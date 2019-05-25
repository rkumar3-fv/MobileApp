using System;
using System.Collections.Generic;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Presenters;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    public class ConversationsFragment : CallerFragment
    {
        private RecyclerView _recyclerView;
        private ConversationRecyclerAdapter _adapter;
        private TextView _noResultText;
        private LinearLayoutManager _layoutManager;
        private ConversationsPresenter _presenter;
        private IContactNameProvider _contactNameProvider;
        private SwipeRefreshLayout _swipeToRefresh;

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_conversations, null, false);
            _swipeToRefresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.conversationFragment_swipeRefresh);
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.conversationFragment_recyclerView);
            _noResultText = view.FindViewById<TextView>(Resource.Id.conversationFragment_noResultText);
            IdSpinner = view.FindViewById<Spinner>(Resource.Id.contatnsFragment_idSpinner);
            SingleId = view.FindViewById<TextView>(Resource.Id.contactsFragment_singleId);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            _adapter = new ConversationRecyclerAdapter((sender, account) =>
            {
                StartActivity(ChatActivity.OpenChat(Activity, account.ConversationId, account.To));
            });
            _layoutManager = new LinearLayoutManager(Context);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            _recyclerView.SetAdapter(_adapter);
            _recyclerView.NestedScrollingEnabled = false;
            _recyclerView.ScrollChange += (sender, args) => { ListScrolled(); };
            _swipeToRefresh.Refresh += SwipeRefresh;
            _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);

            foreach (var code in args.Codes)
            {
                if (code != ActionsHelperEventArgs.ChangePresentation) continue;

                _presenter = new ConversationsPresenter()
                {
                    PhoneNumber = Helper?.SelectedAccount?.PresentationNumber,
                    AccountNumber = Helper?.SelectedAccount?.AccountName
                };
                _presenter.ItemsChanged += UpdateList;
                _presenter.ReloadAsync();
            }
        }
        
        public override void OnResume()
        {
            base.OnResume();
            if (_presenter != null)_presenter.ItemsChanged += UpdateList;
            ContentActivity.SearchListener.OnChange += SearchListenerOnChange;
            ContentActivity.SearchListener.OnApply += SearchListenerOnApply;
            ContentActivity.SearchListener.OnCollapse += SearchListenerOnCancel;
            _contactNameProvider.ContactsUpdated += ProviderOnContactsUpdated;
            _contactNameProvider.ContactsUpdated += ProviderOnContactsUpdated;
            if (_presenter != null) UpdateList(null, null);
        }      

        public override void OnPause()
        {
            base.OnPause();
            if (_presenter != null)_presenter.ItemsChanged -= UpdateList;
            ContentActivity.SearchListener.OnChange -= SearchListenerOnChange;
            ContentActivity.SearchListener.OnApply -= SearchListenerOnApply;
            ContentActivity.SearchListener.OnCollapse -= SearchListenerOnCancel;
            _contactNameProvider.ContactsUpdated -= ProviderOnContactsUpdated;
            _contactNameProvider.ContactsUpdated -= ProviderOnContactsUpdated;
        }

        private void ProviderOnContactsUpdated(object sender, EventArgs e)
        {
            UpdateList(null, null);
        }

        private void SwipeRefresh(object sender, EventArgs e)
        {
            _presenter.ReloadAsync();
        }

        private void ListScrolled()
        {
            const int countItemsToTheEndList = 8;
            var visibleItemCount = _layoutManager.ChildCount;
            var pastVisibleItems = _layoutManager.FindLastVisibleItemPosition();
            
            if (visibleItemCount + pastVisibleItems + countItemsToTheEndList >= _presenter.Items.Count && _presenter.HasMore)
            {
                _presenter.LoadMoreAsync();
            }
        }

        private void UpdateList(object sender, EventArgs eventArgs)
        {
            Activity?.RunOnUiThread(() =>
            {
                _swipeToRefresh.Refreshing = false;
                var newList = _presenter.Items;
                var isEmpty = newList == null || newList.Count == 0;
                _noResultText.Visibility = isEmpty ? ViewStates.Visible : ViewStates.Gone;
                _recyclerView.Visibility = isEmpty ? ViewStates.Gone : ViewStates.Visible;
                _adapter.Update(newList);
                _swipeToRefresh.Refreshing = false;
            });
        }
        
        private void SearchListenerOnCancel(object sender, bool b)
        {
            _presenter.Query = null;
            _presenter.ReloadAsync();
        }

        private void SearchListenerOnApply(object sender, string s)
        {
            _presenter.Query = s;
            _presenter.ReloadAsync();
        }

        private void SearchListenerOnChange(object sender, string s)
        {
            _presenter.Query = s;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _presenter?.Dispose();
        }
    }
}
