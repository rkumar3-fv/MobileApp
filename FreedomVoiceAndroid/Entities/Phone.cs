namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Phone entity
    /// </summary>
    public class Phone
    {
        /// <summary>
        /// Normalized phone number
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Phone type code
        /// <see href="http://developer.android.com/intl/ru/reference/android/provider/ContactsContract.CommonDataKinds.Phone.html#TYPE_HOME">ContactsContract.CommonDataKinds.Phone</see>
        /// </summary>
        public int TypeCode { get; }

        public Phone(string phoneNumber, int typeCode)
        {
            PhoneNumber = phoneNumber;
            TypeCode = typeCode;
        }
    }
}