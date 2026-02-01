using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace BedrockCosmos.App
{
    internal class AsyncFileDownload
    {
        private readonly HttpClient httpClient;
        private readonly string consoleSender = "App";

        internal AsyncFileDownload()
        {
            httpClient = new HttpClient();
        }

        internal async Task DownloadFileAsync(string fileUrl, string downloadPath)
        {
            CosmosConsole.WriteLine(consoleSender, $"Downloading file at {fileUrl}");

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

            CosmosConsole.WriteLine(consoleSender, $"Successfully downloaded file to {downloadPath}");
        }

        internal async Task ExtractFileAsync(string zipFilePath, string extractPath, bool deleteAfterExtracting)
        {
            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            CosmosConsole.WriteLine(consoleSender, $"Extracting {Path.GetFileName(zipFilePath)}...");

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
                    CosmosConsole.WriteLine(consoleSender, $"Removed {Path.GetFileName(zipFilePath)} file.");
                }
            });

            CosmosConsole.WriteLine(consoleSender, $"Successfully extracted {Path.GetFileName(zipFilePath)} to " +
                $"{extractPath}");
        }

        internal void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
