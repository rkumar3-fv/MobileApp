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

namespace FreedomVoice.Core
{
    public static class ApiHelper
    {
        public static CookieContainer CookieContainer { get; set; }

        public static async Task<BaseResult<string>> Login(string login, string password)
        {
            CookieContainer = new CookieContainer();

            var postdata = $"UserName={login}&Password={password}";
            return await MakeAsyncPostRequest<string>("/api/v1/login", postdata, "application/x-www-form-urlencoded", CancellationToken.None);
        }

        public static async Task<BaseResult<string>> Logout()
        {
            CookieContainer = null;
            var task = Task.Run(() => new BaseResult<string> { Code = ErrorCodes.Ok, Result = "" });
            return await task;
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

        public static async Task<BaseResult<DefaultPhoneNumbers>> GetSystems()
        {
            return await MakeAsyncGetRequest<DefaultPhoneNumbers>("/api/v1/systems", "application/json", CancellationToken.None);
        }

        public static async Task<BaseResult<PresentationPhoneNumbers>> GetPresentationPhoneNumbers(string systemPhoneNumber)
        {
            return await MakeAsyncGetRequest<PresentationPhoneNumbers>(
                $"/api/v1/systems/{systemPhoneNumber}/presentationPhoneNumbers",
                "application/json",
                CancellationToken.None);
        }

        public static async Task<BaseResult<CreateCallReservationSetting>> CreateCallReservation(string systemPhoneNumber, string expectedCallerIdNumber, string presentationPhoneNumber, string destinationPhoneNumber)
        {
            var postdata = $"ExpectedCallerIdNumber={expectedCallerIdNumber.Replace("+", "%2B")}&PresentationPhoneNumber={presentationPhoneNumber}&DestinationPhoneNumber={destinationPhoneNumber.Replace("+", "%2B")}";
            return await MakeAsyncPostRequest<CreateCallReservationSetting>(
                $"/api/v1/systems/{systemPhoneNumber}/createCallReservation",
                postdata,
                "application/x-www-form-urlencoded",
                CancellationToken.None);
        }

        public static async Task<BaseResult<List<Mailbox>>> GetMailboxes(string systemPhoneNumber)
        {
            return await MakeAsyncGetRequest<List<Mailbox>>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes",
                "application/json",
                CancellationToken.None);
        }

        public static async Task<BaseResult<List<MailboxWithCount>>> GetMailboxesWithCounts(string systemPhoneNumber)
        {
            return await MakeAsyncGetRequest<List<MailboxWithCount>>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxesWithCounts",
                "application/json",
                CancellationToken.None);
        }

