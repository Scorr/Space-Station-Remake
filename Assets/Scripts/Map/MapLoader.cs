using System;
using UnityEngine;
using UnityEngine.Networking;
using Utility;

internal class MapLoader : MonoBehaviour {

    public GameObject TilePrefab;
    public GameObject TileObjectPrefab;

    private GameObject _spawnPoint;

    public Tile[,] LoadMap(string fileName, Transform parent = null) {
        return LoadMapFromJson(ResourceManager.Instance.LoadString("map/" + fileName + ".json"), parent);
    }

    private TileObject CreateTileObject(Tile tile) {
        if (TileObjectPrefab == null)
            Debug.Log("null");
        GameObject tileObject = Instantiate(TileObjectPrefab);
        if (tile != null)
            tileObject.transform.SetParent(tile.transform, false);

        return tileObject.GetComponent<TileObject>();
    }

    public GameObject CreateObjectFromPath(Tile tile, string dataPath) {
        string dataJson = ResourceManager.Instance.LoadString(dataPath + ".json");
        return CreateObjectFromJson(tile, dataJson);
    }

    public GameObject CreateObjectFromJson(Tile tile, string dataJson) {
        TileObject tileObject = CreateTileObject(tile);

        Atom atom = null;
        if (!string.IsNullOrEmpty(dataJson)) {
            JSONObject typeJson = new JSONObject(dataJson).GetField("type");
            if (typeJson != null && !string.IsNullOrEmpty(typeJson.str)) {
                string typeString = typeJson.str;

                Type type = Type.GetType(typeString, true, true);
                if (type != null) {
                    atom = (Atom)JsonUtility.FromJson(dataJson, type);
                }
            }
            if (atom == null)
                atom = JsonUtility.FromJson<Atom>(dataJson);
            
            tileObject.Atom = atom;
            //tileObject.Sprites.Replace(tileObject.Atom.Sprites);
            tileObject.Tile = tile;

            if (tile != null)
                tile.Add(tileObject.Atom);

            NetworkServer.Spawn(tileObject.gameObject);
        }

        return tileObject.gameObject;
    }

    private Tile[,] LoadMapFromJson(string dataJson, Transform parent = null) {
        if (parent == null)
            parent = transform;

        var json = new JSONObject(dataJson);
        int width;
        json[0].GetField(out width, "width", 0);
        int height;
        json[0].GetField(out height, "height", 0);

        var tiles = new Tile[width, height];

        foreach (JSONObject tileJson in json[1].list) {
            int x;
            tileJson.GetField(out x, "x", 0);
            int y;
            tileJson.GetField(out y, "y", 0);
            int z;
            tileJson.GetField(out z, "z", 0);

            GameObject tileInstance = Instantiate(TilePrefab);

            tileInstance.name = x + ", " + y;
            tileInstance.transform.position = new Vector3(x * MapManager.TileSize, y * MapManager.TileSize);
            var tile = tileInstance.GetComponent<Tile>();
            tile.x = x;
            tile.y = y;
            tiles[x, y] = tile;
            tile.parent = tiles;

            tileInstance.transform.SetParent(parent, false);

            if (x == Mathf.RoundToInt(width * 0.5f) && y == Mathf.RoundToInt(height * 0.5f) && _spawnPoint == null) {
                _spawnPoint = new GameObject("SpawnPoint");
                _spawnPoint.transform.SetParent(tile.transform, false);
                _spawnPoint.AddComponent<NetworkStartPosition>();
                var spawn = _spawnPoint.AddComponent<TileObject>();
                spawn.Tile = tile;
            }

            JSONObject contentsJson = tileJson.GetField("contents");
            if (contentsJson.list != null) {
                foreach (JSONObject content in contentsJson.list) {
                    if (!string.IsNullOrEmpty(content.str))
                        CreateObjectFromPath(tile, "data/" + content.str);
                }
            }
        }

        // TODO: solve this more elegantly
        // We place space tiles for the tiles that are still empty,
        // or when they have not been created at all.
        // Can occur when the width/height specified is higher than the actual tiles in the json.
        for (int y = 0; y < width; y++) {
            for (int x = 0; x < height; x++) {
                if (tiles[x, y] == null) {
                    GameObject tileInstance = Instantiate(TilePrefab);
                    tileInstance.transform.SetParent(transform);

                    tileInstance.name = x + ", " + y;
                    tileInstance.transform.position = new Vector3(x * MapManager.TileSize, y * MapManager.TileSize);
                    var tile = tileInstance.GetComponent<Tile>();
                    tile.x = x;
                    tile.y = y;
                    tile.parent = tiles;
                    tiles[x, y] = tile;
                }

                if (tiles[x,y].Contents.Count == 0) {
                    Tile tile = tiles[x, y];
                    TileObject tileObject = CreateTileObject(tile);
                    tileObject.Atom = new Space();
                    //tileObject.Sprites.Replace(tileObject.Atom.Sprites);
                    tileObject.Tile = tile;
                    tile.Add(tileObject.Atom);
                    NetworkServer.Spawn(tileObject.gameObject);
                }
            }
        }
        return tiles;
    }

    public void SaveMap(Tile[,] tiles) {
        var giantJson = new JSONObject();
        var sizeJson = new JSONObject();
        sizeJson.AddField("width", tiles.GetLength(0));
        sizeJson.AddField("height", tiles.GetLength(1));
        giantJson.AddField("size", sizeJson);

        var tilesJson = new JSONObject();
        foreach (Tile tile in tiles) {

            var tileJson = new JSONObject(JsonUtility.ToJson(tile));
            var contents = new JSONObject();
            foreach (Atom atom in tile.Contents) {
                var atomJson = new JSONObject(JsonUtility.ToJson(atom));

                string contentName;
                atomJson.GetField(out contentName, "name", "");
                contents.Add(contentName);
            }
            tileJson.AddField("contents", contents);

            tilesJson.Add(tileJson);
        }
        giantJson.AddField("tiles", tilesJson);
        
        var loader = new FileLoader(Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Map" + System.IO.Path.DirectorySeparatorChar);
        loader.Save(giantJson.Print(), "newmap.json");
    }

    public Tile[,] CreateTiles(int width, int height) {
        var tiles = new Tile[width, height];
        for (int y = 0; y < width; y++) {
            for (int x = 0; x < height; x++) {
                GameObject tileInstance = Instantiate(TilePrefab);
                tileInstance.transform.SetParent(transform);

                tileInstance.name = x + ", " + y;
                tileInstance.transform.position = new Vector3(x * MapManager.TileSize, y * MapManager.TileSize);
                var tile = tileInstance.GetComponent<Tile>();
                tile.x = x;
                tile.y = y;
                tile.parent = tiles;
                tiles[x, y] = tile;

                if (x == Mathf.RoundToInt(width * 0.5f) && y == Mathf.RoundToInt(height * 0.5f)) {
                    Camera.main.transform.position = new Vector3(tileInstance.transform.position.x, tileInstance.transform.position.y, Camera.main.transform.position.z);
                }
            }
        }

        return tiles;
    }
}