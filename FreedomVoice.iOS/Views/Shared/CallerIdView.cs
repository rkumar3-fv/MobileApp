using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.ViewModels;
using UIKit;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;

namespace FreedomVoice.iOS.Views.Shared
{
    [Register("CallerIdView")]
    public class CallerIdView : UIView
    {
        private PresentationNumber _selectedPresentationNumber;

        private CallerIdPickerViewModel _callerIdPickerModel;

        private UILabel _labelField;
        private UIPickerView _pickerField;
        private UITextField _callerIdTextField;
        private UIImageView _dropdownImage;

        public CallerIdView(RectangleF bounds, IList<PresentationNumber> numbers) : base(bounds)
        {
            Initialize(numbers);
        }

        private void Initialize(IList<PresentationNumber> numbers)
        {
            var labelWidth = ((NSString)"Show as Caller ID:").StringSize(UIFont.SystemFontOfSize(17, UIFontWeight.Regular)).Width;
            _labelField = new UILabel(new CGRect(15, 0, labelWidth + 5, 40)) { Text = "Show as Caller ID:", Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular) };

            if (_selectedPresentationNumber == null && numbers != null && numbers.Count > 0)
                _selectedPresentationNumber = numbers[0];

            _callerIdPickerModel = new CallerIdPickerViewModel(numbers);
            if (_callerIdPickerModel.Items.Count == 0) return;

            _callerIdTextField = new UITextField
            {
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium),
                Frame = new CGRect(_labelField.Frame.X + _labelField.Frame.Width, 0, 140, 40),
                Text = _callerIdPickerModel.SelectedItem.FormattedPhoneNumber,
                UserInteractionEnabled = false,
                TintColor = UIColor.Clear
            };

            AddSubviews(_labelField, _callerIdTextField);

            if (_callerIdPickerModel.Items.Count == 1)
                return;

            _callerIdTextField.UserInteractionEnabled = true;
            _callerIdTextField.TouchDown += SetPicker;

            _callerIdPickerModel.ValueChanged += (s, e) => {
                _selectedPresentationNumber = _callerIdPickerModel.SelectedItem;
                CallerIdEvent.OnCallerIdChangedEvent(new CallerIdEventArgs(_selectedPresentationNumber));
            };

            _pickerField = new UIPickerView
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                ShowSelectionIndicator = true,
                Hidden = false,
                Model = _callerIdPickerModel
            };

            _pickerField.Select(_callerIdPickerModel.Items.IndexOf(_selectedPresentationNumber), 0, true);

            _dropdownImage = new UIImageView(new CGRect(_callerIdTextField.Frame.X + _callerIdTextField.Frame.Width + 5, 17, 10, 7)) { Image = UIImage.FromFile("dropdown_arrow.png") };

            var toolbar = new UIToolbar { BarStyle = UIBarStyle.Black, Translucent = true, BarTintColor = Theme.BarBackgroundColor };
            toolbar.SizeToFit();

            var actionLabel = new UILabel(new CGRect(0, 15, 160, 15)) { Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium), Text = "Show as Caller ID", TextAlignment = UITextAlignment.Center };
            actionLabel.Center = new CGPoint(Center.X, actionLabel.Center.Y);
            toolbar.AddSubview(actionLabel);

            var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) =>
            {
                var phoneNumberFieldWidth = ((NSString)_selectedPresentationNumber.FormattedPhoneNumber).StringSize(UIFont.SystemFontOfSize(17, UIFontWeight.Medium)).Width;
                _dropdownImage.Frame =  new CGRect(_callerIdTextField.Frame.X + phoneNumberFieldWidth + 5, _dropdownImage.Frame.Y, _dropdownImage.Frame.Width, _dropdownImage.Frame.Height);
                _callerIdTextField.Text = _selectedPresentationNumber.FormattedPhoneNumber;
                _callerIdTextField.ResignFirstResponder();
            }) { TintColor = Theme.BlueColor };

            toolbar.SetItems(new[] { doneButton }, true);

            _callerIdTextField.InputView = _pickerField;
            _callerIdTextField.InputView.BackgroundColor = UIColor.White;
            _callerIdTextField.InputAccessoryView = toolbar;

            AddSubview(_dropdownImage);
        }

        public void UpdatePickerData(PresentationNumber selectedPresentationNumber)
        {
            _selectedPresentationNumber = selectedPresentationNumber;
            if (selectedPresentationNumber == null) return;

            _callerIdTextField.Text = selectedPresentationNumber.FormattedPhoneNumber;

            if (_callerIdPickerModel.Items.Count == 1)
                return;

            _pickerField.Select(_callerIdPickerModel.Items.IndexOf(selectedPresentationNumber), 0, true);

            var phoneNumberFieldWidth = ((NSString)selectedPresentationNumber.FormattedPhoneNumber).StringSize(UIFont.SystemFontOfSize(17, UIFontWeight.Medium)).Width;
            _dropdownImage.Frame = new CGRect(_labelField.Frame.X + _labelField.Frame.Width + phoneNumberFieldWidth + 5, _dropdownImage.Frame.Y, _dropdownImage.Frame.Width, _dropdownImage.Frame.Height);
        }

        private void SetPicker(object sender, EventArgs e)
        {
            if (_selectedPresentationNumber != null)
                _pickerField.Select(_callerIdPickerModel.Items.IndexOf(_selectedPresentationNumber), 0, true);
        }
    }
}