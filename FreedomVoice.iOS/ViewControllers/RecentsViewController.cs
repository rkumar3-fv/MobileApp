using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.SharedViews;
using FreedomVoice.iOS.TableViewSources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class RecentsViewController : UIViewController
	{        
        private string tempTtle = string.Empty;
        private UIBarButtonItem tempLeftButton = null;
        private UIBarButtonItem tempRightButton = null;
        private RecentsSource _recentSource;

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

            _recentSource = new RecentsSource(GetRecentsOrdered());
            RecentsTableView.Source = _recentSource;

            _recentSource.OnRowSelected += TableSourceOnRowSelected;
        }

        private UIViewController MainTab { get { return ParentViewController.ParentViewController; } }

        private List<Recent> GetRecentsOrdered()
        {            
            return GetRecents().OrderByDescending(o => o.DialDate).ToList();
        }

        private List<Recent> GetRecents()
        {
            var ctrl = MainTab as MainTabBarController;            
            return ctrl.Recents;
        }   
        
        private void AddRecent(Recent recent)
        {
            var ctrl = MainTab as MainTabBarController;
            ctrl.Recents.Add(recent);
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
                ClearRecent();
                _recentSource.SetRecents(GetRecentsOrdered());
                RecentsTableView.ReloadData();
            }));            
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, a => { ReturnToRecentsView(); }));

            PresentViewController(alertController, true, null);
            return;
        }

        private void TableSourceOnRowSelected(object sender, RecentsSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);
            var recent = GetRecentsOrdered()[e.IndexPath.Row];            

            if (recent != null)
            {
                RecentsTableView.BeginUpdates();
                var newRecent = (Recent)recent.Clone();
                newRecent.DialDate = DateTime.Now;                                
                AddRecent(newRecent);
                _recentSource.SetRecents(GetRecentsOrdered());
                e.TableView.InsertRows(new NSIndexPath[] {
                    NSIndexPath.FromRowSection (e.TableView.NumberOfRowsInSection (0), 0)
                    }, UITableViewRowAnimation.Fade);
                RecentsTableView.EndUpdates();
                RecentsTableView.ReloadData();
            }
        }
    }
}
