using System;
using Android.Support.V7.Widget;
#if DEBUG
using Android.Util;
#endif
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// SearchView listener implementation
    /// </summary>
    public class SearchViewListener : Object, SearchView.IOnQueryTextListener, SearchView.IOnCloseListener
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

        /// <summary>
        /// Cancel query event
        /// </summary>
        public event EventHandler<string> OnCancel; 
             
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

        public bool OnClose()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"Contacts CLOSE with DATA: {QueryString}");
#endif
            OnCancel?.Invoke(this, QueryString);
            return false;
        }
    }
}