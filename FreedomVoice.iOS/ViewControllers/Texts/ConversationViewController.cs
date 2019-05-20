using System;
using System.Drawing;
using CoreGraphics;
using FreedomVoice.Core.Presenters;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.PushNotifications;
using FreedomVoice.iOS.TableViewSources.Texting;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Views;
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ConversationViewController : BaseViewController
    {
        #region Subviews

        private readonly UIRefreshControl refreshControl = new UIRefreshControl();

        private readonly UITableView _tableView = new UITableView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TableFooterView = new UIView(),
        };
        
        protected readonly CallerIdView _callerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance?.GetPresentationNumbers())
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            IsReadOnly = true
        };
    
        private readonly LineView _lineView = new LineView(CGRect.Empty)
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        
        protected readonly ChatTextView _chatField = new ChatTextView
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        
        #endregion

        public long? ConversationId;
        public PresentationNumber CurrentPhone;
  
        private IDisposable _observer1;
        private IDisposable _observer2;
        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        private NSLayoutConstraint _bottomConstraint;
        
        protected ConversationPresenter Presenter;


        public ConversationViewController()
        {
        }

        protected ConversationViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _SetupViews();
            _SetupConstraints();
            _SetupData();
            AutomaticallyAdjustsScrollViewInsets = false;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (TabBarController != null) TabBarController.TabBar.Hidden = true;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _SubscribeToEvents();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (TabBarController != null) TabBarController.TabBar.Hidden = false;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _UnsubscribeFromEvents();

        }

        protected virtual void _SetupViews()
        {
            View.BackgroundColor = UIColor.White;
            
            View.AddSubview(_callerIdView);
            View.AddSubview(_lineView);
            View.AddSubview(_chatField);
            View.AddSubview(_tableView);
        }

        protected virtual void _SubscribeToEvents()
        {
            _observer1 = UIKeyboard.Notifications.ObserveWillShow(WillShowNotification);
            _observer2 = UIKeyboard.Notifications.ObserveWillHide(WillHideNotification);
            _chatField.SendButtonPressed += SendButtonPressed;
            refreshControl.ValueChanged += RefreshControlOnValueChanged;
        }

        private void _UnsubscribeFromEvents()
        {
            _observer1.Dispose();
            _observer2.Dispose();
            _chatField.SendButtonPressed -= SendButtonPressed;
            refreshControl.ValueChanged += RefreshControlOnValueChanged;
        }
        

        private void WillHideNotification(object sender, UIKeyboardEventArgs e)
        {
            UIView.BeginAnimations("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(e.Notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(e.Notification));
            _bottomConstraint.Constant = 0;
            UIView.CommitAnimations();
        }

        private void WillShowNotification(object sender, UIKeyboardEventArgs e)
        {
            var keyboardSize = e.FrameEnd.Size;
            UIView.BeginAnimations("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(e.Notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(e.Notification));
            _bottomConstraint.Constant = -keyboardSize.Height;
            UIView.CommitAnimations();
        }

        protected virtual void _SetupConstraints()
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
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            
            _chatField.TopAnchor.ConstraintEqualTo(_tableView.BottomAnchor).Active = true;
            _bottomConstraint = _chatField.BottomAnchor.ConstraintEqualTo(View.BottomAnchor);
            _bottomConstraint.Active = true;
            
            _chatField.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _chatField.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        protected virtual void _SetupData()
        {
            View.AddGestureRecognizer(new UITapGestureRecognizer((obj) => View.EndEditing(true)));
            _callerIdView.UpdatePickerData(CurrentPhone);
            CallerIdEvent.CallerIdChanged += CallerIdEventOnCallerIdChanged;
            var source = new ConversationSource(_tableView);
            source.NeedMoreEvent += (sender, args) =>
            {
                if (Presenter.HasMore)
                {
                    Presenter.LoadMoreAsync();
                }
            };
            _tableView.Source = source;

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                _tableView.RefreshControl = refreshControl;
            else
                _tableView.AddSubview(refreshControl);
            
            Presenter = new ConversationPresenter
            {
                PhoneNumber = _callerIdView.SelectedNumber.PhoneNumber, ConversationId = ConversationId
            };
            Presenter.ItemsChanged += (sender, args) =>
            {
                var items = Presenter.Items;
                source.UpdateItems(items);
                AppDelegate.ActivityIndicator.Hide();
                refreshControl.EndRefreshing();
            };
            View.AddSubview(AppDelegate.ActivityIndicator);
            AppDelegate.ActivityIndicator.Show();
            Presenter.ReloadAsync();

            _chatField.Text = new PushNotificationTokenDataStore().Get();
        }
        
        protected virtual async void SendButtonPressed()
        {
            if (string.IsNullOrWhiteSpace(_chatField.Text))
                return;
            
            View.AddSubview(AppDelegate.ActivityIndicator);
            AppDelegate.ActivityIndicator.Show();
            await Presenter.SendMessageAsync(_chatField.Text);
            _chatField.Text = "";
            AppDelegate.ActivityIndicator.Hide();
        }

        private void CallerIdEventOnCallerIdChanged(object sender, EventArgs e)
        {
            Presenter.PhoneNumber = _callerIdView.SelectedNumber.PhoneNumber;
            Presenter.ReloadAsync();
        }
        
        private void RefreshControlOnValueChanged(object sender, EventArgs e)
        {
            Presenter.ReloadAsync();
        }
    }
}

