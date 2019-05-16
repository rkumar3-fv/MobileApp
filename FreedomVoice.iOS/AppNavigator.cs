using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS
{
	public interface IAppNavigator
	{
		BaseViewController CurrentController { get; }
		void Configure(AppDelegate app);

		void UpdateCurrentController(BaseViewController currentController);

	}
	
	public class AppNavigator: IAppNavigator
	{
		public BaseViewController CurrentController { get; private set; }

		public AppNavigator()
		{
		}

		public void Configure(AppDelegate app)
		{
			app.Window = new UIWindow(UIScreen.MainScreen.Bounds);

			app.Window.BackgroundColor = UIColor.White;
			app.Window.MakeKeyAndVisible();
		}

		public void UpdateCurrentController(BaseViewController currentController)
		{
			CurrentController = currentController;
		}
	}
}