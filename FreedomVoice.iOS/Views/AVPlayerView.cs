using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Helpers;
using UIKit;
using FreedomVoice.iOS.TableViewCells;

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

        AVAudioSession session;
        private bool IsSpeakerSource = false;

        public AVPlayerView(CGRect bounds, ExpandedCell sourceCell) : base(bounds)
        {
            _sourceCell = sourceCell;
            Initialize();
        }

        private void Initialize()
        {
            session = AVAudioSession.SharedInstance();

            var sliderBackground = new SliderBackgorund(new CGRect(60, 12, 156, 7));

            _playButtonImage = UIImage.FromFile("play.png");
            _pauseButtonImage = UIImage.FromFile("pause.png");

            _playButton = new UIButton(new CGRect(0, 0, 30, 30));
            _playButton.SetImage(_playButtonImage, UIControlState.Normal);
            _playButton.TouchUpInside += OnPlayButtonTouchUpInside;

            _labelElapsed = new UILabel(new CGRect(25, 7, 37, 16)) { Text = "0:00" };

            _progressBar = new UISlider(new CGRect(50, 12, 176, 7)) { Value = 0, MinValue = 0, MaxValue = (float)_sourceCell.GetDuration() };
            _progressBar.SetThumbImage(UIImage.FromFile("scroller.png"), UIControlState.Normal);
            _progressBar.SetMaxTrackImage(new UIImage(), UIControlState.Normal);
            _progressBar.SetMinTrackImage(new UIImage(), UIControlState.Normal);
            _progressBar.ValueChanged += OnProgressBarValueChanged;

            _labelRemaining = new UILabel(new CGRect(225, 7, 37, 16)) { Text = $"-{Formatting.SecondsToFormattedString(_sourceCell.GetDuration())}" };

            foreach (var lbl in new List<UILabel> { _labelElapsed, _labelRemaining })
            {
                lbl.TextColor = UIColor.White;
                lbl.Font = UIFont.SystemFontOfSize(11f);
                lbl.TextAlignment = UITextAlignment.Center;
            }

            AddSubviews(sliderBackground, _playButton, _labelElapsed, _progressBar, _labelRemaining);
        }

        private void OnPlayerFinishedPlaying(object sender, AVStatusEventArgs e)
        {
            if (!e.Status)
                Console.WriteLine(@"Did not complete successfully");

            _player.CurrentTime = 0;
            UpdateViewForPlayerState(sender, EventArgs.Empty);
        }

        private static void OnPlayerDecoderError(object sender, AVErrorEventArgs e)
        {
            Console.WriteLine($"Decoder error: {e.Error.LocalizedDescription}");
        }

        private void OnProgressBarValueChanged(object sender, EventArgs e)
        {
            var progressBarSlider = sender as UISlider;
            if (progressBarSlider != null)
                _player.CurrentTime = progressBarSlider.Value;

            UpdateCurrentTime();
        }

        private void StartPlayback(object sender, EventArgs e)
        {
            if (_player.Play())
                UpdateViewForPlayerState(sender, EventArgs.Empty);
            else
                Console.WriteLine($"Could not play the file {_player.Url}");
        }

        private void OnPlayButtonTouchUpInside(object sender, EventArgs e)
        {
            if (_player != null && _player.Playing)
                PausePlayback(sender, e);
            else if (_player == null)
                InitPlayer(sender, e);
            else
                StartPlayback(sender, e);
        }

        private async void InitPlayer(object sender, EventArgs e)
        {
            var filePath = await _sourceCell.GetMediaPath(Core.Entities.Enums.MediaType.Wav);

            _player = AVAudioPlayer.FromUrl(new NSUrl(filePath, false));
            _player.FinishedPlaying += OnPlayerFinishedPlaying;
            _player.DecoderError += OnPlayerDecoderError;
            _player.BeginInterruption += UpdateViewForPlayerState;
            _player.EndInterruption += StartPlayback;

            StartPlayback(sender, e);
        }

        private void UpdateViewForPlayerState(object sender, EventArgs e)
        {
            UpdateCurrentTime();

            _updateTimer?.Invalidate();

            if (_player.Playing)
            {
                _playButton.SetImage(_pauseButtonImage, UIControlState.Normal);
                _updateTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(0.5), delegate { UpdateCurrentTime(); });
            }
            else
            {
                _playButton.SetImage(_playButtonImage, UIControlState.Normal);
                _updateTimer = null;
            }
        }

        private void UpdateCurrentTime()
        {
            _labelRemaining.Text = $"-{Formatting.SecondsToFormattedString(_player.Duration - _player.CurrentTime)}";
            _labelElapsed.Text = Formatting.SecondsToFormattedString(_player.CurrentTime);
            _progressBar.Value = (float)_player.CurrentTime;
        }

        private void PausePlayback(object sender, EventArgs e)
        {
            _player.Pause();
            UpdateViewForPlayerState(sender, e);
        }

        public void ToggleSoundOutput()
        {
            NSError error;
            if (IsSpeakerSource)
            {
                session.OverrideOutputAudioPort(AVAudioSessionPortOverride.None, out error);
                IsSpeakerSource = false;
            }
            else
            {
                session.OverrideOutputAudioPort(AVAudioSessionPortOverride.Speaker, out error);
                IsSpeakerSource = true;
            }
        }
    }
}