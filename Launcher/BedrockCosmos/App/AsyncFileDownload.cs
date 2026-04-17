using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

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
    internal class AsyncFileDownload
    {
        private readonly HttpClient httpClient;

        internal AsyncFileDownload()
        {
            httpClient = new HttpClient();
        }

        internal async Task DownloadFileAsync(string fileUrl, string downloadPath)
        {
            CosmosConsole.WriteLine(LanguageHandler.Format("Logs.DownloadingFile", fileUrl));

            using (var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                }
            }

            CosmosConsole.WriteLine(LanguageHandler.Format("Logs.DownloadedFile", downloadPath));
        }

        internal async Task ExtractFileAsync(string zipFilePath, string extractPath, bool deleteAfterExtracting)
        {
            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            CosmosConsole.WriteLine(LanguageHandler.Format("Logs.ExtractingFile", Path.GetFileName(zipFilePath)));

            await Task.Run(() =>
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationFilePath = Path.Combine(extractPath, entry.FullName);

                        if (entry.FullName.EndsWith("/"))
                            Directory.CreateDirectory(destinationFilePath);
                        else
                            entry.ExtractToFile(destinationFilePath, overwrite: true);
                    }
                }

                if (deleteAfterExtracting)
                {
                    File.Delete(zipFilePath);
                    CosmosConsole.WriteLine(LanguageHandler.Format("Logs.RemovedFile", Path.GetFileName(zipFilePath)));
                }
            });

            CosmosConsole.WriteLine(LanguageHandler.Format("Logs.ExtractedFile", Path.GetFileName(zipFilePath), extractPath));
        }

        internal async Task<(string version, string responsesVersion)> ReadVersionFileAsync()
        {
            string xml = await httpClient.GetStringAsync("https://raw.githubusercontent.com/Bedrock-Cosmos/Website/refs/heads/main/CurrentVersion.xml");

            XDocument doc = XDocument.Parse(xml);

            string version = doc.Root.Element("version").Value;
            string responsesVersion = doc.Root.Element("responses_version").Value;
            return (version, responsesVersion);
        }

        internal void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
