using System;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.Views
{
	internal class VerticalButton: UIView
	{
		private readonly UILabel TitleLabel = new UILabel();
		private readonly UIImageView ImageView = new UIImageView();

		public Action TouchUpInside;

		public void SetImage(UIImage image)
		{
			ImageView.Image = image;
		}

		public void SetTitle(string title)
		{
			TitleLabel.Text = title;
		}
		
		public void SetTitle(NSAttributedString title)
		{
			TitleLabel.AttributedText = title;
		}

		public void SetTitleColor(UIColor color)
		{
			TitleLabel.TextColor = color;
		}

		public void SetTitleFont(UIFont font)
		{
			TitleLabel.Font = font;
		}
        
		public VerticalButton()
		{
			TitleLabel.TranslatesAutoresizingMaskIntoConstraints = false;
			TitleLabel.TextAlignment = UITextAlignment.Center;
            
			ImageView.TranslatesAutoresizingMaskIntoConstraints = false;
			ImageView.ContentMode = UIViewContentMode.Center;
            
			AddSubviews(TitleLabel, ImageView);

			ImageView.TopAnchor.ConstraintEqualTo(this.TopAnchor, 8).Active = true;
			ImageView.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8).Active = true;
			ImageView.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -8).Active = true;    
            
			TitleLabel.TopAnchor.ConstraintEqualTo(ImageView.BottomAnchor, 4).Active = true;
			TitleLabel.BottomAnchor.ConstraintEqualTo(this.BottomAnchor, -8).Active = true;
			TitleLabel.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8).Active = true;
			TitleLabel.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -8).Active = true;      
          
			AddGestureRecognizer(new UITapGestureRecognizer(() => { TouchUpInside?.Invoke(); }));
		}
	}
}