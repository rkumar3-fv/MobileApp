using Android.Content;
using Android.Support.V4.Content;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class FileUtils
    {
        public static Intent OpenPdfFileIntent(Context context, string filePath)
        {
            var file = new Java.IO.File(filePath);
            file.SetReadable(true);
            var uriForFile = FileProvider.GetUriForFile(
                context,
                context.GetString(Resource.String.FileProvider),
                file
            );

            var intent = new Intent(Intent.ActionView);
            intent.SetFlags(ActivityFlags.GrantReadUriPermission |
                            ActivityFlags.GrantWriteUriPermission |
                            ActivityFlags.NewTask |
                            ActivityFlags.NoHistory);
            intent.SetDataAndType(uriForFile, "application/pdf");
            return intent;
        }
    }
}