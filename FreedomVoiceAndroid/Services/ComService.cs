using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Actions.Requests;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Background server-client communication service
    /// </summary>
    [Service (Exported = false)]
    public class ComService : Service
    {
        public const string ExecuteAction = "ComServiceExecute";
        public const string CancelAction = "ComServiceCancel";
        public const string RequestTag = "ComServiceRequest";
        public const string RequestIdTag = "ComServiceRequestId";

        private ComServiceResultReceiver _receiver;

        //TODO: convert to map
        private readonly List<long> _activeActions = new List<long>(); 

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (_receiver != null)
                _receiver = intent.GetParcelableExtra(ComServiceResultReceiver.ReceiverTag) as ComServiceResultReceiver;
            if (intent.Action == CancelAction)
            {
                //TODO : cancel action
            }
            else if (intent.Action == ExecuteAction)
            {
                _activeActions.Add(intent.GetLongExtra(RequestIdTag, 0));
                var request = intent.GetParcelableExtra(RequestTag) as BaseRequest;
                if (request != null)
                    ExecuteRequest(request);
            }
            return StartCommandResult.NotSticky;
        }

        private async void ExecuteRequest(BaseRequest request)
        {
            var result = await request.ExecuteRequest();
            var data = new Bundle();
            data.PutParcelable(ComServiceResultReceiver.ReceiverDataExtra, result);
            _receiver.Send(Result.Ok, data);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _receiver = null;
        }
    }
}