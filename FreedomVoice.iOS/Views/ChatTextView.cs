using System;
using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    public class ChatTextView: UIView, IUITextViewDelegate
    {
        private UITextView _textView;
        private UIButton _button;
        private UIActivityIndicatorView _indicatorView;
        private NSLayoutConstraint _heightConstraint;

        private nfloat minHeight => 36;
        private nfloat maxHeight => 156;

        public string Text
        {
            get => _textView.Text;
            set
            {
                _textView.Text = value;
                Changed(_textView);
            }
        }

        public Action SendButtonPressed;
        
        public ChatTextView() 
        {
            Init();

            _setupViews();
            _setupConstraints();
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();
            _button.TouchUpInside += ButtonOnTouchUpInside;
        }

        public override void RemoveFromSuperview()
        {
            base.RemoveFromSuperview();
            _button.TouchUpInside -= ButtonOnTouchUpInside;
        }

        public void SetSending(bool sending)
        {
            _button.Hidden = sending;
            _indicatorView.Hidden = !sending;
            if(sending)
            {
                _indicatorView.StartAnimating();
            }
            else
            {
                _indicatorView.StopAnimating();
            }

        }

        private void _setupViews()
        {
            _textView = new UITextView(CGRect.Empty)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.SystemFontOfSize(15),
                ContentInset = UIEdgeInsets.Zero,
                TextContainerInset = new UIEdgeInsets(8, 8, 8, 8),
                Delegate = this,
                ClipsToBounds = true
            };

            _button = new UIButton(UIButtonType.Plain)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.LightGray
            };

            _button.SetTitleColor(UIColor.White, UIControlState.Normal);
            _button.SetImage(new UIImage("arrow_right"), UIControlState.Normal);
            _button.Layer.CornerRadius = 17;
            _button.ClipsToBounds = true;
            AddSubview(_textView);
            AddSubview(_button);

            _textView.Layer.CornerRadius = 18;
            _textView.Layer.BorderWidth = 0.5f;
            _textView.Layer.BorderColor = new UIColor(0.90f, 0.90f, 0.91f, 1.0f).CGColor;

            _indicatorView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _indicatorView.Hidden = true;
            AddSubview(_indicatorView);
        }

        private void _setupConstraints()
        {
            _textView.TopAnchor.ConstraintEqualTo(TopAnchor, 6).Active = true;
            _textView.LeftAnchor.ConstraintEqualTo(LeftAnchor, 6).Active = true;
            _textView.RightAnchor.ConstraintEqualTo(_button.LeftAnchor, -8).Active = true;
            _textView.BottomAnchor.ConstraintEqualTo(BottomAnchor, -6).Active = true;

            _heightConstraint = _textView.HeightAnchor.ConstraintEqualTo(minHeight);
            _heightConstraint.Priority = 999;
            _heightConstraint.Active = true;
            _textView.HeightAnchor.ConstraintLessThanOrEqualTo(maxHeight).Active = true;

            _button.BottomAnchor.ConstraintEqualTo(BottomAnchor, -8).Active = true;
            _button.RightAnchor.ConstraintEqualTo(RightAnchor, -8).Active = true;
            _button.HeightAnchor.ConstraintEqualTo(32).Active = true;
            _button.WidthAnchor.ConstraintEqualTo(32).Active = true;

            _indicatorView.CenterXAnchor.ConstraintEqualTo(_button.CenterXAnchor).Active = true;
            _indicatorView.CenterYAnchor.ConstraintEqualTo(_button.CenterYAnchor).Active = true;

        }
        

        [Foundation.Export("textViewDidChange:")]
        public virtual void Changed(UITextView textView)
        {
            var isEmpty = string.IsNullOrEmpty(textView.Text);
            _button.Enabled = !isEmpty;
            _button.BackgroundColor = isEmpty ? UIColor.LightGray : new UIColor(red: 0.37f, green: 0.81f, blue: 0.36f, alpha: 1.0f);

            var fixedWidth = textView.Frame.Size.Width;
            var newSize = textView.SizeThatFits(new CGSize(fixedWidth, nfloat.MaxValue));
            var newHeight = newSize.Height;
            if( newHeight < minHeight )
            {
                newHeight = minHeight;
            }
            if( newHeight > maxHeight )
            {
                newHeight = maxHeight;
            }

            if (newHeight == _heightConstraint.Constant) return;
            _heightConstraint.Constant = newHeight;
        }
        
        private void ButtonOnTouchUpInside(object sender, EventArgs e)
        {
            SendButtonPressed?.Invoke();
        }
    }
}