using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using FreedomVoice.Core.Presenters;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.DAL.DbEntities;
using Message = FreedomVoice.DAL.DbEntities.Message;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    public class ConversationDetailFragment : CallerFragment
    {
        private EditText _messageEt;
        private ImageView _sendIv;
        private RecyclerView _recycler;
        private TextView _placeHolder;
        private ConversationMessageRecyclerAdapter _adapter;

        private const string ExtraConversationId = "EXTRA_CONVERSATION_ID";
        private const string ExtraConversationPhone = "EXTRA_CONVERSATION_PHONE";
        private ConversationPresenter _presenter;
        private LinearLayoutManager _manager;

        public static ConversationDetailFragment NewInstance(long conversationId, string phone)
        {
            var fragment = new ConversationDetailFragment();
            var args = new Bundle();
            args.PutLong(ExtraConversationId, conversationId);
            args.PutString(ExtraConversationPhone, phone);
            fragment.Arguments = args;
            
            return fragment;
        }

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_conversation_details, null, false);
            _messageEt = view.FindViewById<EditText>(Resource.Id.conversationDetailsFragment_et_message);
            _sendIv = view.FindViewById<ImageView>(Resource.Id.conversationDetailsFragment_iv_send);
            _recycler = view.FindViewById<RecyclerView>(Resource.Id.conversationDetailsFragment_recyclerView);
            _placeHolder = view.FindViewById<TextView>(Resource.Id.conversationDetailsFragment_noResultText);
            IdSpinner = view.FindViewById<Spinner>(Resource.Id.conversationDetailsFragment_idSpinner);
            SingleId = view.FindViewById<TextView>(Resource.Id.conversationDetailsFragment_singleId);
            return view;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _presenter = new ConversationPresenter()
            {
                ConversationId = Arguments.GetLong(ExtraConversationId)
            };
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var bar = ((AppCompatActivity) Activity).SupportActionBar;
            bar.SetIcon(Resource.Drawable.ic_account_white);
            bar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            bar.SetDisplayShowHomeEnabled(true);
            bar.SetDisplayHomeAsUpEnabled(true);
            bar.Title = Arguments.GetString(ExtraConversationPhone);
            _presenter.PhoneNumber = Helper.SelectedAccount.PresentationNumber;
            _presenter.ReloadAsync();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            UpdateSendButton();

            _manager = new LinearLayoutManager(Context);
            _manager.ReverseLayout = true;
            _recycler.SetLayoutManager(_manager);
            _adapter = new ConversationMessageRecyclerAdapter();
            _recycler.SetAdapter(_adapter);
        }

        public override void OnResume()
        {
            base.OnResume();
            _messageEt.TextChanged += MessageTextChanged;
            _sendIv.Click += ClickSend;
            _presenter.ItemsChanged += ItemsChanged;
            _recycler.ScrollChange += ScrollChanged;
            UpdateList();
        }

        public override void OnPause()
        {
            base.OnPause();
            _messageEt.TextChanged -= MessageTextChanged;
            _sendIv.Click -= ClickSend;
            _presenter.ItemsChanged -= ItemsChanged;
            _recycler.ScrollChange -= ScrollChanged;
        }

        private void ScrollChanged(object sender, View.ScrollChangeEventArgs e)
        {
            var visibleItemCount = _manager.ChildCount;

            var pastVisiblesItems = _manager.FindLastVisibleItemPosition();
            if (visibleItemCount + pastVisiblesItems + 15 >= _presenter.Items.Count && _presenter.HasMore)
            {
                _presenter.LoadMoreAsync();
            }
        }

        private void ItemsChanged(object sender, EventArgs e) => UpdateList();

        private void MessageTextChanged(object sender, TextChangedEventArgs args) => UpdateSendButton();

        private void ClickSend(object sender, EventArgs e)
        {
            // todo call vm.SendMessage(_messageEt.Text)
            _messageEt.SetText("", TextView.BufferType.Editable);
        }

        private void UpdateList()
        {
            _adapter.UpdateItems(_presenter.Items);
            _adapter.NotifyDataSetChanged();
        }

        private void UpdateSendButton()
        {
            var btnColor = _messageEt.Text.Length > 0
                ? Resource.Color.colorActivatedControls
                : Resource.Color.colorFieldsHint;
            _sendIv.SetColorFilter(
                new Color(ContextCompat.GetColor(Context, btnColor)),
                PorterDuff.Mode.SrcAtop
            );
        }
    }
}