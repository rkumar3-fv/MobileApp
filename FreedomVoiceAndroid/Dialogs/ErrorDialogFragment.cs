using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    /// <summary>
    /// Indicates error
    /// </summary>
    public class ErrorDialogFragment : BaseDialogFragment
    {

        private TextView messageView;

        public string Message;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_error, container, false);
            CancelButton = view.FindViewById<Button>(Resource.Id.ErroDlg_cancel);
            if (Message!= null)
            {
                messageView = view.FindViewById<TextView>(Resource.Id.ErroDlg_text);
                messageView.SetText(Message, TextView.BufferType.Normal);
            }
            
            return view;
        }
    }
}