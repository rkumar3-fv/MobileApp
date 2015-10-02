using System;
using Android.OS;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class InfoActivity : BaseActivity
    {
        protected Button ActionButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ActionButton.Click += ActionButtonOnClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            var inflater = new SupportMenuInflater(this);
            inflater.Inflate(Resource.Menu.menu_logout, menu);
            return true;
        }

        /// <summary>
        /// Action button click result
        /// </summary>
        protected abstract void ActionButtonOnClick(object sender, EventArgs eventArgs);
    }
}