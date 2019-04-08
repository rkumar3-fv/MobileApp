﻿using System;
using System.Collections.Generic;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    public class ConversationsFragment : CallerFragment
    {
        private RecyclerView _recyclerView;
        private ConversationRecyclerAdapter _adapter;
        private TextView _noResultText;
        private LinearLayoutManager _layoutManager;
        private int TotalItemCount => 20; // todo get from vm.TotalItemsCount (items count all pages)


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
            _adapter = new ConversationRecyclerAdapter(((sender, account) =>
            {
                /* todo delegate to vm.onClickItem */
            }));
            _layoutManager = new LinearLayoutManager(Context);
            _recyclerView.SetLayoutManager(_layoutManager);
            _recyclerView.AddItemDecoration(new DividerItemDecorator(Context));
            _recyclerView.SetAdapter(_adapter);
            _recyclerView.ScrollChange += (sender, args) => { onListScrolled(); };
            UpdateList(new List<Account>());
        }

        private void onListScrolled()
        {
            var visibleItemCount = _layoutManager.ChildCount;
            var totalItemCount = TotalItemCount;
            var pastVisiblesItems = _layoutManager.FindLastVisibleItemPosition();
            if ((visibleItemCount + pastVisiblesItems) >= totalItemCount) 
            {
                // todo call vm.LoadNextPage
            }
        }

        private void UpdateList(List<Account> newList)
        {
            var isEmpty = newList == null || newList.Count == 0;
            _noResultText.Visibility = isEmpty ? ViewStates.Visible : ViewStates.Gone;
            _recyclerView.Visibility = isEmpty ? ViewStates.Gone : ViewStates.Visible;
            _adapter.Update(newList);
        }


        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            //throw new NotImplementedException();
        }
    }
}