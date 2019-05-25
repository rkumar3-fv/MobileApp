using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.Views
{
	internal class SelectActionDialog : UIView
	{
		private struct Appearance
		{
			public static readonly nfloat SideMargin = 8;
			public static readonly nfloat AnimationDuration = 0.24f;
		}
		
		private readonly UIView _containerView = new UIView();
		private readonly UILabel _titleLabel = new UILabel();
		private readonly VerticalButton _callPhoneButton = new VerticalButton();
		private readonly VerticalButton _sentSmsButton = new VerticalButton();

		public event Action CallPhoneButtonPressed;
		public event Action SendSMSButtonPressed;
		public event Action CancelledPressed;
        
		public SelectActionDialog()
		{
			BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f);
			Alpha = 0;
			AddGestureRecognizer(new UITapGestureRecognizer(() => { CancelledPressed?.Invoke(); }));

			AddSubviews();
			ConfigureSubviews();
			CreateConstraints();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (_containerView != null) 
				_containerView.Layer.ShadowPath = CGPath.FromRect(_containerView.Bounds);
		}

		private void AddSubviews()
		{
			AddSubview(_containerView);
			_containerView.AddSubview(_titleLabel);
			_containerView.AddSubview(_callPhoneButton);
			_containerView.AddSubview(_sentSmsButton);
		}

		private void ConfigureSubviews()
		{
			_containerView.TranslatesAutoresizingMaskIntoConstraints = false;
			_containerView.Layer.CornerRadius = 5;
			_containerView.Layer.ShadowColor = UIColor.Black.CGColor;
			_containerView.Layer.ShadowOpacity = 0.5f;
			_containerView.Layer.ShadowOffset = new CGSize(3, 3);
			_containerView.BackgroundColor = UIColor.White;

			_callPhoneButton.TranslatesAutoresizingMaskIntoConstraints = false;
			_callPhoneButton.SetTitle(ContactsViewControllerTexts.CallPhone);
			_callPhoneButton.SetImage(UIImage.FromFile("phone.png"));
			_callPhoneButton.SetTitleColor(UIColor.FromRGB(74, 152, 247));
			_callPhoneButton.SetTitleFont(UIFont.SystemFontOfSize(14));
			_callPhoneButton.TouchUpInside += () => CallPhoneButtonPressed?.Invoke();

			_sentSmsButton.TranslatesAutoresizingMaskIntoConstraints = false;
			_sentSmsButton.SetTitle(ContactsViewControllerTexts.SendSMS);
			_sentSmsButton.SetImage(UIImage.FromFile("sms.png"));
			_sentSmsButton.SetTitleColor(UIColor.FromRGB(74, 152, 247));
			_sentSmsButton.SetTitleFont(UIFont.SystemFontOfSize(14));
			_sentSmsButton.TouchUpInside += () => SendSMSButtonPressed?.Invoke();

			_titleLabel.TranslatesAutoresizingMaskIntoConstraints = false;
			_titleLabel.Text = ContactsViewControllerTexts.Select;
			_titleLabel.Font = UIFont.SystemFontOfSize(14);
		}

		private void CreateConstraints()
		{
			_containerView.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
			_containerView.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
			_containerView.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, Appearance.SideMargin).Active = true;
			_containerView.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -Appearance.SideMargin).Active = true;

			_titleLabel.LeadingAnchor.ConstraintEqualTo(_containerView.LeadingAnchor, Appearance.SideMargin).Active = true;
			_titleLabel.TrailingAnchor.ConstraintEqualTo(_containerView.TrailingAnchor, Appearance.SideMargin).Active = true;
			_titleLabel.TopAnchor.ConstraintEqualTo(_containerView.TopAnchor, Appearance.SideMargin).Active = true;
            
			_callPhoneButton.TopAnchor.ConstraintEqualTo(_titleLabel.BottomAnchor, Appearance.SideMargin).Active = true;
			_callPhoneButton.LeadingAnchor.ConstraintEqualTo(_containerView.LeadingAnchor, Appearance.SideMargin).Active = true;
			_callPhoneButton.BottomAnchor.ConstraintEqualTo(_containerView.BottomAnchor, -2 * Appearance.SideMargin).Active = true;
			_callPhoneButton.TrailingAnchor.ConstraintEqualTo(_sentSmsButton.LeadingAnchor, Appearance.SideMargin).Active = true;
			_callPhoneButton.WidthAnchor.ConstraintEqualTo(_sentSmsButton.WidthAnchor).Active = true;
			_callPhoneButton.HeightAnchor.ConstraintEqualTo(_sentSmsButton.HeightAnchor).Active = true;
     
			_sentSmsButton.TopAnchor.ConstraintEqualTo(_titleLabel.BottomAnchor, Appearance.SideMargin).Active = true;
			_sentSmsButton.TrailingAnchor.ConstraintEqualTo(_containerView.TrailingAnchor, -Appearance.SideMargin).Active = true;
			_sentSmsButton.BottomAnchor.ConstraintEqualTo(_containerView.BottomAnchor, - 2 * Appearance.SideMargin).Active = true;
		}
        
		public void Show()
		{
			UIView.Animate(Appearance.AnimationDuration, () => { Alpha = 1; });
		}

		public void Hide(Action completion)
		{
			UIView.Animate(Appearance.AnimationDuration, () => { Alpha = 0; }, completion);
		}
	}
}