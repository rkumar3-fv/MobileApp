using System;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Entities.Enums;
using FreedomVoice.iOS.Core;
using FreedomVoice.iOS.Utilities.Helpers;
using UserNotifications;
using PushKit;
using static FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.PushNotifications
{
	class PushNotificationsService : NSObject, IPushNotificationsService
	{
		private readonly UNUserNotificationCenter _notificationCenter;
		private Action<PushRegistrationErrors?> registrationCompletionBlock;

		private readonly ILogger _logger;
		private readonly IPushNotificationTokenDataStore _tokenDataStore;
		private readonly IPushService _pushService;
		private readonly NotificationCenterDelegate _voipPushNotificationsCenterDelegate;

		public PushNotificationsService(
			UNUserNotificationCenter notificationCenter,
			IPushNotificationTokenDataStore tokenDataStore,
			IPushService pushService,
			NotificationCenterDelegate voipPushNotificationsCenterDelegate,
			ILogger logger)
		{
			_tokenDataStore = tokenDataStore;
			_pushService = pushService;
			_notificationCenter = notificationCenter;
			_voipPushNotificationsCenterDelegate = voipPushNotificationsCenterDelegate;
			_logger = logger;
		}

		public PushNotificationsService(NotificationCenterDelegate notificationCenterDelegate)
		{
			_tokenDataStore = ServiceContainer.Resolve<IPushNotificationTokenDataStore>();
			_pushService = ServiceContainer.Resolve<IPushService>();
			_notificationCenter = UNUserNotificationCenter.Current;
			_voipPushNotificationsCenterDelegate = notificationCenterDelegate;
			_logger = ServiceContainer.Resolve<ILogger>();
			_voipPushNotificationsCenterDelegate.DidUpdatePushToken += DidRegisterForRemoteNotifications;
		}

		/// <inheritdoc/>
		public void RegisterForPushNotifications(Action<PushRegistrationErrors?> registrationCompletionBlock)
		{
			RegisterForPushNotifications(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge, registrationCompletionBlock);
		}

		/// <inheritdoc/>
		public void RegisterForPushNotifications(UNAuthorizationOptions options, Action<PushRegistrationErrors?> registrationCompletionBlock)
		{
			_logger.Debug(nameof(PushNotificationsService), nameof(RegisterForPushNotifications), $"Registration for PushNotifications with options: {options}");

			_notificationCenter.RequestAuthorization(options, (granted, error) =>
			{
				_logger.Debug(nameof(PushNotificationsService), nameof(RegisterForPushNotifications), $"RequestAuthorization finished: granted: {granted}, error: {error}");

				if (!granted)
				{
					registrationCompletionBlock?.Invoke(PushRegistrationErrors.accessNotGranted);
					return;
				}

				_notificationCenter.GetNotificationSettings((settings) =>
				{
					_logger.Debug(nameof(PushNotificationsService), nameof(RegisterForPushNotifications), $"GetNotificationSettings finished: settings: {settings}");

					if (settings.AuthorizationStatus != UNAuthorizationStatus.Authorized)
					{
						registrationCompletionBlock?.Invoke(PushRegistrationErrors.accessNotGranted);
						return;
					}

					this.registrationCompletionBlock = registrationCompletionBlock;

					DispatchQueue.MainQueue.DispatchAsync(() =>
					{
						_logger.Debug(nameof(PushNotificationsService), nameof(RegisterForPushNotifications), "try to RegisterForRemoteNotifications");

						//INFO: Registration for PushKit.
						var voipRegistry = new PKPushRegistry(DispatchQueue.MainQueue);
						voipRegistry.DesiredPushTypes = new NSSet(new NSObject[] {PKPushType.Voip});
						voipRegistry.Delegate = _voipPushNotificationsCenterDelegate;
						
						//INFO: Registration for regular notification. Disabled now.
						//UIApplication.SharedApplication.RegisterForRemoteNotifications();
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

			_logger.Debug(nameof(PushNotificationsService), nameof(DidRegisterForRemoteNotifications), $"Did register for RemoteNotifications: {token}");
			registrationCompletionBlock?.Invoke(null);
		}

		/// <inheritdoc/>
		public void DidFailToRegisterForRemoteNotifications(NSError error)
		{
			_logger.Debug(nameof(PushNotificationsService), nameof(DidFailToRegisterForRemoteNotifications), $"Did fail to register for RemoteNotifications: {error.LocalizedDescription}");
			registrationCompletionBlock?.Invoke(PushRegistrationErrors.registrationFailed);
		}

		/// <inheritdoc/>
		public async Task RegisterPushNotificationToken()
		{
			var savedToken = _tokenDataStore.Get();
			if (string.IsNullOrWhiteSpace(savedToken))
			{
				_logger.Debug(nameof(PushNotificationsService), nameof(RegisterPushNotificationToken), "Push notification token is null or empty");
				return;
			}

			if (string.IsNullOrWhiteSpace(UserDefault.AccountPhoneNumber))
			{
				_logger.Debug(nameof(PushNotificationsService), nameof(RegisterPushNotificationToken), "AccountPhoneNumber is null or empty");
				return;
			}

			try
			{
				//INFO: Regular push notification. Disabled. DeviceType.iOS - for regular, IOSPushKit - for PushKit
				await _pushService.Register(DeviceType.IOSPushKit, savedToken, NormalizePhoneNumber(UserDefault.AccountPhoneNumber));
				_logger.Debug(nameof(PushNotificationsService), nameof(RegisterPushNotificationToken), $"Token ({savedToken}) has been registered");
			}
			catch (Exception exception)
			{
				_logger.Debug(nameof(PushNotificationsService), nameof(RegisterPushNotificationToken), $"Registration token ({savedToken}) has been failed: {exception}");
				throw;
			}
		}

		/// <inheritdoc/>
		public async Task UnregisterPushNotificationToken()
		{
			var savedToken = _tokenDataStore.Get();

			try
			{
				//INFO: Regular push notification. Disabled. DeviceType.iOS - for regular, IOSPushKit - for PushKit
				await _pushService.Unregister(DeviceType.IOSPushKit, savedToken, UserDefault.AccountPhoneNumber);
				_logger.Debug(nameof(PushNotificationsService), nameof(UnregisterPushNotificationToken), $"Token ({savedToken}) has been unregistered");
			}
			catch (Exception exception)
			{
				_logger.Debug(nameof(PushNotificationsService), nameof(UnregisterPushNotificationToken), $"Unregistration token ({savedToken}) has been failed: {exception}");
				throw;
			}
		}
	}
}