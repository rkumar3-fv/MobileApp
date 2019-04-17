using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using FreedomVoice.Core.Presenters;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources.Texting;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.Views;
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ConversationViewController : BaseViewController
    {

        public int ConversationId;
        public PresentationNumber CurrentPhone;

        private readonly UITableView _tableView;
        private readonly CallerIdView _callerIdView;
        private readonly LineView _lineView;
        private readonly ChatTextView _chatField;
        private ConversationPresenter _presenter;
        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;


        protected ConversationViewController(IntPtr handle) : base(handle)
        {
            _tableView = new UITableView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TableFooterView = new UIView(),
                ContentInset = new UIEdgeInsets(50, 0, -50, 0)
            };

            _callerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance.GetPresentationNumbers())
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };


            _lineView = new LineView(CGRect.Empty)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _chatField = new ChatTextView
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
            TabBarController.TabBar.Hidden = true;
            AutomaticallyAdjustsScrollViewInsets = false;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _SubscribeToKeyboard();
        }


        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _UnsubscribeFromKeyboard();
        }

        private void _SetupViews()
        {
            View.AddSubview(_callerIdView);
            View.AddSubview(_lineView);
            View.AddSubview(_chatField);
            View.AddSubview(_tableView);
        }

        private void _SubscribeToKeyboard()
        {
//            self.notificationCenter.addObserver(self, selector: #selector(keyboardWillShow(_:)), name: UIResponder.keyboardWillShowNotification, object: nil)
//                self.notificationCenter.addObserver(self, selector: #selector(keyboardDidShow(_:)), name: UIResponder.keyboardDidShowNotification, object: nil)
//                    self.notificationCenter.addObserver(self, selector: #selector(keyboardWillHide(_:)), name: UIResponder.keyboardWillHideNotification, object: nil)
//                        self.notificationCenter.addObserver(self, selector: #selector(keyboardWillChangeFrame(_:)), name: UIResponder.keyboardWillChangeFrameNotification, object: nil)
        }
        
        private void _UnsubscribeFromKeyboard()
        {
            
        }

        private void _SetupConstraints()
        {
            _callerIdView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _callerIdView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _callerIdView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _callerIdView.HeightAnchor.ConstraintEqualTo(40).Active = true;

            _lineView.TopAnchor.ConstraintEqualTo(_callerIdView.BottomAnchor).Active = true;
            _lineView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _lineView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _lineView.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;

            _tableView.TopAnchor.ConstraintEqualTo(_lineView.BottomAnchor).Active = true;
//            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            
            _chatField.TopAnchor.ConstraintEqualTo(_tableView.BottomAnchor).Active = true;
            _chatField.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _chatField.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _chatField.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        private void _SetupData()
        {
            _callerIdView.UpdatePickerData(CurrentPhone);
            _tableView.Source = new ConversationSource(_tableView);

            _presenter = new ConversationPresenter
            {
                PhoneNumber = _callerIdView.SelectedNumber.PhoneNumber, ConversationId = ConversationId
            };
            _presenter.ItemsChanged += (sender, args) =>
            {
                AppDelegate.ActivityIndicator.Hide();
            };
            View.AddSubview(AppDelegate.ActivityIndicator);
            AppDelegate.ActivityIndicator.Show();
            _presenter.ReloadAsync();
        }
    }
}

