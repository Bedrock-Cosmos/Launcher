using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.App
{
    internal static class CosmosConsole
    {
        private static RichTextBox _console;
        private static bool _logToMainConsole = false;

        // Scroll locking variables/imports
        private const int WM_SETREDRAW = 0x0b;
        private const int EM_GETSCROLLPOS = 0x0400 + 221;
        private const int EM_SETSCROLLPOS = 0x0400 + 222;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, int msg, IntPtr wParam, ref POINT lParam);

        internal static void Initialize(RichTextBox textBox)
        {
            _console = textBox;
        }

        internal static bool LogToMainConsole
        {
            get { return _logToMainConsole; }
            set { _logToMainConsole = value; }
        }

        private static bool IsScrolledToBottom(RichTextBox textBox)
        {
            int lastVisibleChar = textBox.GetCharIndexFromPosition(
                new Point(1, textBox.ClientSize.Height - 1));

            return lastVisibleChar >= textBox.TextLength - 1;
        }

        internal static void WriteLine(string message)
        {
            if (SettingsManager.EnableLogging)
                WriteLine("App", message);
        }

        internal static void WriteLine(string sender, string message)
        {
            if (SettingsManager.EnableLogging)
            {
                string log = $"[{sender}] {message}";

                if (_console != null)
                {
                    if (_console.InvokeRequired)
                    {
                        _console.Invoke(new Action<string, string>(WriteLine), sender, message);
                    }
                    else
                    {
                        bool shouldAutoScroll = IsScrolledToBottom(_console);

                        POINT scrollPos = new POINT();
                        SendMessage(_console.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref scrollPos); // Save scroll position
                        SendMessage(_console.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero); // Suspend redraw

                        _console.AppendText(log + Environment.NewLine);

                        if (!shouldAutoScroll)
                        {
                            SendMessage(_console.Handle, EM_SETSCROLLPOS, IntPtr.Zero, ref scrollPos);
                        }
                        else
                        {
                            _console.SelectionStart = _console.TextLength;
                            _console.ScrollToCaret();
                        }

                        SendMessage(_console.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero); // Resume redraw
                        _console.Invalidate();
                    }
                }

                if (_logToMainConsole)
                    Console.WriteLine(log);
            }
        }

        internal static void ExportLogs()
        {
            try
            {
                string logsFolder = PathDefinitions.CosmosAppData + @"Logs\";

                if (!Directory.Exists(logsFolder))
                    Directory.CreateDirectory(logsFolder);

                int fileCount = Directory.GetFiles(logsFolder).Length;
                string logPath = logsFolder + @"\Log" + fileCount.ToString() + ".txt";

                File.WriteAllText(logPath, _console.Text);
                WriteLine("App", LanguageHandler.Format("Logs.ExportedLog", logPath));
            }
            catch (Exception)
            {
                WriteLine("App", LanguageHandler.Get("Logs.ExportLogFailed"));
            }
        }
    }
}
