using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.ViewModels;
using UIKit;
using FreedomVoice.iOS.Helpers;

namespace FreedomVoice.iOS.SharedViews
{
    [Register("CallerIdView")]
    public class CallerIdView : UIView
    {
        public PresentationNumber SelectedPresentationNumber;

        private CallerIdPickerViewModel _callerIdPickerModel;
        private UIPickerView _pickerField;
        private UITextField _callerIdTextField;

        public CallerIdView(IList<PresentationNumber> numbers)
        {
            Initialize(numbers);
        }

        public CallerIdView(RectangleF bounds, IList<PresentationNumber> numbers) : base(bounds)
        {
            Initialize(numbers);
        }

        void Initialize(IList<PresentationNumber> numbers)
        {
            var labelField = new UILabel(new CGRect(15, 0, 145, 44)) { Text = "Show as Caller ID:" };

            if (SelectedPresentationNumber == null && numbers != null && numbers.Count > 0)
                SelectedPresentationNumber = numbers[0];

            _callerIdPickerModel = new CallerIdPickerViewModel(numbers);
            _callerIdPickerModel.ValueChanged += (s, e) => {
                SelectedPresentationNumber = _callerIdPickerModel.SelectedItem;
                CallerIDEvent.OnCallerIDChangedEvent(new CallerIDEventArgs(SelectedPresentationNumber));
            };

            _pickerField = new UIPickerView
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                ShowSelectionIndicator = true,
                Hidden = false,
                Model = _callerIdPickerModel
            };

            _pickerField.Select(_callerIdPickerModel.Items.IndexOf(SelectedPresentationNumber), 0, true);

            _callerIdTextField = new UITextField
            {
                Font = UIFont.SystemFontOfSize(18f, UIFontWeight.Semibold),
                Frame = new CGRect(160, 0, 160, 44),
                UserInteractionEnabled = true,
                Text = _callerIdPickerModel.SelectedItem.FormattedPhoneNumber
            };
            _callerIdTextField.TouchDown += SetPicker;

            var toolbar = new UIToolbar { BarStyle = UIBarStyle.Black, Translucent = true };
            toolbar.SizeToFit();

            var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) => {
                                                                 _callerIdTextField.Text = SelectedPresentationNumber.FormattedPhoneNumber;
                                                                 _callerIdTextField.ResignFirstResponder();
                                                             });
            toolbar.SetItems(new[] { doneButton }, true);

            _callerIdTextField.InputView = _pickerField;
            _callerIdTextField.InputAccessoryView = toolbar;

            AddSubviews(labelField, _callerIdTextField);            
        }

        public void UpdatePickerData(PresentationNumber SelectedPresentationNumber)
        {
            this.SelectedPresentationNumber = SelectedPresentationNumber;
            if (SelectedPresentationNumber != null)
            {
                _pickerField.Select(_callerIdPickerModel.Items.IndexOf(SelectedPresentationNumber), 0, true);
                _callerIdTextField.Text = SelectedPresentationNumber.FormattedPhoneNumber;
            }
        }

        private void SetPicker(object sender, EventArgs e)
        {
            if (SelectedPresentationNumber != null)
                _pickerField.Select(_callerIdPickerModel.Items.IndexOf(SelectedPresentationNumber), 0, true);
        }
    }
}