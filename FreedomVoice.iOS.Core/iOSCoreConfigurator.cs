using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.Core.Utilities.Helpers;

namespace FreedomVoice.iOS.Core
{
	public class iOSCoreConfigurator
	{
		public static void RegisterServices()
		{
			ServiceContainer.Register<IContactNameProvider>(() => new ContactNameProvider());
            ServiceContainer.Register<ILogger>(() => new Logger());
        }
    }
}