namespace FreedomVoice.Core
{
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public static class ApiHelper
    {
        public static CookieContainer CookieContainer { get; set; }

        public static string Login(string login, string password)
        {
            CookieContainer = new CookieContainer();
            var postdata = string.Format("UserName={0}&Password={1}", login, password);
            var res = MakeAsyncPostRequest("/api/v1/login", postdata, "application/x-www-form-urlencoded").Result;
            return string.IsNullOrEmpty(res) ? "success" : res;
        }

        public static string GetSystems()
        {
            return MakeAsyncGetRequest("/api/v1/systems", "application/json").Result;
        }

        public static string GetMailboxes(string systemPhoneNumber)
        {
            return MakeAsyncGetRequest(string.Format("/api/v1/systems/{0}/mailboxes", systemPhoneNumber), "application/json").Result;
        }

        public static string GetMailboxesWithCounts(string systemPhoneNumber)
        {
            return MakeAsyncGetRequest(string.Format("/api/v1/systems/{0}/mailboxesWithCounts", systemPhoneNumber), "application/json").Result;
        }

        public static string GetFolders(string systemPhoneNumber, int mailboxNumber)
        {
            return MakeAsyncGetRequest(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders", systemPhoneNumber, mailboxNumber), "application/json").Result;
        }

        public static string GetMesages(string systemPhoneNumber, int mailboxNumber, string folderName, int pageSize, int pageNumber, bool asc)
        {
            return MakeAsyncGetRequest(string.Format("/api/v1/systems/{0}/mailboxes/{1}/folders/{2}/messages?PageSize={3}&PageNumber={4}&SortAsc={5}", systemPhoneNumber, mailboxNumber, folderName, pageSize, pageNumber, asc), "application/json").Result;
        }

        private static HttpWebRequest GetRequest(string url, string method, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(WebResources.AppUrl + url);
            request.ContentType = contentType;
            request.CookieContainer = CookieContainer;
            request.Method = method;
            return request;
        }

        private static async Task<string> MakeAsyncPostRequest(string url, string postData, string contentType)
        {
            var request = GetRequest(url, "POST", contentType);

            var requestStreamTask = await Task.Factory.FromAsync(
                request.BeginGetRequestStream,
                asyncResult => request.EndGetRequestStream(asyncResult),
                null);

            SetRequestStreamData(requestStreamTask, GetRequestBytes(postData));

            Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

            try
            {
                var response = await task;
                return ReadStreamFromResponse(response);
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse) ex.Response;
                return resp.StatusCode.ToString();
            }

        }

        private static async Task<string> MakeAsyncGetRequest(string url, string contentType)
        {
            var request = GetRequest(url, "GET", contentType);

            Task<WebResponse> task = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            try
            {
                var response = await task;
                return ReadStreamFromResponse(response);
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                return resp.StatusCode.ToString();
            }
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
