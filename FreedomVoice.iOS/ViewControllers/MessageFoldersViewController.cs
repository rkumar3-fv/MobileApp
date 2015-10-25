using System;
using System.Collections.Generic;
using System.Text;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.ViewControllers
{
    public class MessageFoldersViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }

        public MessageFoldersViewController(IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }
    }
}
