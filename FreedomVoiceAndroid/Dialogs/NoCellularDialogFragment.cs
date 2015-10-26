using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// No cellular warning dialog
    /// </summary>
    public class NoCellularDialogFragment : BaseDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_cellular, container, false);
            OkButton = view.FindViewById<Button>(Resource.Id.CellularDlg_ok);
            return view;
        }
    }
}