using CoreAnimation;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;
using FreedomVoice.iOS.ViewModels;
using System.Threading.Tasks;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ExpandedCell : UITableViewCell
    {
        #region Controls

        private UIImageView _icon;

        private UILabel _title;
        private UILabel _date;
        private UILabel _length;

        private AVPlayerView _player;

        private UIButton _callBackButton;
        private UIButton _deleteButton;
        private UIButton _speakerButton;
        private UIButton _viewFaxButton;

        #endregion

        private readonly Message _message;

        private string _systemPhoneNumber;
        
        private UINavigationController _navigationController;

        private static readonly NSString ExpandedCellId = new NSString("ExpandedCell");
        public ExpandedCell(Message message) : base (UITableViewCellStyle.Default, ExpandedCellId)
        {
            _message = message;
            SetBackground();            
        }

        private void SetBackground()
        {
            nfloat bgHeight = _message.Type == MessageType.Fax ? 100 : 138;
            var gradientLayer = new CAGradientLayer
            {
                Frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width, bgHeight),
                Colors = new[] { UIColor.FromRGB(51, 71, 98).CGColor, UIColor.FromRGB(98, 120, 149).CGColor }
            };
            Layer.AddSublayer(gradientLayer);
        }

        public void UpdateCell(string systemPhoneNumber, UINavigationController navigationController)
        {
            _systemPhoneNumber = systemPhoneNumber;
            _navigationController = navigationController;

            var selectedViews = new List<UIView>();
            var faxMessageType = _message.Type == MessageType.Fax;

            _icon = Helpers.Appearance.GetMessageImageView(_message.Type, false, true);
            _title = new UILabel(new CGRect(55, 10, 270, 19)) { Text = _message.Name, TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(17) };
            _date = new UILabel(new CGRect(55, 29, 120, 11)) { Text = Formatting.DateTimeFormat(_message.ReceivedOn), TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(12) };

            _length = GetFormattedLength(_message.Length, faxMessageType);

            _deleteButton = new UIButton(new CGRect(285, faxMessageType ? 60 : 99, 25, 25));
            _deleteButton.SetBackgroundImage(UIImage.FromFile("delete.png"), UIControlState.Normal);
            _deleteButton.TouchDown += OnDeleteButtonTouchDown;

            selectedViews.AddRange(new List<UIView> { _icon, _title, _date, _length, _deleteButton });

            if (faxMessageType)
                selectedViews.Add(GetFaxButton());
            else
            {
                _player = new AVPlayerView(new CGRect(46, 54, 256, 23), this);
                selectedViews.AddRange(new List<UIView> { _player, GetSpeakerButton(), GetCallBackButton() });
            }

            AddSubviews(selectedViews.ToArray());

            InitCommonStyles();
        }

        private void InitCommonStyles()
        {
            foreach (var btn in new List<UIButton> { _callBackButton, _viewFaxButton, _speakerButton }.Where(btn => btn != null))
            {
                btn.Font = UIFont.SystemFontOfSize(14);
                btn.ClipsToBounds = true;
                btn.Layer.CornerRadius = 5;
                btn.Layer.BorderWidth = 1;
            }
        }

        private static UILabel GetFormattedLength(double length, bool faxMessageType)
        {
            var formattedLength = faxMessageType ? Formatting.PagesToFormattedString(length) : Formatting.SecondsToFormattedString(length);
            return new UILabel(new CGRect(faxMessageType ? 257 : 275, 29, 50, 13))
            {
                Text = formattedLength,
                TextColor = UIColor.White,
                Font = UIFont.SystemFontOfSize(12)
            };
        }

        private static void BackgroundColorAnimate(UIButton btn, UIColor tapColor)
        {
            Animate(0.2, 0, UIViewAnimationOptions.Autoreverse, () => { btn.BackgroundColor = tapColor; }, () => { btn.BackgroundColor = UIColor.Clear; });
        }

        private UIButton GetSpeakerButton()
        {
            _speakerButton = new UIButton(new CGRect(52, 97, 98, 29));
            _speakerButton.SetTitle("Speaker", UIControlState.Normal);
            _speakerButton.SetTitleColor(UIColor.FromRGB(119, 229, 246), UIControlState.Normal);
            _speakerButton.SetImage(UIImage.FromFile("speaker.png"), UIControlState.Normal);            
            _speakerButton.Layer.BorderColor = UIColor.FromRGBA(119, 229, 246, 110).CGColor;            
            _speakerButton.TouchDown += OnSpeakerButtonTouchDown;
            
            return _speakerButton;
        }

        private UIButton GetFaxButton()
        {
            _viewFaxButton = new UIButton(new CGRect(52, 58, 98, 28));
            _viewFaxButton.SetTitle("View Fax", UIControlState.Normal);
            _viewFaxButton.SetTitleColor(UIColor.FromRGB(198, 242, 138), UIControlState.Normal);
            _viewFaxButton.SetImage(UIImage.FromFile("view_fax.png"), UIControlState.Normal);
            _viewFaxButton.Layer.BorderColor = UIColor.FromRGBA(198, 242, 138, 90).CGColor;
            _viewFaxButton.TouchDown += OnFaxButtonTouchDown;

            return _viewFaxButton;
        }

        private UIButton GetCallBackButton()
        {
            _callBackButton = new UIButton(new CGRect(158, 97, 104, 29));
            _callBackButton.SetTitle("Call Back", UIControlState.Normal);
            _callBackButton.SetTitleColor(UIColor.FromRGB(198, 242, 138), UIControlState.Normal);
            _callBackButton.SetImage(UIImage.FromFile("call_back.png"), UIControlState.Normal);
            _callBackButton.Layer.BorderColor = UIColor.FromRGBA(198, 242, 138, 110).CGColor;
            _callBackButton.TouchDown += OnCallBackButtonTouchDown;

            return _callBackButton;
        }

        private void OnSpeakerButtonTouchDown(object sender, EventArgs args)
        {
            BackgroundColorAnimate(_speakerButton, UIColor.FromRGBA(119, 229, 246, 127));
        }

        public async Task<string> GetMediaPath(MediaType mediaType)
        {
            var viewModel = new MediaViewModel(_systemPhoneNumber, _message.Mailbox, _message.Folder, _message.Id, mediaType, _navigationController);

            await viewModel.GetMediaAsync();

            return viewModel.FilePath;         
        }

        private async void OnFaxButtonTouchDown(object sender, EventArgs args)
        {
            var filePath = await GetMediaPath(MediaType.Pdf);
            OnViewFaxClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(filePath));
        }

        private void OnCallBackButtonTouchDown(object sender, EventArgs args)
        {
            BackgroundColorAnimate(_callBackButton, UIColor.FromRGBA(198, 242, 138, 127));
            OnCallbackClick?.Invoke(this, new ExpandedCellButtonClickEventArgs());
        }

        private void OnDeleteButtonTouchDown(object sender, EventArgs e)
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("Delete", UIAlertActionStyle.Destructive, a => {
                OnRowDeleteMessageClick?.Invoke(this, new ExpandedCellButtonClickEventArgs());
            }));
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, a => { }));

            _navigationController.PresentViewController(alertController, true, null);
        }

        public event EventHandler<ExpandedCellButtonClickEventArgs> OnCallbackClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnViewFaxClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowDeleteMessageClick;

        public double GetDuration()
        {
            return _message.Length;
        }
    }
}