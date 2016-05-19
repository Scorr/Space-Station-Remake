using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using Utility;

public class TileObject : NetworkBehaviour {

    public Tile Tile { get; set; }

    private Atom _atom;
    public Atom Atom {
        get {
            return _atom;
        }
        set {
            _atom = value;
            SetLayer();
            _atom.TileObject = this;
            AtomJson = JsonUtility.ToJson(_atom);
            _atom.SpriteChanged += AtomChangedSprite;

            if (!isServer) return;
            AtomChangedSprite(_atom.Sprites);
        }
    }

    public readonly List<SpriteRenderer> SpriteRenderers = new List<SpriteRenderer>();
    private Movable _movable;

    [SyncVar(hook = "SyncAtom")]
    public string AtomJson;

    public SyncListString Sprites = new SyncListString();
    
    private Direction _previousDirection = Direction.South; // used for determining what direction we were moving in before

    private void Awake() {
        _movable = GetComponent<Movable>();

        AddSpriteRenderer();

        Sprites.Callback += SyncSprites;
    }

    private void Start() {
        if (isServer) {
            if (Atom != null) {
                AtomChangedSprite(Atom.Sprites);
            }
        }
    }

    public override void OnStartClient() {
        if (isServer) return;

        if (!string.IsNullOrEmpty(AtomJson))
            SyncAtom(AtomJson);

        var gas = _atom as Gas;
        if (gas != null) {
            gas.Value = gas.Value;
        }

        SyncSprites();
    }

    private void AddSpriteRenderer() {
        var newSprite = new GameObject("sprite");
        newSprite.transform.SetParent(transform, false);
        var newSpriteRenderer = newSprite.AddComponent<SpriteRenderer>();
        SpriteRenderers.Add(newSpriteRenderer);
    }

    public void SyncSprites() {
        for (int i = 0; i < Sprites.Count; i++) {
            if (SpriteRenderers.Count - 1 < i) {
                AddSpriteRenderer();
            }

            SpriteRenderers[i].sprite = !string.IsNullOrEmpty(Sprites[i]) ? SpriteRepository.GetSpriteFromSheet(Sprites[i]) : null;

            if (!(_atom is Gas))
                SpriteRenderers[i].gameObject.AddComponent<BoxCollider2D>();
        }

        DetermineDrawOrder(SpriteRenderers, _atom);
    }

    public void SyncSprites(SyncListString.Operation op, int index) {
        if (op == SyncList<string>.Operation.OP_ADD) {
            if (SpriteRenderers.Count - 1 < index) {
                AddSpriteRenderer();
            }

            if (!string.IsNullOrEmpty(Sprites[index]))
            SpriteRenderers[index].sprite = SpriteRepository.GetSpriteFromSheet(Sprites[index]);
            else
                SpriteRenderers[index].sprite = null;

            if (!(_atom is Gas))
                SpriteRenderers[index].gameObject.AddComponent<BoxCollider2D>();

            DetermineDrawOrder(SpriteRenderers, _atom);
        }
    }

    public void SyncAtom(string atomJson) {
        if (isServer) return;

        if (!string.IsNullOrEmpty(atomJson)) {
            JSONObject typeJson = new JSONObject(atomJson).GetField("type");
            if (typeJson != null && !string.IsNullOrEmpty(typeJson.str)) {
                string typeString = typeJson.str;

                Type type = Type.GetType(typeString, true, true);
                if (type != null) {
                    Atom = (Atom)JsonUtility.FromJson(atomJson, type);
                }
            }
            if (Atom == null) {
                Atom = JsonUtility.FromJson<Atom>(atomJson);
            }

            if (!Atom.Rotate(Atom.direction))
                Rotate(Atom.direction);
        }
    }
    
    public void AtomChangedSprite(string[] spriteNames) {
        if (!isServer) return; // TODO: fix MissingReferenceException
        Sprites.Replace(spriteNames);
    }

    public void Rotate(Direction direction) {
        switch (Atom.direction) {
            case Direction.South:
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.North:
                    transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.East:
                    transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Direction.West:
                    transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Check if allowed to move before executing on server to reduce network calls.
    /// </summary>
    /// <param name="direction">The direction to move in.</param>
    public void Move(Direction direction) {
        if (!_movable.moving) {
            CmdMove(direction);
        }
    }

    [Command]
    private void CmdMove(Direction direction) {
#if SERVER
        if (!_movable.moving) {
            // change sprite if possible
            if (!_atom.Rotate(direction)) {
                // TODO: fix bug with quaternion rotation movement
                // rotate quaternion if sprite can't be found
                //Rotate(direction);
            }

            Tile adjacentTile = MapManager.Instance.GetAdjacentTile(Tile, direction, true);
            if (adjacentTile != null && !adjacentTile.Dense) {
                if (MapManager.Instance.CheckInRange(Tile, adjacentTile)) {
                    _movable.Move(adjacentTile, OnMoved);
                }
                else {
                    // TODO: add cooldown
                    // Teleport if the tile is far away.
                    transform.position = adjacentTile.transform.position;
                    RpcSetPosition(adjacentTile.transform.position);
                }

                Tile.Remove(Atom, destroyObject: false);
                Tile = adjacentTile;
                Tile.Add(Atom);

                _previousDirection = direction;
            }
        }
#endif
    }

    private void OnMoved(Tile newTile) {
        if (newTile.Slippery) {
            CmdMove(_previousDirection);
        }
    }

    [ClientRpc]
    private void RpcSetPosition(Vector2 position) {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private void SetLayer() {
        if (_atom is Item || _atom is Machinery) {
            gameObject.layer = LayerMask.NameToLayer("Obj");
        }
        else if (_atom is Mob) {
            gameObject.layer = LayerMask.NameToLayer("Mob");
        }
        else if (_atom is Turf) {
            gameObject.layer = LayerMask.NameToLayer("Turf");
        }
        else {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    private static void DetermineDrawOrder(List<SpriteRenderer> renderers, Atom atom) {
        int baseOrder = 0;
        if (atom is Mob) {
            baseOrder = 1;
        }

        for (int i = 0; i < renderers.Count; i++) {
            renderers[i].sortingOrder = baseOrder + i;
        }
    }

    private void OnDestroy() {
        if (_atom == null) return;
        _atom.SpriteChanged -= AtomChangedSprite;
        _atom.TileObject = null;
        _atom = null;
    }
}