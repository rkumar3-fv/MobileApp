using Android.OS;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// No phone numbers in contact
    /// </summary>
    public class NoContactsDialogFragment : BaseDialogFragment
    {
        private readonly Contact _contact;
        private TextView _content;

        public NoContactsDialogFragment(Contact contact)
        {
            _contact = contact;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_contacts_no, container, false);
            _content = view.FindViewById<TextView>(Resource.Id.NoContactsDlg_content);
            _content.Text = $"{GetString(Resource.String.DlgNumbers_content)} {_contact.Name}.";
            OkButton = view.FindViewById<Button>(Resource.Id.NoContactsDlg_ok);
            return view;
        }
    }
}