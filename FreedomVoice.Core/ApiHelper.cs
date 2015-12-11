using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Entities.Enums;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using FreedomVoice.Core.Cache;
using FreedomVoice.Core.Utils;
using ModernHttpClient;

namespace FreedomVoice.Core
{
    public static class ApiHelper
    {
        private const int TimeOut = 20;

        public static CookieContainer CookieContainer
        {
            get { return _clientHandler?.CookieContainer; }
            set
            {
                if (_clientHandler != null)
                    _clientHandler.CookieContainer = value;
            }
        }

        private static CacheStorageClient CacheStorage { get; set; }

        private static NativeMessageHandler _clientHandler;

        private static HttpClient Client { get; set; }

        static ApiHelper()
        {
            InitNewContext();
        }

        private static void InitNewContext()
        {
            _clientHandler = new NativeMessageHandler();
            if (_clientHandler.SupportsAutomaticDecompression)
                _clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            Client = new HttpClient(_clientHandler) {Timeout = new TimeSpan(0, 0, TimeOut)};
            CacheStorage = new CacheStorageClient(ServiceContainer.Resolve<IDeviceCacheStorage>());
        }

        public static async Task<BaseResult<string>> Login(string login, string password)
        {
            InitNewContext();
            var postdata = $"UserName={login}&Password={password}";
            return await MakeAsyncPostRequest<string>("/api/v1/login", postdata, "application/x-www-form-urlencoded", CancellationToken.None);
        }

        public static async Task<BaseResult<string>> Logout()
        {
            InitNewContext();
            await CacheStorage.DropCache();
            return await Task.FromResult(new BaseResult<string> { Code = ErrorCodes.Ok, Result = "" });
        }

        public static async Task<BaseResult<string>> PasswordReset(string login)
        {
            var postdata = $"UserName={login}";
            return await MakeAsyncPostRequest<string>("/api/v1/passwordReset", postdata, "application/x-www-form-urlencoded", CancellationToken.None);
        }

        public static async Task<BaseResult<PollingInterval>> GetPollingInterval()
        {
            return await MakeAsyncGetRequest<PollingInterval>("/api/v1/settings/pollingInterval", "application/json", CancellationToken.None);
        }

        public static async Task<BaseResult<DefaultPhoneNumbers>> GetSystems(bool noCache = false)
        {
            BaseResult<DefaultPhoneNumbers> data = null;
            if (!noCache)
                data = await CacheStorage.GetAccounts();

            if (data != null && data.Result.PhoneNumbers.Length != 0) return data;

            data = await MakeAsyncGetRequest<DefaultPhoneNumbers>("/api/v1/systems", "application/json", CancellationToken.None);
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

            data = await MakeAsyncGetRequest<PresentationPhoneNumbers>($"/api/v1/systems/{systemPhoneNumber}/presentationPhoneNumbers", "application/json", CancellationToken.None);
            if (data?.Result != null && data.Code == ErrorCodes.Ok)
                await CacheStorage.SavePresentationPhones(data.Result.PhoneNumbers);

            return data;
        }

        public static async Task<BaseResult<CreateCallReservationSetting>> CreateCallReservation(string systemPhoneNumber, string expectedCallerIdNumber, string presentationPhoneNumber, string destinationPhoneNumber)
        {
            var expectFormatted = expectedCallerIdNumber.Replace("+", "%2B").Replace("#", "%23").Replace("*", "%2A");
            var presentFormatted = presentationPhoneNumber.Replace("+", "%2B").Replace("#", "%23").Replace("*", "%2A");
            var destFormatted = destinationPhoneNumber.Replace("+", "%2B").Replace("#", "%23").Replace("*", "%2A");
            var postdata = $"ExpectedCallerIdNumber={expectFormatted}&PresentationPhoneNumber={presentFormatted}&DestinationPhoneNumber={destFormatted}";
            return await MakeAsyncPostRequest<CreateCallReservationSetting>(
                $"/api/v1/systems/{systemPhoneNumber}/createCallReservation",
                postdata,
                "application/x-www-form-urlencoded",
                CancellationToken.None);
        }

        public static async Task<BaseResult<List<Mailbox>>> GetMailboxes(string systemPhoneNumber)
        {
            return await MakeAsyncGetRequest<List<Mailbox>>($"/api/v1/systems/{systemPhoneNumber}/mailboxes","application/json",CancellationToken.None);
        }

        public static async Task<BaseResult<List<MailboxWithCount>>> GetMailboxesWithCounts(string systemPhoneNumber)
        {
            return await MakeAsyncGetRequest<List<MailboxWithCount>>($"/api/v1/systems/{systemPhoneNumber}/mailboxesWithCounts","application/json",CancellationToken.None);
        }

        public static async Task<BaseResult<List<Folder>>> GetFolders(string systemPhoneNumber, int mailboxNumber)
        {
            return await MakeAsyncGetRequest<List<Folder>>($"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/folders","application/json",CancellationToken.None);
        }

        public static async Task<BaseResult<List<MessageFolderWithCounts>>> GetFoldersWithCount(string systemPhoneNumber, int mailboxNumber)
        {
            return await MakeAsyncGetRequest<List<MessageFolderWithCounts>>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/foldersWithCounts","application/json",CancellationToken.None);
        }

        public static async Task<BaseResult<List<Message>>> GetMesages(string systemPhoneNumber, int mailboxNumber, string folderName, int pageSize, int pageNumber, bool asc)
        {
            return await MakeAsyncGetRequest<List<Message>>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/folders/{folderName}/messages?PageSize={pageSize}&PageNumber={pageNumber}&SortAsc={asc}",
                "application/json",
                CancellationToken.None);
        }

