using System;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Entities.Enums;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;
using UserNotifications;

namespace FreedomVoice.iOS.PushNotifications
{
	
	/// <summary>
	/// Enum Describes possible errors during registration.
	/// </summary>
	enum PushRegistrationErrors
	{
		/// <summary>
		/// Registration is not completed or access is not denied
		/// </summary>
		accessNotGranted,
		
		/// <summary>
		/// Registration completed with a error
		/// </summary>
		registrationFailed
	}

	interface IPushNotificationsService
	{
		/// <summary>
		/// Method to start push notifications configuration
		/// </summary>
		/// <param name="options">Constants for requesting authorization to interact with the user.</param>
		/// <param name="registrationCompletionBlock">The method which will be invoked after registration</param>
		void RegisterForPushNotifications(UNAuthorizationOptions options, Action<PushRegistrationErrors?> registrationCompletionBlock);
		
		/// <summary>
		/// Method to start push notifications configuration
		/// </summary>
		/// <remarks>This method use the following options:: UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge </remarks>
		/// <param name="registrationCompletionBlock">The method which will be invoked after registration</param>
		void RegisterForPushNotifications(Action<PushRegistrationErrors?> registrationCompletionBlock);
		
		/// <summary>
		/// Will return current App notification settings: Granted, Not Allowed etc.
		/// </summary>
		/// <param name="completion"></param>
		void GetNotificationSettings(Action<UNNotificationSettings> completion);
		
		/// <summary>
		/// Method saves push-token and invokes registrationCompletionBlock
		/// </summary>
		/// <remarks>Must be called from AppDelegate, don't call this manually</remarks>
		/// <param name="deviceToken"></param>
		void DidRegisterForRemoteNotifications(NSData deviceToken);
		
		/// <summary>
		/// Method invokes registrationCompletionBlock with error
		/// </summary>
		/// <remarks>Must be called from AppDelegate, don't call this manually</remarks>
		/// <param name="deviceToken"></param>
		void DidFailToRegisterForRemoteNotifications(NSError error);

		/// <summary>
		/// Method registers push-token on the server.
		/// </summary>
		/// <returns></returns>
		Task RegisterPushNotificationToken();
		
		/// <summary>
		/// Method unregisters push-token on the server.
		/// </summary>
		/// <returns></returns>
		Task UnregisterPushNotificationToken();
	}
	
	class PushNotificationsService: IPushNotificationsService
	{
		private readonly UNUserNotificationCenter _notificationCenter;
		private Action<PushRegistrationErrors?> registrationCompletionBlock;

		private readonly IPushNotificationTokenDataStore _tokenDataStore;
		private readonly IPushService _pushService;

		public PushNotificationsService(UNUserNotificationCenter notificationCenter, IPushNotificationTokenDataStore tokenDataStore, IPushService pushService)
		{
			_tokenDataStore = tokenDataStore;
			_pushService = pushService;
			_notificationCenter = notificationCenter;
		}
		
		public PushNotificationsService()
		{
			_tokenDataStore = ServiceContainer.Resolve<IPushNotificationTokenDataStore>();
			_pushService = ServiceContainer.Resolve<IPushService>();
			_notificationCenter = UNUserNotificationCenter.Current;
		}
		
		/// <inheritdoc/>
		public void RegisterForPushNotifications(Action<PushRegistrationErrors?> registrationCompletionBlock)
		{
			RegisterForPushNotifications( UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge, registrationCompletionBlock);
		}
		
		/// <inheritdoc/>
		public void RegisterForPushNotifications(UNAuthorizationOptions options, Action<PushRegistrationErrors?> registrationCompletionBlock)
		{
			Console.WriteLine($"[{GetType()}] Registration for PushNotifications with options: {options}");

			_notificationCenter.RequestAuthorization(options, (granted, error) =>
			{
				Console.WriteLine($"[{GetType()}] RequestAuthorization finished: granted: {granted}, error: {error}");

				if (!granted)
				{
					registrationCompletionBlock?.Invoke(PushRegistrationErrors.accessNotGranted);
					return;
				}

				_notificationCenter.GetNotificationSettings((settings) =>
				{
					Console.WriteLine($"[{GetType()}] GetNotificationSettings finished: settings: {settings}");

					if (settings.AuthorizationStatus != UNAuthorizationStatus.Authorized)
					{
						registrationCompletionBlock?.Invoke(PushRegistrationErrors.accessNotGranted);
						return;
					}

					this.registrationCompletionBlock = registrationCompletionBlock;

					DispatchQueue.MainQueue.DispatchAsync(() =>
					{
						Console.WriteLine($"[{GetType()}] try to RegisterForRemoteNotifications");
						UIApplication.SharedApplication.RegisterForRemoteNotifications();
					});
				});
			});
		}

		/// <inheritdoc/>
		public void GetNotificationSettings(Action<UNNotificationSettings> completion)
		{
			_notificationCenter.GetNotificationSettings(settings => { completion?.Invoke(settings); });
		}

		/// <inheritdoc/>
		public void DidRegisterForRemoteNotifications(NSData deviceToken)
		{
			var token = deviceToken.Description.Trim('<').Trim('>').Replace(" ", "").ToUpper();
			_tokenDataStore.Save(token);
		
			Console.WriteLine($"[{GetType()}] Did register for RemoteNotifications: {token}");
			registrationCompletionBlock?.Invoke(null);
		}

		/// <inheritdoc/>
		public void DidFailToRegisterForRemoteNotifications(NSError error)
		{
			Console.WriteLine($"[{GetType()}] Did fail to register for RemoteNotifications: {error.LocalizedDescription}");
			registrationCompletionBlock?.Invoke(PushRegistrationErrors.registrationFailed);
		}

		/// <inheritdoc/>
		public async Task RegisterPushNotificationToken()
		{
			var savedToken = _tokenDataStore.Get();
			if (string.IsNullOrWhiteSpace(savedToken))
			{
				var errorText = $"[{GetType()}] Push notification token is null or empty";
				Console.WriteLine(errorText);
				throw new Exception(errorText);
			}

			try
			{
				
				var _ = await _pushService.Register(DeviceType.IOS, savedToken, iOS.Core.Utilities.Helpers.Contacts.NormalizePhoneNumber(UserDefault.AccountPhoneNumber));
				Console.WriteLine($"[{GetType()}] Token ({savedToken}) has been registrated");
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[{GetType()}] Registration token ({savedToken}) has been failed: {exception}");
				throw;
			}
		}

		/// <inheritdoc/>
		public async Task UnregisterPushNotificationToken()
		{
			var savedToken = _tokenDataStore.Get();

			try
			{
				var _ = await _pushService.Unregister(DeviceType.IOS, savedToken, UserDefault.AccountPhoneNumber);
                Console.WriteLine($"[{GetType()}] Token ({savedToken}) has been unregistrated");
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[{GetType()}] Registration token ({savedToken}) has been failed: {exception}");
				throw;
			}
		}
	}
}