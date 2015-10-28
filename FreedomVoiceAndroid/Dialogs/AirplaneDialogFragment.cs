using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// Indicates that airplane mode enabled
    /// </summary>
    public class AirplaneDialogFragment : BaseDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_airplane, container, false);
            CancelButton = view.FindViewById<Button>(Resource.Id.AirplaneDlg_cancel);
            OkButton = view.FindViewById<Button>(Resource.Id.AirplaneDlg_ok);
            return view;
        }
    }
}