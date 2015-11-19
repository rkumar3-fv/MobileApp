using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    public class NoPhoneDialogFragment : BaseDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_no_phone, container, false);
            CancelButton = view.FindViewById<Button>(Resource.Id.NoPhoneDlg_cancel);
            OkButton = view.FindViewById<Button>(Resource.Id.NoPhoneDlg_ok);
            return view;
        }
    }
}