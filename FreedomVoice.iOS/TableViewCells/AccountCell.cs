﻿using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class AccountCell : UITableViewCell
    {
        public static readonly NSString AccountCellId = new NSString("AccoundCell");

        public AccountCell() : base(UITableViewCellStyle.Default, AccountCellId) { }
    }
}