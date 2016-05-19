using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Controller;
using Map.MapEditor;

public class Tile : MonoBehaviour {

    public int x;
    public int y;
    public int z;
    public bool Dense { get { return Contents.Exists(c => c.dense); } }
    public bool BlocksGas { get { return turf != null && turf.blocksGas; } }
    public bool Slippery {
        get {
            Turf floor = turf;
            return floor != null && floor.slippery;
        }
    }
    
    [NonSerialized]
    public List<Atom> Contents = new List<Atom>();
    [NonSerialized]
    public Turf turf;
    [NonSerialized]
    public Gas gas;

    public Tile[,] parent;

    // TODO: fix this hacky mapeditor shit
    private static bool _loadGas = true;

    private void Awake() {
        // TODO: more mapeditor shit that causes slowdown on startup
        if (_loadGas && FindObjectOfType<MapEditor>() != null)
            _loadGas = false;
    }

    public void SetPosition(int posX, int posY) {
        x = posX;
        y = posY;
    }

    public void Add(Atom atom) {
        TileObject atomTileObject = atom.TileObject;
        atomTileObject.transform.position = new Vector3(atomTileObject.transform.position.x, atomTileObject.transform.position.y, -0.01f * (Contents.Count + 1));

        // solved this way to not break existing systems
        // should solve more elegantly later
        var newTurf = atom as Turf;
        if (newTurf != null) {
            if (turf != null) {
                Contents.Remove(turf);
                if (turf.TileObject != null)
                    Destroy(turf.TileObject.gameObject);
            }
            turf = newTurf;
            Contents.Add(newTurf);

            if (atom is Floor) {
                TileObject tileObject = atom.TileObject;
                tileObject.transform.position = new Vector3(tileObject.transform.position.x, tileObject.transform.position.y, -0.01f);

                if (tileObject.isClient && !tileObject.isServer || !_loadGas)
                    return;
                // add gas
                gas = (Gas)MapManager.Instance.CreateAtom(this, "data/gas").Atom;

                if (x == 9 && y == 10)
                    gas.Value = 10000f;

                // set the sprite to the start value
                // otherwise sprite will be visible at start even when value = 0
                gas.Value = gas.Value;
                MasterController.Instance.AtmosController.AddGas(gas);
            }

            if (turf.blocksGas && gas != null) {
                Contents.Remove(gas);
                Destroy(gas.TileObject.gameObject);
            }
            return;
        }
        if (Contents.Exists(c => c.name == atom.name)) {
            TileObject found = Contents.Find(c => c.name == atom.name).TileObject;
            var item = found.Atom as Item;
            if (item != null) {
                if (item.currentStack < item.maxStack) {
                    item.currentStack++;
                    Destroy(atom.TileObject.gameObject);
                    return;
                }
            }
        }

        Contents.Add(atom);
    }

    public bool CheckForType<T>() where T : Atom {
        return Contents.Any(c => c is T);
    }

    public void Remove(Atom atom, bool entireStack = true, bool destroyObject = true) {
        if (!entireStack && atom is Item) {
            Atom found = Contents.Find(c => c.name == atom.name);
            var item = (Item)found;
            if (item.currentStack > 0) {
                item.currentStack--;

                if (item.currentStack <= 0) {
                    Contents.Remove(found);
                    if (destroyObject) {
                        Destroy(found.TileObject.gameObject);
                    }
                }
            }
        }
        else {
            Contents.Remove(atom);
            if (destroyObject)
                Destroy(atom.TileObject.gameObject);
        }
    }
}