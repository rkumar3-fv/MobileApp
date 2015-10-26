using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// Logout confirmation dialog
    /// </summary>
    public class LogoutDialogFragment : BaseDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_logout, container, false);
            OkButton = view.FindViewById<Button>(Resource.Id.LogoutDlg_ok);
            CancelButton = view.FindViewById<Button>(Resource.Id.LogoutDlg_cancel);
            return view;
        }
    }
}