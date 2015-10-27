using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    public class ClearRecentsDialog : BaseDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_clear, container, false);
            OkButton = view.FindViewById<Button>(Resource.Id.ClearDlg_ok);
            CancelButton = view.FindViewById<Button>(Resource.Id.ClearDlg_cancel);
            return view;
        }
    }
}