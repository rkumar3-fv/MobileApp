using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.SharedViews
{
    [Register("CallerIdView")]
    public class CallerIdView : UIView
    {
        public PresentationNumber SelectedPresentationNumber;

        private CallerIdPickerViewModel _callerIdPickerModel;
        private UIPickerView _pickerField;

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

            _callerIdPickerModel = new CallerIdPickerViewModel(numbers);
            _callerIdPickerModel.ValueChanged += (s, e) => { SelectedPresentationNumber = _callerIdPickerModel.SelectedItem; };

            _pickerField = new UIPickerView
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                ShowSelectionIndicator = true,
                Hidden = false,
                Model = _callerIdPickerModel
            };

            _pickerField.Select(_callerIdPickerModel.GetSelectedIndex(), 0, true);

            var callerIdTextField = new UITextField
            {
                Font = UIFont.SystemFontOfSize(18f, UIFontWeight.Semibold),
                Frame = new CGRect(160, 0, 160, 44),
                UserInteractionEnabled = true,
                Text = _callerIdPickerModel.SelectedItem.FormattedPhoneNumber
            };
            callerIdTextField.TouchDown += SetPicker;

            var toolbar = new UIToolbar { BarStyle = UIBarStyle.Black, Translucent = true };
            toolbar.SizeToFit();

            var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) => {
                                                                 callerIdTextField.Text = SelectedPresentationNumber.FormattedPhoneNumber;
                                                                 callerIdTextField.ResignFirstResponder();
                                                             });
            toolbar.SetItems(new[] { doneButton }, true);

            callerIdTextField.InputView = _pickerField;
            callerIdTextField.InputAccessoryView = toolbar;

            AddSubviews(labelField, callerIdTextField);
        }

        private void SetPicker(object sender, EventArgs e)
        {
            if (SelectedPresentationNumber != null)
                _pickerField.Select(_callerIdPickerModel.Items.IndexOf(SelectedPresentationNumber), 0, true);
        }
    }
}