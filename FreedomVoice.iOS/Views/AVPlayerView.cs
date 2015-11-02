using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using FreedomVoice.iOS.Helpers;
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

        public AVPlayerView(CGRect bounds, string fileName) : base(bounds)
        {
            Initialize(fileName);
        }

        private void Initialize(string fileName)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(path, fileName);

            Console.WriteLine($"File {filePath} exists = {File.Exists(filePath)}");

            var sliderBackground = new SliderBackgorund(new CGRect(60, 12, 156, 7));

            _playButtonImage = UIImage.FromFile("play.png");
            _pauseButtonImage = UIImage.FromFile("pause.png");

            _playButton = new UIButton(new CGRect(0, 0, 30, 30));
            _playButton.SetImage(_playButtonImage, UIControlState.Normal);
            _playButton.TouchUpInside += OnPlayButtonTouchUpInside;

            try {
                _player = AVAudioPlayer.FromUrl(new NSUrl(filePath, false));
                _player.FinishedPlaying += OnPlayerFinishedPlaying;
                _player.DecoderError += OnPlayerDecoderError;
                _player.BeginInterruption += UpdateViewForPlayerState;
                _player.EndInterruption += StartPlayback;
            } catch (Exception)
            {
                Console.WriteLine("Media doesn't exist");
            }

            _labelElapsed = new UILabel(new CGRect(25, 7, 37, 16)) { Text = "0:00" };

            _progressBar = new UISlider(new CGRect(50, 12, 176, 7)) { Value = (float)_player.CurrentTime, MinValue = 0, MaxValue = (float)_player.Duration };
            _progressBar.SetThumbImage(UIImage.FromFile("scroller.png"), UIControlState.Normal);
            _progressBar.SetMaxTrackImage(new UIImage(), UIControlState.Normal);
            _progressBar.SetMinTrackImage(new UIImage(), UIControlState.Normal);
            _progressBar.ValueChanged += OnProgressBarValueChanged;

            _labelRemaining = new UILabel(new CGRect(225, 7, 37, 16)) { Text = $"-{Formatting.SecondsToFormattedString(_player.Duration)}" };

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
            if (_player.Playing)
                PausePlayback(sender, e);
            else
                StartPlayback(sender, e);
        }

        private void UpdateViewForPlayerState(object sender, EventArgs e)
        {
            UpdateCurrentTime();

            _updateTimer?.Invalidate();

            if (_player.Playing)
            {
                //TODO: Do we really need a timer with 0.01 interval?
                _playButton.SetImage(_pauseButtonImage, UIControlState.Normal);
                _updateTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(0.01), delegate { UpdateCurrentTime(); });
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
    }
}