using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.ViewModels;
using FreedomVoice.iOS.Views;
using UIKit;
using AppearanceHelper = FreedomVoice.iOS.Utilities.Helpers.Appearance;

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

        #region Public Properties

        public static readonly NSString ExpandedCellId = new NSString("ExpandedCell");

        public int Duration => _message.Length;

        #endregion

        #region Private Properties

        private Message _message;

        private readonly UINavigationController _navigationController;

        private string _systemPhoneNumber;

        private bool IsFaxMessageType => _message?.Type == MessageType.Fax;

        #endregion

        public ExpandedCell(Message cellMessage, UINavigationController navigationController) : base (UITableViewCellStyle.Default, ExpandedCellId)
        {
            _message = cellMessage;
            _navigationController = navigationController;

            SetBackground(IsFaxMessageType);

            _image = AppearanceHelper.GetMessageImageView(48);
            _title = new UILabel(new CGRect(48, 9, Theme.ScreenBounds.Width - 63, 21)) { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(17) };
            _length = new UILabel(new CGRect(Theme.ScreenBounds.Width - 75, 30, 60, 15)) { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(12), TextAlignment = UITextAlignment.Right };
            _date = new UILabel(new CGRect(48, 30, _length.Frame.X - 53, 15)) { TextColor = UIColor.White, Font = UIFont.SystemFontOfSize(12) };
            
            DeleteButton = new UIButton(new CGRect(Theme.ScreenBounds.Width - 37, IsFaxMessageType ? 60 : 98, 25, 25));
            DeleteButton.SetBackgroundImage(UIImage.FromFile("delete.png"), UIControlState.Normal);

            AddSubviews(_image, _title, _date, _length, DeleteButton);
        }

        private void SetBackground(bool isFaxMessageType)
        {
            var gradientLayer = new CAGradientLayer
            {
                Frame = new CGRect(Bounds.X, Bounds.Y, Theme.ScreenBounds.Width, isFaxMessageType ? 100 : 138),
                Colors = new[] { UIColor.FromRGB(51, 71, 98).CGColor, UIColor.FromRGB(98, 120, 149).CGColor }
            };
            Layer.InsertSublayer(gradientLayer, 0);
        }

        public void UpdateCell(Message cellMessage, string systemPhoneNumber, AVPlayerView activePlayer)
        {
            _message = cellMessage;
            _systemPhoneNumber = systemPhoneNumber;

            _image.Image = AppearanceHelper.GetMessageImage(_message.Type, _message.Unread, true);

            _title.Text = _message.Title;
            _length.Text = AppearanceHelper.GetFormattedMessageLength(_message.Length, IsFaxMessageType);
            _date.Text = DataFormatUtils.ToFormattedDate("Yesterday", _message.ReceivedOn);
            
            _title.Center = new CGPoint(_title.Center.X, 16);
            _length.Center = new CGPoint(_length.Center.X, 35);
            _date.Center = new CGPoint(_date.Center.X, 35);

            DeleteButton.Frame = new CGRect(DeleteButton.Frame.X, IsFaxMessageType ? 60 : 98, DeleteButton.Frame.Width, DeleteButton.Frame.Height);

            InitSubviews(activePlayer);
            InitBackground();
            InitCommonStyles();
        }

        private void InitBackground()
        {
            Layer.Sublayers[0].RemoveFromSuperLayer();
            SetBackground(IsFaxMessageType);
        }

        private void InitSubviews(AVPlayerView activePlayer)
        {
            if (IsFaxMessageType)
            {
                if (_viewFaxButton == null)
                    AddSubview(GetFaxButton());

                foreach (var subView in Subviews)
                {
                    if (Equals(subView, _player))
                    {
                        _player?.RemoveFromSuperview();
                    }
                    else if (Equals(subView, _speakerButton))
                    {
                        _speakerButton?.RemoveFromSuperview();
                        _speakerButton = null;
                    }
                    else if (Equals(subView, _callBackButton))
                    {
                        _callBackButton?.RemoveFromSuperview();
                        _callBackButton = null;
                    }
                }
            }
            else
            {
                if (Subviews.Any(subView => Equals(subView, _viewFaxButton)))
                {
                    _viewFaxButton?.RemoveFromSuperview();
                    _viewFaxButton = null;
                }

                foreach (var subView in Subviews)
                {
                    if (Equals(subView, _player))
                    {
                        _player?.RemoveFromSuperview();
                    }
                    else if (Equals(subView, _viewFaxButton))
                    {
                        _viewFaxButton?.RemoveFromSuperview();
                        _viewFaxButton = null;
                    }
                }

                var playerView = GetPlayerView(activePlayer);
                AddSubview(playerView);

                if (_speakerButton == null)
                    AddSubview(GetSpeakerButton());

                if (string.IsNullOrEmpty(_message.SourceNumber))
                {
                    _callBackButton?.RemoveFromSuperview();
                    _callBackButton = null;
                }
                else if (_callBackButton == null)
                    AddSubview(GetCallBackButton());
            }
        }

        private void InitCommonStyles()
        {
            foreach (var btn in new List<UIButton> { _callBackButton, _viewFaxButton, _speakerButton }.Where(btn => btn != null))
            {
                btn.Font = UIFont.SystemFontOfSize(14);
                btn.TitleEdgeInsets = new UIEdgeInsets(0, 5, 0, 0);
                btn.ClipsToBounds = true;
                btn.Layer.CornerRadius = 5;
                btn.Layer.BorderWidth = 1;
            }
        }

        private static void BackgroundColorAnimate(UIButton btn, UIColor tapColor)
        {
            Animate(0.2, 0, UIViewAnimationOptions.Autoreverse, () => { btn.BackgroundColor = tapColor; }, () => { btn.BackgroundColor = UIColor.Clear; });
        }

        private AVPlayerView GetPlayerView(AVPlayerView activePlayerView)
        {
            _player = activePlayerView ?? new AVPlayerView(new CGRect(40, 48, Theme.ScreenBounds.Width - 55, 30), this) { OnPlayButtonClick = OnPlayerPlayButtonTouchDown };
            return _player;
        }

        private UIButton GetFaxButton()
        {
            _viewFaxButton = new UIButton(new CGRect(43, 58, 101, 28));
            _viewFaxButton.SetTitle("View Fax", UIControlState.Normal);
            _viewFaxButton.SetTitleColor(UIColor.FromRGB(198, 242, 138), UIControlState.Normal);
            _viewFaxButton.SetImage(UIImage.FromFile("view_fax.png"), UIControlState.Normal);
            _viewFaxButton.Layer.BorderColor = UIColor.FromRGBA(198, 242, 138, 90).CGColor;
            _viewFaxButton.TouchDown += OnFaxButtonTouchDown;

            return _viewFaxButton;
        }

        private UIButton GetSpeakerButton()
        {
            _speakerButton = new UIButton(new CGRect(43, 98, 98, 28));
            _speakerButton.SetTitle("Speaker", UIControlState.Normal);
            _speakerButton.SetTitleColor(UIColor.FromRGB(119, 229, 246), UIControlState.Normal);
            _speakerButton.SetImage(UIImage.FromFile("speaker.png"), UIControlState.Normal);            
            _speakerButton.Layer.BorderColor = UIColor.FromRGBA(119, 229, 246, 110).CGColor;            
            _speakerButton.TouchDown += OnSpeakerButtonTouchDown;
            
            return _speakerButton;
        }

        private UIButton GetCallBackButton()
        {
            _callBackButton = new UIButton(new CGRect(151, 98, 104, 28));
            _callBackButton.SetTitle("Call Back", UIControlState.Normal);
            _callBackButton.SetTitleColor(UIColor.FromRGB(198, 242, 138), UIControlState.Normal);
            _callBackButton.SetImage(UIImage.FromFile("call_back.png"), UIControlState.Normal);
            _callBackButton.Layer.BorderColor = UIColor.FromRGBA(198, 242, 138, 110).CGColor;
            _callBackButton.TouchDown += OnCallBackButtonTouchDown;

            return _callBackButton;
        }

        private void OnPlayerPlayButtonTouchDown(object sender, EventArgs args)
        {
            AppDelegate.ActivePlayerView = _player;
            AppDelegate.ActivePlayerMessageId = _message.Id;

            OnPlayClick?.Invoke(this, new ExpandedCellButtonClickEventArgs());
        }

        private void OnSpeakerButtonTouchDown(object sender, EventArgs args)
        {
            BackgroundColorAnimate(_speakerButton, UIColor.FromRGBA(119, 229, 246, 127));
            _player?.ToggleSoundOutput();
        }

        public async Task<string> GetMediaPath(MediaType mediaType)
        {
            var viewModel = new MediaViewModel(_systemPhoneNumber, _message.Mailbox, _message.Folder, _message.Id, mediaType, _navigationController);

            await viewModel.GetMediaAsync();

            return viewModel.FilePath;
        }

        private async void OnFaxButtonTouchDown(object sender, EventArgs args)
        {
            if (_message.Length == 0)
            {
                AppearanceHelper.ShowOkAlertWithMessage(_navigationController, AppearanceHelper.AlertMessageType.EmptyFileDownload);
                return;
            }

            var filePath = await GetMediaPath(MediaType.Pdf);
            if (!string.IsNullOrEmpty(filePath))
                OnViewFaxClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(filePath));
        }

        private void OnCallBackButtonTouchDown(object sender, EventArgs args)
        {
            BackgroundColorAnimate(_callBackButton, UIColor.FromRGBA(198, 242, 138, 127));
            OnCallbackClick?.Invoke(this, new ExpandedCellButtonClickEventArgs());
        }

        public event EventHandler<ExpandedCellButtonClickEventArgs> OnCallbackClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnPlayClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnViewFaxClick;
    }
}