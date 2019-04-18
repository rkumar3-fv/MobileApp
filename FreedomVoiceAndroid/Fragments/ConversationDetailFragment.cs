using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using FreedomVoice.Entities;
using Message = FreedomVoice.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    public class ConversationDetailFragment : CallerFragment
    {
        private EditText _messageEt;
        private ImageView _sendIv;
        private RecyclerView _recycler;
        private TextView _placeHolder;
        private Spinner _spinner;
        private TextView _singleId;
        private ConversationMessageRecyclerAdapter _adapter;

        private static string EXTRA_CONVERSATION_ID = "EXTRA_CONVERSATION_ID";
        public static ConversationDetailFragment newInstance(int conversationId)
        {
            var fragment = new ConversationDetailFragment();
            var args = new Bundle();
            args.PutInt(EXTRA_CONVERSATION_ID, conversationId);
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            UpdateSendButton();

            var manager = new LinearLayoutManager(Context);
            manager.StackFromEnd = true;
            _recycler.SetLayoutManager(manager);
            _adapter = new ConversationMessageRecyclerAdapter(3);
            _recycler.SetAdapter(_adapter);
            _adapter.UpdateItems(new List<Message>()
            {
                new Message()
                {
                    Id = 1,
                    Text = "Message alskdfj ajffa alkdfj alsdkfj aaaskdfj laskdfj skdfj lskd jskdfj lfj",
                    To = new Phone() {Id = 1, PhoneNumber = "123123"},
                    From = new Phone() {Id = 3, PhoneNumber = "111111111"},
                    ReadAt = DateTime.Now,
                    ReceivedAt = DateTime.Now,
                    SentAt = DateTime.Now
                },
                new Message()
                {
                    Id = 2,
                    Text = "Message",
                    To = new Phone() {Id = 1, PhoneNumber = "123123"},
                    From = new Phone() {Id = 2, PhoneNumber = "123"},
                    ReadAt = DateTime.Now,
                    ReceivedAt = DateTime.Now,
                    SentAt = DateTime.Now
                }
            });
        }

        public override void OnResume()
        {
            base.OnResume();
            _messageEt.TextChanged += OnMessageTextChanged;
            _sendIv.Click += OnClickSend;
        }

        public override void OnPause()
        {
            base.OnPause();
            _messageEt.TextChanged -= OnMessageTextChanged;
            _sendIv.Click -= OnClickSend;
        }

        private void OnClickSend(object sender, EventArgs e)
        {
            // todo call vm.SendMessage(_messageEt.Text)
            _messageEt.SetText("", TextView.BufferType.Editable);
        }

        private void OnMessageTextChanged(Object sender, TextChangedEventArgs args) => UpdateSendButton();

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