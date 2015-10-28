using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FreedomVoice.iOS.Views;
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class RecentsViewController : UIViewController
	{        
        private string _tempTtle = string.Empty;
        private UIBarButtonItem _tempLeftButton;
        private UIBarButtonItem _tempRightButton;
        private RecentsSource _recentSource;

        public CallerIdView CallerIdView { get; private set; }

        public RecentsViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RecentsTableView.TableFooterView = new UIView(CoreGraphics.CGRect.Empty);

            CallerIdView = new CallerIdView(new RectangleF(0, 65, 320, 40), (MainTab as MainTabBarController)?.GetPresentationNumbers());
            var recentLineView = new RecentLineView(new RectangleF(0, 40, 320, 0.5f));
            View.AddSubviews(CallerIdView, recentLineView);

            RecentsTableView.TableHeaderView = CallerIdView;

            _recentSource = new RecentsSource(GetRecentsOrdered());
            RecentsTableView.Source = _recentSource;

            _recentSource.OnRowSelected += TableSourceOnRowSelected;
            _recentSource.OnRowDeleted += TableSourceOnRowDeleted;
        }

        private UIViewController MainTab => ParentViewController.ParentViewController;

	    private List<Recent> GetRecentsOrdered()
        {            
            return GetRecents().OrderByDescending(o => o.DialDate).ToList();
        }

        private List<Recent> GetRecents()
        {         
            return (MainTab as MainTabBarController)?.Recents;
        }   
        
        private void AddRecent(Recent recent)
        {
            (MainTab as MainTabBarController)?.Recents.Add(recent);
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

            MainTab.Title = _tempTtle;
            MainTab.NavigationItem.SetLeftBarButtonItem(_tempLeftButton, true);
            MainTab.NavigationItem.SetRightBarButtonItem(_tempRightButton, true);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _tempLeftButton = MainTab.NavigationItem.LeftBarButtonItem;
            _tempTtle = MainTab.Title;
            _tempRightButton = MainTab.NavigationItem.RightBarButtonItem;

            MainTab.Title = "Recents";
            MainTab.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);

            _recentSource.SetRecents(GetRecentsOrdered());
            RecentsTableView.ReloadData();

            PresentationNumber selectedNumber = (MainTab as MainTabBarController)?.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);
        }

        private void SetEditMode()
        {
            RecentsTableView.SetEditing(true, true);
            MainTab.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain, (s, args) => { RecentsTableView.ReloadData(); ReturnToRecentsView(); }), true);
            MainTab.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (s, args) => { ClearAll(); }), true);
        }

        private void ReturnToRecentsView()
        {
            RecentsTableView.SetEditing(false, true);
            MainTab.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            MainTab.NavigationItem.SetRightBarButtonItem(_tempRightButton, true);
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
        }

        private void TableSourceOnRowSelected(object sender, RecentsSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);
            var recent = GetRecentsOrdered()[e.IndexPath.Row];
            if (recent == null) return;

            RecentsTableView.BeginUpdates();
            var newRecent = (Recent)recent.Clone();
            newRecent.DialDate = DateTime.Now;                                
            AddRecent(newRecent);
            _recentSource.SetRecents(GetRecentsOrdered());
            e.TableView.InsertRows(new[] { NSIndexPath.FromRowSection (e.TableView.NumberOfRowsInSection (0), 0) }, UITableViewRowAnimation.Fade);
            RecentsTableView.EndUpdates();
        }

        private void TableSourceOnRowDeleted(object sender, RecentsSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);
            var recent = GetRecentsOrdered()[e.IndexPath.Row];
            if (recent == null) return;

            RecentsTableView.BeginUpdates();
            RemoveRecent(recent);
            _recentSource.SetRecents(GetRecentsOrdered());
            RecentsTableView.DeleteRows(new[] { e.IndexPath }, UITableViewRowAnimation.Fade);
            RecentsTableView.EndUpdates();                       
        }

    }
}