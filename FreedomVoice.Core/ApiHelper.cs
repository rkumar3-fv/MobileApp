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

        public async static Task<BaseResult<string>> Login(string login, string password)
        {
            var cts = new CancellationTokenSource();

            CookieContainer = new CookieContainer();
            var postdata = $"UserName={login}&Password={password}";
            return await MakeAsyncPostRequest<string>("/api/v1/login", postdata, "application/x-www-form-urlencoded", cts.Token);
        }

        public async static Task<BaseResult<string>> PasswordReset(string login)
        {
            var cts = new CancellationTokenSource();

            CookieContainer = new CookieContainer();
            var postdata = $"UserName={login}";
            return await MakeAsyncPostRequest<string>("/api/v1/passwordReset", postdata, "application/x-www-form-urlencoded", cts.Token);
        }

        public async static Task<BaseResult<DefaultPhoneNumbers>> GetSystems()
        {
            var cts = new CancellationTokenSource();

            return await MakeAsyncGetRequest<DefaultPhoneNumbers>("/api/v1/systems", "application/json", cts.Token);
        }

        public static BaseResult<PresentationPhoneNumbers> GetPresentationPhoneNumbers(string systemPhoneNumber)
        {
            var cts = new CancellationTokenSource();

            return MakeAsyncGetRequest<PresentationPhoneNumbers>(string.Format("api/v1/systems/{0}/presentationPhoneNumbers", systemPhoneNumber), "application/json", cts.Token).Result;
        }

        public static BaseResult<List<Mailbox>> GetMailboxes(string systemPhoneNumber)
        {
            var cts = new CancellationTokenSource();

            return MakeAsyncGetRequest<List<Mailbox>>(string.Format("/api/v1/systems/{0}/mailboxes", systemPhoneNumber), "application/json", cts.Token).Result;
        }

        public static BaseResult<List<MailboxWithCount>> GetMailboxesWithCounts(string systemPhoneNumber)
        {
            var cts = new CancellationTokenSource();

            return MakeAsyncGetRequest<List<MailboxWithCount>>(string.Format("/api/v1/systems/{0}/mailboxesWithCounts", systemPhoneNumber), "application/json", cts.Token).Result;
        }

        public static BaseResult<List<Folder>> GetFolders(string systemPhoneNumber, int mailboxNumber)
        {
            var cts = new CancellationTokenSource();

            return MakeAsyncGetRequest<List<Folder>>(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders", systemPhoneNumber, mailboxNumber), "application/json", cts.Token).Result;
        }

        public static BaseResult<List<Message>> GetMesages(string systemPhoneNumber, int mailboxNumber, string folderName, int pageSize, int pageNumber, bool asc)
        {
            var cts = new CancellationTokenSource();
            var res = MakeAsyncGetRequest<List<Message>>(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders/{2}/messages?PageSize={3}&PageNumber={4}&SortAsc={5}", systemPhoneNumber, mailboxNumber, folderName, pageSize, pageNumber, asc), "application/json", cts.Token).Result;
            return res;
        }

        public static BaseResult<string> MoveMessages(string systemPhoneNumber, int mailboxNumber, string destinationFolder, List<string> messageIds)
        {
            var cts = new CancellationTokenSource();
            var messagesStr = messageIds.Aggregate(string.Empty, (current, messageId) => current + ("&MessageIds=" + messageId));

            var postdata = string.Format("DestinationFolderName={0}{1}", destinationFolder, messagesStr);
            var res = MakeAsyncPostRequest<string>(string.Format("/api/v1/systems/{0}/mailboxes/{1}/moveMessages", systemPhoneNumber, mailboxNumber), postdata, "application/x-www-form-urlencoded", cts.Token).Result;
            return res;

        }

        public static BaseResult<string> DeleteMessages(string systemPhoneNumber, int mailboxNumber, List<string> messageIds)
        {
            var cts = new CancellationTokenSource();
            var postdata = messageIds.Aggregate(string.Empty, (current, messageId) => current + ("&MessageIds=" + messageId));

            var res = MakeAsyncPostRequest<string>(string.Format("/api/v1/systems/{0}/mailboxes/{1}/deleteMessages", systemPhoneNumber, mailboxNumber), postdata, "application/x-www-form-urlencoded", cts.Token).Result;
            return res;

        }

        public static Stream GetMedia(string systemPhoneNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType)
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            return MakeAsyncFileDownload(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders/{2}/messages/{3}/media/{4}", systemPhoneNumber, mailboxNumber, folderName, messageId, mediaType), "application/json", cts.Token).Result.Result;
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

        private static async Task<BaseResult<T>> MakeAsyncGetRequest<T>(string url, string contentType, CancellationToken cts)
        {
            var request = GetRequest(url, "GET", contentType);

            return await GetResponce<T>(request, cts);
        }


        private static async Task<BaseResult<Stream>> MakeAsyncFileDownload(string url, string contentType, CancellationToken ct)
        {
            var request = GetRequest(url, "GET", contentType);
            BaseResult<Stream> retResult = null;
            using (ct.Register(() => request.Abort(), false))
            {

                Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse,
                    request.EndGetResponse, null);

                try
                {
                    var response = await task;
                    ct.ThrowIfCancellationRequested();
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
                                        Result = Stream.Null
                                    };
                                    break;
                                }

                            case HttpStatusCode.BadRequest:
                                {
                                    retResult = new BaseResult<Stream>
                                    {
                                        Code = ErrorCodes.BadRequest,
                                        Result = Stream.Null
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
                            Result = Stream.Null
                        };
                    }
                }
            }

            return retResult;
        }

        private static async Task<BaseResult<T>> GetResponce<T>(HttpWebRequest request, CancellationToken ct)
        {
            BaseResult<T> retResult = null;

            using (ct.Register(() => request.Abort(), false))
            {
                Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

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

                            case HttpStatusCode.BadRequest:
                                {
                                    retResult = new BaseResult<T>
                                    {
                                        Code = ErrorCodes.BadRequest,
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
            }

            return retResult;
        }

        private static void SetRequestStreamData(Stream response, byte[] postData)
        {
            response.Write(postData, 0, postData.Length);
        }

        private static byte[] GetRequestBytes(string postData)
        {
            if (string.IsNullOrEmpty(postData))
                return new byte[0];

            return Encoding.UTF8.GetBytes(postData);
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }
    }
}
