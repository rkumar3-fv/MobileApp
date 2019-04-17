using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    public class ChatTextView: UIView
    {
        private UITextView _textView;
        private UIButton _button;

        public ChatTextView() 
        {
            Init();

            _setupViews();
            _setupConstraints();
        }

        private void _setupViews()
        {
            
            _textView = new UITextView(CGRect.Empty)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Green
            };

            _button = new UIButton(UIButtonType.Plain)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Blue
            };
            _button.SetTitle("Send", UIControlState.Normal);
            AddSubview(_textView);
            AddSubview(_button);
            BackgroundColor = UIColor.Red;
        }
        
        private void _setupConstraints()
        {
            _textView.TopAnchor.ConstraintEqualTo(TopAnchor, 4).Active = true;
            _textView.LeftAnchor.ConstraintEqualTo(LeftAnchor, 4).Active = true;
            _textView.RightAnchor.ConstraintEqualTo(RightAnchor, -4).Active = true;
            _textView.BottomAnchor.ConstraintEqualTo(BottomAnchor, -4).Active = true;
            _textView.HeightAnchor.ConstraintGreaterThanOrEqualTo(56).Active = true;
            _textView.HeightAnchor.ConstraintLessThanOrEqualTo(156).Active = true;

            _button.BottomAnchor.ConstraintEqualTo(BottomAnchor, 4).Active = true;
            _button.RightAnchor.ConstraintEqualTo(RightAnchor, 4).Active = true;
            _button.HeightAnchor.ConstraintEqualTo(40).Active = true;
            _button.WidthAnchor.ConstraintEqualTo(40).Active = true;
            
        }
        
    }
}