using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.TableViewCells;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    [Register("AVPlayerView")]
    public class AVPlayerView : UIView
    {
        #region Controls

        private AVAudioPlayer _player;

        private UIButton _playButton;

        private UILabel _labelElapsed;
        private UILabel _labelRemaining;

        private UISlider _progressBar;

        private NSTimer _updateTimer;

        private UIImage _playButtonImage;
        private UIImage _pauseButtonImage;

        #endregion

        private readonly ExpandedCell _sourceCell;
        private AVAudioSession _audioSession;

        private bool _isLoudSpeaker;

        public EventHandler OnPlayButtonClick;

        public AVPlayerView(CGRect bounds, ExpandedCell sourceCell) : base(bounds)
        {
            _sourceCell = sourceCell;
            SetupAudioSession();
            InitializeControls();
        }

        private void SetupAudioSession()
        {
            _audioSession = AVAudioSession.SharedInstance();

            _isLoudSpeaker = true;
            var error = _audioSession.SetCategory(AVAudioSessionCategory.Playback);
            if (error != null)
                Console.WriteLine(error);

            if (!_audioSession.SetMode(AVAudioSession.ModeSpokenAudio, out error))
                Console.WriteLine(error);
        }

        private void InitializeControls()
        {
            var sliderBackground = new SliderBackgorundView(new CGRect(68, 12, Bounds.Width - 106, 7));

            _playButtonImage = UIImage.FromFile("play.png");
            _pauseButtonImage = UIImage.FromFile("pause.png");

            _playButton = new UIButton(new CGRect(0, 0, 30, 30));
            _playButton.SetImage(_playButtonImage, UIControlState.Normal);
            _playButton.TouchUpInside += OnPlayButtonTouchUpInside;

            _labelElapsed = new UILabel(new CGRect(30, 0, 38, 30)) { Text = "0:00", TextAlignment = UITextAlignment.Center };
            _progressBar = new UISlider(new CGRect(58, 12, Bounds.Width - 82, 7)) { Value = 0, MinValue = 0, MaxValue = _sourceCell.Duration };
            _progressBar.SetThumbImage(UIImage.FromFile("scroller.png"), UIControlState.Normal);
            _progressBar.SetMaxTrackImage(new UIImage(), UIControlState.Normal);
            _progressBar.SetMinTrackImage(new UIImage(), UIControlState.Normal);
            _progressBar.ValueChanged += OnProgressBarValueChanged;

            _labelRemaining = new UILabel(new CGRect(Bounds.Width - 38, 0, 38, 30)) { Text = $"-{DataFormatUtils.ToDuration(_sourceCell.Duration)}", TextAlignment = UITextAlignment.Right };

            foreach (var lbl in new List<UILabel> { _labelElapsed, _labelRemaining }) 
            {
                lbl.TextColor = UIColor.White;
                lbl.Font = UIFont.SystemFontOfSize(12);
            }

            AddSubviews(sliderBackground, _playButton, _labelElapsed, _progressBar, _labelRemaining);
        }

        private async void OnPlayButtonTouchUpInside(object sender, EventArgs e)
        {
            if (_player == null)
                await InitializePlayer(sender, e);

            if (_player == null) return;

            OnPlayButtonClick?.Invoke(null, EventArgs.Empty);

            if (_player.Playing)
                PausePlayback(sender, e);
            else
                StartPlayback(sender, e);
        }

        private void OnProgressBarValueChanged(object sender, EventArgs e)
        {
            if (_player == null) return;

            var progressBarSlider = sender as UISlider;
            if (progressBarSlider != null)
                _player.CurrentTime = progressBarSlider.Value;

            UpdateCurrentTime();
        }

        private async Task InitializePlayer(object sender, EventArgs e)
        {
            var filePath = await _sourceCell.GetMediaPath(MediaType.Wav);
            if (!string.IsNullOrEmpty(filePath))
            {
                _player = AVAudioPlayer.FromUrl(new NSUrl(filePath, false));
                _player.BeginInterruption += UpdateViewForPlayerState;
                _player.EndInterruption += StartPlayback;
                _player.FinishedPlaying += OnPlayerFinishedPlaying;
                _player.DecoderError += OnPlayerDecoderError;
            }
        }

        private void StartPlayback(object sender, EventArgs e)
        {
            ChangeAudioSessionState(false);
            ChangeAudioSessionState(true);

            if (_player.PrepareToPlay() && _player.Play())
                UpdateViewForPlayerState(sender, EventArgs.Empty);
            else
                Console.WriteLine($"Could not play the file {_player.Url}");
        }

        private void PausePlayback(object sender, EventArgs e)
        {
            _player.Pause();

            ChangeAudioSessionState(false);

            UpdateViewForPlayerState(sender, e);
        }

        public void StopPlayback()
        {
            _player?.Stop();
        }

        private void OnPlayerFinishedPlaying(object sender, AVStatusEventArgs e)
        {
            if (!e.Status)
                Console.WriteLine(@"Did not complete successfully");

            if (_player != null)
                _player.CurrentTime = 0;

            ChangeAudioSessionState(false);

            UpdateViewForPlayerState(sender, EventArgs.Empty);
        }

        private void UpdateViewForPlayerState(object sender, EventArgs e)
        {
            UpdateCurrentTime();

            _updateTimer?.Invalidate();

            if (_player.Playing)
            {
                _playButton.SetImage(_pauseButtonImage, UIControlState.Normal);
                _updateTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(1), delegate { UpdateCurrentTime(); });
            }
            else
            {
                _playButton.SetImage(_playButtonImage, UIControlState.Normal);
                _updateTimer = null;
            }
        }

        private static void OnPlayerDecoderError(object sender, AVErrorEventArgs e)
        {
            Console.WriteLine($"Decoder error: {e.Error.LocalizedDescription}");
        }

        private void UpdateCurrentTime()
        {
            var playerCurrentTime = (int)_player.CurrentTime;

            _labelElapsed.Text = DataFormatUtils.ToDuration(playerCurrentTime);
            _labelRemaining.Text = $"-{DataFormatUtils.ToDuration((int)_player.Duration - playerCurrentTime)}";
            _progressBar.Value = playerCurrentTime;
        }

        public void ToggleSoundOutput()
        {
            var error = _isLoudSpeaker ? _audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.AllowBluetooth)
                                       : _audioSession.SetCategory(AVAudioSessionCategory.Playback);

            _isLoudSpeaker = !_isLoudSpeaker;

            if (error != null)
                Console.WriteLine(error);
        }

        private void ChangeAudioSessionState(bool active)
        {
            NSError error;
            _audioSession.SetActive(active, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);
            if (error != null) Console.WriteLine(error);
        }
    }
}