using System.Collections.Generic;
using Controller;

public class Explosion {
    
    private const int Range = 2;

    public Explosion(Tile epicenter) {
        List<Tile> affectedTiles = MapManager.Instance.GetSurroundingTiles(epicenter, Range, true);

        foreach (Tile tile in affectedTiles) {
            var affectedObjects = new List<Atom>(tile.Contents);
            foreach (Atom atom in affectedObjects) {
                var mob = atom as Mob;
                if (mob != null) {
                    mob.TakeDamage(100);

                    var player = mob.TileObject.GetComponent<Player>();
                    if (player != null) {
                        tile.Remove(mob, destroyObject: false);
                        continue;
                    }
                }

                tile.Remove(atom);
            }
        }
    }
}