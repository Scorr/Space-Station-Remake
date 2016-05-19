using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Saves and loads data to/from disk.
/// </summary>
internal class Cache : FileLoader {
    
    // The maximum cache size in bytes.
    protected long MaxCacheSize { get; set; }

    // The maximum size of all caches combined in bytes.
    private const long MaxTotalCacheSize = 1000*1000000;

    /// <summary>
    /// Create a new cache.
    /// </summary>
    /// <param name="folderName">The foldername the cache will be created in.</param>
    /// <param name="maxSize">The maximum cache size in bytes. Defaults to 200 MB.</param>
    public Cache(string folderName, long maxSize = 200 * 1000000) : base("Cache" + Path.DirectorySeparatorChar) {
        FolderPath += folderName + Path.DirectorySeparatorChar;

        MaxCacheSize = maxSize;
    }

    /// <summary>
    /// Saves a byte array to the cache.
    /// </summary>
    /// <param name="o">Byte array to save.</param>
    /// <param name="fileName">The filename to save to.</param>
    public override void Save(byte[] o, string fileName, string folderPath = "", bool overwrite = true) {
        base.Save(o, fileName, folderPath, overwrite);

        // Check if the cache size has been exceeded and delete oldest files.
        CheckCacheSize();
    }

    /// <summary>
    /// Saves a string to the cache.
    /// </summary>
    /// <param name="text">The text to save.</param>
    /// <param name="fileName">The filename to save to.</param>
    public override void Save(string text, string fileName, string folderPath = "", bool overwrite = true) {
        base.Save(text, fileName, folderPath, overwrite);

        // Check if the cache size has been exceeded and delete oldest files.
        CheckCacheSize();
    }
    
    /// <summary>
    /// Deletes the oldest file(s) from the cache if they exceed the file limit.
    /// </summary>
    private void CheckCacheSize() {
        FileInfo[] fi = new DirectoryInfo(FolderPath).GetFiles("*.*", SearchOption.AllDirectories);
        long totalSize = fi.Sum(file => file.Length);

        if (totalSize > MaxCacheSize) {
            IOrderedEnumerable<FileInfo> sortedbyoldest = fi.OrderBy(x => x.LastWriteTime);

            int i = 0;
            while (totalSize > MaxCacheSize) {
                FileInfo oldest = sortedbyoldest.ElementAt(i);
                Debug.Log(oldest.LastWriteTime);
                totalSize -= oldest.Length;
                oldest.Delete();
                i++;
            }
        }
    }

    /// <summary>
    /// Checks the combined size of all caches and deletes the oldest file(s) if exceeding the limit.
    /// </summary>
    private void CheckTotalCacheSize() {
        FileInfo[] fi = new DirectoryInfo(Application.persistentDataPath + Path.DirectorySeparatorChar).GetFiles("*.*", SearchOption.AllDirectories);
        long totalSize = fi.Sum(file => file.Length);

        if (totalSize > MaxTotalCacheSize) {
            IOrderedEnumerable<FileInfo> sortedbyoldest = fi.OrderBy(x => x.LastWriteTime);

            int i = 0;
            while (totalSize > MaxTotalCacheSize) {
                FileInfo oldest = sortedbyoldest.ElementAt(i);
                totalSize -= oldest.Length;
                oldest.Delete();
                i++;
            }
        }
    }
}