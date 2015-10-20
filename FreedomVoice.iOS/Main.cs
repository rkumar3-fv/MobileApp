using FreedomVoice.iOS.Data;
using UIKit;

namespace FreedomVoice.iOS
{
    public static class Application
    {
        static void Main(string[] args)
        {
            ServiceRegistrar.Startup();

            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}