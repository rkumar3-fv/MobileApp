using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.ViewModels;
using UIKit;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.Views.Shared
{
    [Register("CallerIdView")]
    public class CallerIdView : UIView
    {
        private PresentationNumber _selectedPresentationNumber;

        private CallerIdPickerViewModel _callerIdPickerModel;
        private UIPickerView _pickerField;
        private UITextField _callerIdTextField;

        public CallerIdView(RectangleF bounds, IList<PresentationNumber> numbers) : base(bounds)
        {
            Initialize(numbers);
        }

        void Initialize(IList<PresentationNumber> numbers)
        {
            var labelField = new UILabel(new CGRect(15, 0, Theme.ScreenBounds.Width / 2 - 15, 40)) { Text = "Show as Caller ID:", Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular) };

            if (_selectedPresentationNumber == null && numbers != null && numbers.Count > 0)
                _selectedPresentationNumber = numbers[0];

            _callerIdPickerModel = new CallerIdPickerViewModel(numbers);
            if (_callerIdPickerModel.Items.Count == 0) return;

            _callerIdPickerModel.ValueChanged += (s, e) => {
                _selectedPresentationNumber = _callerIdPickerModel.SelectedItem;
                CallerIDEvent.OnCallerIDChangedEvent(new CallerIDEventArgs(_selectedPresentationNumber));
            };

            _pickerField = new UIPickerView
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                ShowSelectionIndicator = true,
                Hidden = false,
                Model = _callerIdPickerModel
            };

            _pickerField.Select(_callerIdPickerModel.Items.IndexOf(_selectedPresentationNumber), 0, true);

            _callerIdTextField = new UITextField
            {
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium),
                Frame = new CGRect(labelField.Frame.X + labelField.Frame.Width, 0, Theme.ScreenBounds.Width / 2, 40),
                UserInteractionEnabled = true,
                Text = _callerIdPickerModel.SelectedItem.FormattedPhoneNumber,
                TintColor = UIColor.Clear
            };
            _callerIdTextField.TouchDown += SetPicker;

            var dropdownImage = new UIImageView(new CGRect(295, 16, 10, 7)) { Image = UIImage.FromFile("dropdown_arrow.png") };

            var toolbar = new UIToolbar { BarStyle = UIBarStyle.Black, Translucent = true, BarTintColor = Theme.BarBackgroundColor };
            toolbar.SizeToFit();

            var actionLabel = new UILabel(new CGRect(0, 15, 160, 15)) { Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium), Text = "Show as Caller ID", TextAlignment = UITextAlignment.Center };
            actionLabel.Center = new CGPoint(Center.X, actionLabel.Center.Y);
            toolbar.AddSubview(actionLabel);

            var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) =>
            {
                _callerIdTextField.Text = _selectedPresentationNumber.FormattedPhoneNumber;
                _callerIdTextField.ResignFirstResponder();
            }) { TintColor = Theme.BlueColor };

            toolbar.SetItems(new[] { doneButton }, true);

            _callerIdTextField.InputView = _pickerField;
            _callerIdTextField.InputView.BackgroundColor = UIColor.White;
            _callerIdTextField.InputAccessoryView = toolbar;

            AddSubviews(labelField, _callerIdTextField, dropdownImage);
        }

        public void UpdatePickerData(PresentationNumber selectedPresentationNumber)
        {
            _selectedPresentationNumber = selectedPresentationNumber;
            if (selectedPresentationNumber == null) return;

            _pickerField.Select(_callerIdPickerModel.Items.IndexOf(selectedPresentationNumber), 0, true);
            _callerIdTextField.Text = selectedPresentationNumber.FormattedPhoneNumber;
        }

        private void SetPicker(object sender, EventArgs e)
        {
            if (_selectedPresentationNumber != null)
                _pickerField.Select(_callerIdPickerModel.Items.IndexOf(_selectedPresentationNumber), 0, true);
        }
    }
}