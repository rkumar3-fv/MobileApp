using Android.Content;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// Main application helper
    /// </summary>
    public class AppHelper
    {
        private static volatile AppHelper _instance;
        private static readonly object Locker = new object();
        private readonly Context _context;

        private AppHelper(Context context)
        {
            _context = context;
            ActionsHelper = new ActionsHelper(App.GetApplication(context));
            AttachmentsHelper = new AttachmentsHelper(context);
        }

        /// <summary>
        /// Get application helper instance
        /// </summary>
        public static AppHelper Instance(Context context)
        {
            if (_instance != null) return _instance;
            lock (Locker)
            {
                if (_instance == null)
                    _instance = new AppHelper(context);
            }
            return _instance;
        }

        /// <summary>
        /// App network actions helper
        /// </summary>
        public ActionsHelper ActionsHelper { get; }

        /// <summary>
        /// App attachments helper
        /// </summary>
        public AttachmentsHelper AttachmentsHelper { get; }
    }
}