using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public class ContentPagerAdapter : FragmentPagerAdapter
    {
        private readonly List<Fragment> _fragments;
        private readonly List<string> _fragmentTitles; 

        public ContentPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ContentPagerAdapter(FragmentManager fm) : base(fm)
        {
            _fragments = new List<Fragment>();
            _fragmentTitles = new List<string>();
        }

        /// <summary>
        /// Add fragment to viewpager
        /// </summary>
        /// <param name="fragment">new fragment</param>
        /// <param name="title">tab title</param>
        public void AddFragment(Fragment fragment, string title)
        {
            _fragments.Add(fragment);
            _fragmentTitles.Add(title);
        }

        /// <summary>
        /// Remove fragment
        /// </summary>
        /// <param name="fragment">fragment for deletion</param>
        public void RemoveFragment(Fragment fragment)
        {
            if (!_fragments.Contains(fragment)) return;
            var index = _fragments.IndexOf(fragment);
            _fragments.Remove(fragment);
            _fragmentTitles.RemoveAt(index);
        }

        /// <summary>
        /// Remove fragment
        /// </summary>
        /// <param name="index">fragment index</param>
        public void RemoveFragment(int index)
        {
            if (index>=_fragments.Count) return;
            _fragments.RemoveAt(index);
            _fragmentTitles.RemoveAt(index);
        }

        /// <summary>
        /// Get fragments count
        /// </summary>
        public override int Count => _fragments.Count;

        /// <summary>
        /// Get fragment by ID
        /// </summary>
        /// <param name="position">fragment ID</param>
        /// <returns></returns>
        public override Fragment GetItem(int position)
        {
            return _fragments[position];
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(_fragmentTitles[position]);
        }
    }
}