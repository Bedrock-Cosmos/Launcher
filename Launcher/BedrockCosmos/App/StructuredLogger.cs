using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace BedrockCosmos.App
{
    internal static class StructuredLogger
    {
        private static readonly object SyncRoot = new object();

        internal static void Log(string category, string eventName, object details = null)
        {
            try
            {
                if (!Directory.Exists(PathDefinitions.LogsDirectory))
                    Directory.CreateDirectory(PathDefinitions.LogsDirectory);

                var payload = new
                {
                    timestampUtc = DateTime.UtcNow.ToString("o"),
                    category,
                    eventName,
                    details
                };

                string path = Path.Combine(PathDefinitions.LogsDirectory, "proxy-lifecycle.jsonl");
                string line = TextSanitizer.ReplaceInvalidUnicode(JsonConvert.SerializeObject(payload));

                lock (SyncRoot)
                {
                    File.AppendAllText(path, line + Environment.NewLine, new UTF8Encoding(false, false));
                }
            }
            catch
            {
                // Structured logging must never interfere with proxy recovery.
            }
        }
    }
}
