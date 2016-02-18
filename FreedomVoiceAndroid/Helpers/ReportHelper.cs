using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using FreedomVoice.Core.Utils;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public class ReportHelper
    {
        private readonly Context _context;
        private readonly ActionsHelper _actionsHelper;
        private readonly Dictionary<DateTime, string> _logDictionary; 

        public ReportHelper(Context context, ActionsHelper actionsHelper)
        {
            _context = context;
            _actionsHelper = actionsHelper;
            _logDictionary = new Dictionary<DateTime, string>();
        }

        public void Log(string logEntity)
        {
            _logDictionary.Add(DateTime.Now, logEntity);
        }

        public void SendReport(Activity activity, string reportText)
        {
            var title = $"Report {DataFormatUtils.ToFullFormattedDate(DateTime.Now)}";
            var path = $"{_context.GetExternalFilesDir(null)}/{Path.GetRandomFileName()}.log";
            Task.Factory.StartNew(() => CreateFullReport(path, _logDictionary!=null?new Dictionary<DateTime, string>(_logDictionary):new Dictionary<DateTime, string>()))
                .ContinueWith(task => activity.RunOnUiThread(() =>
                {
                    _logDictionary.Clear();
                    SendMail(title, reportText, path);
                }));
        }

        public void SendFeedback(Activity activity, string feedbackText)
        {
            var title = $"Feedback {DataFormatUtils.ToFullFormattedDate(DateTime.Now)}";
            var path = $"{_context.GetExternalFilesDir(null)}/{Path.GetRandomFileName()}.log";
            Task.Factory.StartNew(() => CreateSimpleReport(path))
                .ContinueWith(task => activity.RunOnUiThread(() => SendMail(title, feedbackText, path)));
        }

        private void SendMail(string title, string text, string attachmentFilePath)
        {
            var email = new Intent(Intent.ActionSend);
            email.PutExtra(Intent.ExtraEmail, new[] { "sergey.yakovlev@waveaccess.ru", "julia.andriyash@waveaccess.ru" });
            email.PutExtra(Intent.ExtraCc, new[] { "liza.galyga@waveaccess.ru" });
            email.PutExtra(Intent.ExtraSubject, title);
            email.PutExtra(Intent.ExtraText, text);
            if (!string.IsNullOrEmpty(attachmentFilePath))
                email.PutExtra(Intent.ExtraStream, Uri.Parse("file://" + attachmentFilePath));
            email.SetType("message/rfc822");
            email.SetFlags(ActivityFlags.NewTask);
            try
            {
                _context.StartActivity(email);
            }
            catch (ActivityNotFoundException)
            {
                Toast.MakeText(_context, Resource.String.Toast_noMailClient, ToastLength.Long).Show();
            }
        }

        private void CreateFullReport(string path, Dictionary<DateTime, string> dictionary)
        {
            using (var streamWriter = new StreamWriter(path, true))
            {
                var pInfo = _context.PackageManager.GetPackageInfo(App.AppPackage, 0);
                streamWriter.WriteLine($"Application name: {_context.GetString(Resource.String.ApplicationName)}");
                streamWriter.WriteLine($"Application version: { pInfo.VersionCode} ({ pInfo.VersionName})");
#if DEBUG
                streamWriter.WriteLine($"Application mode: DEBUG");
#else
                streamWriter.WriteLine($"Application mode: RELEASE");
#endif
                streamWriter.WriteLine($"Device manufacturer {Build.Manufacturer}");
                streamWriter.WriteLine($"Device model {Build.Model}");
                streamWriter.WriteLine($"API version: {Build.VERSION.Sdk}");
                streamWriter.WriteLine($"Display resolution: {_context.Resources.DisplayMetrics.WidthPixels} x {_context.Resources.DisplayMetrics.HeightPixels}");
                streamWriter.WriteLine("--- DEVICE STATE ---");
                streamWriter.WriteLine($"Account: {(_actionsHelper.SelectedAccount!=null?_actionsHelper.SelectedAccount.AccountName:"NULL")}");
                streamWriter.WriteLine($"Presentation number {(_actionsHelper.SelectedAccount != null ? _actionsHelper.SelectedAccount.PresentationNumber : "NULL")}");
                if ((_actionsHelper.ExtensionsList != null) && (_actionsHelper.ExtensionsList.Count > 0))
                {
                    streamWriter.WriteLine("--- EXT LIST ---");
                    foreach (var extension in _actionsHelper.ExtensionsList)
                    {
                        streamWriter.WriteLine($"{extension.ExtensionName} (x{extension.Id})");
                    }
                    if ((_actionsHelper.SelectedExtension > -1) &&
                        (_actionsHelper.SelectedExtension < _actionsHelper.ExtensionsList.Count))
                    {
                        streamWriter.WriteLine($"SELECTED EXTENSION: {_actionsHelper.ExtensionsList[_actionsHelper.SelectedExtension].ExtensionName} (x{_actionsHelper.ExtensionsList[_actionsHelper.SelectedExtension].Id})");
                        streamWriter.WriteLine("--- FOLDERS LIST ---");
                        foreach (var folder in _actionsHelper.ExtensionsList[_actionsHelper.SelectedExtension].Folders)
                        {
                            streamWriter.WriteLine(folder.FolderName);
                        }
                        if ((_actionsHelper.SelectedFolder > -1) &&
                            (_actionsHelper.SelectedFolder <
                             _actionsHelper.ExtensionsList[_actionsHelper.SelectedExtension].Folders.Count))
                        {
                            streamWriter.WriteLine($"SELECTED FOLDER: {_actionsHelper.ExtensionsList[_actionsHelper.SelectedExtension].Folders[_actionsHelper.SelectedFolder].FolderName}");
                        }
                        else
                            streamWriter.WriteLine("SELECTED FOLDER: NOT SELECTED");
                    }
                    else
                        streamWriter.WriteLine("SELECTED EXTENSION: NOT SELECTED");
                }
                if ((_actionsHelper.WaitingRequestArray != null) && (_actionsHelper.WaitingRequestArray.Count > 0))
                {
                    streamWriter.WriteLine("---WAITING RESPONSES STACK---");
                    foreach (var request in _actionsHelper.WaitingRequestArray)
                    {
                        streamWriter.WriteLine($"{request.Key}: {request.Value.Class.Name}");
                    }
                }
                else
                    streamWriter.WriteLine("WAITING RESPONSES STACK: EMPTY");
                if (dictionary.Count==0)
                    return;
                streamWriter.WriteLine("--- DEVICE LOG ---");
                foreach (var entity in dictionary)
                {
                    streamWriter.WriteLine($"{DataFormatUtils.ToFullFormattedDate(entity.Key)} - {entity.Value}");
                }
            }
        }

        private void CreateSimpleReport(string path)
        {
            using (var streamWriter = new StreamWriter(path, true))
            {
                var pInfo = _context.PackageManager.GetPackageInfo(App.AppPackage, 0);
                streamWriter.WriteLine($"Application name: {_context.GetString(Resource.String.ApplicationName)}");
                streamWriter.WriteLine($"Application version: {pInfo.VersionCode} ({pInfo.VersionName})");
#if DEBUG
                streamWriter.WriteLine($"Application mode: DEBUG");
#else
                streamWriter.WriteLine($"Application mode: RELEASE");
#endif
                streamWriter.WriteLine($"Device manufacturer {Build.Manufacturer}");
                streamWriter.WriteLine($"Device model {Build.Model}");
                streamWriter.WriteLine($"API version: {Build.VERSION.Sdk}");
                streamWriter.WriteLine($"Display resolution: {_context.Resources.DisplayMetrics.WidthPixels} x {_context.Resources.DisplayMetrics.HeightPixels}");
            }
        }
    }
}