

public class Metal : Item, IPlaceable {

    public bool Place(Tile tile) {
        if (tile.CheckForType<Floor>()) {
            tile.Remove(tile.gas);
            tile.gas = null;
            MapManager.Instance.CreateAtom(tile, "data/wall");
            return true;
        }
        MapManager.Instance.CreateAtom(tile, "data/floor");
        return true;
    }
}
