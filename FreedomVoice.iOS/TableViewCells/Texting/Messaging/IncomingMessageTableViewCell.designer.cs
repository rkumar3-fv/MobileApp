﻿// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace FreedomVoice.iOS.TableViewCells.Texting.Messaging
{
    [Register ("IncomingMessageTableViewCell")]
    partial class IncomingMessageTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView BubbleView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel MessageLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (BubbleView != null) {
                BubbleView.Dispose ();
                BubbleView = null;
            }

            if (MessageLabel != null) {
                MessageLabel.Dispose ();
                MessageLabel = null;
            }
        }
    }
}