namespace FreedomVoice.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Entities;
    using Entities.Base;
    using Entities.Enums;
    using Newtonsoft.Json;

    public static class ApiHelper
    {
        public static CookieContainer CookieContainer { get; set; }

        public static BaseResult<string> Login(string login, string password)
        {
            CookieContainer = new CookieContainer();
            var postdata = string.Format("UserName={0}&Password={1}", login, password);
            return MakeAsyncPostRequest<string>("/api/v1/login", postdata, "application/x-www-form-urlencoded").Result;
        }

        public static BaseResult<DefaultPhoneNumbers> GetSystems()
        {
            return MakeAsyncGetRequest<DefaultPhoneNumbers>("/api/v1/systems", "application/json").Result;
        }

        public static BaseResult<List<Mailbox>> GetMailboxes(string systemPhoneNumber)
        {
            return MakeAsyncGetRequest<List<Mailbox>>(string.Format("/api/v1/systems/{0}/mailboxes", systemPhoneNumber), "application/json").Result;
        }

        public static BaseResult<List<MailboxWithCount>> GetMailboxesWithCounts(string systemPhoneNumber)
        {
            return MakeAsyncGetRequest<List<MailboxWithCount>>(string.Format("/api/v1/systems/{0}/mailboxesWithCounts", systemPhoneNumber), "application/json").Result;
        }

        public static BaseResult<List<Folder>> GetFolders(string systemPhoneNumber, int mailboxNumber)
        {
            return MakeAsyncGetRequest<List<Folder>>(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders", systemPhoneNumber, mailboxNumber), "application/json").Result;
        }

        public static BaseResult<List<Message>> GetMesages(string systemPhoneNumber, int mailboxNumber, string folderName, int pageSize, int pageNumber, bool asc)
        {
            return MakeAsyncGetRequest<List<Message>>(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders/{2}/messages?PageSize={3}&PageNumber={4}&SortAsc={5}", systemPhoneNumber, mailboxNumber, folderName, pageSize, pageNumber, asc), "application/json").Result;
        }

        public static Stream GetMedia(string systemPhoneNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType)
        {
            return MakeAsyncFileDownload(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders/{2}/messages/{3}/media/{4}", systemPhoneNumber, mailboxNumber, folderName, messageId, "Pdf"), "application/json").Result;
        }

        private static HttpWebRequest GetRequest(string url, string method, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(WebResources.AppUrl + url);
            request.ContentType = contentType;
            request.CookieContainer = CookieContainer;
            request.Method = method;
            return request;
        }

        private static async Task<BaseResult<T>> MakeAsyncPostRequest<T>(string url, string postData, string contentType)
        {
            var request = GetRequest(url, "POST", contentType);

            var requestStreamTask = await Task.Factory.FromAsync(
                request.BeginGetRequestStream,
                asyncResult => request.EndGetRequestStream(asyncResult),
                null);

            SetRequestStreamData(requestStreamTask, GetRequestBytes(postData));

            return await GetResponce<T>(request);

        }

        private static async Task<BaseResult<T>> MakeAsyncGetRequest<T>(string url, string contentType)
        {
            var request = GetRequest(url, "GET", contentType);

            return await GetResponce<T>(request);
        }


        private static async Task<Stream> MakeAsyncFileDownload(string url, string contentType)
        {
            var request = GetRequest(url, "GET", contentType);

            Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

            var response = await task;

            return response.GetResponseStream();
        }

        private static async Task<BaseResult<T>> GetResponce<T>(HttpWebRequest request)
        {
            Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

            BaseResult<T> retResult = null;
            try
            {
                var response = await task;
                retResult = new BaseResult<T>
                {
                    Code = HttpStatusCode.OK,
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
                                    Code = HttpStatusCode.Unauthorized,
                                    Result = default(T)
                                };
                                break;
                            }

                        case HttpStatusCode.BadRequest:
                            {
                                retResult = new BaseResult<T>
                                {
                                    Code = HttpStatusCode.BadRequest,
                                    Result = default(T)
                                };
                                break;
                            }
                    }
                }
                else
                {
                    throw ex;
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
