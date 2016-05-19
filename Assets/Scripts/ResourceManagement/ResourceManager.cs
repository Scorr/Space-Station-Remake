using UnityEngine;
using System.Collections.Generic;
using System.IO;

internal sealed class ResourceManager {
    private static readonly ResourceManager _instance = new ResourceManager();
    private static readonly string StreamingAssetsPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar;
    private readonly FileLoader _fileLoader = new FileLoader(StreamingAssetsPath);
    private static readonly string MainPath = StreamingAssetsPath + "";
    private static readonly string ModdingPath = StreamingAssetsPath + "Modding/";
    private static readonly string DataPath = MainPath + "Data/";
    private static readonly string MapPath = MainPath + "Map/";
    private static readonly string SpritesPath = MainPath + "Sprites/";
    private readonly Dictionary<string, string> _stringDict = new Dictionary<string, string>();

    /// <summary>
    /// Prevent 'new' keyword.
    /// </summary>
    private ResourceManager() { }

    /// <summary>
    /// Gets the current instance (singleton).
    /// </summary>
    public static ResourceManager Instance {
        get {
            return _instance;
        }
    }

    public string LoadString(string path) {
        if (_stringDict.ContainsKey(path)) {
            return _stringDict[path];
        }

        string resource = _fileLoader.LoadString(ModdingPath + path);
        if (string.IsNullOrEmpty(resource)) {
            resource = _fileLoader.LoadString(MainPath + path);
        }
        _stringDict.Add(path, resource);

        return resource;
    }

    public string[] GetAllDataStrings() {
        string[] allFiles = _fileLoader.GetAllFiles(DataPath, ".json");

        var allData = new string[allFiles.Length];
        for(int i = 0; i < allFiles.Length; i++) {
            allData[i] = _fileLoader.LoadString(DataPath + allFiles);
        }
        return allData;
    }

    public string[] GetAllDataNames() {
        return _fileLoader.GetAllFiles(DataPath, "json");
    }
}