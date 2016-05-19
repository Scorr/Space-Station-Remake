using UnityEngine;
using System.Collections;

/// <summary>
/// Script that handles making a camera pixel-perfect.
/// </summary>
[RequireComponent(typeof(Camera))]
internal class OrthographicCamera : MonoBehaviour {

    private readonly int minZoom = -1;
    private readonly int maxZoom = 2;
	private int zoomLevel = 1;
    private new Camera camera;

    void Awake() {
        camera = GetComponent<Camera>();
    }

	void Start() {
        camera.orthographicSize = (Screen.height * 0.5f) * 0.01f;
        if (camera.orthographicSize > 3)
            camera.orthographicSize -= (Screen.height * 0.5f) * 0.01f * 0.5f;
	}

	void Update() {
		Zoom();
	}

	private void Zoom() {
        // Check if hovering over an UI element.
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && zoomLevel < maxZoom) { // zoom in
                float newSize = camera.orthographicSize - (Screen.height * 0.5f) * 0.01f * 0.5f;
                if (newSize != 0) {
                    camera.orthographicSize = newSize;
                    zoomLevel++;
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0 && zoomLevel > minZoom) { //zoom out
                camera.orthographicSize += (Screen.height * 0.5f) * 0.01f * 0.5f;
                zoomLevel--;
            }
        }
	}
}
