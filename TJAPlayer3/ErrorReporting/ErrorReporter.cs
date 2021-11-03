using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sentry;
using Sentry.Protocol;

namespace TJAPlayer3.ErrorReporting
{
    public static class ErrorReporter
    {
        public static void WithErrorReporting(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                ReportError(e);

#if !DEBUG
                try
                {
                    NotifyUserOfError(e);
                }
                catch
                {
                    // ignored
                }
#endif
            }
        }

        public static string ToSha256InBase64(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(value);
                var sha256Bytes = sha256.ComputeHash(utf8Bytes);
                return Convert.ToBase64String(sha256Bytes);
            }
        }

        private static void ReportError(Exception e)
        {
            Trace.WriteLine("");
            Trace.WriteLine(e);
            Trace.WriteLine("");
            Trace.WriteLine("エラーだゴメン！（涙");
        }

        private static void NotifyUserOfError(Exception exception)
        {
            var messageBoxText =
                "An error has occurred and was automatically reported.\n\n" +
                "If you wish, you can provide additional information, look for similar issues, etc. by visiting our GitHub Issues page.\n\n" +
                "Would you like the error details copied to the clipboard and your browser opened?\n\n" +
                exception;
            var dialogResult = MessageBox.Show(
                messageBoxText,
                $"{TJAPlayer3.AppDisplayNameWithThreePartVersion} Error",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);
            if (dialogResult == DialogResult.Yes)
            {
                Clipboard.SetText(exception.ToString());
                Process.Start("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            }
        }
    }
}