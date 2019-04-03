using System;
using System.Collections.Generic;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources.Texting;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ListConversationsViewController : BaseViewController
    {
        protected override string PageName => "Texts";

        private UILabel _noItemsLabel;
        private UITableView _tableView;

        public ListConversationsViewController(IntPtr handle) : base(handle)
        {
            _noItemsLabel = new UILabel
            {
                Text = "No Items",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true
            };
            
            _tableView = new UITableView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _SetupViews();
            _SetupConstraints();
            _SetupData();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = PageName;

            base.ViewWillAppear(animated);
        }

        private void _SetupViews()
        {
            View.AddSubview(_noItemsLabel);
            View.AddSubview(_tableView);

            var createButton = new UIBarButtonItem("Create", UIBarButtonItemStyle.Plain, (s, e) => { });
            NavigationItem.RightBarButtonItem = createButton;
        }

        private void _SetupConstraints()
        {
            _noItemsLabel.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _noItemsLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 15).Active = true;
            _noItemsLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -15).Active = true;
            _noItemsLabel.HeightAnchor.ConstraintEqualTo(40).Active = true;

            _tableView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        private void _SetupData()
        {
            _tableView.Source = new ConversationsSource(new List<Account>(), NavigationController, _tableView);
            _tableView.ReloadData();
        }
    }
}

