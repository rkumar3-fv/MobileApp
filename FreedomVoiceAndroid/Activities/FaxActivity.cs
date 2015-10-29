using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with text content details
    /// </summary>
    [Activity(
        Label = "@string/ActivityFax_title",
        Theme = "@style/AppThemeActionBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class FaxActivity : MessageDetailsActivity
    {
        private Button _openFaxButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_fax);
            RootLayout = FindViewById(Resource.Id.faxActivity_root);
            RemoveButton = FindViewById<ImageButton>(Resource.Id.faxActivity_deleteButton);
            SenderText = FindViewById<TextView>(Resource.Id.faxActivity_senderText);
            MessageDate = FindViewById<TextView>(Resource.Id.faxActivity_dateText);
            MessageStamp = FindViewById<TextView>(Resource.Id.faxActivity_stampText);
            _openFaxButton = FindViewById<Button>(Resource.Id.faxActivity_openFax);
            _openFaxButton.Click += OpenFaxButtonOnClick;
            SupportActionBar.SetTitle(Resource.String.ActivityFax_title);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (Msg.Length < 1)
                _openFaxButton.Activated = false;
        }

        /// <summary>
        /// Open fax action
        /// </summary>
        private void OpenFaxButtonOnClick(object sender, EventArgs eventArgs)
        {
            
        }

        protected override void OnResume()
        {
            base.OnResume();
            _openFaxButton.Activated = Msg.Length != 0;
            MessageStamp.Text = Msg.Length == 1 ? GetString(Resource.String.FragmentMessages_onePage) : $"{Msg.Length} {GetString(Resource.String.FragmentMessages_morePage)}";
        }
    }
}