        public static async Task<BaseResult<string>> MoveMessages(string systemPhoneNumber, int mailboxNumber, string destinationFolder, List<string> messageIds)
        {
            var messagesStr = messageIds.Aggregate(string.Empty, (current, messageId) => current + ("&MessageIds=" + messageId));

            var postdata = $"DestinationFolderName={destinationFolder}{messagesStr}";

            return await MakeAsyncPostRequest<string>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/moveMessages",
                postdata,
                "application/x-www-form-urlencoded",
                CancellationToken.None);
        }

        public static async Task<BaseResult<string>> DeleteMessages(string systemPhoneNumber, int mailboxNumber, List<string> messageIds)
        {
            var postdata = messageIds.Aggregate(string.Empty, (current, messageId) => current + ("&MessageIds=" + messageId));

            return await MakeAsyncPostRequest<string>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/deleteMessages",
                postdata,
                "application/x-www-form-urlencoded",
                CancellationToken.None);
        }

        public static async Task<BaseResult<Stream>> GetMedia(string systemPhoneNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, CancellationToken token)
        {
            return await MakeAsyncFileDownload(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/folders/{folderName}/messages/{messageId}/media/{mediaType}",
                "application/json", messageId,
                token);
        }

        private static string MakeFullApiUrl(string url)
        {
            return WebResources.AppUrl + url;
        }

        private static async Task<BaseResult<T>> MakeAsyncPostRequest<T>(string url, string postData, string contentType, CancellationToken ct)
        {
            BaseResult<T> baseRes;
            try
            {
                var content = new StringContent(postData, Encoding.UTF8, contentType);
                content.Headers.Add("Keep-Alive", "true");
                var postResp = Client.PostAsync(MakeFullApiUrl(url), content, ct);
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

        public static async Task<BaseResult<T>> MakeAsyncGetRequest<T>(string url, string contentType, CancellationToken ct)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(MakeFullApiUrl(url)),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Connection", new [] { "Keep-Alive" });
            var getResp = Client.SendAsync(request, ct);
            return await GetResponse<T>(getResp, ct);
        }

        public static async Task<BaseResult<Stream>> MakeAsyncFileDownload(string url, string contentType, string messageId, CancellationToken ct)
        {
            BaseResult<Stream> retResult;
            try
            {
                if (ct.IsCancellationRequested)
                    return new BaseResult<Stream> { Code = ErrorCodes.Cancelled };

                var getResp = Client.GetAsync(MakeFullApiUrl(url), ct);

                var h = getResp.Result.Headers.FirstOrDefault(x => x.Key.Equals("Content-Length"));

                var streamResp = getResp.Result.Content.ReadAsStreamAsync();
                retResult = new BaseResult<Stream>
                {
                    Code = ErrorCodes.Ok,
                    Result = await streamResp
                };
            }
            catch (Exception ex)
            {
                return new BaseResult<Stream>
                {
                    Code = ErrorCodes.Unknown,
                    ErrorText = ex.Message
                };
            }

            return retResult;
        }

        public static async Task<BaseResult<MediaResponse>> MakeAsyncFileDownload(string url, string contentType, CancellationToken ct)
        {
            BaseResult<MediaResponse> retResult;

            try
            {
                if (ct.IsCancellationRequested)
                    return new BaseResult<MediaResponse> { Code = ErrorCodes.Cancelled };

                var streamResp = Client.GetStreamAsync(MakeFullApiUrl(url));

                var stream = await streamResp;
                retResult = new BaseResult<MediaResponse>
                {
                    Code = ErrorCodes.Ok,
                    Result = new MediaResponse(stream.Length, stream)
                };
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
                    try
                    {
                        if (ct.IsCancellationRequested)
                            return new BaseResult<T> { Code = ErrorCodes.Cancelled };

                        response.EnsureSuccessStatusCode();

                        string content = await response.Content.ReadAsStringAsync();
                        retResult = new BaseResult<T>
                        {
                            Code = ErrorCodes.Ok,
                            Result = JsonConvert.DeserializeObject<T>(content)
                        };
                    }
                    catch (HttpRequestException ex)
                    {
                        return HandleErrorState<T>(response.StatusCode, ex);
                    }
                }
            }
            catch (Exception ex2)
            {
                retResult = new BaseResult<T>
                {
                    Code = ErrorCodes.Unknown,
                    Result = default(T),
                    ErrorText = ex2.Message
                };
            }

            return retResult;
        }

        private static BaseResult<T> HandleErrorState<T>(HttpStatusCode code, Exception ex)
        {
            string msg = string.Empty;

            if (ex != null)
                msg = ex.Message;

            switch (code)
            {
                case HttpStatusCode.Unauthorized:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.Unauthorized,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
                case HttpStatusCode.Forbidden:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.Forbidden,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
                case HttpStatusCode.BadRequest:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.BadRequest,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
                case HttpStatusCode.RequestTimeout:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.RequestTimeout,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
                case HttpStatusCode.GatewayTimeout:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.GatewayTimeout,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
                case HttpStatusCode.NotFound:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.NotFound,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
                case HttpStatusCode.PaymentRequired:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.PaymentRequired,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
                case HttpStatusCode.InternalServerError:
                    {
                        return new BaseResult<T>
                        {
                            Code = ErrorCodes.InternalServerError,
                            Result = default(T),
                            ErrorText = msg
                        };
                    }
            }

            return new BaseResult<T>
            {
                Code = ErrorCodes.Unknown,
                Result = default(T),
                ErrorText = msg
            };
        }
    }
}