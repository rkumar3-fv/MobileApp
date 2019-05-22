﻿using System;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using FreedomVoice.Entities.Enums;
using FreedomVoice.iOS.Views;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells.Texting.Messaging
{
    public partial class OutgoingMessageTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("OutgoingMessageTableViewCell");
        public static readonly UINib Nib;

        static OutgoingMessageTableViewCell()
        {
            Nib = UINib.FromName("OutgoingMessageTableViewCell", NSBundle.MainBundle);
        }

        protected OutgoingMessageTableViewCell(IntPtr handle) : base(handle)
        {
        }

        private void _startSending()
        {
            foreach (var item in SendingView.ArrangedSubviews.ToList().Select((r, i) => new {Row=r, Index=i}))
            {
                Animate(
                    0.8f,
                    item.Index,
                    UIViewAnimationOptions.Repeat | UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.CurveEaseIn,
                    () => item.Row.Alpha = 0.5f,
                    null);
            }
        }

        private void _stopSending()
        {
            foreach (var view in SendingView.ArrangedSubviews)
            {
                view.Layer.RemoveAllAnimations();
            }
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            base.AwakeFromNib();
            var image = new UIImage("bubble_sent.png")
                .CreateResizableImage(new UIEdgeInsets(17, 21, 17, 21), UIImageResizingMode.Stretch)
                .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            BubbleView.Image = image;
            BubbleView.TintColor = new UIColor(0.37f, 0.81f, 0.36f, 1.0f);
            MessageLabel.TextColor = UIColor.White;
            foreach (var view in SendingView.ArrangedSubviews)
            {
                view.ClipsToBounds = true;
                view.Layer.CornerRadius = view.Frame.Width / 2;
            }
        }

        public string Text
        {
            set => MessageLabel.Text = value;
        }

        public string Time
        {
            set => TimeLabel.Text = value;
        }

        private SendingState _state;
        public SendingState State
        {
            set
            {
                _state = value;
                switch (value)
                {
                    case SendingState.Error: 
                        SendingView.Hidden = true;
                        _stopSending();
                        ErrorContainer.Hidden = false;
                        break;
                    case SendingState.Sending:
                        SendingView.Hidden = false;
                        ErrorContainer.Hidden = true;
                        _startSending();
                        break;
                    default:
                        SendingView.Hidden = true;
                        _stopSending();
                        ErrorContainer.Hidden = true;
                        break;
                }
            }
        }

        public void VisibilityUpdated()
        {
            State = _state;
        }
    }
}
