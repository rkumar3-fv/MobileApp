using System;
using System.Collections.Generic;
using Android.OS;
using Android.Runtime;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Presentation numbers list response
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems-systemPhoneNumber-presentationPhoneNumbers">API - Get accounts request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class GetPresentationNumbersResponse : BaseResponse, IEquatable<GetPresentationNumbersResponse>
    {
        /// <summary>
        /// Presentation numbers list
        /// </summary>
        public List<string> NumbersList { get; }

        /// <summary>
        /// Response init for GetAccountsRequest
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="numbers">Presentation numbers' enum</param>
        public GetPresentationNumbersResponse(long requestId, IEnumerable<string> numbers) : base(requestId)
        {
            NumbersList = new List<string>();
            foreach (var number in numbers)
            {
                NumbersList.Add(number);
            }
        }

        public GetPresentationNumbersResponse(Parcel parcel) : base(parcel)
        {
            parcel.ReadList(NumbersList, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteList(NumbersList);
        }

        [ExportField("CREATOR")]
        public static ParcelableAccountsResponseCreator InitializeCreator()
        {
            return new ParcelableAccountsResponseCreator();
        }

        public class ParcelableAccountsResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetAccountsResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(GetPresentationNumbersResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(NumbersList, other.NumbersList);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GetPresentationNumbersResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (NumbersList?.GetHashCode() ?? 0);
            }
        }
    }
}