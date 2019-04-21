using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core.Presenters;
using FreedomVoice.Core.Services;
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


        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_conversations, null, false);
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
                ChatActivity.Start(Activity, account.ConversationId, account.Collocutor);
            });
            _layoutManager = new LinearLayoutManager(Context);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            _recyclerView.SetAdapter(_adapter);
            _recyclerView.ScrollChange += (sender, args) => { onListScrolled(); };

            var provider = ServiceContainer.Resolve<IContactNameProvider>();
            provider.ContactsUpdated += ProviderOnContactsUpdated;
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);

            foreach (var code in args.Codes)
            {
                if (code != ActionsHelperEventArgs.ChangePresentation) continue;

                _presenter = new ConversationsPresenter() { PhoneNumber = this.Helper?.SelectedAccount?.PresentationNumber };
                _presenter.ItemsChanged += (sender, e) =>
                {
                    UpdateList(_presenter.Items);
                };

                _presenter.ReloadAsync();
            }
        }

        private void ProviderOnContactsUpdated(object sender, EventArgs e)
        {
            UpdateList(_presenter.Items);
        }

        private void onListScrolled()
        {
            var visibleItemCount = _layoutManager.ChildCount;

            var pastVisiblesItems = _layoutManager.FindLastVisibleItemPosition();
            if (visibleItemCount + pastVisiblesItems + 8 >= _presenter.Items.Count && _presenter.HasMore)
            {
                _presenter.LoadMoreAsync();
            }
        }

        private void UpdateList(List<ConversationViewModel> newList)
        {
            var isEmpty = newList == null || newList.Count == 0;
            _noResultText.Visibility = isEmpty ? ViewStates.Visible : ViewStates.Gone;
            _recyclerView.Visibility = isEmpty ? ViewStates.Gone : ViewStates.Visible;
            _adapter.Update(newList);
        }
    }
}