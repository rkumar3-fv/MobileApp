using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    [Register("AVPlayerView")]
    public class AVPlayerView : UIView
    {

        UIButton btnPlay;
        AVAudioPlayer player;
        UILabel labelElapsed, labelRemaining;
        UISlider progressBar;
        NSTimer update_timer;
        UIImage playBtnBg, pauseBtnBg;

        public AVPlayerView(CGRect bounds, string fileName): base(bounds)
        {
            Initialize(fileName);
        }

        private void Initialize(string fileName)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(path, fileName);
            Console.WriteLine($"File {filePath} exists = {File.Exists(filePath)}");

            playBtnBg = UIImage.FromFile("play.png");
            pauseBtnBg = UIImage.FromFile("pause.png");

            btnPlay = new UIButton(new CGRect(0, 0, 30, 30));
            btnPlay.SetImage(playBtnBg, UIControlState.Normal);
            btnPlay.TouchUpInside += BtnPlay_TouchUpInside;

            player = AVAudioPlayer.FromUrl(new NSUrl(filePath, false));
            player.FinishedPlaying += delegate (object sender, AVStatusEventArgs e) {
                if (!e.Status)
                    Console.WriteLine("Did not complete successfully");

                player.CurrentTime = 0;
                UpdateViewForPlayerState();
            };
            player.DecoderError += delegate (object sender, AVErrorEventArgs e) {
                Console.WriteLine("Decoder error: {0}", e.Error.LocalizedDescription);
            };
            player.BeginInterruption += delegate {
                UpdateViewForPlayerState();
            };
            player.EndInterruption += delegate {
                StartPlayback();
            };

            labelElapsed = new UILabel(new CGRect(25, 7, 37, 16)) { Text = "0:00"};

            progressBar = new UISlider(new CGRect(50, 12, 176, 7)) { Value = (float)player.CurrentTime, MinValue = 0, MaxValue = (float)player.Duration };
            progressBar.SetThumbImage(UIImage.FromFile("scroller.png"), UIControlState.Normal);            
            progressBar.SetMaxTrackImage(new UIImage(), UIControlState.Normal);
            progressBar.SetMinTrackImage(new UIImage(), UIControlState.Normal);
            progressBar.ValueChanged += ProgressBar_ValueChanged;            

            var sliderBg = new SliderBackgorund(new CGRect(60, 12, 156, 7));
            AddSubview(sliderBg);


            labelRemaining = new UILabel(new CGRect(225, 7, 37, 16)) { Text= $"-{SecondsToFormattedString(player.Duration)}" };            

            foreach(var lbl in new List<UILabel>() {labelElapsed, labelRemaining })
            {
                lbl.TextColor = UIColor.White;
                lbl.Font = UIFont.SystemFontOfSize(11f);
                lbl.TextAlignment = UITextAlignment.Center;
            }

            AddSubviews(btnPlay, labelElapsed, progressBar, labelRemaining);
        }

        private void ProgressBar_ValueChanged(object sender, EventArgs e)
        {
            player.CurrentTime = (sender as UISlider).Value;
            UpdateCurrentTime();
        }

        public double Duration { get { return player.Duration; } }

        private void StartPlayback()
        {
            if (player.Play())
                UpdateViewForPlayerState();
            else
                Console.WriteLine("Could not play the file {0}", player.Url);
        }

        private void BtnPlay_TouchUpInside(object sender, EventArgs e)
        {
            if (player.Playing)
                PausePlayback();
            else
                StartPlayback();
        }

        public void UpdateViewForPlayerState()
        {
            UpdateCurrentTime();

            if (update_timer != null)
                update_timer.Invalidate();

            if (player.Playing)
            {   
                btnPlay.SetImage(pauseBtnBg, UIControlState.Normal);                
                update_timer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(0.01), delegate {
                    UpdateCurrentTime();
                });
            }
            else
            {
                btnPlay.SetImage(playBtnBg, UIControlState.Normal);                
                update_timer = null;
            }
        }

        public void UpdateCurrentTime()
        {
            labelRemaining.Text = $"-{SecondsToFormattedString(player.Duration - player.CurrentTime)}";
            labelElapsed.Text = SecondsToFormattedString(player.CurrentTime);
            progressBar.Value = (float)player.CurrentTime;
        }

        public static string SecondsToFormattedString(double seconds)
        {
            if (seconds < 600)
                return TimeSpan.FromSeconds(seconds).ToString(@"m\:ss");

            return TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
        }

        void PausePlayback()
        {
            player.Pause();
            UpdateViewForPlayerState();
        }

    }
}
