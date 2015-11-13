using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.OS;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Helpers;
using HockeyApp;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        MainLauncher = true,
        Label = "@string/ApplicationTitle",
        Icon = "@mipmap/ic_launcher",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        NoHistory = true,
        Theme = "@style/AuthAppTheme")]
    public class LoadingActivity : BaseActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_loading);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Helper.IsLoggedIn)
            {
                if ((Helper.AccountsList == null) || (Helper.SelectedAccount == null))
                {
                    Helper.GetAccounts();
                    return;
                }
                if ((Helper.SelectedAccount.PresentationNumbers == null) ||
                    (string.IsNullOrEmpty(Helper.SelectedAccount.PresentationNumber)))
                    Helper.GetPresentationNumbers();
            }
            else
            {
#if !DEBUG
#if TRACE
                if (AppHelper.Instance(this).IsHockeyAppOn)
                    AppHelper.Instance(this).InitHockeyUpdater(this);
#endif
#endif
#if TRACE
                if (AppHelper.Instance(this).InitGa(false))
#else
                if (AppHelper.Instance(this).InitGa(true))
#endif
                    AppHelper.Instance(this).AnalyticsTracker.Send(new HitBuilders.ScreenViewBuilder().Build());
            }
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.ConnectionLostError:
                    case ActionsHelperEventArgs.InternalError:
                    case ActionsHelperEventArgs.AuthLoginError:
                    case ActionsHelperEventArgs.AuthPasswdError:
                        var intent = new Intent(this, typeof(LoginActivity));
                        StartActivity(intent);
                        return;
                }
            }
        }
    }
}