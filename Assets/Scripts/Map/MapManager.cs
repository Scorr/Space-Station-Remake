using UnityEngine;
using System.Collections.Generic;
using System;

public enum Direction { North, East, South, West }

[RequireComponent(typeof(MapLoader))]
internal class MapManager : Singleton<MapManager> {

    public static readonly float TileSize = 0.32f;
    // Allow multiple levels.
    private readonly List<Tile[,]> _tileMapList = new List<Tile[,]>();
    private MapLoader _mapLoader;

    public GameObject Ship;

    private void Awake() {
        _mapLoader = gameObject.GetComponent<MapLoader>();
    }

    public void Initialize() {
        _tileMapList.Add(_mapLoader.LoadMap("station"));
        
        _tileMapList.Add(_mapLoader.LoadMap("ship", Ship.transform));
    }
    
    public Tile GetAdjacentTile(Tile currentTile, Direction direction, bool wrapping = false) {
        Tile[,] tiles = currentTile.parent;
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        Tile adjacentTile = null;
        switch (direction) {
            case Direction.North:
                if (currentTile.y + 1 < height)
                    adjacentTile = tiles[currentTile.x, currentTile.y + 1];
                else if (wrapping)
                    adjacentTile = tiles[currentTile.x, 0];
                break;
            case Direction.East:
                if (currentTile.x + 1 < width)
                    adjacentTile = tiles[currentTile.x + 1, currentTile.y];
                else if (wrapping)
                    adjacentTile = tiles[0, currentTile.y];
                break;
            case Direction.South:
                if (currentTile.y - 1 >= 0)
                    adjacentTile = tiles[currentTile.x, currentTile.y - 1];
                else if (wrapping)
                    adjacentTile = tiles[currentTile.x, tiles.GetLength(1) - 1];
                break;
            case Direction.West:
                if (currentTile.x - 1 >= 0)
                    adjacentTile = tiles[currentTile.x - 1, currentTile.y];
                else if (wrapping)
                    adjacentTile = tiles[tiles.GetLength(0) - 1, currentTile.y];
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, null);
        }

        return adjacentTile;
    }

    /// <summary>
    /// Finds what map a tile belongs to.
    /// </summary>
    /// <param name="tile">The tile to search.</param>
    /// <returns>The map the tile belongs to.</returns>
    private Tile[,] FindMap(Tile tile) {
        foreach (Tile[,] array in _tileMapList) {
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++) {
                    if (array[i, j] == tile) {
                        return array;
                    }
                }
        }
        return null;
    }

    /// <summary>
    /// Checks if tiles are in range but also accounts for density when dealing with diagonal tiles.
    /// </summary>
    public bool CheckAdjacentDense(Tile src, Tile dest) {
        // check if within 1.5 range
        if (!CheckInRange(src, dest)) return false;

        // check diagonal
        // todo: make this cleaner
        if (dest.x - src.x == 1 && dest.y - src.y == 1) {
            if (src.parent[src.x + 1, src.y].Dense && src.parent[src.x, src.y + 1].Dense)
                return false;
        }
        else if (dest.x - src.x == 1 && dest.y - src.y == -1) {
            if (src.parent[src.x + 1, src.y].Dense && src.parent[src.x, src.y - 1].Dense)
                return false;
        }
        else if (dest.x - src.x == -1 && dest.y - src.y == 1) {
            if (src.parent[src.x - 1, src.y].Dense && src.parent[src.x, src.y + 1].Dense)
                return false;
        }
        else if (dest.x - src.x == -1 && dest.y - src.y == -1) {
            if (src.parent[src.x - 1, src.y].Dense && src.parent[src.x, src.y - 1].Dense)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if tiles are in range but also accounts for blocksGas when dealing with diagonal tiles.
    /// </summary>
    public bool CheckAdjacentGas(Tile src, Tile dest) {
        // check if within 1.5 range
        if (!CheckInRange(src, dest)) return false;

        // check diagonal
        // todo: make this cleaner
        if (dest.x - src.x == 1 && dest.y - src.y == 1) {
            if (src.parent[src.x + 1, src.y].BlocksGas && src.parent[src.x, src.y + 1].Dense)
                return false;
        }
        else if (dest.x - src.x == 1 && dest.y - src.y == -1) {
            if (src.parent[src.x + 1, src.y].BlocksGas && src.parent[src.x, src.y - 1].Dense)
                return false;
        }
        else if (dest.x - src.x == -1 && dest.y - src.y == 1) {
            if (src.parent[src.x - 1, src.y].BlocksGas && src.parent[src.x, src.y + 1].Dense)
                return false;
        }
        else if (dest.x - src.x == -1 && dest.y - src.y == -1) {
            if (src.parent[src.x - 1, src.y].BlocksGas && src.parent[src.x, src.y - 1].Dense)
                return false;
        }

        return true;
    }

    public float GetDistance(Tile src, Tile dest) {
        return Mathf.Sqrt(Mathf.Pow(src.x - dest.x, 2) + Mathf.Pow(src.y - dest.y, 2));
    }

    /// <summary>
    /// Checks if the destination tile is in range.
    /// </summary>
    public bool CheckInRange(Tile src, Tile dest, float range = 1.5f) {
        return GetDistance(src, dest) <= range;
    }

    public TileObject CreateAtom(Tile tile, string datapath) {
        return _mapLoader.CreateObjectFromPath(tile, datapath).GetComponent<TileObject>();
    }

    public TileObject CreateAtomFromJson(Tile tile, string dataJson) {
        return _mapLoader.CreateObjectFromJson(tile, dataJson).GetComponent<TileObject>();
    }

    /// <summary>
    /// Gets a list of all tiles surrounding a given tile. Includes diagonal tiles.
    /// </summary>
    public List<Tile> GetSurroundingTiles(Tile startingTile, int range, bool includeCenter = false, bool includeDiagonals = false) {
        Tile[,] tiles = startingTile.parent;

        var tileList = new List<Tile>();
        for (int x = startingTile.x - range; x <= startingTile.x + range; x++) {
            for (int y = startingTile.y - range; y <= startingTile.y + range; y++) {
                // ignore diagonal tiles if includeDiagonals is false
                // todo: make this cleaner
                if (includeDiagonals || !(x == startingTile.x - range && y == startingTile.y - range || x == startingTile.x - range && y == startingTile.y + range || x == startingTile.x + range && y == startingTile.y - range || x == startingTile.x + range && y == startingTile.y + range)) {
                    // ignore center tile if includeCenter is false
                    if (includeCenter || !(x == startingTile.x && y == startingTile.y)) {
                        // outofbounds check
                        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1)) {
                            tileList.Add(tiles[x, y]);
                        }
                    }
                }
            }
        }
        return tileList;
    }

    public Direction GetDirection(Tile src, Tile dest) {
        if (src.x == dest.x && src.y == dest.y)
            return Direction.South;
        if (dest.x - src.x < 0 && Mathf.Abs(dest.x - src.x) > Mathf.Abs(dest.y - src.y))
            return Direction.West;
        if (dest.x - src.x > 0 && Mathf.Abs(dest.x - src.x) > Mathf.Abs(dest.y - src.y))
            return Direction.East;
        if (dest.y - src.y < 0)
            return Direction.South;
        return Direction.North;
    }
}
