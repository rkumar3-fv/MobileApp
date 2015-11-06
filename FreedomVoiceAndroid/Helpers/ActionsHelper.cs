using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
#if DEBUG
using Android.Util;
using FreedomVoice.Core.Utils;
#endif
using com.FreedomVoice.MobileApp.Android.Actions.Requests;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Services;
using Java.Util.Concurrent.Atomic;
using Uri = Android.Net.Uri;

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
        /// Current phone number
        /// May be null if cellular inactive
        /// </summary>
        public string PhoneNumber { get; private set; }

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
        public SortedDictionary<long, Recent> RecentsDictionary { get; }

        public SortedDictionary<long, Recent> GetRecents()
        {
#if DEBUG
            Log.Debug(App.AppPackage, RecentsDictionary.Count.ToString());
#endif
            return RecentsDictionary;
        } 

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

        private long _successDial = -1;
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
            _receiver = new ComServiceResultReceiver(new Handler());
            _receiver.SetListener(this);
            _preferencesHelper = AppPreferencesHelper.Instance(_app);
            IsFirstRun = _preferencesHelper.IsFirstRun();
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER: " + (IsFirstRun ? "First run" : "Not first run"));
#endif
            PhoneNumber = _preferencesHelper.GetPhoneNumber();
            var telemanager = app.GetSystemService(Context.TelephonyService) as TelephonyManager;

            if ((telemanager?.Line1Number == null)||(telemanager.SimSerialNumber == null)||(telemanager.SimSerialNumber.Length==0))
                PhoneNumber = null;
            else if ((PhoneNumber != telemanager.Line1Number) && (telemanager.Line1Number.Length > 1))
            {
                if ((telemanager.Line1Number.StartsWith("1")) && (telemanager.Line1Number.Length == 11))
                    PhoneNumber = telemanager.Line1Number.Substring(1);
                else if ((telemanager.Line1Number.StartsWith("+1")) && (telemanager.Line1Number.Length == 12))
                    PhoneNumber = telemanager.Line1Number.Substring(2);
                else
                    PhoneNumber = telemanager.Line1Number;
                _preferencesHelper.SavePhoneNumber(PhoneNumber);
            }
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER: " + ((PhoneNumber == null) ? "NO CELLULAR" : $" PHONE NUMBER IS {PhoneNumber}"));
#endif
            RecentsDictionary = new SortedDictionary<long, Recent>(Comparer<long>.Create((x, y) => y.CompareTo(x)));
            ExtensionsList = new List<Extension>();
            SelectedExtension = -1;
            SelectedFolder = -1;
            SelectedMessage = -1;
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
            foreach (var request in _waitingRequestArray.Where(response => response.Value is LogoutRequest).Where(request => ((LogoutRequest)(request.Value)).Equals(logoutRequest)))
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
            var restoreRequest = new RestorePasswordRequest(requestId, email);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is RestorePasswordRequest).Where(request => ((RestorePasswordRequest)(request.Value)).Equals(restoreRequest)))
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
        /// Get accounts action
        /// </summary>
        /// <returns>request ID</returns>
        public long GetAccounts()
        {
            var requestId = RequestId;
            var getAccsRequest = new GetAccountsRequest(requestId);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetAccountsRequest).Where(request => ((GetAccountsRequest)(request.Value)).Equals(getAccsRequest)))
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
            var getPresNumbersRequest = new GetPresentationNumbersRequest(requestId, SelectedAccount.AccountName);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetPresentationNumbersRequest).Where(request => ((GetPresentationNumbersRequest)(request.Value)).Equals(getPresNumbersRequest)))
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
            var getExtRequest = new GetExtensionsRequest(requestId, SelectedAccount.AccountName);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetExtensionsRequest).Where(request => ((GetExtensionsRequest)(request.Value)).Equals(getExtRequest)))
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
            var getFoldersRequest = new GetFoldersRequest(requestId, SelectedAccount.AccountName, ExtensionsList[SelectedExtension].Id);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetFoldersRequest).Where(request => ((GetFoldersRequest)(request.Value)).Equals(getFoldersRequest)))
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
            var reserveCallRequest = new CallReservationRequest(requestId, SelectedAccount.AccountName, SelectedAccount.PresentationNumber, PhoneNumber, number);
            foreach (var request in _waitingRequestArray.Where(response => response.Value is GetMessagesRequest).Where(request => ((GetMessagesRequest)(request.Value)).Equals(reserveCallRequest)))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "HELPER REQUEST: Duplicate call reservation request. Execute ID=" + request.Key);
