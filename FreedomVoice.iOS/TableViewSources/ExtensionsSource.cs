using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class ExtensionsSource : UITableViewSource
    {
        private readonly List<ExtensionWithCount> _extensions;

        private readonly Account _selectedAccount;

        private readonly UINavigationController _navigationController;

        public ExtensionsSource(List<ExtensionWithCount> extensions, Account selectedAccount, UINavigationController navigationController)
        {
            _extensions = extensions;
            _selectedAccount = selectedAccount;
            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var extension = _extensions[indexPath.Row];

            var cell = tableView.DequeueReusableCell(ExtensionCell.ExtensionCellId) as ExtensionCell ?? new ExtensionCell();
            cell.TextLabel.Text = extension.ExtensionNumber + " - " + extension.DisplayName;
            cell.DetailTextLabel.Text = extension.UnreadMessagesCount.ToString();
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _extensions?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);
            var selectedExtension = _extensions[indexPath.Row];

            var foldersController = AppDelegate.GetViewController<FoldersViewController>();
            foldersController.SelectedAccount = _selectedAccount;
            foldersController.SelectedExtension = selectedExtension;

            _navigationController.PushViewController(foldersController, true);
        }
    }
}