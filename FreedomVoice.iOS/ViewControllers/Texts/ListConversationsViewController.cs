using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources.Texting;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ListConversationsViewController : BaseViewController
    {
        protected override string PageName => "Texts";

        private UILabel _noItemsLabel;
        private UITableView _tableView;
        private CallerIdView _callerIdView;
        private LineView _lineView;
        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

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

            _callerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance.GetPresentationNumbers())
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _lineView = new LineView(CGRect.Empty)
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
            View.AddSubview(_callerIdView);
            View.AddSubview(_lineView);
            View.AddSubview(_tableView);
            View.AddSubview(_noItemsLabel);

            var createButton = new UIBarButtonItem(UIBarButtonSystemItem.Compose, (sender, e) => { });

            NavigationItem.RightBarButtonItem = createButton;
        }

        private void _SetupConstraints()
        {
            _noItemsLabel.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _noItemsLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 15).Active = true;
            _noItemsLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -15).Active = true;
            _noItemsLabel.HeightAnchor.ConstraintEqualTo(40).Active = true;

            _callerIdView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _callerIdView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _callerIdView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _callerIdView.HeightAnchor.ConstraintEqualTo(40).Active = true;

            _lineView.TopAnchor.ConstraintEqualTo(_callerIdView.BottomAnchor).Active = true;
            _lineView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _lineView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _lineView.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;

            _tableView.TopAnchor.ConstraintEqualTo(_lineView.BottomAnchor).Active = true;
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

