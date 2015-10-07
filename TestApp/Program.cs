using System;
using FreedomVoice.Core;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.Write(@"Login method: " + ApiHelper.Login("freedomvoice.user1.267055@gmail.com", "user1654654").Result.Code);
            Console.Write(Environment.NewLine);

            Console.WriteLine(@"Systems method: ");
            var phoneNumbers = ApiHelper.GetSystems().Result.Result.PhoneNumbers;
            foreach (var phoneNumber in phoneNumbers)
                Console.WriteLine(phoneNumber);    
            Console.Write(Environment.NewLine);

            Console.Write(@"Mailboxes method: " + ApiHelper.GetMailboxes("7607124648"));
            Console.Write(Environment.NewLine);
            Console.Write(@"Mailboxes method: " + ApiHelper.GetMailboxesWithCounts("7607124648"));
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

            // var res = ApiHelper.GetMedia("7607124648", 802, "Sent", "I160057839", MediaType.Pdf);
            // using (MemoryStream ms = new MemoryStream())
            // {
            //     res.CopyTo(ms);
            //     var bytes = ms.ToArray();
            // 
            //     using (FileStream file = new FileStream(@"D:\file1.Pdf", FileMode.Create,FileAccess.Write))
            //     {
            //         ms.Read(bytes, 0, (int)ms.Length);
            //         file.Write(bytes, 0, bytes.Length);
            //         ms.Close();
            //     }
            // 
            // }
            // 
            // Console.Write(@"Media method: ");
            // Console.Write(Environment.NewLine);

            Console.ReadKey();
        }
    }
}
