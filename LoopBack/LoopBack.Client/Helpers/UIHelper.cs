﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;

namespace LoopBack.Client.Helpers
{
    internal static partial class UIHelper
    {
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new();
            _ = builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { _ = builder.AppendLine($"Message: {ex.Message}"); }
            _ = builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16).ToUpperInvariant()})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { _ = builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { _ = builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }

        public static TResult AwaitByTaskCompleteSource<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Run(async () =>
            {
                try
                {
                    TResult result = await function.Invoke().ConfigureAwait(false);
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }, cancellationToken);
            TResult taskResult = task.Result;
            return taskResult;
        }

        public static bool ValidateAndGetUri(string url, out Uri uri)
        {
            uri = null;
            if (string.IsNullOrWhiteSpace(url)) { return false; }
            try
            {
                uri = new Uri(url);
                return true;
            }
            catch (FormatException ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UIHelper)).Warn(ex.ExceptionToMessage(), ex);
            }
            return false;
        }
    }
}
