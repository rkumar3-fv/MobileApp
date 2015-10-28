using Android.OS;
using Android.Views;
using Android.Widget;
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
            IdSpinner.ItemSelected += (sender, args) => Helper.SetPresentationNumber(args.Position);
            var adapter = new CallerIdSpinnerAdapter(Activity, Helper.SelectedAccount.PresentationNumbers);
            IdSpinner.Adapter = adapter;
        }

        public override void OnResume()
        {
            base.OnResume();
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
    }
}