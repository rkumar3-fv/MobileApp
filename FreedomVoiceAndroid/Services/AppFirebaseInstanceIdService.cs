using Android.App;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Firebase.Iid;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Entities.Enums;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    [Service]
    [IntentFilter(new[] {"com.google.firebase.INSTANCE_ID_EVENT"})]
    public class AppFirebaseInstanceIdService : FirebaseInstanceIdService
    {
        private IPushService _pushService;
        private ActionsHelper _actionsHelper;

        public override void OnCreate()
        {
            base.OnCreate();
            _actionsHelper = App.GetApplication(this).ApplicationHelper.ActionsHelper;
            _pushService = ServiceContainer.Resolve<IPushService>();
        }

        public override async void OnTokenRefresh()
        {
            base.OnTokenRefresh();
            if (_actionsHelper.IsLoggedIn)
            {
                var instanceToken = FirebaseInstanceId.Instance.Token;
                await _pushService.Register(DeviceType.Android, instanceToken);
            }
        }
    }
}