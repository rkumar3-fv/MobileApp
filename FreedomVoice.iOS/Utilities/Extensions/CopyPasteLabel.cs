using System;
using System.Drawing;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.Utilities.Extensions
{
    [Register("CopyPasteLabel")]
    public class CopyPasteLabel : UILabel
    {
        public CopyPasteLabel()
        {
            Initialize();
        }

        public CopyPasteLabel(RectangleF frame) : base(frame)
        {
            Initialize();
        }

        public CopyPasteLabel(IntPtr p) : base(p)
        {
            Initialize();
        }

        public CopyPasteLabel(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        private void Initialize()
        {
            UserInteractionEnabled = true;
            AddGestureRecognizer(new UITapGestureRecognizer(HandleTap));
        }

        private void HandleTap()
        {
            BecomeFirstResponder();
            var menu = UIMenuController.SharedMenuController;
            menu.SetTargetRect(Bounds, this);
            menu.SetMenuVisible(true, true);
        }

        public override bool CanBecomeFirstResponder => true;

        public override bool CanPerform(ObjCRuntime.Selector action, NSObject withSender)
        {
            return action.Name == "copy:" || action.Name == "paste:";
        }

        public EventHandler OnPaste;

        [Export("copy:")]
        public override void Copy(NSObject sender)
        {
            UIPasteboard.General.String = Text;
        }

        [Export("paste:")]
        public override void Paste(NSObject sender)
        {
            Text = UIPasteboard.General.String;
        }
    }
}