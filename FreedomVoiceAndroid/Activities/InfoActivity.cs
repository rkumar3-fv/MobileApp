using System;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Abstract activity with info & one confirmation button
    /// </summary>
    public abstract class InfoActivity : BaseActivity
    {
        protected Button ActionButton;

        protected override void OnStart()
        {
            base.OnStart();
            ActionButton.Click += ActionButtonOnClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            var inflater = new SupportMenuInflater(this);
            inflater.Inflate(Resource.Menu.menu_logout, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_action_logout:
                    Helper.Logout();
                    return true;
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            } 
        }

        /// <summary>
        /// Action button click result
        /// </summary>
        protected abstract void ActionButtonOnClick(object sender, EventArgs eventArgs);
    }
}