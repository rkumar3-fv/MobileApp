using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Fragment with caller ID managment
    /// </summary>
    public abstract class CallerFragment : BasePagerFragment
    {
        protected Spinner IdSpinner;
        protected TextView SingleId;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            if (!CheckLoading()) return;
            if (IdSpinner == null) return;
            var adapter = new CallerIdSpinnerAdapter(Context, Helper.SelectedAccount.PresentationNumbers);
            IdSpinner.ItemSelected += (sender, args) =>
            {
                IdSpinner.SetSelection(args.Position);
                Helper.SetPresentationNumber(args.Position);
            };
            IdSpinner.Adapter = adapter;
        }

        public override void OnResume()
        {
            base.OnResume();
            if (!CheckLoading()) return;
            if (Helper.SelectedAccount.PresentationNumbers.Count == 1)
            {
                IdSpinner.Visibility = ViewStates.Invisible;
                SingleId.Text = DataFormatUtils.ToPhoneNumber(Helper.SelectedAccount.PresentationNumber);
                SingleId.Visibility = ViewStates.Visible;
            }
            else
            {
                IdSpinner.Visibility = ViewStates.Visible;
                SingleId.Visibility = ViewStates.Invisible;
                if (IdSpinner.SelectedItemPosition != Helper.SelectedAccount.SelectedPresentationNumber)
                    IdSpinner.SetSelection(Helper.SelectedAccount.SelectedPresentationNumber);
            }
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.ChangePresentation:
                        if (IdSpinner.SelectedItemPosition != Helper.SelectedAccount.SelectedPresentationNumber)
                            IdSpinner.SetSelection(Helper.SelectedAccount.SelectedPresentationNumber);
                        break;
                }
            }
        }

        private bool CheckLoading()
        {
            if (Helper.SelectedAccount?.PresentationNumbers != null)
               return true;
            if ((Helper.SelectedAccount == null)||(Helper.AccountsList == null))
                Helper.GetAccounts();
            else if ((Helper.SelectedAccount.PresentationNumbers == null)||(Helper.SelectedAccount.PresentationNumbers.Count == 0))
                Helper.GetPresentationNumbers();
            var intent = new Intent(ContentActivity, typeof(LoadingActivity));
            intent.SetFlags(ActivityFlags.NewTask);
            intent.SetFlags(ActivityFlags.ClearTop);
            ContentActivity.StartActivity(intent);
            return false;
        }
    }
}