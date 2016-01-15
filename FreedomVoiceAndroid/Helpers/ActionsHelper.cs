using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
#if DEBUG
using Android.Util;
#endif
using System.Diagnostics;
using System.Threading.Tasks;
using com.FreedomVoice.MobileApp.Android.Actions.Requests;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Services;
using com.FreedomVoice.MobileApp.Android.Storage;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core;
using FreedomVoice.Core.Cache;
using FreedomVoice.Core.Cookies;
using FreedomVoice.Core.Utils;
using Java.Util.Concurrent.Atomic;
using Pair = Android.Support.V4.Util.Pair;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionsHelper event delegate
    /// </summary>
    /// <param name="sender">ActionsHelper</param>
    /// <param name="args">ActionsHelperEventArgs</param>
    public delegate void ActionsHelperEvent(object sender, EventArgs args);

    public class ActionsHelper : IAppServiceResultReceiver
    {
        /// <summary>
        /// Is first app launch flag
        /// </summary>
        public bool IsFirstRun { get; private set; }

        /// <summary>
        /// Messages polling interval
        /// </summary>
        public double PollingInterval {
            get
            {
                if (!(Math.Abs(_pollingInterval) > 0))
                    GetPolling();
                return _pollingInterval;
            }
        }
        
        private double _pollingInterval;
        private int _repeats;

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
        /// Available extensions
        /// </summary>
        public List<Extension>ExtensionsList { get; private set; } 

        /// <summary>
        /// Selected extension index
        /// </summary>
        public int SelectedExtension { get; private set; } 

        /// <summary>
        /// Selected folder index
        /// </summary>
        public int SelectedFolder { get; private set; }

        /// <summary>
        /// Selected message index
        /// </summary>
        public int SelectedMessage { get; set; }

        /// <summary>
        /// Recents list
        /// </summary>
        public SortedDictionary<long, RecentHolder> RecentsDictionary { get; }

        /// <summary>
        /// Event for activity or fragment handling
        /// </summary>
        public event ActionsHelperEvent HelperEvent;

        /// <summary>
        /// App instance
        /// </summary>
        private readonly App _app;

        private Pair _accPair;

        /// <summary>
        /// Result receiver for service communication
        /// </summary>
        private readonly ComServiceResultReceiver _receiver;

        /// <summary>
        /// Waiting requests dictionary
        /// </summary>
        private readonly Dictionary<long, BaseRequest> _waitingRequestArray;

        /// <summary>
        /// Watchers dictionary
        /// </summary>
        private readonly Dictionary<long, Stopwatch> _watchersDictionary;

        private readonly AppDbHelper _dbHelper;
        private readonly AtomicLong _idCounter;
        private readonly AppPreferencesHelper _preferencesHelper;
        private long _preferencesTime;
        private long _initHelperTime;

        /// <summary>
        /// Set actions helper for current context
        /// </summary>
        /// <param name="app">app base context</param>
        public ActionsHelper(App app)
        {
            var watcher = Stopwatch.StartNew();
            _app = app;
            _dbHelper = AppDbHelper.Instance(app);
            _idCounter = new AtomicLong();
            _waitingRequestArray = new Dictionary<long, BaseRequest>();
            _watchersDictionary = new Dictionary<long, Stopwatch>();
            _receiver = new ComServiceResultReceiver(new Handler());
            _receiver.SetListener(this);
            _preferencesHelper = AppPreferencesHelper.Instance(_app);
            IsFirstRun = _preferencesHelper.IsFirstRun();
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER: " + (IsFirstRun ? "First run" : "Not first run"));
#endif
            var cacheImpl = new PclCacheImpl(_app);
            var cookieImpl = new PclCookieImpl(_app);
            ServiceContainer.Register<IDeviceCacheStorage>(() => cacheImpl);
            ServiceContainer.Register<IDeviceCookieStorage>(() => cookieImpl);
            RecentsDictionary = new SortedDictionary<long, RecentHolder>(Comparer<long>.Create((x, y) => y.CompareTo(x)));
            ExtensionsList = new List<Extension>();
            SelectedExtension = -1;
            SelectedFolder = -1;
            SelectedMessage = -1;
            _repeats = 0;
            if (!IsFirstRun)
            {
                var intentLoading = new Intent(_app, typeof(LoadingActivity));
                intentLoading.SetFlags(ActivityFlags.NewTask);
                intentLoading.SetFlags(ActivityFlags.ClearTop);
                HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(-2, intentLoading));

                var watcherLoading = Stopwatch.StartNew();
                var pair = _preferencesHelper.GetLoginPass(AppHelper.InsightsKey);
                if (pair != null)
                {
                    _userLogin = (string) pair.First;
                    _userPassword = (string) pair.Second;
                }
                _accPair = _preferencesHelper.GetAccCaller();
                _pollingInterval = _preferencesHelper.GetPollingInterval();
                watcherLoading.Stop();
                _preferencesTime = watcherLoading.ElapsedMilliseconds;
                if (ApiHelper.CookieContainer != null)
                {
                    var expireFlag = false;
                    var collection = CookieHelper.GetAllCookies(ApiHelper.CookieContainer);
                    if ((collection != null)&&(collection.Count>0))
                    {
                        if (collection.Any(cookie => cookie.Expired))
                            expireFlag = true; 
                    }
                    else
                        expireFlag = true;
                    if (!expireFlag)
                    {
                        IsLoggedIn = true;
                        if ((AccountsList == null) || (SelectedAccount == null))
                        {
                            GetAccounts();
                            if (watcher.IsRunning)
                                watcher.Stop();
                            _initHelperTime = watcher.ElapsedMilliseconds;
                            return;
                        }
                        if ((SelectedAccount.PresentationNumbers == null) ||
                            (string.IsNullOrEmpty(SelectedAccount.PresentationNumber)))
                        {
                            GetPresentationNumbers();
                            if (watcher.IsRunning)
                                watcher.Stop();
                            _initHelperTime = watcher.ElapsedMilliseconds;
                            return;
                        }
                        var intent = new Intent(_app, typeof (ContentActivity));
                        intent.SetFlags(ActivityFlags.NewTask);
                        if (watcher.IsRunning)
                            watcher.Stop();
                        _initHelperTime = watcher.ElapsedMilliseconds;
                        _app.StartActivity(intent);
                        return;
                    }
                }
#if DEBUG
                else
                {
                    Log.Debug(App.AppPackage, "EXPIRED COOKIE");
                }
#endif
                if (!string.IsNullOrEmpty(_userLogin) && !string.IsNullOrEmpty(_userPassword))
                {
                    Authorize(_userLogin, _userPassword);
                    if (watcher.IsRunning)
                        watcher.Stop();
                    _initHelperTime = watcher.ElapsedMilliseconds;
                    return;
                }
            }
            var authIntent = new Intent(_app, typeof(AuthActivity));
            authIntent.SetFlags(ActivityFlags.NewTask);
            if (watcher.IsRunning)
                watcher.Stop();
            _initHelperTime = watcher.ElapsedMilliseconds;
             _app.StartActivity(authIntent);
        }

        /// <summary>
        /// Get next ID for request
        /// </summary>
        private long RequestId => _idCounter.IncrementAndGet();

        /// <summary>
        /// Sets that not first app usage
        /// </summary>
        public void DisclaimerApplied()
        {
            IsFirstRun = false;
            _preferencesHelper.SetNotIsFirstRun();
        }

        public long Authorize()
        {
            if (!string.IsNullOrEmpty(_userLogin) && !string.IsNullOrEmpty(_userPassword))
                return Authorize(_userLogin, _userPassword);
            if ((_accPair != null) && (!string.IsNullOrEmpty((string) _accPair.First)) && (!string.IsNullOrEmpty((string) _accPair.Second)))
                return Authorize((string) _accPair.First, (string) _accPair.Second);
            return -100;
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
            foreach (var request in _waitingRequestArray.Where(response => response.Value is LoginRequest).Where(request => ((LoginRequest)request.Value).Equals(loginRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate Authorize request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
            _userLogin = login;
            _userPassword = password;
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: Authorize ID="+requestId);
#endif
            PrepareIntent(requestId, loginRequest);
            return requestId;
        }

        /// <summary>
        /// Logout action
        /// </summary>
        /// <returns>request ID</returns>
        public long Logout()
        {
            var requestId = RequestId;
            var logoutRequest = new LogoutRequest(requestId);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is LogoutRequest).Where(request => ((LogoutRequest)request.Value).Equals(logoutRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate Logout request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: Logout ID=" + requestId);
#endif
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
            if (!CheckRequestAbility(requestId)) return requestId;
            var restoreRequest = new RestorePasswordRequest(requestId, email);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is RestorePasswordRequest).Where(request => ((RestorePasswordRequest)request.Value).Equals(restoreRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate RestorePassword request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: RestorePassword ID=" + requestId);
#endif
            PrepareIntent(requestId, restoreRequest);
            return requestId;
        }

        /// <summary>
        /// Get current polling interval
        /// </summary>
        /// <returns>request ID</returns>
        public long GetPolling()
        {
            var requestId = RequestId;
            if (!CheckRequestAbility(requestId)) return requestId;        
            var restoreRequest = new GetPollingRequest(requestId);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetPollingRequest))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate GetPollingInterval request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: GetPollingInterval ID=" + requestId);
#endif
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
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetAccountsRequest).Where(request => ((GetAccountsRequest)request.Value).Equals(getAccsRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate GetAccounts request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: GetAccounts ID=" + requestId);
#endif
            PrepareIntent(requestId, getAccsRequest);  
            return requestId;
        }

        /// <summary>
        /// Get presentation numbers action
        /// </summary>
        /// <returns>request ID</returns>
        public long GetPresentationNumbers()
        {
            var requestId = RequestId;
            if (!CheckRequestAbility(requestId)) return requestId;
            var getPresNumbersRequest = new GetPresentationNumbersRequest(requestId, SelectedAccount.AccountName);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetPresentationNumbersRequest).Where(request => ((GetPresentationNumbersRequest)request.Value).Equals(getPresNumbersRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate GetPresentationNumbers request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: GetPresentationNumbers ID=" + requestId);
#endif
            PrepareIntent(requestId, getPresNumbersRequest);
            return requestId;
        }

        /// <summary>
        /// Get or update extensions list
        /// </summary>
        /// <returns>request ID</returns>
        public long GetExtensions()
        {
            if (ExtensionsList != null && ExtensionsList.Count == 0)
                return ForceLoadExtensions();
            return -1;
        }

        /// <summary>
        /// Force update extensions list
        /// </summary>
        /// <returns>request ID</returns>
        public long ForceLoadExtensions()
        {
            var requestId = RequestId;
            if (!CheckMessageUpdate(requestId)) return requestId;
            var getExtRequest = new GetExtensionsRequest(requestId, SelectedAccount.AccountName);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetExtensionsRequest).Where(request => ((GetExtensionsRequest)request.Value).Equals(getExtRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate ForceLoadExtensions request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: ForceLoadExtensions ID=" + requestId);
#endif
            PrepareIntent(requestId, getExtRequest);
            return requestId;
        }

        /// <summary>
        /// Force update current folders list
        /// </summary>
        /// <returns>request ID</returns>
        public long ForceLoadFolders()
        {
            if (SelectedExtension == -1) return -1;
            var requestId = RequestId;
            if (!CheckMessageUpdate(requestId)) return requestId;
            var getFoldersRequest = new GetFoldersRequest(requestId, SelectedAccount.AccountName, ExtensionsList[SelectedExtension].Id);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetFoldersRequest).Where(request => ((GetFoldersRequest)request.Value).Equals(getFoldersRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate ForceLoadFolders request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: ForceLoadFolders ID=" + requestId);
#endif
            PrepareIntent(requestId, getFoldersRequest);
            return requestId;
        }

        /// <summary>
        /// Force update current messages list
        /// </summary>
        /// <returns>request ID</returns>
        public long ForceLoadMessages()
        {
            if ((SelectedExtension == -1)||(SelectedFolder == -1)) return -1;
            var requestId = RequestId;
            if (!CheckMessageUpdate(requestId)) return requestId;
            var getMsgRequest = new GetMessagesRequest(requestId, SelectedAccount.AccountName, ExtensionsList[SelectedExtension].Id, ExtensionsList[SelectedExtension].Folders[SelectedFolder].FolderName);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetMessagesRequest).Where(request => ((GetMessagesRequest)(request.Value)).Equals(getMsgRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate ForceLoadMessages request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: ForceLoadMessages ID=" + requestId);
#endif
            PrepareIntent(requestId, getMsgRequest);
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
            if (!CheckRequestAbility(requestId)) return requestId;
            if (!_app.ApplicationHelper.CheckCallsPermission())
            {
                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(requestId, new[] {ActionsHelperEventArgs.CallPermissionDenied}));
            }
            else
            {
                var phone = _app.ApplicationHelper.GetMyPhoneNumber();
                if (phone == null)
                {
                    HelperEvent?.Invoke(this,
                        new ActionsHelperEventArgs(requestId, new[] {ActionsHelperEventArgs.CallReservationNotSupports}));
                }
                else if (phone.Length < 10)
                {
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(requestId, new[] {ActionsHelperEventArgs.PhoneNumberNotSets}));
                }
                else
                {
                    var reserveCallRequest = new CallReservationRequest(requestId, SelectedAccount.AccountName,
                        SelectedAccount.PresentationNumber, phone, number);
                    foreach (var request in _waitingRequestArray.Where(response => response.Value is CallReservationRequest)
                                .Where(request => ((CallReservationRequest) (request.Value)).Equals(reserveCallRequest)))
                    {
#if DEBUG
                        Log.Debug(App.AppPackage,
                            "HELPER REQUEST: Duplicate call reservation request. Execute ID=" + request.Key);
#endif
                        return request.Key;
                    }
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER REQUEST: Call ID={requestId}");
                    Log.Debug(App.AppPackage,
                        $"Call from {reserveCallRequest.Account} (shows as {reserveCallRequest.PresentationNumber}) to {reserveCallRequest.DialingNumber} using SIM {reserveCallRequest.RealSimNumber}");
#endif
                    PrepareIntent(requestId, reserveCallRequest);
                }
            }
            return requestId;
        }

        /// <summary>
        /// Remove message action
        /// </summary>
        /// <param name="index">index of message</param>
        /// <returns>request ID</returns>
        public long RemoveMessage(int index)
        {
            var requestId = RequestId;
            if (!CheckRequestAbility(requestId)) return requestId;
            var removeRequest = new RemoveMessageRequest(requestId, SelectedAccount.AccountName, ExtensionsList[SelectedExtension].Id, 
                ExtensionsList[SelectedExtension].Folders[SelectedFolder].MessagesList[index].Name);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is RemoveMessageRequest).Where(request => ((RemoveMessageRequest)(request.Value)).Equals(removeRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate Remove message request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: Remove message request ID=" + requestId);
#endif
            PrepareIntent(requestId, removeRequest);
            return requestId;
        }

        /// <summary>
        /// Delete message action
        /// </summary>
        /// <param name="index">index of message</param>
        /// <returns>request ID</returns>
        public long DeleteMessage(int index)
        {
            var requestId = RequestId;
            if (!CheckRequestAbility(requestId)) return requestId;
            var deleteRequest = new DeleteMessageRequest(requestId, SelectedAccount.AccountName, ExtensionsList[SelectedExtension].Id,
                ExtensionsList[SelectedExtension].Folders[SelectedFolder].MessagesList[index].Name);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is DeleteMessageRequest).Where(request => ((DeleteMessageRequest)(request.Value)).Equals(deleteRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate Delete message request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: Delete message request ID=" + requestId);
#endif
            PrepareIntent(requestId, deleteRequest);
            return requestId;
        }

        /// <summary>
        /// Restore message action
        /// </summary>
        /// <param name="messageCode">message ID</param>
        /// <returns>request ID</returns>
        public long RestoreMessage(string messageCode)
        {
            var requestId = RequestId;
            if (!CheckRequestAbility(requestId)) return requestId;
            var restoreRequest = new RestoreMessageRequest(requestId, SelectedAccount.AccountName, ExtensionsList[SelectedExtension].Id, messageCode,
                ExtensionsList[SelectedExtension].Folders[SelectedFolder].FolderName);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is RestoreMessageRequest).Where(request => ((RestoreMessageRequest)(request.Value)).Equals(restoreRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate Restore message request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER REQUEST: Restore message request ID=" + requestId);
#endif
            PrepareIntent(requestId, restoreRequest);
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
            intent.PutExtra(ComService.RequestIdTag, requestId);
            intent.PutExtra(ComServiceResultReceiver.ReceiverTag, _receiver);
            intent.PutExtra(ComService.RequestTag, request);
            _waitingRequestArray.Add(requestId, request);
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER INTENT CREATED: request ID="+requestId);
#endif
            _watchersDictionary.Add(requestId, Stopwatch.StartNew());
            _app.StartService(intent);
        }

        private bool CheckRequestAbility(long requestId)
        {
            if (!_app.ApplicationHelper.IsInternetConnected())
            {
                HelperEvent?.Invoke(this,
                    new ActionsHelperEventArgs(requestId, new[] {ActionsHelperEventArgs.NoInternetConnection}));
                return false;
            }
            return true;
        }

        private bool CheckMessageUpdate(long requestId)
        {
            if (_app.ApplicationHelper.IsAirplaneModeOn())
            {
                HelperEvent?.Invoke(this,
                    new ActionsHelperEventArgs(requestId, new[] { ActionsHelperEventArgs.MsgUpdateFailedAirplane }));
                return false;
            }
            if (!_app.ApplicationHelper.IsInternetConnected())
            {
                HelperEvent?.Invoke(this,
                    new ActionsHelperEventArgs(requestId, new[] { ActionsHelperEventArgs.MsgUpdateFailed }));
                return false;
            }
            return true;

        }

        /// <summary>
        /// Get current messages list content
        /// </summary>
        /// <returns>list content</returns>
        public List<MessageItem> GetCurrent()
        {
            if ((SelectedExtension == -1)||(SelectedExtension > ExtensionsList.Count))
                return ExtensionsList?.Cast<MessageItem>().ToList();
            if ((SelectedFolder ==-1)||(SelectedFolder > ExtensionsList[SelectedExtension].Folders.Count))
                return ExtensionsList[SelectedExtension].Folders?.Cast<MessageItem>().ToList();
            return ExtensionsList[SelectedExtension].Folders[SelectedFolder].MessagesList?.Cast<MessageItem>().ToList();
        } 

        /// <summary>
        /// Move messages list to next level
        /// </summary>
        /// <param name="position">selected element</param>
        /// <returns>next level list content</returns>
        public List<MessageItem> GetNext(int position)
        {
            if (SelectedExtension == -1)
                SelectedExtension = position;
            else if (SelectedFolder == -1)
                SelectedFolder = position;
            else
                SelectedMessage = position;
            return GetCurrent();
        }

        /// <summary>
        /// Move messages list to previous level
        /// </summary>
        public void GetPrevious()
        {
            if (SelectedMessage != -1)
                SelectedMessage = -1;
            else if (SelectedFolder != -1)
                SelectedFolder = -1;
            else if (SelectedExtension != -1)
            {
                if (ExtensionsList.Count != 1)
                    SelectedExtension = -1;
            }
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(-1, new[] { ActionsHelperEventArgs.MsgUpdated }));
        }

        public void RemoveRecent(RecentHolder holder)
        {
            _dbHelper.RemoveRecent(SelectedAccount.AccountName, holder.SingleRecent.PhoneNumber);
        }

        /// <summary>
        /// ClearRecents
        /// </summary>
        public void ClearAllRecents()
        {
            _dbHelper.RemoveRecents(SelectedAccount.AccountName);
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(-1, new[] { ActionsHelperEventArgs.ClearRecents }));
        }

        /// <summary>
        /// Changing presentation number
        /// </summary>
        /// <param name="index">number index</param>
        public void SetPresentationNumber(int index)
        {
            SelectedAccount.SelectedPresentationNumber = index;
            _preferencesHelper.SaveAccCaller(SelectedAccount.AccountName, SelectedAccount.PresentationNumber);
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(-1, new[] { ActionsHelperEventArgs.ChangePresentation }));
        }

        public void SaveNewNumber(string number)
        {
            _app.ApplicationHelper.SetMyPhoneNumber(number);
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
            {
                if (_watchersDictionary.ContainsKey(response.RequestId)&&(_watchersDictionary[response.RequestId] != null))
                {
                    if (_watchersDictionary[response.RequestId].IsRunning)
                        _watchersDictionary[response.RequestId].Stop();
                    var time = _watchersDictionary[response.RequestId].ElapsedMilliseconds;
                    _watchersDictionary.Remove(response.RequestId);
                    var requestName = _waitingRequestArray[response.RequestId].GetType().Name;
                    var responseName = response.GetType().Name;
                    if (responseName == "ErrorResponse")
                    {
                        var errorResponse = (ErrorResponse) response;
                        string postfix;
                        switch (errorResponse.ErrorCode)
                        {
                            case ErrorResponse.ErrorInternal:
                                postfix = "500 - Internal Server Error";
                                break;
                            case ErrorResponse.ErrorBadRequest:
                                postfix = "400 - Bad Request";
                                break;
                            case ErrorResponse.ErrorCancelled:
                                postfix = "Cancelled";
                                break;
                            case ErrorResponse.ErrorConnection:
                                postfix = "Connection Lost";
                                break;
                            case ErrorResponse.ErrorGatewayTimeout:
                                postfix = "504 - Gateway Timeout";
                                break;
                            case ErrorResponse.ErrorRequestTimeout:
                                postfix = "408 - Request Timeout";
                                break;
                            case ErrorResponse.ErrorNotFound:
                                postfix = "404 - Not Found";
                                break;
                            case ErrorResponse.ErrorNotPaid:
                                postfix = "402 - Payment Required";
                                break;
                            case ErrorResponse.ErrorUnauthorized:
                                postfix = "401 - Unauthorized";
                                break;
                            case ErrorResponse.ErrorForbidden:
                                postfix = "403 - Forbidden";
                                break;
                            default:
                                postfix = "Unknown Error";
                                break;
                        }
                        _app.ApplicationHelper.ReportTime(TimingEvent.Request, requestName, $"{responseName}: {postfix}", time);
#if DEBUG
                        Log.Debug(App.AppPackage, $"<{requestName}> - <{responseName}>: {postfix}");
                        Log.Debug(App.AppPackage, $"JSON: {errorResponse.ErrorJson}");
#endif
                    }
                    else
                    {
                        _repeats = 0;
                        _app.ApplicationHelper.ReportTime(TimingEvent.Request, requestName, responseName, time);
#if DEBUG
                        Log.Debug(App.AppPackage, $"<{requestName}> OK");
#endif
                    }
                    if (_preferencesTime != 0)
                    {
                        _app.ApplicationHelper.ReportTime(TimingEvent.LongAction, "LOADING FROM STORAGE", "", _preferencesTime);
                        _preferencesTime = 0;
                    }
                    if (_initHelperTime != 0)
                    {
                        _app.ApplicationHelper.ReportTime(TimingEvent.LongAction, "APP HELPER RESTORATION", "", _initHelperTime);
                        _initHelperTime = 0;
                    }
                }
                ResponseResultActionExecutor(response);
            }
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
                return;

            switch (type)
            {
                case "ErrorResponse":
                    var errorResponse = (ErrorResponse) response;
                    switch (errorResponse.ErrorCode)
                    {
                        // Connection lost
                        case ErrorResponse.ErrorConnection:
                        case ErrorResponse.ErrorGatewayTimeout:
                        case ErrorResponse.ErrorRequestTimeout:
                            if (!_waitingRequestArray.ContainsKey(response.RequestId)) break;
                            var req = _waitingRequestArray[response.RequestId].GetType().Name;
                            if ((req == "GetExtensionsRequest")||(req == "GetFoldersRequest")||(req == "GetMessagesRequest"))
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgUpdateFailed }));
                            else if (req == "GetPollingRequest")
                            {}
                            else 
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.ConnectionLostError}));
                            break;
                        // Authorization failed
                        case ErrorResponse.ErrorUnauthorized:
                            if (IsLoggedIn)
                            {
                                if (_accPair != null)
                                {
                                    var login = (string) _accPair.First;
                                    var pass = (string) _accPair.Second;
                                    if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(pass))
                                    {
                                        IsLoggedIn = false;
                                        Authorize(login, pass);
                                    }
                                    else
                                        DoLogout(response.RequestId);
                                }
                                else
                                    DoLogout(response.RequestId);
                            }
                            else
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new []{ActionsHelperEventArgs.AuthPasswdError}));
                            break;
                        //Bad request format
                        case ErrorResponse.ErrorBadRequest:
                            // Bad login format
                            if (!IsLoggedIn)
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new []{ActionsHelperEventArgs.AuthLoginError, ActionsHelperEventArgs.RestoreError}));             
                            else if (_waitingRequestArray.ContainsKey(response.RequestId))
                            {
                                // Call reservation bad request
                                if (_waitingRequestArray[response.RequestId] is CallReservationRequest)
                                {
                                    SaveRecent(response);
                                    HelperEvent?.Invoke(this,
                                        new ActionsHelperEventArgs(response.RequestId,
                                            new[] {ActionsHelperEventArgs.CallReservationFail}));
                                }
                                // Messages loading bad request
                                else if (_waitingRequestArray[response.RequestId] is GetMessagesRequest ||
                                     _waitingRequestArray[response.RequestId] is GetExtensionsRequest ||
                                     _waitingRequestArray[response.RequestId] is GetFoldersRequest)
                                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgUpdateFailedInternal }));
                                // Not polling interval bad request
                                else if (!(_waitingRequestArray[response.RequestId] is GetPollingRequest))
                                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.InternalError }));
                            }
                            break;
                        //Account not found error
                        case ErrorResponse.ErrorNotFound:
                            if (!IsLoggedIn)
                            {
                                HelperEvent?.Invoke(this,
                                    new ActionsHelperEventArgs(response.RequestId,
                                        new[] {ActionsHelperEventArgs.RestoreWrongEmail}));
                            }
                            else
                            {
                                // Messages 404
                                if (_waitingRequestArray.ContainsKey(response.RequestId) &&
                                    (_waitingRequestArray[response.RequestId] is GetMessagesRequest ||
                                     _waitingRequestArray[response.RequestId] is GetExtensionsRequest ||
                                     _waitingRequestArray[response.RequestId] is GetFoldersRequest))
                                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.MsgUpdateFailedInternal}));
                                // Not polling interval 404
                                else if (!(_waitingRequestArray[response.RequestId] is GetPollingRequest))
                                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.InternalError}));
                            }
                            break;
                        //Number on hold (payment required) error
                        case ErrorResponse.ErrorNotPaid:
                            intent = AccountsList.Count > 1 ? new Intent(_app, typeof(InactiveActivityWithBack)) : new Intent(_app, typeof(InactiveActivity));
                            intent.PutExtra(InactiveActivity.InactiveAccontTag, SelectedAccount.AccountName);
                            SelectedAccount.AccountState = false;
                            SelectedAccount = null;
                            HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                            break;
                        //Forbidden 403
                        case ErrorResponse.ErrorForbidden:
                            // Call reservation bad destination phone
                            if (_waitingRequestArray.ContainsKey(response.RequestId) && _waitingRequestArray[response.RequestId] is CallReservationRequest)
                            {
                                SaveRecent(response);
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.CallReservationWrong}));
                            }
                            else
                                DoLogout(response.RequestId);
                            break;
                        case ErrorResponse.ErrorInternal:
                            if (!_waitingRequestArray.ContainsKey(response.RequestId)) break;
                            var reqErr = _waitingRequestArray[response.RequestId].GetType().Name;
                            if ((reqErr == "GetExtensionsRequest")||(reqErr == "GetFoldersRequest")||(reqErr == "GetMessagesRequest"))
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgUpdateFailedInternal}));
                            else if (reqErr != "GetPollingRequest")
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.InternalError}));
                            break;
                        case ErrorResponse.ErrorUnknown:
                            if (!_waitingRequestArray.ContainsKey(response.RequestId)) break;
                            var reqError = _waitingRequestArray[response.RequestId].GetType().Name;
                            if (reqError != "GetPollingRequest")
                            {
                                if (_repeats < 5)
                                {
                                    var repeatable = _waitingRequestArray[response.RequestId];
                                    _waitingRequestArray.Remove(response.RequestId);
                                    _repeats++;
                                    PrepareIntent(RequestId, repeatable);
                                }
                                else
                                {
                                    if ((reqError == "GetExtensionsRequest") || (reqError == "GetFoldersRequest") || (reqError == "GetMessagesRequest"))
                                        HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.MsgUpdateFailedInternal}));
                                    else
                                        HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.InternalError}));
                                }
                            }
                            break;
                    }
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    break;

                // Login action response
                case "LoginResponse":
                    IsLoggedIn = true;
                    _preferencesHelper.SaveCredentials(_userLogin, _userPassword, AppHelper.InsightsKey);
                    _waitingRequestArray.Remove(response.RequestId);
                    if (_app.ApplicationHelper.IsVoicecallsSupported()&&(IsFirstRun || (_app.ApplicationHelper.GetMyPhoneNumber() == "")))
                    {
                        intent = new Intent(_app, typeof(SetNumberActivity));
                        HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));          
                    }
                    else
                        GetAccounts();
                    break;

                // Restore password response
                case "RestorePasswordResponse":
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.RestoreOk }));
                    break;

                // Restore password response
                case "GetPollingResponse":
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    var pollingResponse = (GetPollingResponse) response;
                    if (Math.Abs(pollingResponse.PollingInterval) > 0)
                    {
                        _pollingInterval = pollingResponse.PollingInterval;
                        _preferencesHelper.SavePollingInterval(PollingInterval);
                    }
                    else
                        _pollingInterval = 30000;
                    break;

                // Login action response
                case "LogoutResponse":
                    DoLogout(response.RequestId);
                    break;

                // GetAccounts action response
                case "GetAccountsResponse":
                    var accsResponse = (GetAccountsResponse) response;
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    switch (accsResponse.AccountsList.Count)
                    {
                        // No one account is active
                        case 0:
                            SelectedAccount = null;
                            intent = new Intent(_app, typeof (InactiveActivity));
                            HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                            break;

                        // Only one active account
                        case 1:
                            AccountsList = accsResponse.AccountsList;
                            SelectedAccount = AccountsList[0];
                            GetPresentationNumbers();
                            break;

                        // More than one active accounts
                        default:
                            AccountsList = accsResponse.AccountsList;
                            if (_accPair != null)
                            {
                                var accName = (string) _accPair.First;
                                if (!string.IsNullOrEmpty(accName))
                                {
                                    var selAccount = new Account(accName, new List<string>());
                                    foreach (var account in AccountsList.Where(account => account.Equals(selAccount)))
                                    {
                                        SelectedAccount = account;
                                        break;
                                    }
                                }
                                else
                                    _accPair = null;
                            }
                            if ((SelectedAccount != null) && (AccountsList.Contains(SelectedAccount)))
                                GetPresentationNumbers();
                            else
                            {
                                intent = new Intent(_app, typeof (SelectAccountActivity));
                                HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                            }
                            break;
                    }
                    break;

                // Get presentation numbers response
                case "GetPresentationNumbersResponse":
                    var numbResponse = (GetPresentationNumbersResponse)response;
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    switch (numbResponse.NumbersList.Count)
                    {
                        // No one active presentation numbers
                        case 0:
                            intent = new Intent(_app, typeof(InactiveActivityWithBack));
                            break;
                        // One or more presentation numbers
                        default:
                            SelectedAccount.PresentationNumbers = numbResponse.NumbersList;
                            RestoreRecents(_dbHelper.GetRecents(SelectedAccount.AccountName));
                            if (IsFirstRun)
                                intent = new Intent(_app, typeof (DisclaimerActivity));
                            else
                            {
                                if (_accPair != null)
                                {
                                    var caller = (string) _accPair.Second;
                                    if (!string.IsNullOrEmpty(caller))
                                    {
                                        for (var i = 0; i < SelectedAccount.PresentationNumbers.Count; i++)
                                        {
                                            if (SelectedAccount.PresentationNumbers[i] != caller) continue;
                                            SelectedAccount.SelectedPresentationNumber = i;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    _accPair = new Pair(SelectedAccount.AccountName, SelectedAccount.PresentationNumber);
                                    _preferencesHelper.SaveAccCaller(SelectedAccount.AccountName, SelectedAccount.PresentationNumber);
                                }
                                intent = new Intent(_app, typeof(ContentActivity));
                                intent.AddFlags(ActivityFlags.ClearTop);
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(-1, new[] { ActionsHelperEventArgs.ChangePresentation }));
                            }
                            ForceLoadExtensions();
                            break;
                    }

                    _dbHelper.InsertAccounts(AccountsList);
                    HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                    break;

                // Get extensions response
                case "GetExtensionsResponse":
                    var extResponse = (GetExtensionsResponse)response;
                    if (!ExtensionsList.Equals(extResponse.ExtensionsList))
                    {
                        ExtensionsList = extResponse.ExtensionsList;
                        SelectedExtension = -1;
                        SelectedFolder = -1;
                        SelectedMessage = -1;
                    }
                    if ((ExtensionsList != null) && (ExtensionsList.Count == 1))
                    {
                        SelectedExtension = 0;
                        ForceLoadFolders();
                    }
                    else
                        HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new []{ActionsHelperEventArgs.MsgUpdated}));
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    break;

                // Get folders response
                case "GetFoldersResponse":
                    var foldersResponse = (GetFoldersResponse)response;
                    if ((SelectedExtension != -1)&&!ExtensionsList[SelectedExtension].Folders.Equals(foldersResponse.FoldersList)&&(SelectedMessage == -1)&&(SelectedFolder == -1))
                    {
                        ExtensionsList[SelectedExtension].Folders = foldersResponse.FoldersList;
                        HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgUpdated }));
                    }
                    _waitingRequestArray.Remove(response.RequestId);
                    break;

                // Get messages response
                case "GetMessagesResponse":
                    var msgResponse = (GetMessagesResponse)response;
                    if ((SelectedExtension != -1)&&(SelectedFolder != -1)&&(!ExtensionsList[SelectedExtension].Folders[SelectedFolder].MessagesList.Equals(msgResponse.MessagesList)))
                    {
                        ExtensionsList[SelectedExtension].Folders[SelectedFolder].MessagesList = msgResponse.MessagesList;
                        SelectedMessage = -1;
                    }
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgUpdated }));
                    break;

                // Call reservation response
                case "CallReservationResponse":
                    var callResponse = (CallReservationResponse)response;
                    SaveRecent(response);
                    if (callResponse.ServiceNumber.Length > 6)
                    {
                        var callIntent = new Intent(Intent.ActionCall, Uri.Parse("tel:" + callResponse.ServiceNumber));
#if DEBUG
                        Log.Debug(App.AppPackage,
                            $"ACTIVITY {GetType().Name} CREATES CALL to {callResponse.ServiceNumber}");
#endif
                        if (_waitingRequestArray.ContainsKey(response.RequestId))
                            _waitingRequestArray.Remove(response.RequestId);
                        HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.CallReservationOk }));
                        HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, callIntent));
                    }
                    else
                    {
                        if (_waitingRequestArray.ContainsKey(response.RequestId))
                            _waitingRequestArray.Remove(response.RequestId);
                        HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.CallReservationFail}));
                    }
                    break;

                // Move message to trash response
                case "RemoveMessageResponse":
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgMessagesUpdated }));
                    break;

                // Restore message from trash response
                case "RestoreMessageResponse":
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgMessagesUpdated }));
                    break;

                // Delete message from trash response
                case "DeleteMessageResponse":
                    if (_waitingRequestArray.ContainsKey(response.RequestId))
                        _waitingRequestArray.Remove(response.RequestId);
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgMessagesUpdated }));
                    break;
            }
        }

        /// <summary>
        /// Clear login info
        /// </summary>
        /// <param name="id">request ID</param>
        private void DoLogout(long id)
        {
            if (_waitingRequestArray.ContainsKey(id))
                _waitingRequestArray.Remove(id);
            _userLogin = "";
            _userPassword = "";
            IsLoggedIn = false;
            AccountsList = null;
            SelectedAccount = null;
            SelectedExtension = -1;
            SelectedFolder = -1;
            SelectedMessage = -1;
            _accPair = null;
            RecentsDictionary.Clear();
            _preferencesHelper.ClearCredentials();
            _preferencesHelper.SaveAccCaller("", "");
            _preferencesHelper.SavePollingInterval(0);
            foreach (var stopwatch in _watchersDictionary.Where(stopwatch => (stopwatch.Value!=null)&&(stopwatch.Value.IsRunning)))
            {
                stopwatch.Value.Stop();
            }
            _watchersDictionary.Clear();
            var intent = new Intent(_app, typeof(AuthActivity));
            HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(id, intent));
        }

        /// <summary>
        /// Recents saving
        /// </summary>
        /// <param name="response">response from server</param>
        private void SaveRecent(BaseResponse response)
        {
            var callReservation = (CallReservationRequest)_waitingRequestArray[response.RequestId];
            var recent = new Recent(callReservation.DialingNumber);
            var task = Task.Run(() =>
            {
                _dbHelper.InsertRecent(SelectedAccount.AccountName, recent);
            });
            var values = RecentsDictionary.Values.ToList();
            var keys = RecentsDictionary.Keys.ToList();
            long keyForRemove = 0;
            var counter = 1;
            for (var i = 0; i < values.Count; i++)
            {
                if (!values[i].SingleRecent.Equals(recent)) continue;
                keyForRemove = keys[i];
                break;
            }
            if (keyForRemove != 0)
            {
                counter = RecentsDictionary[keyForRemove].Count + 1;
                RecentsDictionary.Remove(keyForRemove);
            }
            RecentsDictionary.Add(response.RequestId, new RecentHolder(recent, counter));
            if (task.IsCompleted)
                return;
            task.Wait();
        }

        private void RestoreRecents(IEnumerable<Recent> recents)
        {
            var index = -1;
            var dict = new Dictionary<string, RecentHolder>();
            foreach (var recent in recents)
            {
                if (dict.ContainsKey(recent.PhoneNumber))
                    dict[recent.PhoneNumber].Count++;
                else
                    dict.Add(recent.PhoneNumber, new RecentHolder(recent));
            }
            foreach (var recentHolder in dict)
            {
                RecentsDictionary.Add(index, recentHolder.Value);
                index--;
            }
        }
    }
}
