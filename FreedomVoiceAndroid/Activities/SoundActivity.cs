using System;
using System.Timers;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
#if DEBUG
using Android.Util;
#endif
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Services;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with audio content details
    /// </summary>
    public abstract class SoundActivity : MessageDetailsActivity, IServiceConnection
    {
        private const long FireTime = 1000000;
        private bool _isBinded;
        private MediaServiceBinder _serviceBinder;
        private string _soundPath;
        protected ImageButton PlayerButton;
        protected TextView StartTextView;
        protected TextView EndTextView;
        protected ToggleButton SpeakerButton;
        protected Button CallBackButton;
        protected Button SmsButton;
        protected SeekBar PlayerSeek;
        protected RelativeLayout TouchLayout;
        private AudioManager _audioManager;
        private bool _isSeeking;
        private bool _isPlayed;
        private bool _isCurrent;
        private Timer _timer;
        private DateTime _callbackPrevious;
        private DateTime _playPrevious;
        private bool _isBacked;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _callbackPrevious = DateTime.Now;
            _playPrevious = DateTime.Now;
            _audioManager = GetSystemService(AudioService).JavaCast<AudioManager>();
            _audioManager.Mode = Mode.Normal;
            _audioManager.SpeakerphoneOn = false;

            VolumeControlStream = Stream.VoiceCall;
        }

        protected override void OnStart()
        {
            base.OnStart();
            SpeakerButton.Checked = _audioManager.SpeakerphoneOn;
            SpeakerButton.CheckedChange += SpeakerButtonOnClick;
            CallBackButton.Click += CallBackButtonOnClick;
            SmsButton.Click += SmsButtonOnClick;
            if (Msg.FromNumber.Length < 2)
            {
                CallBackButton.Enabled = false;
                SmsButton.Enabled = false;
            }
            
            PlayerButton.Click += PlayerButtonOnClick;
            MessageStamp.Text = DataFormatUtils.ToDuration(Msg.Length);
            EndTextView.Text = $"-{DataFormatUtils.ToDuration(Msg.Length)}";
            PlayerSeek.Enabled = false;
            PlayerSeek.Max = Msg.Length;
            PlayerSeek.StartTrackingTouch += PlayerSeekOnStartTrackingTouch;
            PlayerSeek.StopTrackingTouch += PlayerSeekOnStopTrackingTouch;
            PlayerSeek.ProgressChanged += PlayerSeekOnProgressChanged;
            _timer = new Timer {Interval = 1000};
            _timer.Elapsed += TimerOnElapsed;
            var mediaBinderIntent = new Intent(this, typeof(MediaService));
            BindService(mediaBinderIntent, this, Bind.AutoCreate);
            TouchHelper.IncreaseClickArea(TouchLayout, PlayerSeek);
        }

        protected override void OnStop()
        {
            base.OnStop();
            SmsButton.Click -= SmsButtonOnClick;
            if (!_isPlayed)
            {
                _audioManager.Mode = Mode.Normal;
                _audioManager.SpeakerphoneOn = _isBacked;
            }
            UnbindService(this);
        }

        private void PlayerButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (_playPrevious.ToFileTime() + FireTime > DateTime.Now.ToFileTime()) return;
                _playPrevious = DateTime.Now;
            if (string.IsNullOrEmpty(_soundPath))
                AttachmentId = Appl.ApplicationHelper.AttachmentsHelper.LoadAttachment(Msg);
            else
                PlayerAction();
        }

        private void PlayerSeekOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs progressChangedEventArgs)
        {
            if (!_isSeeking) return;
            if (_isBinded && _isCurrent)
            {
                var progress = progressChangedEventArgs.Progress;
                EndTextView.Text = $"-{DataFormatUtils.ToDuration(Msg.Length - progress)}";
                StartTextView.Text = DataFormatUtils.ToDuration(progress);
                var intent = new Intent(this, typeof(MediaService));
                intent.SetAction(MediaService.MediaActionSeek);
                intent.PutExtra(MediaService.MediaIdTag, Msg.Id);
                intent.PutExtra(MediaService.MediaSeekTag, progress*1000);
                if (!App.GetApplication(this).IsAppInForeground && Build.VERSION.SdkInt >= BuildVersionCodes.O) return;
                ServiceUtils.StartService(this, intent);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _isBacked = false;
            if (_isBinded)
            {
                CheckSoundOutput(SpeakerButton.Checked);
                if (Msg.Equals(_serviceBinder.AppMediaService.Msg))
                {
                    _isCurrent = true;
                    _isPlayed = _serviceBinder.AppMediaService.IsPlaying;
                }
                else
                    _isCurrent = false;
            }
        }

        /// <summary>
        /// Changing output sound device
        /// </summary>
        private void SpeakerButtonOnClick(object sender, CompoundButton.CheckedChangeEventArgs eventArgs)
        {
            if (eventArgs == null)
                return;
            if (!_isBinded || _serviceBinder?.AppMediaService == null || !_serviceBinder.AppMediaService.IsInChange)
                CheckSoundOutput(eventArgs.IsChecked);
            else
                SpeakerButton.Checked = !eventArgs.IsChecked;
        }

        private void CheckSoundOutput(bool isInSpeaker)
        {
            var intent = new Intent(this, typeof(MediaService));
            intent.SetAction(MediaService.MediaActionChangeOut);
            intent.PutExtra(MediaService.MediaOutputTag, !isInSpeaker);
            if (!App.GetApplication(this).IsAppInForeground && Build.VERSION.SdkInt >= BuildVersionCodes.O) return;
            ServiceUtils.StartService(this, intent);
        }

        /// <summary>
        /// Remove message request
        /// </summary>
        protected override void RemoveButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (_timer.Enabled)
                _timer.Stop();
            var intent = new Intent(this, typeof(MediaService));
            StopService(intent);
            _isPlayed = false;
            PlayerButton.SetImageResource(Resource.Drawable.ic_action_play);
            base.RemoveButtonOnClick(sender, eventArgs);
        }

        /// <summary>
        /// Call back action
        /// </summary>
        private void CallBackButtonOnClick(object sender, EventArgs eventArgs)
        {
            var prev = _callbackPrevious.ToFileTime();
            var curr = DateTime.Now.ToFileTime();
#if DEBUG
            Log.Debug(App.AppPackage, $"Previous - {prev}; Current - {curr}");
#else
            Appl.ApplicationHelper.Reports?.Log($"Previous - {prev}; Current - {curr}");
#endif
            if (prev+FireTime > curr) return;
            _callbackPrevious = DateTime.Now;
            if (MarkForRemove != (-1))
                MarkForRemove = -1;
            Pause();
            Call(Msg.FromNumber);
        }
        
        /// <summary>
        /// Sms action
        /// </summary>
        private void SmsButtonOnClick(object sender, EventArgs eventArgs)
        {
            Pause();
            ChatActivity.StartNewChat(this, Msg.FromNumber);
        }

        private void PlayerAction()
        {
            if (!_isCurrent)
                Play();
            else
            {
                if (_isPlayed)
                    Pause();
                else
                    Play();
            }
        }

        /// <summary>
        /// Play action
        /// </summary>
        private void Play()
        {
            var intent = new Intent(this, typeof(MediaService));
            intent.SetAction(MediaService.MediaActionPlay);
            intent.PutExtra(MediaService.MediaIdTag, Msg.Id);
            intent.PutExtra(MediaService.MediaPathTag, _soundPath);
            intent.PutExtra(MediaService.MediaMsgTag, Msg);
            if (!App.GetApplication(this).IsAppInForeground && Build.VERSION.SdkInt >= BuildVersionCodes.O) return;
            ServiceUtils.StartService(this, intent);
            _isCurrent = true;
            _isPlayed = true;
            PlayerSeek.Enabled = true;
            PlayerButton.SetImageResource(Resource.Drawable.ic_action_pause);
            _timer.Start();
        }

        /// <summary>
        /// Pause action
        /// </summary>
        private void Pause()
        {
            var intent = new Intent(this, typeof (MediaService));
            intent.SetAction(MediaService.MediaActionPause);
            intent.PutExtra(MediaService.MediaIdTag, Msg.Id);
            if (!App.GetApplication(this).IsAppInForeground && Build.VERSION.SdkInt >= BuildVersionCodes.O) return;
            ServiceUtils.StartService(this, intent);
            PlayerButton.SetImageResource(Resource.Drawable.ic_action_play);
            _isPlayed = false;
            _timer.Stop();
        }

        /// <summary>
        /// Loading progress event handling
        /// </summary>
        protected override void AttachmentsHelperOnProgressLoading(object sender, AttachmentHelperEventArgs<int> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnProgressLoading(sender, args);
            if (PlayerButton.Enabled)
                PlayerButton.Enabled = false;
        }

        /// <summary>
        /// Fail loading event handling
        /// </summary>
        protected override void AttachmentsHelperOnFailLoadingEvent(object sender, AttachmentHelperEventArgs<bool> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnFailLoadingEvent(sender, args);
            if (!PlayerButton.Enabled)
                PlayerButton.Enabled = true;
        }

        /// <summary>
        /// Finish loading event handling
        /// </summary>
        protected override void AttachmentsHelperOnFinishLoading(object sender, AttachmentHelperEventArgs<string> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnFinishLoading(sender, args);
            if (!PlayerButton.Enabled)
                PlayerButton.Enabled = true;
            _soundPath = args.Result;
            Play();
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var binder = service as MediaServiceBinder;
            if (binder == null) return;
            _serviceBinder = binder;
            _isBinded = true;

            _serviceBinder.AppMediaService.EndEvent += AppMediaServiceOnEndEvent;
            if ((_serviceBinder.AppMediaService.Msg != null) && (_serviceBinder.AppMediaService.Msg.Equals(Msg)))
            {
                _isCurrent = true;
                _isPlayed = _serviceBinder.AppMediaService.IsPlaying;
            }

            PlayerSeek.Enabled = _isCurrent;
            PlayerButton.SetImageResource(_isPlayed
                ? Resource.Drawable.ic_action_pause
                : Resource.Drawable.ic_action_play);
        }

        private void AppMediaServiceOnEndEvent(object sender, bool b)
        {
            _isPlayed = false;
            _isCurrent = false;
            PlayerSeek.Enabled = false;
            PlayerSeek.Progress = 0;
            StartTextView.Text = "0:00";
            EndTextView.Text = $"-{DataFormatUtils.ToDuration(Msg.Length)}";
            PlayerButton.SetImageResource(Resource.Drawable.ic_action_play);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _serviceBinder = null;
            _isBinded = false;
        }

        public override void OnBackPressed()
        {
            if (_timer.Enabled)
                _timer.Stop();
            var intent = new Intent(this, typeof(MediaService));
            _isBacked = true;
            StopService(intent);
            base.OnBackPressed();
        }

        private void PlayerSeekOnStopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs stopTrackingTouchEventArgs)
        {
            _isSeeking = false;
            if (!_timer.Enabled)
                _timer.Start();
        }

        private void PlayerSeekOnStartTrackingTouch(object sender, SeekBar.StartTrackingTouchEventArgs startTrackingTouchEventArgs)
        {
            _isSeeking = true;
            if (_timer.Enabled)
                _timer.Stop();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_isBinded && PlayerSeek.Enabled)
            {
                RunOnUiThread(delegate
                {
                    var progress = _serviceBinder.AppMediaService.SeekPosition / 1000;
                    EndTextView.Text = $"-{DataFormatUtils.ToDuration(Msg.Length - progress)}";
                    StartTextView.Text = DataFormatUtils.ToDuration(progress);
                    PlayerSeek.Progress = progress;
                });
            }
        }
    }
}