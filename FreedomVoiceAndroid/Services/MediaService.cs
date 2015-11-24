using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
#if DEBUG
using Android.Util;
using FreedomVoice.Core.Utils;
#endif
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Media playback service
    /// </summary>
    [Service(Exported = false)]
    public class MediaService : Service, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener
    {
        public const int MediaNotificationId = 103;
        public const string MediaActionPlay = "MediaActionPlay";
        public const string MediaActionPause = "MediaActionPause";
        public const string MediaActionSeek = "MediaActionSeek";
        public const string MediaActionStop = "MediaActionStop";
        public const string MediaActionChangeOut = "MediaActionChangeOut";
        public const string MediaIdTag = "MediaIdTag";
        public const string MediaMsgTag = "MediaMsgTag";
        public const string MediaPathTag = "MediaPathTag";
        public const string MediaSeekTag = "MediaSeekTag";
        public const string MediaOutputTag = "MediaOutputTag";

        private AudioManager _audioManager;
        private MediaPlayer _mediaPlayer;
        private string _path;

        private bool _isPlaying;
        private int _position;

        public event EventHandler<bool> EndEvent;

        /// <summary>
        /// Current message for playing
        /// </summary>
        public Message Msg { get; private set; }

        public bool IsInChange { get; private set; }

        /// <summary>
        /// Playing status
        /// </summary>
        public bool IsPlaying => _mediaPlayer != null && _mediaPlayer.IsPlaying;

        /// <summary>
        /// Current player position
        /// </summary>
        public int SeekPosition => _mediaPlayer?.CurrentPosition ?? 0;

        public override void OnCreate()
        {
            base.OnCreate();
            _audioManager = GetSystemService(AudioService).JavaCast<AudioManager>();
        }

        public override IBinder OnBind(Intent intent)
        {
            return new MediaServiceBinder(this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var id = intent.GetIntExtra(MediaIdTag, 0);
            switch (intent.Action)
            {
                case MediaActionPlay:
                    var message = (Message)intent.GetParcelableExtra(MediaMsgTag);
                    var path = intent.GetStringExtra(MediaPathTag);
                    if ((!message.Equals(Msg))||(path != _path))
                    {
                        ReleasePlayer();
                        _path = path;
                        Msg = message;
                    }
                    if (_mediaPlayer == null)
                    {
#if DEBUG
                        Log.Debug(App.AppPackage, $"PREPARE PLAYING {_path}");
#endif
                        _position = 0;
                        _isPlaying = true;
                        PreparePlayer();
                    }
                    else
                    {
#if DEBUG
                        Log.Debug(App.AppPackage, $"RESUME PLAYING ON {DataFormatUtils.ToDuration(_mediaPlayer.CurrentPosition/1000)}");
#endif
                        _mediaPlayer.Start();
                    }
                    break;
                case MediaActionChangeOut:
                    var type = intent.GetBooleanExtra(MediaOutputTag, false);
                    _audioManager.SpeakerphoneOn = !type;
                    break;
                case MediaActionPause:
                    if (IsInChange) break;
                    if ((Msg != null) && (!string.IsNullOrEmpty(_path)))
                    {
#if DEBUG
                        Log.Debug(App.AppPackage, $"PAUSE ON {DataFormatUtils.ToDuration(_mediaPlayer.CurrentPosition/1000)}");
#endif
                        _mediaPlayer?.Pause();
                    }
                    break;
                case MediaActionSeek:
                    if (IsInChange) break;
                    if ((Msg != null) && (!string.IsNullOrEmpty(_path)))
                    {
                        var pos = intent.GetIntExtra(MediaSeekTag, -1);
                        if ((_mediaPlayer != null) && (pos > -1) && (pos <= _mediaPlayer.Duration))
                        {
#if DEBUG
                            Log.Debug(App.AppPackage, $"SEEKING TO {DataFormatUtils.ToDuration(pos/1000)}");
#endif
                            _mediaPlayer.SeekTo(pos);
                        }
                    }
                    break;
                case MediaActionStop:
                    var msg = intent.GetParcelableExtra(MediaMsgTag);
                    if (msg.Equals(Msg))
                        ReleasePlayer();
                    break;
            }
            return StartCommandResult.NotSticky;
        }

        private void PreparePlayer()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.SetAudioStreamType(Stream.VoiceCall);
            _mediaPlayer.SetOnPreparedListener(this);
            _mediaPlayer.Looping = false;
            _mediaPlayer.SetOnCompletionListener(this);
            _mediaPlayer.SetOnErrorListener(this);
            _mediaPlayer.SetDataSource(_path);
            _mediaPlayer.PrepareAsync();
        }

        /// <summary>
        /// Media player ready callback
        /// </summary>
        public void OnPrepared(MediaPlayer mp)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "MEDIA PLAYER PREPAIRED");
#endif
            if (mp != null)
            {
                IsInChange = false;
                if (_position != 0)
                    mp.SeekTo(_position);
                if (_isPlaying)
                    mp.Start();
            }
        }

        private void ReleasePlayer()
        {
            _position = 0;
            _isPlaying = false;
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Release();
                _mediaPlayer = null;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ReleasePlayer();
#if DEBUG
            Log.Debug(App.AppPackage, "MEDIA SERVICE DESTROYED");
#endif
        }

        public void OnCompletion(MediaPlayer mp)
        {
            EndEvent?.Invoke(this, true);
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            if ((int) what == -38) return true;
            ReleasePlayer();
            EndEvent?.Invoke(this, true);
            return true;
        }
    }
}