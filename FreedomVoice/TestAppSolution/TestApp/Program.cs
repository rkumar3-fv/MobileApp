namespace TestApp
{
    using System;
    using System.IO;
    using Aspose.Pdf.Generator;
    using FreedomVoice.Core;
    using MediaType = FreedomVoice.Core.Entities.Enums.MediaType;

    class Program
    {
        static void Main(string[] args)
        {

            Console.Write(@"Login method: " + ApiHelper.Login("freedomvoice.user1.267055@gmail.com", "user1654654").Result);
            Console.Write(Environment.NewLine);

            Console.Write(@"Systems method: " + string.Join(",", ApiHelper.GetSystems().Result.PhoneNumbers));
            Console.Write(Environment.NewLine);
            Console.Write(@"Mailboxes method: " + ApiHelper.GetMailboxes("7607124648"));
            Console.Write(Environment.NewLine);
            Console.Write(@"Mailboxes method: " + ApiHelper.GetMailboxesWithCounts("7607124648"));
            Console.Write(Environment.NewLine);

            Console.Write(@"Folders method: " + ApiHelper.GetFolders("7607124648", 802));
            Console.Write(Environment.NewLine);

            Console.Write(@"Messages method: " + ApiHelper.GetMesages("7607124648", 802, "Sent", 10, 1, true));
            Console.Write(Environment.NewLine);


            var str = MediaType.Mp3.ToString();

            var res = ApiHelper.GetMedia("7607124648", 802, "Sent", "I160057839", MediaType.Mp3);
            using (MemoryStream ms = new MemoryStream())
            {
                res.CopyTo(ms);
                var bytes = ms.ToArray();

                using (FileStream file = new FileStream(@"D:\file1.Pdf", FileMode.Create,FileAccess.Write))
                {
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    ms.Close();
                }

            }



            // Initialize the bytes array with the stream length and then fill it with data
            // Use write method to write to the file specified above



            Console.Write(@"Media method: ");
            Console.Write(Environment.NewLine);

            Console.ReadKey();
        }
    }
}
