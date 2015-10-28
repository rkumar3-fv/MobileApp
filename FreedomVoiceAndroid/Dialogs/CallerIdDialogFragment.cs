using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// Indicates that Caller ID disabled
    /// </summary>
    public class CallerIdDialogFragment : BaseDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_callerid, container, false);
            CancelButton = view.FindViewById<Button>(Resource.Id.CallerIdDlg_cancel);
            OkButton = view.FindViewById<Button>(Resource.Id.CallerIdDlg_ok);
            return view;
        }
    }
}