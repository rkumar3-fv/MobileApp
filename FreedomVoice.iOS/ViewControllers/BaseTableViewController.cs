using System;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    public class BaseTableViewController : UITableViewController
    {
        /// <summary>
        /// Constructor for use when controller is not in a storyboard
        /// </summary>
        public BaseTableViewController() { }

        /// <summary>
        /// Required constructor for Storyboard to work
        /// </summary>
        /// <param name='handle'>
        /// Handle to Obj-C instance of object
        /// </param>
        public BaseTableViewController(IntPtr handle) : base(handle) { 
        }
    }
}