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
using FreedomVoice.Core.Utils;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ExpandedCell : UITableViewCell
    {
        #region Controls

        private readonly UIImageView _image;

        private readonly UILabel _title;
        private readonly UILabel _date;
        private readonly UILabel _length;

        private AVPlayerView _player;

        private UIButton _callBackButton;
        private UIButton _speakerButton;
        private UIButton _viewFaxButton;

        public readonly UIButton DeleteButton;

        #endregion

        private Message _message;

        private string _systemPhoneNumber;

        private readonly UINavigationController _navigationController;

        public static readonly NSString ExpandedCellId = new NSString("ExpandedCell");
        public ExpandedCell(Message cellMessage, UINavigationController navigationController) : base (UITableViewCellStyle.Default, ExpandedCellId)
        {
            _message = cellMessage;
            _navigationController = navigationController;

            var isFaxMessageType = _message.Type == MessageType.Fax;

            _image = GetMessageImageView();
            _title = new UILabel(new CGRect(55, 10, 270, 19)) { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(17) };
            _date = new UILabel(new CGRect(55, 29, 120, 11)) { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(12) };
            _length = new UILabel(new CGRect(isFaxMessageType ? 257 : 275, 29, 50, 13)) { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(12) };

            DeleteButton = new UIButton(new CGRect(285, isFaxMessageType ? 60 : 99, 25, 25));
            DeleteButton.SetBackgroundImage(UIImage.FromFile("delete.png"), UIControlState.Normal);            
        }

        private void SetBackground(bool isFaxMessageType)
        {
            var gradientLayer = new CAGradientLayer
            {
                Frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width, isFaxMessageType ? 100 : 138),
                Colors = new[] { UIColor.FromRGB(51, 71, 98).CGColor, UIColor.FromRGB(98, 120, 149).CGColor }
            };
            Layer.AddSublayer(gradientLayer);
        }

        public void UpdateCell(Message cellMessage, string systemPhoneNumber)
        {
            _message = cellMessage;
            _systemPhoneNumber = systemPhoneNumber;

            var isFaxMessageType = _message.Type == MessageType.Fax;
            SetBackground(isFaxMessageType);

            AddSubviews(_image, _title, _date, _length, DeleteButton);

            _image.Image = Helpers.Appearance.GetMessageImage(_message.Type, _message.Unread, true);

            _title.Text = !string.IsNullOrEmpty(_message.SourceName.Trim()) ? _message.SourceName.Trim() : (!string.IsNullOrEmpty(_message.SourceNumber.Trim()) ? DataFormatUtils.ToPhoneNumber(_message.SourceNumber.Trim()) : "Unavailable");
            _date.Text = DataFormatUtils.ToFormattedDate("Yesterday", _message.ReceivedOn);

            _length.Text = GetFormattedLength(_message.Length, isFaxMessageType);
            _length.Frame = new CGRect(isFaxMessageType ? 257 : 275, _length.Frame.Y, _length.Frame.Width, _length.Frame.Height);

            DeleteButton.Frame = new CGRect(DeleteButton.Frame.X, isFaxMessageType ? 60 : 99, DeleteButton.Frame.Width, DeleteButton.Frame.Height);

            AddSubviews(isFaxMessageType ? new UIView[] { GetFaxButton() } : new UIView[] { _player = GetPlayerView(), GetSpeakerButton(), GetCallBackButton() });

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

        private static void BackgroundColorAnimate(UIButton btn, UIColor tapColor)
        {
            Animate(0.2, 0, UIViewAnimationOptions.Autoreverse, () => { btn.BackgroundColor = tapColor; }, () => { btn.BackgroundColor = UIColor.Clear; });
        }

        private static UIImageView GetMessageImageView()
        {
            return new UIImageView(new CGRect(15, 15, 25, 25));
        }

        private AVPlayerView GetPlayerView()
        {
            return new AVPlayerView(new CGRect(46, 54, 256, 23), this);
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
            _player?.ToggleSoundOutput();
        }

        private static string GetFormattedLength(int length, bool faxMessageType)
        {
            return faxMessageType ? DataFormatUtils.PagesToFormattedString(length) : DataFormatUtils.ToDuration(length);
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

        public event EventHandler<ExpandedCellButtonClickEventArgs> OnCallbackClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnViewFaxClick;

        public int GetDuration()
        {
            return _message.Length;
        }
    }
}