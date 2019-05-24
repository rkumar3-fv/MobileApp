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
using com.FreedomVoice.MobileApp.Android.Utils;
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
        private const string ExtraConversationId = "EXTRA_CONVERSATION_ID";
        private const string ExtraConversationPhone = "EXTRA_CONVERSATION_PHONE";

        protected EditText _messageEt;
        protected ImageView _sendIv;
        protected RecyclerView _recycler;
        protected TextView _placeHolder;
        protected ActionBar Toolbar;
        protected ViewGroup _spinnerContainer;

        protected ViewGroup _selectContactContainer;
        protected ImageView _contactsIcon;
        protected EditText _contactPhoneEt;

        protected ConversationPresenter _presenter = new ConversationPresenter();
        protected ConversationMessageRecyclerAdapter _adapter;
        protected LinearLayoutManager _manager;

        protected string ConversationPhone;
        protected long? ConversationId;
        protected ProgressBar _progressBar;


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
            _spinnerContainer = view.FindViewById<ViewGroup>(Resource.Id.conversationDetailsFragment_spinnerArea);
            IdSpinner = view.FindViewById<Spinner>(Resource.Id.conversationDetailsFragment_idSpinner);
            SingleId = view.FindViewById<TextView>(Resource.Id.conversationDetailsFragment_singleId);
            _selectContactContainer = view.FindViewById<ViewGroup>(Resource.Id.conversationDetailsFragment_select_phone);
            _contactsIcon = view.FindViewById<ImageView>(Resource.Id.conversationDetailsFragment_iv_select_contact);
            _contactPhoneEt = view.FindViewById<EditText>(Resource.Id.conversationDetailsFragment_et_contact_phone);
            _progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar);
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            Toolbar = ((AppCompatActivity) Activity).SupportActionBar;
            Toolbar.SetDisplayHomeAsUpEnabled(true);
            Toolbar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SetTitle(null);

            ConversationPhone = Arguments?.GetString(ExtraConversationPhone);
            var convId = Arguments?.GetLong(ExtraConversationId, -1);
            ConversationId = convId == -1 ? null : convId;
            
            ConversationSelected();
        }

        
        protected void SetTitle(string title)
        {
            if (title == null || title.Length <= 0)
            {
                Toolbar.Title = Context.GetString(Resource.String.ConversationDetails_new_message);
            }
            else
            {
                Toolbar.Title = title;
            }
        }
       
        protected void ConversationSelected()
        {
            if (ConversationPhone != null && ConversationId != null)
            {
                Toolbar.SetIcon(Resource.Drawable.ic_account_white);
                Toolbar.SetDisplayShowHomeEnabled(true);
                ContactsHelper.Instance(Context).GetName(ConversationPhone, out var name);
                Toolbar.Title = string.IsNullOrEmpty(name) ? ConversationPhone : name;
                _spinnerContainer.Visibility = ViewStates.Visible;
                _selectContactContainer.Visibility = ViewStates.Gone;

                _presenter.ConversationId = ConversationId.Value;
                _presenter.PhoneNumber = Helper.SelectedAccount?.PresentationNumber;

                _progressBar.Visibility = ViewStates.Visible;
                _presenter.ReloadAsync();
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            _contactsIcon.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.colorBlack)),
                PorterDuff.Mode.SrcAtop);
            
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

            var pastVisibleItems = _manager.FindLastVisibleItemPosition();
            if (visibleItemCount + pastVisibleItems + 15 >= _presenter.Items.Count && _presenter.HasMore)
            {
                _presenter.LoadMoreAsync();
            }
        }

        private void ItemsChanged(object sender, EventArgs e) => UpdateList();

        private void MessageTextChanged(object sender, TextChangedEventArgs args) => UpdateSendButton();

        private void ClickSend(object sender, EventArgs e)
        {
            SendMessage();
            _messageEt.SetText("", TextView.BufferType.Editable);
        }

        protected virtual async void SendMessage()
        {
            _progressBar.Visibility = ViewStates.Visible;
            await _presenter.SendMessageAsync(_messageEt.Text);
        }

        protected void UpdateSendButton()
        {
            var buttonEnabled = IsSendButtonEnabled();

            var btnColor = buttonEnabled
                ? Resource.Color.colorActivatedControls
                : Resource.Color.colorFieldsHint;
            _sendIv.Enabled = buttonEnabled;
            _sendIv.SetColorFilter(
                new Color(ContextCompat.GetColor(Context, btnColor)),
                PorterDuff.Mode.SrcAtop
            );
        }

        protected virtual bool IsSendButtonEnabled()
        {
            return _messageEt.Text.Length > 0;
        }

        private void UpdateList()
        {
            Activity.RunOnUiThread(() =>
            {
                _progressBar.Visibility = ViewStates.Gone;
                _adapter.UpdateItems(_presenter.Items);
                _adapter.NotifyDataSetChanged();
            });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _presenter?.Dispose();
        }
    }
}
