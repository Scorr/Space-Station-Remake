using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Controller {
    internal class Player : NetworkBehaviour {

        public GameObject Camera;
        private GameObject _oldCamera;

        public TileObject TileObject { get; set; }
        public Mob Character { get { return (Mob)TileObject.Atom; } }
        private Chat _chat;
        private bool _isGhost;

        public event Action ToggleHands;

        public string Name {
            get { return TileObject.Atom.name; }
        }

        public GameObject TileObjectPrefab;

        private bool _attackCooldown;

        private void Awake() {
            _chat = MasterController.Instance.Chat;
            TileObject = GetComponent<TileObject>();
        }

        public override void OnStartLocalPlayer() {
            _oldCamera = UnityEngine.Camera.main.gameObject;
            _oldCamera.SetActive(false);
            Camera.SetActive(true);

            if (!isServer)
                CmdSynchronizeInventory();
        }

        public override void OnStartServer() {
            Character.OnDeath += CmdOnDeath;
            Character.OnDeath += MasterController.Instance.RoundController.PlayerDeath;
        }

        [Command]
        public void CmdOnDeath() {
#if SERVER
            _isGhost = true;
            //tileObject.atom = JsonUtility.FromJson<Mob>(Resources.Load<TextAsset>("data/skeleton").text);
            //tileObject.SyncSprite(tileObject.atom.Sprite);
            RpcOnDeath();
#endif
        }

        [ClientRpc]
        public void RpcOnDeath() {
            _isGhost = true;
        }

        private void Update() {
            if (!isLocalPlayer) {
                return;
            }

            if (_isGhost) {
                float xAxisValue = Input.GetAxis("Horizontal");
                float yAxisValue = Input.GetAxis("Vertical");
                Camera.transform.Translate(new Vector3(xAxisValue * 0.05f, yAxisValue * 0.05f, 0.0f));
            }
            else {

                if (Input.GetKey("right")) {
                    TileObject.Move(Direction.East);
                }
                if (Input.GetKey("left")) {
                    TileObject.Move(Direction.West);
                }
                if (Input.GetKey("up")) {
                    TileObject.Move(Direction.North);
                }
                if (Input.GetKey("down")) {
                    TileObject.Move(Direction.South);
                }

                if (Input.GetKeyDown("space")) {
                    _chat.PostChatMessage("allahu akbar");
                    CmdExplode();
                }

                if (Input.GetKeyDown(KeyCode.Z)) {
                    CmdDropItem();
                }
                
                if (Input.GetMouseButtonDown(0)) {
                    OnLeftClick();
                }

                if (Input.GetMouseButtonDown(1)) {
                    OnRightClick();
                }
                if (Input.GetMouseButtonDown(2)) {
                    if (ToggleHands != null)
                        ToggleHands.Invoke();
                }
            }
        }

        [Command]
        public void CmdExplode() {
#if SERVER
            new Explosion(GetTile());
#endif
        }

        public Tile GetTile() {
            return TileObject.Tile;
        }

        private void OnLeftClick() {
            Vector2 worldPoint = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);

            GameObject spriteObject = RayCastSprites(worldPoint);
            if (spriteObject != null)
                CmdUse(spriteObject);
        }

        private GameObject RayCastSprites(Vector2 worldPoint) {
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero, 10f);
            // order hits by distance because RaycastAll doesn't guarantee order
            //hits.OrderBy(h => h.distance);

            for (int i = 0; i < hits.Length; i++) {
                RaycastHit2D hit = hits[i];
                var spriteRenderer = hit.collider.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null) {
                    // get the raycast hit position inside the sprite
                    Vector2 localPos = hit.point - ((Vector2) hit.transform.position + new Vector2(-0.16f, -0.16f));

                    Texture2D spritesheet = spriteRenderer.sprite.texture;
                    int rows = spritesheet.width/32;
                    int columns = spritesheet.height/32;
                    int index = SpriteRepository.GetIndexFromSprite(spriteRenderer.sprite.name);

                    // Unity puts the origin of a sprite atlas at the bottom left, so we use this formula to get the required sprite:
                    // x = index % rows * tilesize
                    // y = (max - tilesize) - round(index / columns) * tilesize
                    if (
                        spritesheet.GetPixel((index%rows*32) + (int) (localPos.x*100f),
                            (spritesheet.height - 32) - ((index/columns)*32) + (int) (localPos.y*100f)).a != 0) {
                        // if the pixel is not transparent, it is counted as a succesful hit
                        return hit.collider.transform.parent.gameObject;
                    }
                }
            }

            return null;
        }

        [Command]
        private void CmdUse(GameObject gameObject) {
#if SERVER
            var tileObject = gameObject.GetComponent<TileObject>();

            // rotate towards the destination
            Character.Rotate(MapManager.Instance.GetDirection(GetTile(), tileObject.Tile));

            if (Character.inventory.ActiveHand is IPlaceable && tileObject.Atom is Turf && !tileObject.Tile.Dense) {
                var floor = gameObject.GetComponent<TileObject>();
                if (floor != null) {
                    PlaceItem(floor.gameObject);
                }
            }
            else if (tileObject.Atom is Item) {
                PickUp(gameObject);
            }
            else if (tileObject.Atom is IDeconstructible) {
                Deconstruct(gameObject);
            }
            else if (tileObject.Atom is Mob) {
                Attack(gameObject);
            }
            else if (tileObject.Atom is IInteractive) {
                Interact(gameObject);
            }
#endif
        }

        private void Interact(GameObject target) {
#if SERVER
            var targetTileObject = target.GetComponent<TileObject>();
            if (MapManager.Instance.CheckAdjacentDense(GetTile(), targetTileObject.Tile)) {
                var interactive = (IInteractive)targetTileObject.Atom;
                interactive.Interact();
            }
#endif
        }

        private void Attack(GameObject target) {
#if SERVER
            if (_attackCooldown)
                return;

            var targetTileObject = target.GetComponent<TileObject>();
            if (MapManager.Instance.CheckAdjacentDense(GetTile(), targetTileObject.Tile)) {
                var mob = (Mob)targetTileObject.Atom;
            
                int damage = 20;
                string attackVerb = "punches";
                string itemName = "fist";
                if (Character.inventory.ActiveHand != null) {
                    damage = Character.inventory.ActiveHand.damage;
                    attackVerb = Character.inventory.ActiveHand.attackVerb;
                    itemName = Character.inventory.ActiveHand.name;
                }
                mob.TakeDamage(damage);
            
                string attackMessage = Character.name + " " + attackVerb + " " + mob.name + " with their " + itemName + " for " + damage + " damage!";
                _chat.PostServerMessage(attackMessage);
                RpcPlaySound("sound/punch1");
                StopAllCoroutines();
                StartCoroutine(AttackCooldown(1.0f));
            }
#endif
        }

        [ClientRpc]
        public void RpcPlaySound(string path) {
            SoundManager.Instance.PlaySound(path);
        }
    
        private void PlaceItem(GameObject floor) {
#if SERVER
            var floorTileObject = floor.GetComponent<TileObject>();
            if (MapManager.Instance.CheckAdjacentDense(GetTile(), floorTileObject.Tile)) {
                var placeable = (IPlaceable)Character.inventory.ActiveHand;
                if (placeable.Place(floorTileObject.Tile)) {
                    RpcRemoveItemFromHand();
                    Character.inventory.RemoveActiveItem();
                }
            }
#endif
        }
    
        private void Deconstruct(GameObject deconstructiblePrefab) {
#if SERVER
            var deconstructibleTileObject = deconstructiblePrefab.GetComponent<TileObject>();
            if (MapManager.Instance.CheckAdjacentDense(GetTile(), deconstructibleTileObject.Tile)) {
                var deconstructible = (IDeconstructible)deconstructibleTileObject.Atom;

                deconstructibleTileObject.Tile.Remove(deconstructibleTileObject.Atom);
                deconstructible.Deconstruct(deconstructibleTileObject.Tile);
            }
#endif
        }

        [Command]
        private void CmdDropItem() {
#if SERVER
            Tile currentTile = GetTile();
            MapManager.Instance.CreateAtomFromJson(currentTile, JsonUtility.ToJson(Character.inventory.ActiveHand));

            RpcRemoveItemFromHand();
            Character.inventory.RemoveActiveItem();
#endif
        }

        [ClientRpc]
        private void RpcRemoveItemFromHand() {
            if (!isServer)
                Character.inventory.RemoveActiveItem();
        }
    
        private void PickUp(GameObject itemPrefab) {
#if SERVER
            var tileObject = itemPrefab.GetComponent<TileObject>();
            var item = tileObject.Atom as Item;
            if (item == null) return;

            if (MapManager.Instance.CheckAdjacentDense(GetTile(), tileObject.Tile)) {
                if (Character.inventory.Add(item)) {
                    RpcPickup(JsonUtility.ToJson(item));

                    tileObject.Tile.Remove(tileObject.Atom);
                }
            }
#endif
        }

        [ClientRpc]
        private void RpcPickup(string itemJson) {
            if (!isServer) {
                Character.inventory.Add(JsonUtility.FromJson<Metal>(itemJson));
            }
        }
    
        private void OnRightClick() {
            Vector2 worldPoint = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);

            GameObject spriteObject = RayCastSprites(worldPoint);
            if (spriteObject != null) {
                var examinable = spriteObject.GetComponent<TileObject>();
                if (examinable.Atom != null && !string.IsNullOrEmpty(examinable.Atom.name)) {
                    var item = examinable.Atom as Item;
                    if (item != null) {
                        _chat.AddMessage(item.name + ". " + item.description + ". Stack: " + item.currentStack);
                    }
                    else {
                        _chat.AddMessage(examinable.Atom.name + ". " + examinable.Atom.description);
                    }
                }
            }
        }

        [Command]
        private void CmdSynchronizeInventory() {
#if SERVER
            Item leftItem = Character.inventory.LeftHand;
            if (leftItem != null) {
                RpcAddItem(JsonUtility.ToJson(leftItem), 0);
            }
            Item rightItem = Character.inventory.RightHand;
            if (rightItem != null) {
                RpcAddItem(JsonUtility.ToJson(rightItem), 1);
            }
#endif
        }

        [ClientRpc]
        private void RpcAddItem(string itemJson, byte slot) {
            switch (slot) {
                case 0:
                    Character.inventory.LeftHand = JsonUtility.FromJson<Item>(itemJson);
                    break;
                case 1:
                    Character.inventory.RightHand = JsonUtility.FromJson<Item>(itemJson);
                    break;
                default:
                    return;
            }
        }

        private void OnDestroy() {
            if (_oldCamera != null)
                _oldCamera.SetActive(true);
        }

        [Command]
        public void CmdSetActiveHand(bool leftHand) {
#if SERVER
            Inventory.Hand hand = leftHand ? Inventory.Hand.Left : Inventory.Hand.Right;
            Character.inventory.SetActiveHand(hand);
#endif
        }

        private IEnumerator AttackCooldown(float countdown) {
#if SERVER
            // might be worth looking into client-side cooldown to save server bandwidth
            _attackCooldown = true;

            while (_attackCooldown) {
                countdown -= Time.deltaTime;

                yield return new WaitForFixedUpdate();

                if (countdown <= 0) {
                    _attackCooldown = false;
                }
            }
        }
#endif
    }
}
