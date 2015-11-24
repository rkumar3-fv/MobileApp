using System;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.CustomControls.Callbacks
{
    /// <summary>
    /// SearchView listener implementation
    /// </summary>
    public class SearchViewListener : Object, SearchView.IOnQueryTextListener, MenuItemCompat.IOnActionExpandListener
    {
        public string QueryString { get; private set; }

        /// <summary>
        /// Changing query string event
        /// </summary>
        public event EventHandler<string> OnChange;

        /// <summary>
        /// Apply query event
        /// </summary>
        public event EventHandler<string> OnApply;

        public event EventHandler<bool> OnCollapse;

        public event EventHandler<bool> OnExpand;
             
        public bool OnQueryTextChange(string newText)
        {
            QueryString = newText;
#if DEBUG
            Log.Debug(App.AppPackage, $"Contacts QUERY: {QueryString}");
#endif
            OnChange?.Invoke(this, newText);
            return false;
        }

        public bool OnQueryTextSubmit(string query)
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"Contacts FINAL QUERY: {query}");
#endif
            QueryString = "";
            OnApply?.Invoke(this, query);
            return false;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            OnCollapse?.Invoke(this, true);
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            OnExpand?.Invoke(this, true);
            return true;
        }
    }
}