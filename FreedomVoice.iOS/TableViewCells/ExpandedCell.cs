using CoreAnimation;
using CoreGraphics;
using Foundation;
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

        public static readonly NSString ExpandedCellId = new NSString("ExpandedCell");
        public ExpandedCell() : base (UITableViewCellStyle.Default, ExpandedCellId)
        {
            SetBackground();            
        }

        void SetBackground()
        {
            var gradientLayer = new CAGradientLayer();
            gradientLayer.Frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width, 100);            
            gradientLayer.Colors = new CGColor[] { UIColor.FromRGB(51, 72, 98).CGColor, UIColor.FromRGB(98, 120, 149).CGColor };
            Layer.AddSublayer(gradientLayer);
        }

        public void UpdateCell(RecordType type, String title, String date, String length, String resourceFileName)
        {
            Icon = new UIImageView(new CGRect(12,15, 25, 25));
            switch (type)
            {
                case RecordType.CallRecord:
                    Icon.Image = UIImage.FromFile("callrecord_active.png");
                    break;
                case RecordType.Fax:
                    Icon.Image = UIImage.FromFile("fax_active.png");
                    break;
                case RecordType.VoiceMail:
                    Icon.Image = UIImage.FromFile("voicemail_active.png");
                    break;
            }
            AddSubview(Icon);

            Title = new UILabel(new CGRect(46, 10, 270, 15)) { Text = title, TextColor = UIColor.White};
            Title.Font = UIFont.SystemFontOfSize(17f);
            AddSubview(Title);

            Date = new UILabel(new CGRect(46, 29, 110, 11)) { Text = date, TextColor = UIColor.White };
            Date.Font = UIFont.SystemFontOfSize(11f);
            AddSubview(Date);

            Length = new UILabel(new CGRect(29, 275, 28, 11)) { Text = length, TextColor = UIColor.White };
            Length.Font = UIFont.SystemFontOfSize(11f);
            AddSubview(Length);

            Player = new AVPlayerView(new CGRect(53, 48, 256, 23), resourceFileName);
            AddSubview(Player);
        }
    }

    public enum RecordType
    {
        Fax, VoiceMail, CallRecord
    }
}
