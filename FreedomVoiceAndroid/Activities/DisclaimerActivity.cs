using Android.App;
using Android.OS;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        Label = "@string/ActivityDisclaimer_title",
        Theme = "@style/AppThemeActionBar",
        NoHistory = true)]
    public class DisclaimerActivity : BaseActivity
    {
        private Button _agreeButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_disclaimer);
            _agreeButton = FindViewById<Button>(Resource.Id.disclaimerActivity_agreeButton);

            SupportActionBar.SetTitle(Resource.String.ActivityDisclaimer_title);
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