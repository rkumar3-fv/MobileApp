﻿using System;
using Foundation;
using FreedomVoice.Core.Utils;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
public class BaseViewController : UIViewController
    {
        /// <summary>
        /// Required constructor for Storyboard to work
        /// </summary>
        /// <param name='handle'>
        /// Handle to Obj-C instance of object
        /// </param>
        protected BaseViewController(IntPtr handle) : base(handle)
        {
            InternalInit();
        }

        protected BaseViewController()
        {
            InternalInit();
        }

        protected virtual void InternalInit()
        {
            

            if (!HandlesKeyboardNotifications) return;

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        protected virtual string PageName { get; } = string.Empty;

        protected virtual bool HandlesKeyboardNotifications => false;

        private void OnKeyboardNotification(NSNotification notification)
        {
            if (!IsViewLoaded)
                return;

            bool visible = notification.Name == UIKeyboard.WillShowNotification;

            UIView.BeginAnimations("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));

            var keyboardFrame = UIKeyboard.FrameBeginFromNotification(notification);
            OnKeyboardChanged(visible, keyboardFrame.Height);

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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            ServiceContainer.Resolve<IAppNavigator>().UpdateCurrentController(this);
        }
    }
}