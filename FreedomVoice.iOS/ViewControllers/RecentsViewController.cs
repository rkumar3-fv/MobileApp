using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.SharedViews;
using FreedomVoice.iOS.TableViewSources;
using System;
using System.Collections.Generic;
using System.Drawing;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class RecentsViewController : UIViewController
	{

        public List<Recent> _recents { get; set; }
        private string tempTtle = string.Empty;
        private UIBarButtonItem tempLeftButton = null;
        private UIBarButtonItem tempRightButton = null;

        public RecentsViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var callerIdView = new CallerIdView(new RectangleF(0, 65, 320, 44));
            var recentViewLine = new RecentViewLine(new RectangleF(0, 36, 320, 1));
            View.AddSubviews(callerIdView, recentViewLine);

            RecentsTableView.TableHeaderView = callerIdView;
            RecentsTableView.Source = new RecentsSource(GetRecents());
        }

        private UIViewController MainTab { get { return ParentViewController.ParentViewController; } }


        private List<Recent> GetRecents()
        {
            //var tabBarController = AppDelegate.GetViewController<MainTabBarController>();
            //return tabBarController.Recents;

            return _recents;
        }   
        
        private void AddRecent(Recent recent)
        {
            GetRecents().Add(recent);
        }

        private void ClearRecent()
        {
            GetRecents().Clear();
        }

        private void RemoveRecent(Recent recent)
        {
            GetRecents().Remove(recent);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            MainTab.Title = tempTtle;
            MainTab.NavigationItem.SetLeftBarButtonItem(tempLeftButton, true);
            MainTab.NavigationItem.SetRightBarButtonItem(tempRightButton, true);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            tempLeftButton = MainTab.NavigationItem.LeftBarButtonItem;
            tempTtle = MainTab.Title;
            tempRightButton = MainTab.NavigationItem.RightBarButtonItem;

            MainTab.Title = "Recents";
            MainTab.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
        }

        private void SetEditMode()
        {
            RecentsTableView.SetEditing(true, true);
            MainTab.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain,
                (s, args) => {                    
                    RecentsTableView.ReloadData();
                    ReturnToRecentsView();
                }), true);
            MainTab.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain,
                (s, args) => {
                    ClearAll();              
                }), true);
        }

        private void ReturnToRecentsView()
        {
            RecentsTableView.SetEditing(false, true);
            MainTab.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            MainTab.NavigationItem.SetRightBarButtonItem(tempRightButton, true);
        }

        private UIBarButtonItem GetEditButton()
        {
            return new UIBarButtonItem("Edit", UIBarButtonItemStyle.Plain, (s, args) => { SetEditMode(); });
        }

        private void ClearAll()
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            alertController.AddAction(UIAlertAction.Create("Clear All Recents", UIAlertActionStyle.Default, a => {
                ReturnToRecentsView();
                _recents = new List<Recent>();
                RecentsTableView.Source = new RecentsSource(GetRecents());
                RecentsTableView.ReloadData();
            }));            
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, a => { ReturnToRecentsView(); }));

            PresentViewController(alertController, true, null);
            return;
        }
    }
}
