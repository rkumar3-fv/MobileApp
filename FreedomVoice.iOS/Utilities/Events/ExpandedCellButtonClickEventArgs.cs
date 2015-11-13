using System;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Utilities.Events
{
    public class ExpandedCellButtonClickEventArgs : EventArgs
    {
        public Message SelectedMessage { get; private set; }
        public string FilePath { get; private set; } = string.Empty;

        public ExpandedCellButtonClickEventArgs() { }

        public ExpandedCellButtonClickEventArgs(Message selectedMessage)
        {
            SelectedMessage = selectedMessage;
        }

        public ExpandedCellButtonClickEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}