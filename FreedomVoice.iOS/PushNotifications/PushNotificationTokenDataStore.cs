using System;
using Foundation;

namespace FreedomVoice.iOS.PushNotifications
{
	interface IPushNotificationTokenDataStore
	{
		/// <summary>
		/// Method saves push-token to the internal storage
		/// </summary>
		/// <param name="token">push-token</param>
		void Save(string token);
		
		/// <summary>
		/// Method removes push-token from the internal storage
		/// </summary>
		void Clear();
		
		/// <summary>
		/// Method retrieves push-token from the internal storage
		/// </summary>
		/// <returns>Push-token. May be nil or empty</returns>
		string Get();
	}
	
	class PushNotificationTokenDataStore : IPushNotificationTokenDataStore
	{
		private readonly NSUserDefaults _userDefaultsStore;
		private const string _tokenKey = "PushNotificationTokenDataStore_TokenKey";

		public PushNotificationTokenDataStore(NSUserDefaults userDefaultsStore)
		{
			_userDefaultsStore = userDefaultsStore;
		}
		
		public PushNotificationTokenDataStore()
		{
			_userDefaultsStore = NSUserDefaults.StandardUserDefaults;
		}
		
		/// <inheritdoc/>
		public void Save(string token)
		{ 
			_userDefaultsStore.SetString(token, _tokenKey);
			Console.WriteLine($"[{GetType()}] Token has been saved: {token}");
		}

		/// <inheritdoc/>
		public void Clear()
		{
			_userDefaultsStore.RemoveObject(_tokenKey);
			Console.WriteLine($"[{GetType()}] Token has been cleared");
		}

		/// <inheritdoc/>
		public string Get()
		{
			return _userDefaultsStore.StringForKey(_tokenKey);
		}
	}
}