using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core.ViewModels;
using Java.Util.Zip;
using ActionBar = Android.App.ActionBar;
using Fragment = Android.Support.V4.App.Fragment;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    public class NewConversationDetailFragment : ConversationDetailFragment
    {
        private static int PICK_PHONE = 20009;

        public static NewConversationDetailFragment NewInstance()
        {
            return new NewConversationDetailFragment();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            _spinnerContainer.Visibility = ViewStates.Gone;
            _selectContactContainer.Visibility = ViewStates.Visible;
        }

        public override void OnResume()
        {
            base.OnResume();
            _contactsIcon.Click += ClickSelectContact;
            _contactPhoneEt.TextChanged += ContactPhoneChanged;
        }

        public override void OnPause()
        {
            base.OnPause();
            _contactsIcon.Click -= ClickSelectContact;
            _contactPhoneEt.TextChanged -= ContactPhoneChanged;
        }

        private async void ContactPhoneChanged(object sender, TextChangedEventArgs e)
        {
            ContactsHelper.Instance(Context).GetName(e.Text.ToString(), out var name);
            SetTitle(name);

            _progressBar.Visibility = ViewStates.Visible;
            
            var convId =
                await _presenter.GetConversationId(Helper.SelectedAccount.PresentationNumber, e.Text.ToString());

            if (convId.HasValue)
            {
                ConversationId = convId;
                _presenter.PhoneNumber = Helper.SelectedAccount.PresentationNumber;
                _presenter.ConversationId = ConversationId;
                _presenter.ReloadAsync();
            }
            else
            {
                _presenter.Clear();
            }

            UpdateSendButton();
        }

        protected override async void SendMessage()
        {
            _progressBar.Visibility = ViewStates.Visible;
            
            if (ConversationId == null)
            {
                ConversationPhone = _contactPhoneEt.Text;
                var convId = await _presenter.SendMessage(
                    Helper.SelectedAccount.PresentationNumber,
                    ConversationPhone,
                    _messageEt.Text
                );
    
                ConversationId = convId.Value;
                ConversationSelected();
            }
            else
            {
                base.SendMessage();
            }
        }

        protected override bool IsSendButtonEnabled()
        {
            return base.IsSendButtonEnabled() && _contactPhoneEt.Text.Length > 0;
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == PICK_PHONE && resultCode == (int) Result.Ok)
            {
                var cursor = Context.ContentResolver.Query(
                    data.Data,
                    new string[] {ContactsContract.CommonDataKinds.Phone.Number},
                    null, null);

                if (cursor != null && cursor.MoveToFirst())
                {
                    var phoneIndex = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);
                    var phone = cursor.GetString(phoneIndex);
                    cursor.Close();

                    _contactPhoneEt.Text = phone;
                    ContactPhoneChanged(this, new TextChangedEventArgs(phone, 0, 0, 0));
                    _contactPhoneEt.SetSelection(_contactPhoneEt.Text.Length);
                }
            }
        }

        private void ClickSelectContact(object sender, EventArgs e)
        {
            Intent i = new Intent(Intent.ActionPick, ContactsContract.CommonDataKinds.Phone.ContentUri);
            StartActivityForResult(i, PICK_PHONE);
        }
    }
}