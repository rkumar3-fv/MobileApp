namespace TestApp
{
    using System;
    using FreedomVoice.Core;

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

            Console.Write(@"Messages method: " + ApiHelper.GetMesages("7607124648", 802, "Sent", 1, 1, true));
            Console.ReadKey();
        }
    }
}
