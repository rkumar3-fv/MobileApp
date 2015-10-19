using System;
using FreedomVoice.Core;
using System.IO;
using System.Threading;
using FreedomVoice.Core.Entities.Enums;

namespace TestApp
{
    class Program
    {
        private static string _login;
        private static string _passwd;

        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                _login = args[0];
                _passwd = args[1];
            }
            else
            {
                _login = "freedomvoice.user1.267055@gmail.com";
                _passwd = "user1654654";
            }

            // Console.Write(@"Restore method: " + ApiHelper.PasswordReset("a055@gmail.com").Result);
            // Console.Write(Environment.NewLine);

            Console.Write(@"Login method: " + ApiHelper.Login(_login, _passwd).Result.Code);
            Console.Write(Environment.NewLine);


            Console.Write(@"Polling Interval: " + ApiHelper.GetPollingInterval().Result.Result.PollingIntervalSeconds);
            Console.Write(Environment.NewLine);


            Console.Write(@"Logout method: " + ApiHelper.Logout().Result.Code);
            Console.Write(Environment.NewLine);

            Console.Write(@"Login method: " + ApiHelper.Login(_login, _passwd).Result.Code);
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Systems method: ");
            var phoneNumbers = ApiHelper.GetSystems().Result.Result.PhoneNumbers;
            foreach (var phoneNumber in phoneNumbers)
                Console.WriteLine(phoneNumber);
            Console.Write(Environment.NewLine);

            Console.Write(@"Mailboxes method: " + ApiHelper.GetMailboxes("7607124648"));
            Console.Write(Environment.NewLine);


            var a = ApiHelper.GetPresentationPhoneNumbers("7607124648").Result;


            var c = ApiHelper.CreateCallReservation("7607124648", "7607124641", "7606468294", "7607124648").Result;



            var resMb = ApiHelper.GetMailboxesWithCounts("7607124648").Result;
            Console.WriteLine(@"Mailboxes method with count: " + resMb.Result.Count);
            foreach (var mailboxWithCount in resMb.Result)
            {
                Console.WriteLine($"{mailboxWithCount.DisplayName} (x{mailboxWithCount.MailboxNumber}) - {mailboxWithCount.UnreadMessages}");
            }
            Console.Write(Environment.NewLine);

            Console.Write(@"Folders method: " + ApiHelper.GetFolders("7607124648", 802));
            Console.Write(Environment.NewLine);

            Console.Write(@"Messages method: " + ApiHelper.GetMesages("7607124648", 802, "Sent", 10, 1, true));
            Console.Write(Environment.NewLine);


            // Console.Write(@"Messages method: " + ApiHelper.MoveMessages("7607124648", 802, "New", new List<string> { "I159985458", "I159987614" }));
            // Console.Write(Environment.NewLine);
            //
            // Console.Write(@"Messages method: " + ApiHelper.DeleteMessages("7607124648", 802, new List<string> { "I159985458", "I159987614" }));
            // Console.Write(Environment.NewLine);

            Console.WriteLine(@"Media method: ");
            if (File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file.pdf"}"))
                File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file.pdf"}");
            var res = ApiHelper.GetMedia("7607124648", 802, "Sent", "I160057839", MediaType.Pdf, CancellationToken.None).Result;
             using (var ms = new MemoryStream())
             {
                 res.Result.CopyTo(ms);
                 var bytes = ms.ToArray();
             
                 using (var file = new FileStream(
                     $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file.pdf"}", FileMode.Create,FileAccess.Write))
                 {
                     ms.Read(bytes, 0, (int)ms.Length);
                     file.Write(bytes, 0, bytes.Length);
                     ms.Close();
                 }
             
             }
            var files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "*.pdf");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
            Console.Write(Environment.NewLine);

            Console.ReadKey();
        }
    }
}
