using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// Password restoration dialog
    /// </summary>
    public class RestoreDialogFragment : BaseDialogFragment
    {

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_restore, container, false);
            OkButton = view.FindViewById<Button>(Resource.Id.RestoreDlg_ok);
            return view;
        }
    }
}