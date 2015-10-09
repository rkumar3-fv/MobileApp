using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Actions.Requests;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Services;
using Java.Util.Concurrent.Atomic;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionsHelper event delegate
    /// </summary>
    /// <param name="sender">ActionsHelper</param>
    /// <param name="args">ActionsHelperEventArgs</param>
    public delegate void ActionsHelperEvent(object sender, EventArgs args);

    public class ActionsHelper : IComServiceResultReceiver
    {
        /// <summary>
        /// Is first app launch flag
        /// </summary>
        public bool IsFirstRun { get; private set; }

        /// <summary>
        /// Last entered user login
        /// </summary>
        private string _userLogin;

        /// <summary>
        /// Last entered user password
        /// </summary>
        private string _userPassword;

        /// <summary>
        /// Check is logged in service
        /// </summary>
        public bool IsLoggedIn { get; private set; }

        /// <summary>
        /// Selected account
        /// </summary>
        public Account SelectedAccount { get; set; }

        /// <summary>
        /// Available accounts list
        /// </summary>
        public List<Account>AccountsList { get; private set; }

        /// <summary>
        /// Event for activity or fragment handling
        /// </summary>
        public event ActionsHelperEvent HelperEvent;

        /// <summary>
        /// App instance
        /// </summary>
        private readonly App _app;

        /// <summary>
        /// Result receiver for service communication
        /// </summary>
        private readonly ComServiceResultReceiver _receiver;

        /// <summary>
        /// Waiting requests queue
        /// </summary>
        private readonly Dictionary<long, BaseRequest> _waitingRequestArray;

        /// <summary>
        /// Unhandled responses from server
        /// </summary>
        private readonly Dictionary<long, BaseResponse> _unhandledResponseArray;

        private readonly AtomicLong _idCounter;
        private readonly AppPreferencesHelper _preferencesHelper;

        /// <summary>
        /// Set actions helper for current context
        /// </summary>
        /// <param name="app">app base context</param>
        public ActionsHelper(App app)
        {
            _app = app;
            _idCounter = new AtomicLong();
            _waitingRequestArray = new Dictionary<long, BaseRequest>();
            _unhandledResponseArray = new Dictionary<long, BaseResponse>();
            _receiver = new ComServiceResultReceiver(new Handler());
            _receiver.SetListener(this);
            _preferencesHelper = AppPreferencesHelper.Instance(_app);
            IsFirstRun = _preferencesHelper.IsFirstRun();
            Log.Debug(App.AppPackage, "HELPER: " + (IsFirstRun ? "First run" : "Not first run"));
        }

        /// <summary>
        /// Get next ID for request
        /// </summary>
        private long RequestId => _idCounter.IncrementAndGet();

        /// <summary>
        /// Get all unhandled responses
        /// </summary>
        public void GetUnhandledResponses()
        {
            foreach (var response in _unhandledResponseArray)
            {
                //TODO: notification
            }
        }

        /// <summary>
        /// Get unhandled responses by type
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        public void GetUnhandledResponse<T>()
        {
            foreach (var response in _unhandledResponseArray.Where(response => response.Value is T))
            {
                //TODO: notification
            }
        }

        /// <summary>
        /// Remove unhandled response
        /// </summary>
        /// <param name="id">request ID</param>
        public void RemoveUnhandledResponse(long id)
        {
            if (_unhandledResponseArray.ContainsKey(id))
                _unhandledResponseArray.Remove(id);
        }

        /// <summary>
        /// Sets that not first app usage
        /// </summary>
        public void DisclaimerApplied()
        {
            IsFirstRun = false;
            _preferencesHelper.SetNotIsFirstRun();
        }

        /// <summary>
        /// Authorization action
        /// </summary>
        /// <param name="login">typed login</param>
        /// <param name="password">typed password</param>
        /// <returns>request ID</returns>
        public long Authorize(string login, string password)
        {
            if (IsLoggedIn)
                return -1;

            var requestId = RequestId;
            var loginRequest = new LoginRequest(requestId, login, password);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is LoginRequest).Where(request => ((LoginRequest)(request.Value)).Equals(loginRequest)))
            {
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate Authorize request. Execute ID=" + request.Key);
                return request.Key;
            }
            _userLogin = login;
            _userPassword = password;   
            Log.Debug(App.AppPackage, "HELPER REQUEST: Authorize ID="+requestId);
            PrepareIntent(requestId, loginRequest);
            return requestId;
        }

        /// <summary>
        /// Logout action
        /// </summary>
        /// <returns>request ID</returns>
        public long Logout()
        {
            if (!IsLoggedIn)
                return -1;

            var requestId = RequestId;
            var logoutRequest = new LogoutRequest(requestId);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is LogoutRequest).Where(request => ((LogoutRequest)(request.Value)).Equals(logoutRequest)))
            {
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate Logout request. Execute ID=" + request.Key);
                return request.Key;
            }
            Log.Debug(App.AppPackage, "HELPER REQUEST: Logout ID=" + requestId);
            PrepareIntent(requestId, logoutRequest);
            return requestId;
        }

        /// <summary>
        /// Restore password action
        /// </summary>
        /// <param name="email">e-mail</param>
        /// <returns>request ID</returns>
        public long RestorePassword(string email)
        {
            var requestId = RequestId;
            var restoreRequest = new RestorePasswordRequest(requestId, email);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is RestorePasswordRequest).Where(request => ((RestorePasswordRequest)(request.Value)).Equals(restoreRequest)))
            {
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate RestorePassword request. Execute ID=" + request.Key);
                return request.Key;
            }
            Log.Debug(App.AppPackage, "HELPER REQUEST: RestorePassword ID=" + requestId);
            PrepareIntent(requestId, restoreRequest);
            return requestId;
        }

        /// <summary>
        /// Get accounts action
        /// </summary>
        /// <returns>request ID</returns>
        public long GetAccounts()
        {
            var requestId = RequestId;
            var getAccsRequest = new GetAccountsRequest(requestId);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetAccountsRequest).Where(request => ((GetAccountsRequest)(request.Value)).Equals(getAccsRequest)))
            {
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate RestorePassword request. Execute ID=" + request.Key);
                return request.Key;
            }
            Log.Debug(App.AppPackage, "HELPER REQUEST: GetAccounts ID=" + requestId);
            PrepareIntent(requestId, getAccsRequest);  
            return requestId;
        }

        /// <summary>
        /// Call request
        /// </summary>
        /// <param name="number">number for outgoing call</param>
        /// <returns>request ID</returns>
        public long Call(string number)
        {
            var requestId = RequestId;

            Log.Debug(App.AppPackage, "HELPER REQUEST: Call ID=" + requestId);

            return requestId;
        }

        /// <summary>
        /// Prepare intent for request
        /// </summary>
        /// <param name="requestId">request ID</param>
        /// <param name="request">request for sending</param>
        /// <returns>prepaired intent</returns>
        private void PrepareIntent(long requestId, BaseRequest request)
        {
            var intent = new Intent(_app, typeof(ComService));
            intent.SetAction(ComService.ExecuteAction);
            intent.PutExtra(ComService.RequestIdTag, requestId);
            intent.PutExtra(ComServiceResultReceiver.ReceiverTag, _receiver);
            intent.PutExtra(ComService.RequestTag, request);
            _waitingRequestArray.Add(requestId, request);
            Log.Debug(App.AppPackage, "HELPER INTENT CREATED: request ID="+requestId);
            _app.StartService(intent);
        }

        /// <summary>
        /// Responses from ComService
        /// </summary>
        /// <param name="resultCode">result code</param>
        /// <param name="resultData">result data bundle</param>
        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            if (resultCode != (int)Result.Ok) return;
            var response = resultData.GetParcelable(ComServiceResultReceiver.ReceiverDataExtra) as BaseResponse;
            if (response != null)
                ResponseResultActionExecutor(response);
        }

        /// <summary>
        /// Check response and react
        /// </summary>
        /// <param name="response">response from ComService</param>
        private void ResponseResultActionExecutor(BaseResponse response)
        {
            var type = response.GetType().Name;
            Intent intent;
            if (!_waitingRequestArray.ContainsKey(response.RequestId))
            {
                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: NOT WAITED response for request with ID={response.RequestId}, Type is {type}");
                return;
            }
            Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId}, Type is {type}");
            switch (type)
            {
                // Login action response
                case "LoginResponse":
                    IsLoggedIn = true;
                    GetAccounts();
                    break;

                // Login action response
                case "LogoutResponse":
                    _userLogin = "";
                    _userPassword = "";
                    IsLoggedIn = false;
                    AccountsList = null;
                    SelectedAccount = null;
                    intent = new Intent(_app, typeof(AuthActivity));
                    HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                    break;

                // GetAccounts action response
                case "GetAccountsResponse":
                    var accsResponse = (GetAccountsResponse) response;
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: detect {accsResponse.AccountsList.Count} accounts");
                    switch (accsResponse.AccountsList.Count)
                    {
                        // No one account is active
                        case 0:
                            SelectedAccount = null;
                            intent = new Intent(_app, typeof (InactiveActivity));
                            break;

                        // Only one active account
                        case 1:
                            AccountsList = accsResponse.AccountsList;
                            SelectedAccount = AccountsList[0];
                            intent = IsFirstRun ? new Intent(_app, typeof(DisclaimerActivity)) : new Intent(_app, typeof(SelectAccountActivity));
                            break;

                        // More than one active accounts
                        default:
                            AccountsList = accsResponse.AccountsList;
                            intent = new Intent(_app, typeof(SelectAccountActivity));
                            break;
                    }
                    HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                    break;
            }
            _waitingRequestArray.Remove(response.RequestId);
        }
    }
}
