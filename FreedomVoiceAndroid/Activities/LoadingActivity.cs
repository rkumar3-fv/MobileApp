using System.Diagnostics;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        MainLauncher = true,
        Label = "@string/ApplicationName",
        Icon = "@mipmap/ic_launcher",
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        NoHistory = true,
        Theme = "@style/AuthAppTheme")]
    public class LoadingActivity : BaseActivity
    {
        private const int TimeOut = 7;
        private Timer _timer;
        private Stopwatch _watcher;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (!IsTaskRoot)
            {
                Finish();
                return;
            }
            SetContentView(Resource.Layout.act_loading);
            _timer = new Timer { Interval = TimeOut*1000, AutoReset = false };
            _timer.Elapsed += TimerOnElapsed;
        }

        protected override void OnStart()
        {
            base.OnStart();
            _watcher = Stopwatch.StartNew();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if ((_watcher != null)&&(_watcher.IsRunning))
            {
                _watcher.Stop();
                Appl.ApplicationHelper.ReportTime(TimingEvent.LongAction, "LoadingScreen", "", _watcher.ElapsedMilliseconds);
            }
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
            if (!Appl.ApplicationHelper.IsInternetConnected() || Appl.ApplicationHelper.IsAirplaneModeOn())
            {
                if (_timer.Enabled)
                    _timer.Stop();
                StartActivity(new Intent(this, typeof (AuthActivity)));
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_timer.Enabled)
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
                if ((Helper.SelectedAccount != null) && (Helper.SelectedAccount.PresentationNumbers.Count > 0))
                {
                    Helper.ForceLoadExtensions();
                    var intent = new Intent(this, typeof(ContentActivity));
                    intent.SetFlags(ActivityFlags.NewTask);
                    intent.AddFlags(ActivityFlags.ClearTop);
                    StartActivity(intent);
                }
                else if ((Helper.AccountsList != null) && (Helper.AccountsList.Count > 0))
                    Helper.GetExtensions();
                else if (Helper.IsLoggedIn)
                    Helper.GetAccounts();
                else if (Helper.Authorize() == -100)
                {
                    var intent = new Intent(this, typeof(AuthActivity));
                    intent.SetFlags(ActivityFlags.NewTask);
                    intent.AddFlags(ActivityFlags.ClearTop);
                    StartActivity(intent);
                }
            });
        }
    }
}