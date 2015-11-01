using System;
using System.Collections.Generic;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class ExtensionsViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public List<ExtensionWithCount> ExtensionsList { private get; set; }

        public ExtensionsViewController(IntPtr handle) : base(handle) { }

        private UIViewController MainTab => ParentViewController.ParentViewController;

        public override void ViewDidLoad()
        {
            ExtensionsTableView.TableFooterView = new UIView(CGRect.Empty);

            ExtensionsTableView.Source = new ExtensionsSource(ExtensionsList, SelectedAccount, NavigationController);

            View.AddSubview(ExtensionsTableView);

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            MainTab.Title = SelectedAccount.FormattedPhoneNumber;

            base.ViewWillAppear(animated);
        }
    }
}