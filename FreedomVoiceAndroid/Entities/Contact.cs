using System.Collections.Generic;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
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