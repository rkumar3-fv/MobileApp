using System;
using CoreGraphics;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ListConversationsViewController : BaseViewController
    {
        protected override string PageName => "Texts";

        private UILabel _NoItemsLabel;

        public ListConversationsViewController(IntPtr handle) : base(handle)
        {
            //MessagesList = new List<Message>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _SetupViews();
            _SetupConstraints();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = PageName;

            base.ViewWillAppear(animated);
        }

        private void _SetupViews()
        {
            _NoItemsLabel = new UILabel
            {
                Frame = CGRect.Empty,
                Text = "No Items",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(_NoItemsLabel);
        }

            private void _SetupConstraints()
        {
            _NoItemsLabel.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _NoItemsLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 15).Active = true;
            _NoItemsLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -15).Active = true;
            _NoItemsLabel.HeightAnchor.ConstraintEqualTo(40).Active = true;
        }
    }
}

