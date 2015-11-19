using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Actions.Requests;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Background server-client communication service
    /// </summary>
    [Service (Exported = false)]
    public class ComService : Service
    {
        public const string RequestTag = "ComServiceRequest";
        public const string RequestIdTag = "ComServiceRequestId";

        private ResultReceiver _receiver;
        private readonly Dictionary<long, Task> _activeActions = new Dictionary<long, Task>(); 

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

#if DEBUG
        public override void OnCreate()
        {
            base.OnCreate();
            Log.Debug(App.AppPackage, "SERVICE CREATED");
        }
#endif

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (_receiver == null)
            {
                _receiver = intent.GetParcelableExtra(ComServiceResultReceiver.ReceiverTag) as ResultReceiver;
                if (_receiver == null) return StartCommandResult.NotSticky;
            }
            var id = intent.GetLongExtra(RequestIdTag, 0);
            var request = intent.GetParcelableExtra(RequestTag) as BaseRequest;
            if (request == null) return StartCommandResult.NotSticky;
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE REQUEST ID=" + request.Id);
#endif
            var task = ExecuteRequest(request);
            if (!_activeActions.ContainsKey(id))
                _activeActions.Add(id, task);
            return StartCommandResult.NotSticky;
        }

        /// <summary>
        /// Async request execution
        /// </summary>
        /// <param name="request">request for execution</param>
        private async Task ExecuteRequest(BaseRequest request)
        {
            var result = await request.ExecuteRequest();
            var data = new Bundle();
            data.PutParcelable(ComServiceResultReceiver.ReceiverDataExtra, result);
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE RESPONSE ID=" + request.Id);
#endif
            _activeActions.Remove(request.Id);
            _receiver.Send(Result.Ok, data);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE DESTROYED");
#endif
            _receiver = null;
        }
    }
}