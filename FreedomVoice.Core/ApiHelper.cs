﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core.Cache;
using FreedomVoice.Core.Cookies;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using ModernHttpClient;
using Newtonsoft.Json;

namespace FreedomVoice.Core
{
    public static class ApiHelper
    {
        private const int TimeOut = 20;
        private const int LongTimeOut = 60;

        public static CookieContainer CookieContainer => _clientHandler.CookieContainer;

        private static NativeMessageHandler _clientHandler;

        private static HttpClient Client { get; set; }

        private static CacheStorageClient CacheStorage { get; set; }
        private static CookieStorageClient CookieStorage { get; set; }

        private static bool Native { get; set; }

        static ApiHelper()
        {
            InitNewContext();
        }

        public static void InitNewContext()
        {
            Client = CreateClient();
        }

        public static async Task<BaseResult<string>> Login(string login, string password)
        {
            var loginEncoded = DataFormatUtils.UrlEncodeWithSpaces(login);
            var passEncoded = DataFormatUtils.UrlEncodeWithSpaces(password);
            var postdata = $"UserName={loginEncoded}&Password={passEncoded}";

            var loginResponse = await MakeAsyncPostRequest<string>("/api/v1/login", postdata, "application/x-www-form-urlencoded", CancellationToken.None, LongTimeOut);
            if (Native) return loginResponse;

            if (loginResponse != null && loginResponse.Code == ErrorCodes.Ok)
                CookieStorage.SaveCookieContainer(CookieContainer);

            return loginResponse;
        }

        public static async Task<BaseResult<string>> Logout()
        {
            CookieStorage.ClearCookieContainer();

            await CacheStorage.DropCache();

            return await Task.FromResult(new BaseResult<string> { Code = ErrorCodes.Ok, Result = "" });
        }

        public static async Task<BaseResult<string>> PasswordReset(string login)
        {
            var loginEncoded = DataFormatUtils.UrlEncodeWithSpaces(login);
            var postdata = $"UserName={loginEncoded}";
            return await MakeAsyncPostRequest<string>("/api/v1/passwordReset", postdata, "application/x-www-form-urlencoded", CancellationToken.None);
        }

        public static async Task<BaseResult<PollingInterval>> GetPollingInterval()
        {
            return await MakeAsyncGetRequest<PollingInterval>("/api/v1/settings/pollingInterval", CancellationToken.None);
        }

        public static async Task<BaseResult<DefaultPhoneNumbers>> GetSystems(bool noCache = false)
        {
            BaseResult<DefaultPhoneNumbers> data = null;
            if (!noCache)
                data = await CacheStorage.GetAccounts();

            if (data != null && data.Result.PhoneNumbers.Length != 0) return data;

            data = await MakeAsyncGetRequest<DefaultPhoneNumbers>("/api/v1/systems", CancellationToken.None);
            if (data?.Result != null && data.Code == ErrorCodes.Ok)
                await CacheStorage.SaveAccounts(data.Result.PhoneNumbers);

            return data;
        }

        public static async Task<BaseResult<PresentationPhoneNumbers>> GetPresentationPhoneNumbers(string systemPhoneNumber, bool noCache = false)
        {
            BaseResult<PresentationPhoneNumbers> data = null;
            if (!noCache)
                data = await CacheStorage.GetPresentationPhones();

            if (data != null && data.Result.PhoneNumbers.Length != 0) return data;

            data = await MakeAsyncGetRequest<PresentationPhoneNumbers>($"/api/v1/systems/{systemPhoneNumber}/presentationPhoneNumbers", CancellationToken.None);
            if (data?.Result != null && data.Code == ErrorCodes.Ok)
                await CacheStorage.SavePresentationPhones(data.Result.PhoneNumbers);

            return data;
        }

        public static async Task<BaseResult<CreateCallReservationSetting>> CreateCallReservation(string systemPhoneNumber, string expectedCallerIdNumber, string presentationPhoneNumber, string destinationPhoneNumber)
        {
            var expectFormatted = WebUtility.UrlEncode(expectedCallerIdNumber);
            var presentFormatted = WebUtility.UrlEncode(presentationPhoneNumber);
            var destFormatted = WebUtility.UrlEncode(destinationPhoneNumber);
            var postdata = $"ExpectedCallerIdNumber={expectFormatted}&PresentationPhoneNumber={presentFormatted}&DestinationPhoneNumber={destFormatted}";
            return await MakeAsyncPostRequest<CreateCallReservationSetting>(
                $"/api/v1/systems/{systemPhoneNumber}/createCallReservation",
                postdata,
                "application/x-www-form-urlencoded",
                CancellationToken.None);
        }

