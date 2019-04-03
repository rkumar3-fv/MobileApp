﻿using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.TableViewCells.Texting;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources.Texting
{
    public class ConversationsSource : UITableViewSource
    {
        private readonly List<Account> _accounts;
        private readonly UINavigationController _navigationController;

        public ConversationsSource(List<Account> accounts, UINavigationController navigationController, UITableView tableView)
        {
            _accounts = accounts;
            _navigationController = navigationController;
            
            tableView.RegisterNibForCellReuse(UINib.FromName("ConversationItemTableViewCell", NSBundle.MainBundle), "cell" );
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 40;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("cell") as ConversationItemTableViewCell;
            //cell.TextLabel.Text = _accounts[indexPath.Row].FormattedPhoneNumber;
            //cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return 50;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, true);

        }
    }
}