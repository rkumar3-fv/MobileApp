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
        public List<ExtensionWithCount> Extensions { private get; set; }

        private readonly UINavigationController _navigationController;

        public ExtensionsSource(List<ExtensionWithCount> extensions, UINavigationController navigationController)
        {
            Extensions = extensions;
            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var extension = Extensions[indexPath.Row];

            var cell = tableView.DequeueReusableCell(ExtensionCell.ExtensionCellId) as ExtensionCell ?? new ExtensionCell { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            cell.UpdateCell(extension);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Extensions?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);
            var selectedExtension = Extensions[indexPath.Row];

            var foldersController = AppDelegate.GetViewController<FoldersViewController>();
            foldersController.SelectedExtension = selectedExtension;

            _navigationController.PushViewController(foldersController, false);
        }
    }
}