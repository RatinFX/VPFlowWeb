using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace VPFlowWebMain
{
    internal static class Logging
    {
        private static TextBox LogArea => MainForm.Instance.LogArea;

        private const int MAX_LOG_LINES = 50;

        private static void Commit(string prefix, string text)
        {
            // build new entry and append to existing text
            var entry = prefix + text;
            var current = string.IsNullOrEmpty(LogArea.Text) ? entry : (LogArea.Text + "\r\n" + entry);

            // trim to last MAX_LOG_LINES lines
            var lines = current.Split(new[] { "\r\n" }, StringSplitOptions.None);
            if (lines.Length > MAX_LOG_LINES)
            {
                current = string.Join("\r\n", lines.Skip(lines.Length - MAX_LOG_LINES));
            }

            LogArea.Text = current;

            // move caret to end and ensure visible
            LogArea.SelectionStart = LogArea.Text.Length;
            LogArea.ScrollToCaret();
        }

        public static void Log(string text)
        {
            Debug.WriteLine(text);
            Commit("> ", text);
        }

        public static void Warn(string text)
        {
            Debug.WriteLine(text);
            Commit("[!] ", text);
        }

        public static void Error(string text)
        {
            Debug.WriteLine(text);
            Commit("[ERROR] ", text);
        }
    }
}
