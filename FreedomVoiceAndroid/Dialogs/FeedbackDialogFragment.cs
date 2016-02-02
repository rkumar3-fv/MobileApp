using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    class FeedbackDialogFragment : BaseDialogFragment
    {
        private readonly Context _context;
        private Button _sendButton;
        private EditText _feedbackText;
        private CheckBox _logCheckBox;

        public FeedbackDialogFragment(Context context)
        {
            _context = context;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_feedback, container, false);
            _feedbackText = view.FindViewById<EditText>(Resource.Id.FeedbackDlg_text);
            _logCheckBox = view.FindViewById<CheckBox>(Resource.Id.FeedbackDlg_logState);
            _sendButton = view.FindViewById<Button>(Resource.Id.FeedbackDlg_send);
            CancelButton = view.FindViewById<Button>(Resource.Id.FeedbackDlg_cancel);
            return view;
        }

        public override void OnStart()
        {
            base.OnStart();
            _sendButton.Click += SendButtonOnClick;
        }

        private void SendButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (_logCheckBox.Checked)
                App.GetApplication(_context).ApplicationHelper.Reports.SendReport(Activity, _feedbackText.Text);
            else
                App.GetApplication(_context).ApplicationHelper.Reports.SendFeedback(Activity, _feedbackText.Text);
            Dismiss();
        }
    }
}