        public static async Task<BaseResult<List<Folder>>> GetFolders(string systemPhoneNumber, int mailboxNumber)
        {
            return await MakeAsyncGetRequest<List<Folder>>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/folders",
                "application/json",
                CancellationToken.None);
        }

        public static async Task<BaseResult<List<MessageFolderWithCounts>>> GetFoldersWithCount(string systemPhoneNumber, int mailboxNumber)
        {
            return await MakeAsyncGetRequest<List<MessageFolderWithCounts>>(
                $"/api/v1/systems/{systemPhoneNumber}/mailboxes/{mailboxNumber}/foldersWithCounts",
                "application/json",
                CancellationToken.None);
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

        private static HttpWebRequest GetRequest(string url, string method, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(WebResources.AppUrl + url);
            request.ContentType = contentType;
            request.CookieContainer = CookieContainer;
            request.Method = method;
            return request;
        }

        private static async Task<BaseResult<T>> MakeAsyncPostRequest<T>(string url, string postData, string contentType, CancellationToken cts)
        {
            var request = GetRequest(url, "POST", contentType);

            var res = await Task.Factory.FromAsync(
                request.BeginGetRequestStream, async asyncResult =>
                {
                    BaseResult<T> baseRes;
                    try
                    {
                        using (var resultStream = request.EndGetRequestStream(asyncResult))
                        {
                            SetRequestStreamData(resultStream, GetRequestBytes(postData));
                            baseRes = await GetResponce<T>(request, cts);
                        }
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
                },
                null);

            return await res;
        }

        public static async Task<BaseResult<T>> MakeAsyncGetRequest<T>(string url, string contentType, CancellationToken cts)
        {
            var request = GetRequest(url, "GET", contentType);

            return await GetResponce<T>(request, cts);
        }

        public static async Task<BaseResult<Stream>> MakeAsyncFileDownload(string url, string contentType, string messageId, CancellationToken ct)
        {
            var request = GetRequest(url, "GET", contentType);
            BaseResult<Stream> retResult = null;
            var task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            try
            {
                var response = await task;
                retResult = new BaseResult<Stream>
                {
                    Code = ErrorCodes.Ok,
                    Result = response.GetResponseStream()
                };
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                if (resp != null)
                {
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            {
                                retResult = new BaseResult<Stream>
                                {
                                    Code = ErrorCodes.Unauthorized,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.Forbidden:
                            {
                                retResult = new BaseResult<Stream>
                                {
                                    Code = ErrorCodes.Forbidden,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.BadRequest:
                            {
                                retResult = new BaseResult<Stream>
                                {
                                    Code = ErrorCodes.BadRequest,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.NotFound:
                            {
                                retResult = new BaseResult<Stream>
                                {
                                    Code = ErrorCodes.NotFound,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.PaymentRequired:
                            {
                                retResult = new BaseResult<Stream>
                                {
                                    Code = ErrorCodes.PaymentRequired,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.InternalServerError:
                            {
                                retResult = new BaseResult<Stream>
                                {
                                    Code = ErrorCodes.InternalServerError,
                                    Result = null
                                };
                                break;
                            }
                    }
                }

                if (ct.IsCancellationRequested)
                {
                    retResult = new BaseResult<Stream>
                    {
                        Code = ErrorCodes.Cancelled,
                        Result = null
                    };
                }
            }

            return retResult;
        }

        public static async Task<BaseResult<MediaResponse>> MakeAsyncFileDownload(string url, string contentType, CancellationToken ct)
        {
            var request = GetRequest(url, "GET", contentType);
            BaseResult<MediaResponse> retResult = null;
            var task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            try
            {
                var response = await task;
                retResult = new BaseResult<MediaResponse>
                {
                    Code = ErrorCodes.Ok,
                    Result = new MediaResponse(response.ContentLength, response.GetResponseStream())
                };
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                if (resp != null)
                {
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            {
                                retResult = new BaseResult<MediaResponse>
                                {
                                    Code = ErrorCodes.Unauthorized,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.Forbidden:
                            {
                                retResult = new BaseResult<MediaResponse>
                                {
                                    Code = ErrorCodes.Forbidden,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.BadRequest:
                            {
                                retResult = new BaseResult<MediaResponse>
                                {
                                    Code = ErrorCodes.BadRequest,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.NotFound:
                            {
                                retResult = new BaseResult<MediaResponse>
                                {
                                    Code = ErrorCodes.NotFound,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.PaymentRequired:
                            {
                                retResult = new BaseResult<MediaResponse>
                                {
                                    Code = ErrorCodes.PaymentRequired,
                                    Result = null
                                };
                                break;
                            }
                        case HttpStatusCode.InternalServerError:
                            {
                                retResult = new BaseResult<MediaResponse>
                                {
                                    Code = ErrorCodes.InternalServerError,
                                    Result = null
                                };
                                break;
                            }
                    }
                }

                if (ct.IsCancellationRequested)
                {
                    retResult = new BaseResult<MediaResponse>
                    {
                        Code = ErrorCodes.Cancelled,
                        Result = null
                    };
                }
            }
            return retResult;
        }

        private static async Task<BaseResult<T>> GetResponce<T>(HttpWebRequest request, CancellationToken ct)
        {
            BaseResult<T> retResult = null;

            var task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

            try
            {
                var response = await task;
                ct.ThrowIfCancellationRequested();
                retResult = new BaseResult<T>
                {
                    Code = ErrorCodes.Ok,
                    Result = JsonConvert.DeserializeObject<T>(ReadStreamFromResponse(response))
                };
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                if (resp != null)
                {
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            {
                                retResult = new BaseResult<T>
                                {
                                    Code = ErrorCodes.Unauthorized,
                                    Result = default(T)
                                };
                                break;
                            }
                        case HttpStatusCode.Forbidden:
                            {
                                retResult = new BaseResult<T>
                                {
                                    Code = ErrorCodes.Forbidden,
                                    Result = default(T)
                                };
                                break;
                            }
                        case HttpStatusCode.BadRequest:
                            {
                                retResult = new BaseResult<T>
                                {
                                    Code = ErrorCodes.BadRequest,
                                    Result = default(T)
                                };
                                break;
                            }
                        case HttpStatusCode.NotFound:
                            {
                                retResult = new BaseResult<T>
                                {
                                    Code = ErrorCodes.NotFound,
                                    Result = default(T)
                                };
                                break;
                            }
                        case HttpStatusCode.PaymentRequired:
                            {
                                retResult = new BaseResult<T>
                                {
                                    Code = ErrorCodes.PaymentRequired,
                                    Result = default(T)
                                };
                                break;
                            }
                        case HttpStatusCode.InternalServerError:
                            {
                                retResult = new BaseResult<T>
                                {
                                    Code = ErrorCodes.InternalServerError,
                                    Result = default(T)
                                };
                                break;
                            }
                    }
                }

                if (ct.IsCancellationRequested)
                {
                    retResult = new BaseResult<T>
                    {
                        Code = ErrorCodes.Cancelled,
                        Result = default(T)
                    };
                }
            }

            return retResult;
        }

        private static void SetRequestStreamData(Stream response, byte[] postData)
        {
            response.Write(postData, 0, postData.Length);
        }

        private static byte[] GetRequestBytes(string postData)
        {
            return string.IsNullOrEmpty(postData) ? new byte[0] : Encoding.UTF8.GetBytes(postData);
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (var responseStream = response.GetResponseStream())
            using (var sr = new StreamReader(responseStream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}