        public static async Task<BaseResult<List<Mailbox>>> GetMailboxes(string systemPhoneNumber)
        {
            return await MakeAsyncGetRequest<List<Mailbox>>($"/api/v1/systems/{systemPhoneNumber}/mailboxes", CancellationToken.None);
        }

        public static async Task<BaseResult<List<MailboxWithCount>>> GetMailboxesWithCounts(string systemPhoneNumber)
        {
            return await MakeAsyncGetRequest<List<MailboxWithCount>>($"/api/v1/systems/{systemPhoneNumber}/mailboxesWithCounts", CancellationToken.None);
        }

        public static async Task<BaseResult<List<Folder>>> GetFolders(string systemPhoneNumber, int mailboxNumber)
        {
            return await MakeAsyncGetRequest<List<Folder>>($"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/folders", CancellationToken.None);
        }

        public static async Task<BaseResult<List<MessageFolderWithCounts>>> GetFoldersWithCount(string systemPhoneNumber, int mailboxNumber)
        {
            return await MakeAsyncGetRequest<List<MessageFolderWithCounts>>($"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/foldersWithCounts", CancellationToken.None);
        }

        public static async Task<BaseResult<List<Message>>> GetMesages(string systemPhoneNumber, int mailboxNumber, string folderName, int pageSize, int pageNumber, bool asc)
        {
            var folder = DataFormatUtils.UrlEncodeWithSpaces(folderName);
            return await MakeAsyncGetRequest<List<Message>>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/folders/{folder}/messages?PageSize={pageSize}&PageNumber={pageNumber}&SortAsc={asc}",
                CancellationToken.None, LongTimeOut);
        }

