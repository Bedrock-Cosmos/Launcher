using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.App.UI
{
    // Cache works by writing a thumbnail to disk if not downloaded before, then
    // loads in to RAM, with a max of MaxMemoryEntries loaded at one time.
    internal static class ImageCache
    {
        private const int MaxMemoryEntries = 250; // Max thumbnails in RAM at once.

        private const int DecodeSize = 64; // Cache size, e.g. 64x64 pixels.

        private static readonly Dictionary<string, Image> Memory = new Dictionary<string, Image>();
        private static readonly LinkedList<string> RecencyOrder = new LinkedList<string>();
        private static readonly Dictionary<string, LinkedListNode<string>> RecencyNodes = new Dictionary<string, LinkedListNode<string>>();
        private static readonly HashSet<string> InFlight = new HashSet<string>();
        private static readonly object Lock = new object();

        private static readonly string CacheDirectory = PathDefinitions.CacheDirectory;

        // Raised on a background thread once a thumbnail finishes loading.
        public static event Action<string> ImageLoaded;

        // Returns the cached image for a node id, or null if not in memory yet.
        public static Image TryGet(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            lock (Lock)
            {
                if (Memory.TryGetValue(id, out var image))
                {
                    Touch(id);
                    return image;
                }
            }

            return null;
        }

        // Starts background load for node id/url if it isn't already cached in memory.
        public static void RequestLoad(string id, string url)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(url))
                return;

            lock (Lock)
            {
                if (Memory.ContainsKey(id) || InFlight.Contains(id))
                    return;

                InFlight.Add(id);
            }

            ThreadPool.QueueUserWorkItem(_ => LoadWorker(id, url));
        }

        private static void LoadWorker(string id, string url)
        {
            try
            {
                string path = GetDiskPath(id);
                Bitmap thumbnail;

                if (File.Exists(path))
                {
                    using (var fileStream = File.OpenRead(path))
                    using (var decoded = Image.FromStream(fileStream))
                    {
                        // Clone so FileStream can close immediately.
                        thumbnail = new Bitmap(decoded);
                    }
                }
                else
                {
                    using (var client = new WebClient())
                    {
                        byte[] bytes = client.DownloadData(url);

                        using (var memoryStream = new MemoryStream(bytes))
                        using (var fullSize = Image.FromStream(memoryStream))
                        {
                            thumbnail = DownscaleTo(fullSize, DecodeSize);
                        }
                    }

                    SaveToDisk(thumbnail, path);
                }

                lock (Lock)
                {
                    StoreInMemory(id, thumbnail);
                    InFlight.Remove(id);
                }
            }
            catch
            {
                // Called if URL or connection is bad, can be re-requested.
                lock (Lock)
                {
                    InFlight.Remove(id);
                }
            }

            RaiseImageLoaded(id);
        }

        // Notifies every subscriber individually, isolating each one in its own try/catch.
        private static void RaiseImageLoaded(string id)
        {
            var handler = ImageLoaded;
            if (handler == null)
                return;

            foreach (var singleHandler in handler.GetInvocationList())
            {
                try
                {
                    ((Action<string>)singleHandler)(id);
                }
                catch
                {
                    // Only a redraw signal.
                }
            }
        }

        private static Bitmap DownscaleTo(Image source, int maxSize)
        {
            if (source.Width <= maxSize && source.Height <= maxSize)
                return new Bitmap(source);

            double scale = Math.Min((double)maxSize / source.Width, (double)maxSize / source.Height);
            int targetWidth = Math.Max(1, (int)(source.Width * scale));
            int targetHeight = Math.Max(1, (int)(source.Height * scale));

            var result = new Bitmap(targetWidth, targetHeight);
            using (var g = Graphics.FromImage(result))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(source, 0, 0, targetWidth, targetHeight);
            }

            return result;
        }

        private static void SaveToDisk(Bitmap bitmap, string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                bitmap.Save(path, ImageFormat.Png);
            }
            catch
            {

            }
        }

        private static string GetDiskPath(string id)
        {
            return Path.Combine(CacheDirectory, SanitizeFileName(id) + ".png");
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidChar, '_');
            return name;
        }

        // Must be called while holding ImageCache.Lock.
        private static void StoreInMemory(string id, Image image)
        {
            if (Memory.TryGetValue(id, out var existing))
            {
                existing.Dispose();
                Memory[id] = image;
                Touch(id);
                return;
            }

            Memory[id] = image;
            RecencyNodes[id] = RecencyOrder.AddLast(id);

            while (Memory.Count > MaxMemoryEntries)
            {
                var oldestNode = RecencyOrder.First;
                if (oldestNode == null) break;

                RecencyOrder.RemoveFirst();
                RecencyNodes.Remove(oldestNode.Value);

                if (Memory.TryGetValue(oldestNode.Value, out var oldImage))
                {
                    oldImage.Dispose();
                    Memory.Remove(oldestNode.Value);
                }
            }
        }

        // Must be called while holding ImageCache.Lock.
        private static void Touch(string id)
        {
            if (!RecencyNodes.TryGetValue(id, out var node))
                return;

            RecencyOrder.Remove(node);
            RecencyNodes[id] = RecencyOrder.AddLast(id);
        }

        public static void ClearMemoryCache()
        {
            lock (Lock)
            {
                foreach (var image in Memory.Values)
                    image.Dispose();

                Memory.Clear();
                RecencyOrder.Clear();
                RecencyNodes.Clear();
            }
        }

        public static void ClearDiskCache()
        {
            try
            {
                if (Directory.Exists(CacheDirectory))
                    Directory.Delete(CacheDirectory, recursive: true);
            }
            catch
            {

            }
        }
    }
}