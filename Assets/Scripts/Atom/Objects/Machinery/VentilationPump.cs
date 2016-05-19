using Controller;
using UnityEngine;

public class VentilationPump : Machinery, IUpdate {

    public float pressureLimit = 100f; // Highest pressure for this vent to pump to.

    public VentilationPump() {
        MasterController.Instance.AddUpdate(this);
    }

    private void Pump() {
        if (TileObject.Tile.gas.Value < 100f) {
            TileObject.Tile.gas.Value += 10f;
            Mathf.Clamp(TileObject.Tile.gas.Value, 0f, pressureLimit);
        }
    }

    public void Update() {
        if (TileObject == null) { // Dispose
            MasterController.Instance.RemoveUpdate(this);
            return;
        }
        Pump();
    }
}