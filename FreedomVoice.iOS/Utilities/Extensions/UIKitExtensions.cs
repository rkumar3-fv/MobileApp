using System;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.Utilities.Extensions
{
    public static class UIKitExtensions
    {
        /// <summary>
		/// Sets a callback for registering text changed notifications on a UITextField
		/// </summary>
		public static void SetDidChangeNotification(this UITextField textField, Action<UITextField> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            NSNotificationCenter.DefaultCenter.AddObserver(UITextField.TextFieldTextDidChangeNotification, _ => callback(textField), textField);
        }

        /// <summary>
        /// Sets a callback for registering text changed notifications on a UITextField
        /// </summary>
        public static void SetDidChangeNotification(this UITextView textView, Action<UITextView> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidChangeNotification, _ => callback(textView), textView);
        }
    }
}