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
        private string _soundPath;
        private int _currentPlayPosition;
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
            PlayerButton.Click += PlayerButtonOnClick;
        }

        private void PlayerButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(_soundPath))
                AttachmentId = AppHelper.Instance(this).AttachmentsHelper.LoadAttachment(Msg);
            else
                Play();
        }

        protected override void OnResume()
        {
            base.OnResume();
            MessageStamp.Text = DataFormatUtils.ToDuration(Msg.Length);
            if (!string.IsNullOrEmpty(_soundPath))
            {
                PlayerSeek.Enabled = true;
            }
            else
            {
                PlayerSeek.Enabled = false;
            }
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

        private void Play()
        {
            
        }

        private void Pause()
        {
            
        }

        protected override void AttachmentsHelperOnProgressLoading(object sender, AttachmentHelperEventArgs<int> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnProgressLoading(sender, args);
            if (PlayerButton.Enabled)
                PlayerButton.Enabled = false;
        }

        protected override void AttachmentsHelperOnFailLoadingEvent(object sender, AttachmentHelperEventArgs<bool> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnFailLoadingEvent(sender, args);
            if (!PlayerButton.Enabled)
                PlayerButton.Enabled = true;
        }

        protected override void AttachmentsHelperOnFinishLoading(object sender, AttachmentHelperEventArgs<string> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnFinishLoading(sender, args);
            if (!PlayerButton.Enabled)
                PlayerButton.Enabled = true;
            _soundPath = args.Result;
            if (!PlayerSeek.Enabled)
                PlayerSeek.Enabled = true;
            Play();
        }
    }
}