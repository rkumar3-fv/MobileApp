using System;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    public class BaseViewController : UIViewController
    {
        /// <summary>
        /// Constructor for use when controller is not in a storyboard
        /// </summary>
        public BaseViewController() { }

        /// <summary>
        /// Required constructor for Storyboard to work
        /// </summary>
        /// <param name='handle'>
        /// Handle to Obj-C instance of object
        /// </param>
        public BaseViewController(IntPtr handle) : base(handle)
        {
            //Only do this if required
            if (HandlesKeyboardNotifications)
            {
                NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
                NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
            }
        }

        public virtual bool HandlesKeyboardNotifications
        {
            get { return false; }
        }

        /// <summary>
        /// This is how orientation is setup on iOS 6
        /// </summary>
        public override bool ShouldAutorotate()
        {
            return true;
        }

        /// <summary>
        /// This is how orientation is setup on iOS 6
        /// </summary>
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.All;
        }

        void OnKeyboardNotification(NSNotification notification)
        {
            if (!IsViewLoaded)
                return;

            //Check if the keyboard is becoming visible
            bool visible = notification.Name == UIKeyboard.WillShowNotification;

            //Start an animation, using values from the keyboard
            UIView.BeginAnimations("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));

            //Pass the notification, calculating keyboard height, etc.
            bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
            if (visible)
            {
                var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
                OnKeyboardChanged(visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);
            }
            else
            {
                var keyboardFrame = UIKeyboard.FrameBeginFromNotification(notification);
                OnKeyboardChanged(visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);
            }

            //Commit the animation
            UIView.CommitAnimations();
        }

        /// <summary>
        /// Override this method to apply custom logic when the keyboard is shown/hidden
        /// </summary>
        /// <param name='visible'>
        /// If the keyboard is visible
        /// </param>
        /// <param name='height'>
        /// Calculated height of the keyboard (width not generally needed here)
        /// </param>
        protected virtual void OnKeyboardChanged(bool visible, nfloat height) { }
    }
}