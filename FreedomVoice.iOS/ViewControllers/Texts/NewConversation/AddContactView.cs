using System;
using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts.NewConversation
{
	internal sealed class AddContactView : UIView
	{
		public Action AddContactButtonPressed;
		public Action<string> PhoneNumberChanged;

		public string Text
		{
			get => _contactInputTextView.Text;
			set => _contactInputTextView.Text = value;
		}

		private readonly UILabel _prefixLabel = new UILabel();
		private readonly UIButton _addContactButton = new UIButton();
		private readonly UITextField _contactInputTextView = new UITextField();

		private readonly nfloat _addContactButtonSize = _height - 8 * 2;
		private static readonly nfloat _height = 40;
		
		
		public AddContactView() : base(CGRect.Empty)
		{
			ConfigureSubviews();
			AddSubviews();
			MakeConstraints();
		}

		public override void MovedToSuperview()
		{
			base.MovedToSuperview();
			_addContactButton.TouchUpInside += AddContactButtonPressedHandler;
			_contactInputTextView.EditingChanged += ContactInputTextViewOnValueChanged;
		}

		public override void RemoveFromSuperview()
		{
			base.RemoveFromSuperview();
			_addContactButton.TouchUpInside -= AddContactButtonPressedHandler;
			_contactInputTextView.EditingChanged -= ContactInputTextViewOnValueChanged;
		}

		public override bool BecomeFirstResponder()
		{
			return _contactInputTextView.BecomeFirstResponder();
		}

		private void ConfigureSubviews()
		{
			BackgroundColor = UIColor.FromRGB(250, 250, 250);

			_prefixLabel.Font = UIFont.SystemFontOfSize(16);
			_prefixLabel.TextColor = UIColor.Gray;
			_prefixLabel.Text = NewConversationTexts.To;


			var addContactButtonColor = UIColor.FromRGB(74, 152, 247);
			_addContactButton.SetTitle(NewConversationTexts.Plus, UIControlState.Normal);
			_addContactButton.Font = UIFont.SystemFontOfSize(16);
			_addContactButton.SetTitleColor(addContactButtonColor, UIControlState.Normal);
			_addContactButton.Layer.CornerRadius = _addContactButtonSize / 2;
			_addContactButton.Layer.BorderColor = addContactButtonColor.CGColor;
			_addContactButton.Layer.BorderWidth = 1;
		}

		private void AddSubviews()
		{
			AddSubviews(_prefixLabel, _addContactButton, _contactInputTextView);
		}

		private void MakeConstraints()
		{
			_prefixLabel.TranslatesAutoresizingMaskIntoConstraints = false;
			_addContactButton.TranslatesAutoresizingMaskIntoConstraints = false;
			_contactInputTextView.TranslatesAutoresizingMaskIntoConstraints = false;
			
			var viewsAndMetrics = new object[]
			{
				nameof(_prefixLabel), _prefixLabel, 
				nameof(_addContactButton), _addContactButton,
				nameof(_contactInputTextView), _contactInputTextView
			};

			var _prefixLabelWidth = _prefixLabel.SizeThatFits(new CGSize(nfloat.MaxValue, _height)).Width;
			AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-[_prefixLabel({_prefixLabelWidth})]-[_contactInputTextView]-[_addContactButton({_addContactButtonSize})]-|", 0, viewsAndMetrics));
			AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-[_prefixLabel]-|", 0, viewsAndMetrics));
			AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-[_addContactButton]-|", 0, viewsAndMetrics));
			AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[_contactInputTextView]|", 0, viewsAndMetrics));
			
			HeightAnchor.ConstraintEqualTo(_height).Active = true;
		}
		
		private void AddContactButtonPressedHandler(object sender, EventArgs e)
		{
			AddContactButtonPressed?.Invoke();
		}
		
		private void ContactInputTextViewOnValueChanged(object sender, EventArgs e)
		{
			PhoneNumberChanged?.Invoke(Text);
		}
	}
}