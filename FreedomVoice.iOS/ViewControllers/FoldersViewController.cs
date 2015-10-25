using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    class FoldersViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }

        public FoldersViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            var FoldersTableView = new UITableView();

            var extensionsViewModel = new FoldersViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber);
            await extensionsViewModel.GetFoldersListAsync();

            FoldersTableView.Source = new FoldersSource(extensionsViewModel.FoldersList, SelectedExtension, SelectedAccount, NavigationController);

            View.AddSubview(FoldersTableView);

            base.ViewDidLoad();
        }
    }
}