        public static async Task<BaseResult<string>> MoveMessages(string systemPhoneNumber, int mailboxNumber, string destinationFolder, IEnumerable<string> messageIds)
        {
            var folder = DataFormatUtils.UrlEncodeWithSpaces(destinationFolder);
            var messagesStr = messageIds.Aggregate(string.Empty, (current, messageId) => current + ("&MessageIds=" + messageId));

            var postdata = $"DestinationFolderName={folder}{messagesStr}";

            return await MakeAsyncPostRequest<string>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/moveMessages",
                postdata,
                "application/x-www-form-urlencoded",
                CancellationToken.None);
        }

        public static async Task<BaseResult<string>> DeleteMessages(string systemPhoneNumber, int mailboxNumber, IEnumerable<string> messageIds)
        {
            var postdata = messageIds.Aggregate(string.Empty, (current, messageId) => current + ("&MessageIds=" + messageId));

            return await MakeAsyncPostRequest<string>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/deleteMessages",
                postdata,
                "application/x-www-form-urlencoded",
                CancellationToken.None);
        }

        public static async Task<BaseResult<MediaResponse>> GetMedia(string systemPhoneNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, CancellationToken token)
        {
            var folder = DataFormatUtils.UrlEncodeWithSpaces(folderName);
            return await MakeAsyncFileDownload($"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/folders/{folder}/messages/{messageId}/media/{mediaType}", token);
        }
        
        public static async Task<BaseResult<List<FreedomVoice.Entities.Response.Conversation>>> GetConversations(string phone, DateTime startDate, DateTime lastUpdateDate, int start, int limit)
        {
            //api/v1/system/[controller]/conversations/{telephoneNumber}/{lastModify}/{start}/{limit}
            return await MakeAsyncGetRequest<List<FreedomVoice.Entities.Response.Conversation>>(
                $"/api/v1/systems/forward/{phone}/conversations?startDate={startDate}&lastModify={lastUpdateDate.Ticks}&start={start}&limit={limit}",
                CancellationToken.None, LongTimeOut);
        }

        private static HttpClient CreateClient()
        {
            CookieStorage = new CookieStorageClient(ServiceContainer.Resolve<IDeviceCookieStorage>());
            CacheStorage = new CacheStorageClient(ServiceContainer.Resolve<IDeviceCacheStorage>());

            try
            {
                _clientHandler = ServiceContainer.Resolve<IHttpClientHelper>().MessageHandler;
                Native = true;
            }
            catch (KeyNotFoundException)
            {
                _clientHandler = new NativeMessageHandler { CookieContainer = CookieStorage.GetCookieContainer() };
                Native = false;
            }

            if (_clientHandler.SupportsAutomaticDecompression)
                _clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            var httpClient = new HttpClient(_clientHandler) { BaseAddress = new Uri(WebResources.AppUrl) };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        private static async Task<BaseResult<T>> MakeAsyncPostRequest<T>(string url, string postData, string contentType, CancellationToken ct, int timeout = TimeOut)
        {
            Client.Timeout = TimeSpan.FromSeconds(timeout);

            BaseResult<T> baseRes;
            try
            {
                var content = new StringContent(postData, Encoding.UTF8, contentType);
                var postResp = Client.PostAsync(url, content, ct);
                baseRes = await GetResponse<T>(postResp, ct);
            }
            catch (WebException)
            {
                baseRes = new BaseResult<T>
                {
                    Code = ErrorCodes.ConnectionLost,
                    Result = default(T)
                };
            }

            return baseRes;
        }

        private static async Task<BaseResult<T>> MakeAsyncGetRequest<T>(string url, CancellationToken ct, int timeout = TimeOut)
        {
            Client.Timeout = TimeSpan.FromSeconds(timeout);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var getResp = Client.SendAsync(request, ct);
            return await GetResponse<T>(getResp, ct);
        }

        public static async Task<BaseResult<MediaResponse>> MakeAsyncFileDownload(string url, CancellationToken ct)
        {
            BaseResult<MediaResponse> retResult;

            try
            {
                var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                try
                {
                    if (ct.IsCancellationRequested)
                        return new BaseResult<MediaResponse> { Code = ErrorCodes.Cancelled };

                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();
                    retResult = new BaseResult<MediaResponse>
                    {
                        Code = ErrorCodes.Ok,
                        Result = new MediaResponse(response.Content.Headers.ContentLength ?? stream.Length, stream)
                    };
                }
                catch (HttpRequestException ex)
                {
                    return HandleErrorState<MediaResponse>(response.StatusCode, ex);
                }
            }
            catch (OperationCanceledException)
            {
                return new BaseResult<MediaResponse> { Code = ErrorCodes.Cancelled };
            }
            catch (Exception ex)
            {
                return new BaseResult<MediaResponse>
                {
                    Code = ErrorCodes.Unknown,
                    ErrorText = ex.Message
                };
            }

            return retResult;
        }

        private static async Task<BaseResult<T>> GetResponse<T>(Task<HttpResponseMessage> r, CancellationToken ct)
        {
            BaseResult<T> retResult;

            try
            {
                using (var response = await r)
                {
                    string content = null;
                    try
                    {
                        if (ct.IsCancellationRequested)
                            return new BaseResult<T> { Code = ErrorCodes.Cancelled };
                        content = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();         
                        retResult = new BaseResult<T>
                        {
                            Code = ErrorCodes.Ok,
                            Result = JsonConvert.DeserializeObject<T>(content),
                            HttpCode = (int)response.StatusCode
                        };
                    }
                    catch (HttpRequestException ex)
                    {
                        return content != null ? HandleErrorState<T>(response.StatusCode, ex, content) : HandleErrorState<T>(response.StatusCode, ex);
                    }
                }
            }
            catch (Exception ex2)
            {
                retResult = new BaseResult<T>
                {
                    Code = ErrorCodes.Unknown,
                    Result = default(T),
                    ErrorText = ex2.Message,
                    HttpCode = -1, 
                    JsonText = ex2.Message
                };
            }

            return retResult;
        }

        private static BaseResult<T> HandleErrorState<T>(HttpStatusCode code, Exception ex)
        {
            return HandleErrorState<T>(code, ex, "JSON is NULL");
        }

        private static BaseResult<T> HandleErrorState<T>(HttpStatusCode code, Exception ex, string json)
        {
            var baseResult = new BaseResult<T>
            {
                Result = default(T),
                ErrorText = ex != null ? ex.Message : string.Empty,
                HttpCode = (int) code,
                JsonText = json
            };
            switch (code)
            {
                case HttpStatusCode.Unauthorized:
                    baseResult.Code = ErrorCodes.Unauthorized;
                    break;
                case HttpStatusCode.Forbidden:
                    baseResult.Code = ErrorCodes.Forbidden;
                    break;
                case HttpStatusCode.BadRequest:
                    baseResult.Code = ErrorCodes.BadRequest;
                    break;
                case HttpStatusCode.RequestTimeout:
                    baseResult.Code = ErrorCodes.RequestTimeout;
                    break;
                case HttpStatusCode.GatewayTimeout:
                    baseResult.Code = ErrorCodes.GatewayTimeout;
                    break;
                case HttpStatusCode.NotFound:
                    baseResult.Code = ErrorCodes.NotFound;
                    break;
                case HttpStatusCode.PaymentRequired:
                    baseResult.Code = ErrorCodes.PaymentRequired;
                    break;
                case HttpStatusCode.InternalServerError:
                    baseResult.Code = ErrorCodes.InternalServerError;
                    break;
                default:
                    baseResult.Code = ErrorCodes.Unknown;
                    break;
            }
            return baseResult;
        }
    }
}