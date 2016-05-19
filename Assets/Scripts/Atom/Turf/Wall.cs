
public class Wall : Turf, IDeconstructible {

    public Wall() {
        blocksGas = true;
    }

    public void Deconstruct(Tile tile) {
        if (!tile.CheckForType<Floor>())
            MapManager.Instance.CreateAtom(tile, "data/floor");
        MapManager.Instance.CreateAtom(tile, "data/metal");
    }
}