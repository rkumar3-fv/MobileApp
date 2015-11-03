using FreedomVoice.iOS.Entities;
using System;

namespace FreedomVoice.iOS.Helpers
{
    public class ExpandedCellButtonClickEventArgs : EventArgs
    {

        public Message SelectedMessage { get; private set; }
        public string FilePath { get; set; } = string.Empty;

        public ExpandedCellButtonClickEventArgs(Message selectedMessage)
        {
            SelectedMessage = selectedMessage;
        }

        public ExpandedCellButtonClickEventArgs()
        {

        }

        public ExpandedCellButtonClickEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
