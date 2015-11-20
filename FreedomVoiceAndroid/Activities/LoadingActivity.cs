using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.OS;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        MainLauncher = true,
        Label = "@string/ApplicationTitle",
        Icon = "@mipmap/ic_launcher",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        NoHistory = true,
        Theme = "@style/AuthAppTheme")]
    public class LoadingActivity : BaseActivity
    {
        private Timer _timer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_loading);
            _timer = new Timer { Interval = 20000, AutoReset = false };
            _timer.Elapsed += TimerOnElapsed;
        }

        protected override void OnResume()
        {
            base.OnResume();
            _timer.Start();
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
#if TRACE
                if (Appl.ApplicationHelper.InitGa(false))
#else
                if (Appl.ApplicationHelper.InitGa(true))
#endif
                    Appl.ApplicationHelper.AnalyticsTracker.Send(new HitBuilders.ScreenViewBuilder().Build());
            }
            if (!Appl.ApplicationHelper.IsInternetConnected() || Appl.ApplicationHelper.IsAirplaneModeOn())
                StartActivity(new Intent(this, typeof(AuthActivity)));
        }

        protected override void OnPause()
        {
            base.OnPause();
            _timer.Stop();
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
                        StartActivity(new Intent(this, typeof(AuthActivity)));
                        return;
                }
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            RunOnUiThread(delegate
            {
                Helper.GetAccounts();
            });
        }
    }
}