using System;
using System.Collections.Generic;
using System.Drawing;
using FreedomVoice.iOS.Entities;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class CallerIdPickerViewModel : UIPickerViewModel
    {
        public event EventHandler<EventArgs> ValueChanged;

        private int _selectedIndex = 0;

        public PresentationNumber SelectedItem => Items[_selectedIndex];

        public IList<PresentationNumber> Items { get; }

        public CallerIdPickerViewModel(IList<PresentationNumber> items)
        {
            Items = items;
        }

        public override nint GetRowsInComponent(UIPickerView picker, nint component)
        {
            return Items.Count;
        }

        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            return new UILabel(new RectangleF(0, 0, 160, 40))
            {
                TextColor = UIColor.Black,
                Font = UIFont.SystemFontOfSize(18),
                TextAlignment = UITextAlignment.Center,
                Text = GetTitleForItem(Items[(int)row])
            };
        }

        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            _selectedIndex = (int)row;
            ValueChanged?.Invoke(this, new EventArgs());
        }

        public override nint GetComponentCount(UIPickerView picker)
        {
            return 1;
        }

        protected virtual string GetTitleForItem(PresentationNumber item)
        {
            return item.FormattedPhoneNumber;
        }
    }
}