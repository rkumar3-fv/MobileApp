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
    /// Folders list response
    /// </summary>
    public class GetFoldersResponse : BaseResponse, IEquatable<GetFoldersResponse>
    {
        /// <summary>
        /// Folders list
        /// </summary>
        public List<Folder> FoldersList { get; }

        public GetFoldersResponse(long requestId, List<Folder> folders) : base(requestId)
        {
            FoldersList = folders;
        }

        public GetFoldersResponse(Parcel parcel) : base(parcel)
        {
            parcel.ReadList(FoldersList, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteList(FoldersList);
        }

        [ExportField("CREATOR")]
        public static ParcelableFoldersResponseCreator InitializeCreator()
        {
            return new ParcelableFoldersResponseCreator();
        }

        public class ParcelableFoldersResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetFoldersResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(GetFoldersResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(FoldersList, other.FoldersList);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GetFoldersResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (FoldersList?.GetHashCode() ?? 0);
            }
        }
    }
}