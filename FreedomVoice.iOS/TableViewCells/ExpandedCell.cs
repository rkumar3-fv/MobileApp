using CoreAnimation;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Views;
using System;
using System.Collections.Generic;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ExpandedCell : UITableViewCell
    {
        UIImageView Icon;
        UILabel Title, Date, Length;
        AVPlayerView Player;
        UIButton Callback, Delete, Speaker, ViewFax;
        MessageType type;
        

        public static readonly NSString ExpandedCellId = new NSString("ExpandedCell");
        public ExpandedCell(MessageType type) : base (UITableViewCellStyle.Default, ExpandedCellId)
        {
            this.type = type;
            SetBackground();            
        }

        void SetBackground()
        {
            nfloat bgHeight = type == MessageType.Fax ? 100 : 138;
            var gradientLayer = new CAGradientLayer();
            gradientLayer.Frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width, bgHeight);
            gradientLayer.Colors = new CGColor[] { UIColor.FromRGB(51, 71, 98).CGColor, UIColor.FromRGB(98, 120, 149).CGColor };
            Layer.AddSublayer(gradientLayer);
        }

        public void UpdateCell(String title, String date, double length, String resourceFileName)
        {
            Icon = new UIImageView(new CGRect(12,15, 25, 25));
            string formattedLength = string.Empty;
            switch (type)
            {
                case MessageType.Recording:
                    Icon.Image = UIImage.FromFile("callrecord_active.png");
                    formattedLength = AVPlayerView.SecondsToFormattedString(length);
                    break;
                case MessageType.Fax:
                    Icon.Image = UIImage.FromFile("fax_active.png");
                    formattedLength = $"{length} pages";
                    break;
                case MessageType.Voicemail:
                    Icon.Image = UIImage.FromFile("voicemail_active.png");
                    formattedLength = AVPlayerView.SecondsToFormattedString(length);
                    break;
            }
            AddSubview(Icon);

            Title = new UILabel(new CGRect(46, 10, 270, 15)) { Text = title, TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(17f) };            
            AddSubview(Title);

            Date = new UILabel(new CGRect(46, 29, 110, 11)) { Text = date, TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(11f) };            
            AddSubview(Date);

            SetLengthLabel(type, formattedLength);

            if (type == MessageType.Fax)
                AddFax();
            else
                AddVoice(resourceFileName);

            AddDeleteButton(type);

            InitCommonStyles();
        }

        private void AddVoice(String resourceFileName)
        {
            Player = new AVPlayerView(new CGRect(46, 54, 256, 23), resourceFileName);
            AddSubview(Player);
            AddSpeaker();
            AddCallback();            
        }

        void SetLengthLabel(MessageType type, string formattedLength)
        {
            nfloat x = type == MessageType.Fax ? 257 : 275;
            nfloat width = type == MessageType.Fax ? 49 : 35;
            Length = new UILabel(new CGRect(x, 29, width, 13)) { Text = formattedLength, TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(12f) };
            AddSubview(Length);
        }

        void AddDeleteButton(MessageType type)
        {
            nfloat y = type == MessageType.Fax ? 60 : 99;
            Delete = new UIButton(new CGRect(285, y, 25, 25));
            Delete.SetBackgroundImage(UIImage.FromFile("delete.png"), UIControlState.Normal);
            AddSubview(Delete);
        }

        private void BackgroundColorAnimate(UIButton btn, UIColor tapColor)
        {
            UIView.Animate(0.2, 0, UIViewAnimationOptions.Autoreverse,
                  () => { btn.BackgroundColor = tapColor; },
                  () => { btn.BackgroundColor = UIColor.Clear; });
        }

        void AddSpeaker()
        {
            Speaker = new UIButton(new CGRect(43, 97, 98, 29));
            Speaker.SetTitle("Speaker", UIControlState.Normal);
            Speaker.SetTitleColor(UIColor.FromRGB(119, 229, 246), UIControlState.Normal);
            Speaker.SetImage(UIImage.FromFile("speaker.png"), UIControlState.Normal);            
            Speaker.Layer.BorderColor = UIColor.FromRGBA(119, 229, 246, 110).CGColor;            

            Speaker.TouchDown += delegate (object sender, EventArgs e)
            {
                BackgroundColorAnimate(Speaker, UIColor.FromRGBA(119, 229, 246, 127));
            };

            AddSubview(Speaker);
        }

        void AddCallback()
        {
            Callback = new UIButton(new CGRect(149, 97, 104, 29));
            Callback.SetTitle("Call Back", UIControlState.Normal);
            Callback.SetTitleColor(UIColor.FromRGB(198, 242, 138), UIControlState.Normal);
            Callback.SetImage(UIImage.FromFile("call_back.png"), UIControlState.Normal);
            Callback.Layer.BorderColor = UIColor.FromRGBA(198, 242, 138, 110).CGColor;
            Callback.TouchDown += delegate (object sender, EventArgs e)
            {
                BackgroundColorAnimate(Callback, UIColor.FromRGBA(198, 242, 138, 127));
            };
            AddSubview(Callback);
        }

        void AddFax()
        {
            ViewFax = new UIButton(new CGRect(43, 58, 98, 28));
            ViewFax.SetTitle("View Fax", UIControlState.Normal);            
            ViewFax.SetTitleColor(UIColor.FromRGB(198, 242, 138), UIControlState.Normal);
            ViewFax.SetImage(UIImage.FromFile("view_fax.png"), UIControlState.Normal);            
            ViewFax.Layer.BorderColor = UIColor.FromRGBA(198, 242, 138, 90).CGColor;
            ViewFax.TouchDown += delegate (object sender, EventArgs e)
            {
                BackgroundColorAnimate(ViewFax, UIColor.FromRGBA(198, 242, 138, 127));
            };
            AddSubview(ViewFax);
        }

        void InitCommonStyles()
        {
            foreach(var btn in new List<UIButton>() { Callback, ViewFax, Speaker})
            {
                if (btn != null)
                {
                    btn.Font = UIFont.SystemFontOfSize(14f);
                    btn.ClipsToBounds = true;
                    btn.Layer.CornerRadius = 5;
                    btn.Layer.BorderWidth = 1f;
                }
            }
        }
    }
}
