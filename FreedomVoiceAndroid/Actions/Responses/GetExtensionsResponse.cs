using System;
using System.Collections.Generic;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Entities;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Extensions list response
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems-systemPhoneNumber-mailboxesWithCounts">API - Get extensions request</see>
    /// </summary>
    public class GetExtensionsResponse : BaseResponse, IEquatable<GetExtensionsResponse>
    {
        /// <summary>
        /// Extensions list
        /// </summary>
        public List<Extension> ExtensionsList { get; }

        public GetExtensionsResponse(long requestId, List<Extension> extensions) : base(requestId)
        {
            ExtensionsList = extensions;
        }

        public GetExtensionsResponse(Parcel parcel) : base(parcel)
        {
            parcel.ReadList(ExtensionsList, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteList(ExtensionsList);
        }

        [ExportField("CREATOR")]
        public static ParcelableExtensionsResponseCreator InitializeCreator()
        {
            return new ParcelableExtensionsResponseCreator();
        }

        public class ParcelableExtensionsResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetExtensionsResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(GetExtensionsResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(ExtensionsList, other.ExtensionsList);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GetExtensionsResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (ExtensionsList?.GetHashCode() ?? 0);
            }
        }
    }
}