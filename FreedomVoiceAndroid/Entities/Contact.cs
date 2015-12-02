using System.Collections.Generic;
using Android.Runtime;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Contact entity
    /// </summary>
    [Preserve(AllMembers = true)]
    public class Contact
    {
        /// <summary>
        /// Contact name
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Contact phones
        /// </summary>
        public List<Phone> PhonesList { get; }

        public Contact(string name, List<Phone> phonesList)
        {
            Name = name;
            PhonesList = phonesList;
        }

        public Contact(string name) : this(name, new List<Phone>())
        { }
    }
}