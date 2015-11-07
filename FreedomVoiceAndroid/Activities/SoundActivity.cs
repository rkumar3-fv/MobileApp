using System;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with audio content details
    /// </summary>
    public abstract class SoundActivity : MessageDetailsActivity
    {
        protected ImageButton PlayerButton;
        protected TextView StarTextView;
        protected TextView EndTextView;
        protected Button SpeakerButton;
        protected Button CallBackButton;
        protected SeekBar PlayerSeek;

        protected override void OnStart()
        {
            base.OnStart();
            SpeakerButton.Click += SpeakerButtonOnClick;
            CallBackButton.Click += CallBackButtonOnClick;
            PlayerSeek.Activated = false;
        }

        protected override void OnResume()
        {
            base.OnResume();
            MessageStamp.Text = DataFormatUtils.ToDuration(Msg.Length);
        }

        /// <summary>
        /// Changing output sound device
        /// </summary>
        private void SpeakerButtonOnClick(object sender, EventArgs eventArgs)
        {

        }

        /// <summary>
        /// Call back action
        /// </summary>
        private void CallBackButtonOnClick(object sender, EventArgs eventArgs)
        {
            Call(Msg.FromNumber);
        }

        protected override void AttachmentsHelperOnProgressLoading(object sender, AttachmentHelperEventArgs<int> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnProgressLoading(sender, args);
            if (PlayerButton.Activated)
                PlayerButton.Activated = false;
        }

        protected override void AttachmentsHelperOnFailLoadingEvent(object sender, AttachmentHelperEventArgs<bool> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnFailLoadingEvent(sender, args);
            if (!PlayerButton.Activated)
                PlayerButton.Activated = true;
        }
    }
}