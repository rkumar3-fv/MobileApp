using System;

using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells.Texting
{
    public partial class ConversationItemTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("ConversationItemTableViewCell");
        public static readonly UINib Nib;

        static ConversationItemTableViewCell()
        {
            Nib = UINib.FromName("ConversationItemTableViewCell", NSBundle.MainBundle);
        }

        protected ConversationItemTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            NewMessageView.ClipsToBounds = true;
            NewMessageView.Layer.CornerRadius = NewMessageView.Frame.Height / 2;
        }

        public String Title
        {
            get => NameLabel.Text;
            set => NameLabel.Text = value;
        }
        
        public String Detail
        {
            get => MessageLabel.Text;
            set => MessageLabel.Text = value;
        }
        
        public String Date
        {
            get => DateLabel.Text;
            set => DateLabel.Text = value;
        }

        private bool _isNew;
        public bool isNew
        {
            get => _isNew;
            set
            {
                _isNew = value;
                DateLabel.Font = UIFont.SystemFontOfSize(15, value ? UIFontWeight.Medium : UIFontWeight.Regular);
                NewMessageView.Hidden = !value;
            }
        }
    }
}
