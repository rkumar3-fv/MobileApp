using Android.App;
using Android.OS;
using Android.Support.V7.Internal.View;
using Android.Support.V7.Widget;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Phone number selection activity
    /// </summary>
    [Activity
        (Label = "@string/ActivitySelect_title",
        Theme = "@style/AppThemeActionBar")]
    public class SelectActivity : BaseActivity
    {
        private bool _logoutInProcess;
        private RecyclerView _selectView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_select);
            _selectView = FindViewById<RecyclerView>(Resource.Id.selectActivity_accountsList);
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
                    if (!_logoutInProcess)
                    {
                        _logoutInProcess = true;
                        WaitingActions.Add(Helper.Logout());
                    }
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}