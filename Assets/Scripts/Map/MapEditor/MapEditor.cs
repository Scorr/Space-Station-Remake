using System;
using UnityEngine;

namespace Map.MapEditor {

    /// <summary>
    /// This class is the controller for map creation.
    /// </summary>
    internal class MapEditor : MonoBehaviour {

        public const int Width = 15;
        public const int Height = 15;
        public Sprite gridSprite;
        private MapLoader mapLoader;
        private Tile[,] tiles;
        public SelectionWindow selectionWindow;

        public event Action<Item> OnItemAdded;

        private void Awake() {
            mapLoader = GetComponent<MapLoader>();
        }

        private void Start() {
            tiles = mapLoader.CreateTiles(Width, Height);
            for (int y = 0; y < tiles.GetLength(0); y++) {
                for (int x = 0; x < tiles.GetLength(1); x++) {
                    Tile tile = tiles[x, y];
                    SpriteRenderer sprite = tile.gameObject.AddComponent<SpriteRenderer>();
                    sprite.sprite = gridSprite;
                    sprite.sortingOrder = 10;
                }
            }
        }

        private void Update() {
            float xAxisValue = Input.GetAxis("Horizontal");
            float yAxisValue = Input.GetAxis("Vertical");
            if (Camera.main != null) {
                Camera.main.transform.Translate(new Vector3(xAxisValue * 0.05f, yAxisValue * 0.05f, 0.0f));
            }

            // Check if hovering over an UI element first.
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0)) {
                OnLeftClick();
            }

            if (Input.GetMouseButtonDown(1)) {
                OnRightClick();
            }
        }

        private void OnLeftClick() {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 10f);
            
            if (hit.collider == null || selectionWindow.CurrentSelection == null) return;

            var tile = hit.collider.GetComponent<Tile>();
            if (tile == null) {
                // If we didn't hit a tile we must've hit a TileObject.
                var tileObject = hit.collider.GetComponent<TileObject>();
                if (tileObject != null) {
                    tile = tileObject.Tile;
                }
            }
                
            if (tile != null) {
                // Have tile still be null here because we need to check if the TileObject is valid to place.
                var newTileObject = mapLoader.CreateObjectFromPath(null, "data/" + selectionWindow.CurrentSelection).GetComponent<TileObject>();
                Atom atom = newTileObject.Atom;

                if (atom is Turf && tile.CheckForType<Turf>()) {
                    Destroy(newTileObject.gameObject);
                    return;
                }

                var item = atom as Item;
                if (item != null) {
                    if (OnItemAdded != null)
                        OnItemAdded.Invoke(item);
                }

                // Tile is valid so set the values that would've been set if tile wasn't null when creating from MapLoader (kind of a dirty hack).
                newTileObject.Tile = tile;
                newTileObject.transform.SetParent(tile.transform, false);
                tile.Add(newTileObject.Atom);
            }
        }

        private void OnRightClick() {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 10f);

            if (hit.collider != null) {
                var tileObject = hit.collider.transform.parent.GetComponent<TileObject>();
                if (tileObject != null) {
                    tileObject.Tile.Remove(tileObject.Atom, false);
                }
            }
        }

        public void SaveMap() {
            mapLoader.SaveMap(tiles);
        }

        public void LoadMap() {
            foreach (Tile tile in tiles) {
                Destroy(tile.gameObject);
            }
            tiles = mapLoader.LoadMap("station");
            
            for (int y = 0; y < tiles.GetLength(1); y++) {
                for (int x = 0; x < tiles.GetLength(0); x++) {
                    Tile tile = tiles[x, y];
                    var sprite = tile.gameObject.AddComponent<SpriteRenderer>();
                    sprite.sprite = gridSprite;
                    sprite.sortingOrder = 10;
                }
            }
        }
    }
}