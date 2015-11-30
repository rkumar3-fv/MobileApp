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
        /// Account state
        /// </summary>
        public bool AccountState { get; set; }

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
        public string PresentationNumber => PresentationNumbers[SelectedPresentationNumber];

        public Account(string name, List<string> numbers, bool state)
        {
            AccountName = name;
            AccountState = state;
            PresentationNumbers = numbers;
        }

        public Account(string name, List<string> numbers) : this (name, numbers, true)
        {}

        private Account(Parcel parcel)
        {
            AccountName = parcel.ReadString();
            AccountState = parcel.ReadByte() == 1;
            parcel.ReadList(PresentationNumbers, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteString(AccountName);
            dest.WriteByte(AccountState ? (sbyte)1 : (sbyte)0);
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
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(AccountName, other.AccountName) && AccountState == other.AccountState;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Account) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (AccountName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}