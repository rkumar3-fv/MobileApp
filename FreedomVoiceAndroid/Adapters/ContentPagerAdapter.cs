using System;
using System.Collections.Generic;
using Android.Content;
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
        private readonly Context _context;
        private readonly List<Fragment> _fragments;
        private readonly List<int> _fragmentTitles;
        private readonly List<int> _fragmentImages; 

        public ContentPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ContentPagerAdapter(FragmentManager fm, Context context) : base(fm)
        {
            _context = context;
            _fragments = new List<Fragment>();
            _fragmentTitles = new List<int>();
            _fragmentImages = new List<int>();
        }

        public View GetTabView(int position)
        {
            var view = LayoutInflater.From(_context).Inflate(Resource.Layout.tab_header, null);
            var textView = view.FindViewById<TextView>(Resource.Id.tabHeader_title);
            var imageView = view.FindViewById<ImageView>(Resource.Id.tabHeader_icon);
            textView.SetText(_fragmentTitles[position]);
            imageView.SetImageResource(_fragmentImages[position]);
            return view;
        }

        /// <summary>
        /// Add fragment to viewpager
        /// </summary>
        /// <param name="fragment">new fragment</param>
        /// <param name="titleRes">tab title resource</param>
        /// <param name="iconRes">tab icon resource</param>
        public void AddFragment(Fragment fragment, int titleRes, int iconRes)
        {
            _fragments.Add(fragment);
            _fragmentTitles.Add(titleRes);
            _fragmentImages.Add(iconRes);
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
            _fragmentImages.RemoveAt(index);
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
            _fragmentImages.RemoveAt(index);
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
            return new Java.Lang.String(_context.GetString(_fragmentTitles[position]));
        }
    }
}