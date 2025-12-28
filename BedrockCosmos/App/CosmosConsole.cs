using System;
using System.Windows.Forms;

namespace BedrockCosmos.App
{
    public class CosmosConsole
    {
        private static TextBox _console;
        public static bool endableLogging = false;
        public static bool logToMainConsole = false;

        public static void Initialize(TextBox textBox)
        {
            _console = textBox;
        }

        public static void WriteLine(string sender, string message)
        {
            string log = $"[{sender}] {message}";

            if (_console != null)
            {
                // Use Invoke to ensure thread safety when updating UI control from non-UI thread
                if (_console.InvokeRequired)
                    _console.Invoke(new Action<string, string>(WriteLine), sender, message);
                else
                    _console.AppendText(log + Environment.NewLine);
            }

            if (logToMainConsole)
                Console.WriteLine(log);
        }
    }
}