#endif
                return request.Key;
            }
#if DEBUG
            Log.Debug(App.AppPackage, $"HELPER REQUEST: Call ID={requestId}");
            Log.Debug(App.AppPackage, $"Call from {reserveCallRequest.Account} (shows as {reserveCallRequest.PresentationNumber}) to {reserveCallRequest.DialingNumber} using SIM {reserveCallRequest.RealSimNumber}");
#endif
            PrepareIntent(requestId, reserveCallRequest);
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
            _app.StartService(intent);
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
                SelectedExtension = -1;
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(-1, new[] { ActionsHelperEventArgs.MsgUpdated }));
        }

        /// <summary>
        /// Mark outgoing call as finished
        /// </summary>
        public void MarkCallAsFinished()
        {
            if (RecentsDictionary.ContainsKey(_successDial))
                RecentsDictionary[_successDial].CallEnded();
        }

        /// <summary>
        /// ClearRecents
        /// </summary>
        public void ClearAllRecents()
        {
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(-1, new[] { ActionsHelperEventArgs.ClearRecents }));
        }

        /// <summary>
        /// Changing presentation number
        /// </summary>
        /// <param name="index">number index</param>
        public void SetPresentationNumber(int index)
        {
            SelectedAccount.SelectedPresentationNumber = index;
#if DEBUG
            Log.Debug(App.AppPackage, $"PRESENTATION NUMBER SET to {DataFormatUtils.ToPhoneNumber(SelectedAccount.PresentationNumber)}");
#endif
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(-1, new[] { ActionsHelperEventArgs.ChangePresentation }));
        }

        public void SaveNewNumber(string number)
        {
            PhoneNumber = number;
            _preferencesHelper.SavePhoneNumber(number);
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
#if DEBUG
            {

                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: NOT WAITED response for request with ID={response.RequestId}, Type is {type}");
                return;
            }
            Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId}, Type is {type}");
#else
                return;
#endif
            switch (type)
            {
                case "ErrorResponse":
                    var errorResponse = (ErrorResponse) response;
                    switch (errorResponse.ErrorCode)
                    {
                        // Connection lost
                        case ErrorResponse.ErrorConnection:
#if DEBUG
                            Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: CONNECTION LOST");
#endif
                            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.ConnectionLostError}));
                            break;
                        // Authorization failed
                        case ErrorResponse.ErrorUnauthorized:
                            if (IsLoggedIn)
                            {
#if DEBUG
                                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: UNAUTHORIZED");
#endif
                                DoLogout(response.RequestId);
                            }
                            else
                            {
#if DEBUG
                                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: BAD PASSWORD");
#endif
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new []{ActionsHelperEventArgs.AuthPasswdError}));
                            }
                            break;
                        //Bad request format
                        case ErrorResponse.ErrorBadRequest:
                            // Bad login format
                            if (!IsLoggedIn)
                            {
#if DEBUG
                                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: BAD LOGIN FORMAT");
#endif
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new []{ActionsHelperEventArgs.AuthLoginError, ActionsHelperEventArgs.RestoreError}));
                            }
                            // Call reservation bad request
                            else if (_waitingRequestArray.ContainsKey(response.RequestId) && _waitingRequestArray[response.RequestId] is CallReservationRequest)
                            {
#if DEBUG
                                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: BAD REQUEST");
#endif
                                SaveRecent(response, Recent.ResultFail);
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.CallReservationFail }));
                            }
                            break;
                        //Account not found error
                        case ErrorResponse.ErrorNotFound:
                            if (!IsLoggedIn)
                            {
#if DEBUG
                                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: BAD LOGIN");
#endif
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] {ActionsHelperEventArgs.RestoreWrongEmail }));
                            }
                            break;
                        //Number on hold (payment required) error
                        case ErrorResponse.ErrorNotPaid:
                            intent = AccountsList.Count > 1 ? new Intent(_app, typeof(InactiveActivityWithBack)) : new Intent(_app, typeof(InactiveActivity));
                            intent.PutExtra(InactiveActivity.InactiveAccontTag, SelectedAccount.AccountName);
                            SelectedAccount = null;
                            HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                            break;
                        case ErrorResponse.Forbidden:
                            // Call reservation bad destination phone
                            if (_waitingRequestArray.ContainsKey(response.RequestId) && _waitingRequestArray[response.RequestId] is CallReservationRequest)
                            {
#if DEBUG
                                Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: FORBIDDEN");
#endif
                                SaveRecent(response, Recent.ResultFail);
                                HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.CallReservationWrong }));
                            }
                            break;
                        case ErrorResponse.ErrorInternal:
                        case ErrorResponse.ErrorUnknown:
