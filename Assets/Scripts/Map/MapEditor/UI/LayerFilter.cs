using UnityEngine;
using System.Collections;

/// <summary>
/// This class handles toggling layers on the camera.
/// </summary>
public class LayerFilter : MonoBehaviour {

    [SerializeField]
    private Camera camera;

    public void ToggleLayer(string layer) {
        camera.cullingMask ^= 1 << LayerMask.NameToLayer(layer);
    }
}
