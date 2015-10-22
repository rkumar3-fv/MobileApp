using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class RecentCell : UITableViewCell
    {
        UILabel phoneTitle, dialDate;
        //UIImageView icon;
        public static readonly NSString RecentCellId = new NSString("RecentCell");
        public RecentCell() : base(UITableViewCellStyle.Default, RecentCellId) {
            //icon = new UIImageView();
            phoneTitle = new UILabel()
            {                
                BackgroundColor = UIColor.Clear
            };
            dialDate = new UILabel()
            {
                TextColor = UIColor.FromRGB(127, 127, 127),
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(12)
            };
            
            ContentView.AddSubviews(new UIView[] { phoneTitle, dialDate/*, icon*/ });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();            
            phoneTitle.Frame = new CGRect(15, 5, 220, 32);
            dialDate.Frame = new CGRect(221, 5, 60, 32);
            //if (icon != null)
            //    icon.Frame = new CGRect(288, 10, 22, 22);
        }

        public void UpdateCell(string title, string dialDate/*, bool renderIcon*/)
        {
            phoneTitle.Text = title;
            this.dialDate.Text = dialDate;
            //if (renderIcon)            
            //    icon.Image = UIImage.FromFile("recent.png");
            //else
            //    icon = null;
        }


    }
}
