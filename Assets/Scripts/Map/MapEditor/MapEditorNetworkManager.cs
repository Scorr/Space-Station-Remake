using UnityEngine.Networking;

namespace Map.MapEditor {

    /// <summary>
    /// Custom network manager because MapLoader depends on network, but we don't want to be online.
    /// </summary>
    public class MapEditorNetworkManager : NetworkManager {

        void Start() {
            maxConnections = 0;
            StartHost();
        }
    }
}