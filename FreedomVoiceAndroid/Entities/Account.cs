using System;
using System.Collections.Generic;
using Android.OS;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Account entity
    /// </summary>
    public class Account : Entity, IEquatable<Account>
    {
        private int _selectedNumber;
        private List<string> _presentationNumbers; 

        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; }

        /// <summary>
        /// Presentation numbers
        /// </summary>
        public List<string>PresentationNumbers {
            get { return _presentationNumbers; }
            set
            {
                _presentationNumbers = value;
                _selectedNumber = 0;
            }
        } 

        /// <summary>
        /// Selected presentation number index
        /// </summary>
        public int SelectedPresentationNumber
        {
            get
            {
                if ((PresentationNumbers != null) && (PresentationNumbers.Count > 0))
                    return _selectedNumber;
                return -1;
            }
            set {
                _selectedNumber = value < PresentationNumbers.Count ? value : 0;
            }
        }

        /// <summary>
        /// Get presentation number
        /// </summary>
        public string PresentationNumber => PresentationNumbers[_selectedNumber];

        public Account(string name, List<string> numbers)
        {
            AccountName = name;
            PresentationNumbers = numbers;
        }

        private Account(Parcel parcel)
        {
            AccountName = parcel.ReadString();
            parcel.ReadList(PresentationNumbers, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteString(AccountName);
            dest.WriteList(PresentationNumbers);
        }

        [ExportField("CREATOR")]
        public static ParcelableAccountCreator InitializeCreator()
        {
            return new ParcelableAccountCreator();
        }

        public class ParcelableAccountCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new Account(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(Account other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(AccountName, other.AccountName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Account) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (AccountName?.GetHashCode() ?? 0);
            }
        }
    }
}