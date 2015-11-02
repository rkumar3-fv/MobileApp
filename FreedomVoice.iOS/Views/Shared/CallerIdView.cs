using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.ViewModels;
using UIKit;
using FreedomVoice.iOS.Helpers;

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
            var dropdownImage = new UIImageView(new CGRect(278, 16, 10, 7)) { Image = UIImage.FromFile("dropdown_arrow.png") };

            var labelField = new UILabel(new CGRect(15, 0, 145, 40)) {
                Text = "Show as Caller ID:",
                Font = UIFont.SystemFontOfSize(16f, UIFontWeight.Regular)};

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
                Font = UIFont.SystemFontOfSize(16f, UIFontWeight.Medium),
                Frame = new CGRect(150, 0, 160, 40),
                UserInteractionEnabled = true,
                Text = _callerIdPickerModel.SelectedItem.FormattedPhoneNumber,
                TintColor = UIColor.Clear
            };
            _callerIdTextField.TouchDown += SetPicker;

            var toolbar = new UIToolbar { BarStyle = UIBarStyle.Black, Translucent = true, BarTintColor = UIColor.FromRGB(230, 234, 238),  };
            toolbar.SizeToFit();

            var actionLbl = new UILabel(new CGRect(100, 15, 140, 15))  { Font = UIFont.SystemFontOfSize(16, UIFontWeight.Medium), Text="Show as Caller ID" };
            toolbar.AddSubview(actionLbl);

            var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) =>
            {
                _callerIdTextField.Text = _selectedPresentationNumber.FormattedPhoneNumber;
                _callerIdTextField.ResignFirstResponder();
            }){  TintColor = UIColor.FromRGB(3, 138, 193) };

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