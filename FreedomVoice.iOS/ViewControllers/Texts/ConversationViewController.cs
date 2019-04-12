﻿using System;
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
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ConversationViewController : BaseViewController
    {

        public int converstaionId;
        public PresentationNumber currentPhone;

        private UITableView _tableView;
        private CallerIdView _callerIdView;
        private LineView _lineView;
        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;


        protected ConversationViewController(IntPtr handle) : base(handle)
        {
            _tableView = new UITableView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TableFooterView = new UIView()
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

        private void _SetupViews()
        {
            View.AddSubview(_callerIdView);
            View.AddSubview(_lineView);
            View.AddSubview(_tableView);
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
            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        private void _SetupData()
        {
            _callerIdView.UpdatePickerData(currentPhone);
        }
    }
}

