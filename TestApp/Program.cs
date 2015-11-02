using System;
using System.Collections.Generic;
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
                _login = "freedomvoice.adm.267055@gmail.com";
                _passwd = "adm654654";
            }

            var resRestore = ApiHelper.PasswordReset("oops@gmail.com").Result;
            Console.WriteLine(@"Restore method not registered email: " + (resRestore?.Code.ToString() ?? "null"));
            Console.Write(Environment.NewLine);

            resRestore = ApiHelper.PasswordReset("oops").Result;
            Console.WriteLine(@"Restore method wrong email format: " + (resRestore?.Code.ToString() ?? "null"));
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Restore method valid: " + ApiHelper.PasswordReset("freedomvoice.user2.267055@gmail.com").Result.Code);
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Login method not regisered email: " + ApiHelper.Login("oops@gmail.com", _passwd).Result.Code);
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Login method wrong format: " + ApiHelper.Login("oops", _passwd).Result.Code);
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Login method: " + ApiHelper.Login(_login, _passwd).Result.Code);
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Logout method: " + ApiHelper.Logout().Result.Code);
            Console.Write(Environment.NewLine);

            var systemsUn = ApiHelper.GetSystems().Result;
            Console.WriteLine($"Systems method unauthorized: {systemsUn.Code}\n");

            var reserv = ApiHelper.CreateCallReservation("7607124648", "+18005551212", "7607124648", "8005556767").Result;
            Console.WriteLine($"CallReservation method unauthorized: +18005551212 -> 8005556767 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result == null) ? ("NULL") : (reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Login method: " + ApiHelper.Login(_login, _passwd).Result.Code);
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Login method another one: " + ApiHelper.Login(_login, _passwd).Result.Code);
            Console.Write(Environment.NewLine);

            var timeRestore = ApiHelper.GetPollingInterval().Result;
            Console.WriteLine(@"Polling interval request: " + (timeRestore?.Code.ToString() ?? "null"));
            Console.WriteLine($"Polling interval is : {(timeRestore?.Result.PollingIntervalSeconds.ToString() ?? "empty")}");
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Systems method: ");
            var syst = ApiHelper.GetSystems().Result;
            foreach (var phoneNumber in syst.Result.PhoneNumbers)
                Console.WriteLine(phoneNumber);
            Console.Write(Environment.NewLine);

            var phoneNumbers = ApiHelper.GetPresentationPhoneNumbers("7607124648").Result;
            Console.WriteLine($"Presentation phones method: {phoneNumbers.Code}");
            foreach (var phoneNumber in phoneNumbers.Result.PhoneNumbers)
                Console.WriteLine(phoneNumber);
            Console.Write(Environment.NewLine);

            var mb = ApiHelper.GetMailboxes("4153730879").Result;
            Console.WriteLine(@"Mailboxes method with count for #4153730879: " + mb.Code);
            foreach (var mailbox in mb.Result)
            {
                Console.WriteLine($"{mailbox.DisplayName} (x{mailbox.MailboxNumber})");
            }
            Console.Write(Environment.NewLine);

            var resMb = ApiHelper.GetMailboxesWithCounts("7607124648").Result;
            Console.WriteLine(@"Mailboxes method with count for #7607124648: " + resMb.Code);
            foreach (var mailboxWithCount in resMb.Result)
            {
                Console.WriteLine($"{mailboxWithCount.DisplayName} (x{mailboxWithCount.MailboxNumber}) - {mailboxWithCount.UnreadMessages}");
            }
            Console.Write(Environment.NewLine);

            var resFolders = ApiHelper.GetFolders("7607124648", 802).Result;
            Console.WriteLine($"Folders method: {resFolders.Code}");
            foreach (var folder in resFolders.Result)
            {
                Console.WriteLine($"{folder.Name} - {folder.MessageCount}");
            }
            Console.Write(Environment.NewLine);

            resFolders = ApiHelper.GetFolders("7607124648", 803).Result;
            Console.WriteLine($"Folders method: {resFolders.Code}");
            foreach (var folder in resFolders.Result)
            {
                Console.WriteLine($"{folder.Name} - {folder.MessageCount}");
            }
            Console.Write(Environment.NewLine);

            var resMsg = ApiHelper.GetMesages("7607124648", 802, "Sent", 10, 1, true).Result;
            Console.WriteLine($"Messages method: {resMsg.Code}");
            foreach (var msg in resMsg.Result)
            {
                Console.WriteLine($"Message {msg.Name} ({msg.Id}) from {msg.SourceName} ({msg.SourceNumber}) is "+((msg.Unread)?"new":"old"));
            }
            Console.Write(Environment.NewLine);

            resMsg = ApiHelper.GetMesages("7607124648", 80, "New", 10, 1, true).Result;
            Console.WriteLine($"Get Messages method Account=7607124648; x80 - New: {resMsg.Code}");
            foreach (var msg in resMsg.Result)
            {
                Console.WriteLine($"Message {msg.Name} ({msg.Id}) from {msg.SourceName} ({msg.SourceNumber}) is " + ((msg.Unread) ? "new" : "old"));
            }
            Console.Write(Environment.NewLine);

            
            if (File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file80.pdf"}"))
                File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file80.pdf"}");
            var resFile = ApiHelper.GetMedia("7607124648", 80, "New", "I161578702", MediaType.Pdf, CancellationToken.None).Result;
            Console.WriteLine($"Get Media method: {resFile.Code}");
            using (var ms = new MemoryStream())
            {
                resFile.Result.CopyTo(ms);
                var bytes = ms.ToArray();

                using (var file = new FileStream(
                    $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file80.pdf"}", FileMode.Create, FileAccess.Write))
                {
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    ms.Close();
                }
                Console.WriteLine(@"Content from Account=7607124648; x80 - New; I161223944 received: file80.pdf");
            }
            Console.Write(Environment.NewLine);

            resMsg = ApiHelper.GetMesages("7607124648", 80, "New", 10, 1, true).Result;
            Console.WriteLine($"Get Messages method Account=7607124648; x80 - New: {resMsg.Code}");
            foreach (var msg in resMsg.Result)
            {
                Console.WriteLine($"Message {msg.Name} ({msg.Id}) from {msg.SourceName} ({msg.SourceNumber}) is " + ((msg.Unread) ? "new" : "old"));
            }
            Console.Write(Environment.NewLine);

            reserv = ApiHelper.CreateCallReservation("7607124648", "+18005551212", "7607124648", "8005556767").Result;
            Console.WriteLine($"CallReservation method: +18005551212 -> 8005556767 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result == null) ? ("NULL") : (reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            reserv = ApiHelper.CreateCallReservation("7607124648", "8005551212", "7607124648", "8005556767").Result;
            Console.WriteLine($"CallReservation method: 8005551212 -> 8005556767 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result == null) ? ("NULL") : (reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            reserv = ApiHelper.CreateCallReservation("7607124648", "+18005551212", "7607124648", "+18005556767").Result;
            Console.WriteLine($"CallReservation method: +18005551212 -> +18005556767 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result==null)?("NULL"):(reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            reserv = ApiHelper.CreateCallReservation("7607124648", "18005551212", "7607124648", "18005556767").Result;
            Console.WriteLine($"CallReservation method: 18005551212 -> 18005556767 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result == null) ? ("NULL") : (reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            reserv = ApiHelper.CreateCallReservation("7607124648", "+79213092113", "7607124648", "8005556767").Result;
            Console.WriteLine($"CallReservation method: +79213092113 -> 8005556767 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result == null) ? ("NULL") : (reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            reserv = ApiHelper.CreateCallReservation("7607124648", "79119998877", "7607124648", "8005556767").Result;
            Console.WriteLine($"CallReservation method: 79119998877 -> 8005556767 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result == null) ? ("NULL") : (reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            reserv = ApiHelper.CreateCallReservation("7607124648", "8034445566", "7607124648", "+79213092113").Result;
            Console.WriteLine($"CallReservation method: 8034445566 -> +79213092113 - {reserv.Code}");
            Console.WriteLine($"Result: {((reserv.Result == null) ? ("NULL") : (reserv.Result.SwitchboardPhoneNumber))}");
            Console.Write(Environment.NewLine);

            var folders = ApiHelper.GetFoldersWithCount("7607124648", 802).Result;
            Console.Write($"GetFoldersWithCount method: {folders.Code}");
            Console.Write(Environment.NewLine);

            Console.Write(@"Messages move method Saved -> New: " + ApiHelper.MoveMessages("7607124648", 802, "New", new List<string> { "A159991316" }).Result.Code);
            Console.Write(Environment.NewLine);
            Console.Write(@"Messages move method New -> Saved: " + ApiHelper.MoveMessages("7607124648", 802, "Saved", new List<string> { "A159991316" }).Result.Code);
            Console.Write(Environment.NewLine);

            Console.Write(@"Messages move method Saved -> Trash: " + ApiHelper.MoveMessages("7607124648", 802, "Trash", new List<string> { "I160057801" }).Result.Code);
            Console.Write(Environment.NewLine);
            Console.Write(@"Messages move method Trash -> Saved: " + ApiHelper.MoveMessages("7607124648", 802, "Saved", new List<string> { "I160057801" }).Result.Code);
            Console.Write(Environment.NewLine);

            // WARNING! Delete message without restoration
            //Console.Write(@"Messages delete method: " + ApiHelper.DeleteMessages("7607124648", 802, new List<string> { "I160057839" }));
            Console.Write(Environment.NewLine);

            
            if (File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file.pdf"}"))
                File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{"file.pdf"}");
            var res = ApiHelper.GetMedia("7607124648", 80, "New", "I161625181", MediaType.Pdf, CancellationToken.None).Result;
            Console.WriteLine($"Media method: {res.Code}");
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