#if DEBUG
                            Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: INTERNAL SERVER ERROR");
#endif
                            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.InternalError }));
                            break;
                    }
                    break;

                // Login action response
                case "LoginResponse":
                    IsLoggedIn = true;
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed: YOU ARE LOGGED IN");
#endif
                    if ((IsFirstRun) || (PhoneNumber == null))
                    {
                        intent = new Intent(_app, typeof(SetNumberActivity));
                        HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));          
                    }
                    else
                        GetAccounts();
                    break;

                // Restore password response
                case "RestorePasswordResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} failed: BAD LOGIN");
#endif
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.RestoreOk }));
                    break;

                // Login action response
                case "LogoutResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed: YOU ARE LOGGED OUT");
#endif
                    DoLogout(response.RequestId);
                    break;

                // GetAccounts action response
                case "GetAccountsResponse":
                    var accsResponse = (GetAccountsResponse) response;
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: detect {accsResponse.AccountsList.Count} accounts");
#endif
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
                            intent = new Intent(_app, typeof(SelectAccountActivity));
                            HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                            break;
                    }
                    break;

                // Get presentation numbers response
                case "GetPresentationNumbersResponse":
                    var numbResponse = (GetPresentationNumbersResponse)response;
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: detect {numbResponse.NumbersList.Count} numbers");
#endif
                    switch (numbResponse.NumbersList.Count)
                    {
                        // No one active presentation numbers
                        case 0:
                            intent = new Intent(_app, typeof(InactiveActivityWithBack));
                            break;
                        // One or more presentation numbers
                        default:
                            SelectedAccount.PresentationNumbers = numbResponse.NumbersList;
                            if (IsFirstRun)
                                intent = new Intent(_app, typeof (DisclaimerActivity));
                            else
                            {
                                intent = new Intent(_app, typeof(ContentActivity));
                                intent.AddFlags(ActivityFlags.ClearTop);
                            }
                            ForceLoadExtensions();
                            break;
                    }
                    HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, intent));
                    break;

                // Get extensions response
                case "GetExtensionsResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed - YOU GET EXTENSIONS LIST");
#endif
                    var extResponse = (GetExtensionsResponse)response;
                    if (!ExtensionsList.Equals(extResponse.ExtensionsList))
                    {
                        ExtensionsList = extResponse.ExtensionsList;
                        SelectedExtension = -1;
                        SelectedFolder = -1;
                        SelectedMessage = -1;
                    }
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new []{ActionsHelperEventArgs.MsgUpdated}));
                    break;

                // Get folders response
                case "GetFoldersResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed - YOU GET FOLDERS LIST");
