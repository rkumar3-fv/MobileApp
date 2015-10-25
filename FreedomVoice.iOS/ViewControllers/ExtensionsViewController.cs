using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    public class ExtensionsViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }

        public ExtensionsViewController(IntPtr handle) : base (handle) { }

        public override async void ViewDidLoad()
        {
            var ExtensionsTableView = new UITableView();

            var extensionsViewModel = new ExtensionsViewModel(SelectedAccount.PhoneNumber);
            await extensionsViewModel.GetExtensionsListAsync();

            ExtensionsTableView.Source = new ExtensionsSource(extensionsViewModel.ExtensionsList, SelectedAccount, NavigationController);

            View.AddSubview(ExtensionsTableView);

            base.ViewDidLoad();
        }
    }
}
