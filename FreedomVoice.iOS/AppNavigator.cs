using System;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS
{
	public interface IAppNavigator
	{
		BaseViewController CurrentController { get; }
		MainTabBarController MainTabBarController { get; }

		Action<BaseViewController> CurrentControllerChanged { get; set;  }
		Action<MainTabBarController> MainTabBarControllerChanged { get; set; }

		void Configure(AppDelegate app);

		void UpdateCurrentController(BaseViewController currentController);
		void UpdateMainTabBarController(MainTabBarController currentController);
	}
	
	public class AppNavigator: IAppNavigator
	{
		public BaseViewController CurrentController { get; private set; }
		public MainTabBarController MainTabBarController { get; private set; }
		
		public Action<BaseViewController> CurrentControllerChanged { get; set; }
		public Action<MainTabBarController> MainTabBarControllerChanged { get; set; }

		public AppNavigator()
		{
		}

		public void Configure(AppDelegate app)
		{
			app.Window = new UIWindow(UIScreen.MainScreen.Bounds);
		}
		

		public void UpdateCurrentController(BaseViewController currentController)
		{
			CurrentController = currentController;
			CurrentControllerChanged?.Invoke(CurrentController);
		}

		public void UpdateMainTabBarController(MainTabBarController currentController)
		{
			MainTabBarController = currentController;
			MainTabBarControllerChanged?.Invoke(MainTabBarController);
		}
	}
}