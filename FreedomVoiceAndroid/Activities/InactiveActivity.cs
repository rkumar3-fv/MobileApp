using Android.App;
using Android.OS;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        Label = "@string/ActivityInactive_title",
        Theme = "@style/AppThemeActionBar")]
    public class InactiveActivity : BaseActivity
    {
        private Button _dialButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_inactive);
            _dialButton = FindViewById<Button>(Resource.Id.inactiveActivity_dialButton);

            SupportActionBar.SetTitle(Resource.String.ActivityInactive_title);
            SupportActionBar.SetIcon(Resource.Drawable.ic_action_back);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            var inflater = new SupportMenuInflater(this);
            inflater.Inflate(Resource.Menu.menu_logout, menu);
            return true;
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}