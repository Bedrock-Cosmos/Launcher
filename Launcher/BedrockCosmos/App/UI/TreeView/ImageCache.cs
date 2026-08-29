using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;

namespace BedrockCosmos.App.UI
{
    /// <summary>
    /// Process-wide, two-tier thumbnail cache shared by every <see cref="ImageTreeView"/>
    /// instance, keyed by each node's <see cref="ImageItem.Id"/> (not its URL).
    ///
    /// Tier 1 - disk (%LocalAppData%\Bedrock Cosmos\Cache\{id}.png): once a
    /// thumbnail has been downloaded it is written here forever, so it is only
    /// ever fetched from the network once per machine.
    ///
    /// Tier 2 - memory: a small, capped LRU of decoded <see cref="Bitmap"/>
    /// objects. This is the piece that actually keeps RAM usage flat regardless
    /// of how many total items you've ever loaded - only the most recently used
    /// ~<see cref="MaxMemoryEntries"/> thumbnails are ever resident at once; the
    /// rest live only on disk until they're needed again (a fast local read, no
    /// network round trip).
    ///
    /// Every downloaded image is also downscaled to <see cref="DecodeSize"/>
    /// pixels before it is ever kept anywhere (cache or disk). Marketplace
    /// thumbnail URLs can point at fairly large source images (some in the
    /// sample payload request 800x450), and decoding/keeping those at full size
    /// for a UI that only ever displays a small square is what actually causes
    /// large memory growth - shrinking on first use fixes that at the source,
    /// independent of where the result is cached.
    /// </summary>
    internal static class ImageCache
    {
        /// <summary>Maximum number of decoded thumbnails kept in RAM at once.</summary>
        private const int MaxMemoryEntries = 250;

        /// <summary>Thumbnails are downscaled to at most this many pixels per side before being cached anywhere.</summary>
        private const int DecodeSize = 64;

        private static readonly Dictionary<string, Image> Memory = new Dictionary<string, Image>();
        private static readonly LinkedList<string> RecencyOrder = new LinkedList<string>();
        private static readonly Dictionary<string, LinkedListNode<string>> RecencyNodes = new Dictionary<string, LinkedListNode<string>>();
        private static readonly HashSet<string> InFlight = new HashSet<string>();
        private static readonly object Lock = new object();

        private static readonly string CacheDirectory = PathDefinitions.CacheDirectory;

        /// <summary>
        /// Raised on a background thread once a thumbnail finishes loading
        /// (from disk or network). The argument is the node id, not the URL.
        /// Subscribers must marshal back to the UI thread before touching a control.
        /// </summary>
        public static event Action<string> ImageLoaded;

        /// <summary>Returns the cached image for a node id, or null if it isn't in memory yet.</summary>
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

        /// <summary>
        /// Kicks off a background load for the given node id/url if it isn't
        /// already cached in memory or in flight. Safe to call repeatedly (e.g.
        /// once per paint) without causing duplicate work.
        /// </summary>
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
                        // Clone so the FileStream can be closed immediately.
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
                // Bad URL / network hiccup / corrupt file - give up on this one
                // thumbnail. The cell will keep re-requesting on future paints.
                lock (Lock)
                {
                    InFlight.Remove(id);
                }
            }

            RaiseImageLoaded(id);
        }

        /// <summary>
        /// Notifies every subscriber individually, isolating each one in its
        /// own try/catch. This runs on a ThreadPool thread - on .NET Framework
        /// an unhandled exception there terminates the whole process by
        /// default, so a single subscriber that's been disposed (or, for a
        /// design-time control instance, torn down by a Visual Studio designer
        /// reload) must never be allowed to take the app - or the designer -
        /// down with it, and must never be allowed to stop other, still-valid
        /// subscribers from being notified.
        /// </summary>
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
                    // Swallow - see remarks above. This is only a "please
                    // redraw" signal, not critical work.
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
                // Non-fatal - worst case we just re-download next run.
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

        /// <summary>Must be called while holding <see cref="Lock"/>.</summary>
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

        /// <summary>Must be called while holding <see cref="Lock"/>.</summary>
        private static void Touch(string id)
        {
            if (!RecencyNodes.TryGetValue(id, out var node))
                return;

            RecencyOrder.Remove(node);
            RecencyNodes[id] = RecencyOrder.AddLast(id);
        }

        /// <summary>Frees every in-memory bitmap (does not touch the on-disk cache). Call on app shutdown if you want to be tidy.</summary>
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

        /// <summary>Deletes every file in the on-disk cache. Use sparingly (forces re-downloads).</summary>
        public static void ClearDiskCache()
        {
            try
            {
                if (Directory.Exists(CacheDirectory))
                    Directory.Delete(CacheDirectory, recursive: true);
            }
            catch
            {
                // Ignore - a locked file here isn't worth surfacing to the caller.
            }
        }
    }
}