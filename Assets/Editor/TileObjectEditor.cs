using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileObject))]
public class TileObjectEditor : Editor {

    private void OnSceneGUI() {
        var _target = (TileObject)target;

        if (_target.Tile == null || _target.Tile.gas == null) return;
        Gas gas = _target.Tile.gas;
        if (gas != null) {
            Handles.color = Color.cyan;
            Handles.Label(_target.transform.position, gas.Value.ToString());
        }
    }
}
