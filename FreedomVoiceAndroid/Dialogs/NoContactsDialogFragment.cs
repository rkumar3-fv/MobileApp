using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// No phone numbers in contact
    /// </summary>
    public class NoContactsDialogFragment : BaseDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_contacts_no, container, false);
            OkButton = view.FindViewById<Button>(Resource.Id.NoContactsDlg_ok);
            return view;
        }
    }
}