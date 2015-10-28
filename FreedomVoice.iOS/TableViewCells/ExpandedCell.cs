using CoreAnimation;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Views;
using System;
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
            gradientLayer.Colors = new CGColor[] { UIColor.FromRGB(51, 72, 98).CGColor, UIColor.FromRGB(98, 120, 149).CGColor };
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
        }

        private void AddVoice(String resourceFileName)
        {
            Player = new AVPlayerView(new CGRect(46, 54, 256, 23), resourceFileName);            

            Speaker = new UIButton(new CGRect(43, 97, 98, 29));
            Speaker.SetBackgroundImage(UIImage.FromFile("speaker.png"), UIControlState.Normal);

            Callback = new UIButton(new CGRect(149, 97, 104, 29));
            Callback.SetBackgroundImage(UIImage.FromFile("callback.png"), UIControlState.Normal);

            AddSubviews(Player, Speaker, Callback);
        }

        void SetLengthLabel(MessageType type, string formattedLength)
        {
            nfloat x = type == MessageType.Fax ? 257 : 275;
            nfloat width = type == MessageType.Fax ? 49 : 32;
            Length = new UILabel(new CGRect(x, 29, width, 12)) { Text = formattedLength, TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(11f) };
            AddSubview(Length);
        }

        void AddDeleteButton(MessageType type)
        {
            nfloat y = type == MessageType.Fax ? 60 : 99;
            Delete = new UIButton(new CGRect(285, y, 20, 25));
            Delete.SetBackgroundImage(UIImage.FromFile("delete.png"), UIControlState.Normal);
            AddSubview(Delete);
        }

        void AddFax()
        {
            ViewFax = new UIButton(new CGRect(43, 58, 98, 29));
            ViewFax.SetBackgroundImage(UIImage.FromFile("viewfax.png"), UIControlState.Normal);
            AddSubview(ViewFax);
        }
    }
}
