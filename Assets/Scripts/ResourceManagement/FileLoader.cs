using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// Saves and loads data to/from disk.
/// </summary>
internal class FileLoader {

    /// <summary>
    /// Performs deserializing on a seperate thread.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    public class DeserializeThread<T> : ThreadedJob {
        private readonly Stream _stream;
        public T RetVal;

        internal DeserializeThread(Stream stream) {
            _stream = stream;
        }

        protected override void ThreadFunction() {
            using (_stream) {
                var formatter = new BinaryFormatter();
                _stream.Position = 0;
                RetVal = (T)formatter.Deserialize(_stream);
            }
        }
    }

    // Data will be stored here.
    protected string FolderPath { get; set; }

    /// <summary>
    /// Create a new FileLoader.
    /// </summary>
    /// <param name="folderName">The foldername the cache will be created in.</param>
    public FileLoader(string folderName) {
        FolderPath = folderName;
    }

    /// <summary>
    /// Saves a byte array to the cache.
    /// </summary>
    /// <param name="o">Byte array to save.</param>
    /// <param name="fileName">The filename to save to.</param>
    public virtual void Save(byte[] o, string fileName, string folderPath = "", bool overwrite = true) {
        folderPath = this.FolderPath + folderPath;

        Directory.CreateDirectory(folderPath);

        string fullpath = folderPath + Path.DirectorySeparatorChar + fileName;
        if (!overwrite && File.Exists(fullpath))
            return;

        File.WriteAllBytes(fullpath, o);
    }

    public void CreateFolder(string folderPath) {
        Directory.CreateDirectory(folderPath);
    }

    /// <summary>
    /// Saves a string to the cache.
    /// </summary>
    /// <param name="text">The text to save.</param>
    /// <param name="fileName">The filename to save to.</param>
    public virtual void Save(string text, string fileName, string folderPath = "", bool overwrite = true) {
        folderPath = this.FolderPath + folderPath;

        Directory.CreateDirectory(folderPath);

        string fullpath = folderPath + Path.DirectorySeparatorChar + fileName;
        if (!overwrite && File.Exists(fullpath))
            return;

        File.WriteAllText(fullpath, text);
    }

    /// <summary>
    /// Loads a byte array from the cache.
    /// </summary>
    /// <param name="fileName">Filename to load data from.</param>
    /// <returns>The retrieved byte array.</returns>
    public virtual byte[] Load(string fileName) {
        byte[] retVal = null;
        if (File.Exists(FolderPath + fileName)) {
            retVal = File.ReadAllBytes(FolderPath + fileName);
        }
        return retVal;
    }

    /// <summary>
    /// Loads a string from a text file.
    /// </summary>
    /// <param name="fileName">Filename to load data from.</param>
    /// <returns>The retrieved string.</returns>
    public virtual string LoadString(string fileName) {
        string retVal = null;
        if (File.Exists(FolderPath + fileName)) {
            retVal = File.ReadAllText(FolderPath + fileName);
        }
        return retVal;
    }



    /// <summary>
    /// Method for loading strings from the streaming assets folder.
    /// </summary>
    /// <param name="fileName">Filename of the file to load from, including extension.</param>
    /// <returns></returns>
    public static IEnumerator LoadStringFromStreamingAssets(string fileName, Action<string> callback) {
        string urlPrefix = "";
#if UNITY_EDITOR
        // in the editor you must use file:// for local files
        // android has it included in the streamingassetspath
        urlPrefix += "file://";
#endif
        // get the bundles from Assets/StreamingAssets/
        // TODO: remove for deployment
        // loading locally for testing only
        urlPrefix += Application.streamingAssetsPath + "/";
        string url = urlPrefix + fileName;
        var www = new WWW(url);

        yield return www;

        callback.Invoke(www.text);
    }

    /// <summary>
    /// Serializes an object and returns a stream. Does not save to cache, but can be saved using Save() 
    /// with the returned MemoryStream.
    /// </summary>
    /// <param name="source">The object to serialize.</param>
    /// <returns>Returns a memorystream of the serialized object.</returns>
    public MemoryStream Serialize(object source) {
        using (var stream = new MemoryStream()) {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, source);
            return stream;
        }
    }

    /// <summary>
    /// Deserializes from a stream.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <returns></returns>
    public T Deserialize<T>(Stream stream) {
        using (stream) {
            var formatter = new BinaryFormatter();
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }

    /// <summary>
    /// Deserializes from a byte array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="o"></param>
    /// <returns></returns>
    public T Deserialize<T>(byte[] o) {
        Stream stream = new MemoryStream(o);
        return Deserialize<T>(stream);
    }

    /// <summary>
    /// Deserializes a stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>The deserialization thread to wait for.</returns>
    public DeserializeThread<T> GetDeserializeThread<T>(Stream stream) {
        var thread = new DeserializeThread<T>(stream);
        return thread;
    }

    /// <summary>
    /// Deserializes a byte[].
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="o"></param>
    /// <returns>A thread that will perform deserialization when started.</returns>
    public DeserializeThread<T> GetDeserializeThread<T>(byte[] o) {
        Stream stream = new MemoryStream(o);
        return GetDeserializeThread<T>(stream);
    }

    public string[] GetAllFiles(string path, string extension) {
        return Directory.GetFiles(path, "*." + extension);
    }
}