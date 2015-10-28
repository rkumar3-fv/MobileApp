using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
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

            playBtnBg = UIImage.FromFile("btn_play.png");
            pauseBtnBg = UIImage.FromFile("btn_pause.png");

            btnPlay = new UIButton(new CGRect(0, 0, 16, 16));
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

            labelElapsed = new UILabel(new CGRect(25, 0, 32, 16)) { Text = "00:00", TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(11f)};

            progressBar = new UISlider(new CGRect(60, 6, 156, 4)) { Value = (float)player.CurrentTime, MinValue = 0, MaxValue = (float)player.Duration };
            progressBar.SetThumbImage(UIImage.FromFile("progress_bar_thumb.png"), UIControlState.Normal);            
            
            labelRemaining = new UILabel(new CGRect(225, 0, 37, 16)) { Text= $"-{SecondsToFormattedString(player.Duration)}", TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(11f) };            

            AddSubviews(btnPlay, labelElapsed, progressBar, labelRemaining);
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
            return TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
        }

        void PausePlayback()
        {
            player.Pause();
            UpdateViewForPlayerState();
        }

    }
}