#endif
                    var foldersResponse = (GetFoldersResponse)response;
                    if (!ExtensionsList[SelectedExtension].Folders.Equals(foldersResponse.FoldersList))
                    {
                        ExtensionsList[SelectedExtension].Folders = foldersResponse.FoldersList;
                        SelectedFolder = -1;
                        SelectedMessage = -1;
                    }
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgUpdated }));
                    break;

                // Get messages response
                case "GetMessagesResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed - YOU GET MESSAGES LIST");
#endif
                    var msgResponse = (GetMessagesResponse)response;
                    if (!ExtensionsList[SelectedExtension].Folders[SelectedFolder].MessagesList.Equals(msgResponse.MessagesList))
                    {
                        ExtensionsList[SelectedExtension].Folders[SelectedFolder].MessagesList = msgResponse.MessagesList;
                        SelectedMessage = -1;
                    }
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgUpdated }));
                    break;

                // Call reservation response
                case "CallReservationResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed (Call reservation)");
#endif
                    var callResponse = (CallReservationResponse)response;
                    if (callResponse.ServiceNumber.Length > 6)
                    {
                        var callIntent = new Intent(Intent.ActionCall, Uri.Parse("tel:" + callResponse.ServiceNumber));
#if DEBUG
                        Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} CREATES CALL to {callResponse.ServiceNumber}");
#endif
                        SaveRecent(response, Recent.ResultOk);
                        _successDial = response.RequestId;
                        HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(response.RequestId, callIntent));
                    }
                    else
                    {
                        SaveRecent(response, Recent.ResultFail);
                        HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.CallReservationFail }));
                    }
                    break;

                // Move message to trash response
                case "RemoveMessageResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed - MESSAGE REMOVED");
#endif
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgMessagesUpdated }));
                    break;

                // Restore message from trash response
                case "RestoreMessageResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed - MESSAGE RESTORED");
#endif
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgMessagesUpdated }));
                    break;

                // Delete message from trash response
                case "DeleteMessageResponse":
#if DEBUG
                    Log.Debug(App.AppPackage, $"HELPER EXECUTOR: response for request with ID={response.RequestId} successed - MESSAGE DELETED");
#endif
                    HelperEvent?.Invoke(this, new ActionsHelperEventArgs(response.RequestId, new[] { ActionsHelperEventArgs.MsgMessagesUpdated }));
                    break;
            }
            _waitingRequestArray.Remove(response.RequestId);
        }

        /// <summary>
        /// Clear login info
        /// </summary>
        /// <param name="id">request ID</param>
        private void DoLogout(long id)
        {
            _userLogin = "";
            _userPassword = "";
            IsLoggedIn = false;
            AccountsList = null;
            SelectedAccount = null;
            SelectedExtension = -1;
            SelectedFolder = -1;
            SelectedMessage = -1;
            _successDial = -1;
            RecentsDictionary.Clear();
            var intent = new Intent(_app, typeof(AuthActivity));
            HelperEvent?.Invoke(this, new ActionsHelperIntentArgs(id, intent));
        }

        /// <summary>
        /// Recents saving
        /// </summary>
        /// <param name="response">response from server</param>
        /// <param name="result">success or failure</param>
        private void SaveRecent(BaseResponse response, int result)
        {
            var callReservation = (CallReservationRequest)_waitingRequestArray[response.RequestId];
            var recent = new Recent(callReservation.DialingNumber, result);
            var values = RecentsDictionary.Values.ToList();
            var keys = RecentsDictionary.Keys.ToList();
            long keyForRemove = -1;
            for (var i = 0; i < values.Count; i++)
            {
                if (!values[i].Equals(recent)) continue;
                keyForRemove = keys[i];
                break;
            }
            if (keyForRemove != -1)
                RecentsDictionary.Remove(keyForRemove);
            RecentsDictionary.Add(response.RequestId, recent);
        }
    }
}
