using Mono.Nat;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

/// <summary>
/// This class handles all the HLAPI network connections.
/// </summary>
internal sealed class CoreNetworkManager : NetworkManager {

    // List of player IP addresses and character to allow reconnecting with the same character.
    private readonly Dictionary<string, GameObject> _players = new Dictionary<string, GameObject>();

    private MasterController _masterController;

    private void Awake() {
        // Only one NetworkManager should exist at the same time
        if (FindObjectsOfType<CoreNetworkManager>().Length > 1)
            Destroy(gameObject);

        // Register NAT punchthrough events so no port forwarding is needed (not 100% effective)
        NatUtility.DeviceFound += DeviceFound;
        NatUtility.DeviceLost += DeviceLost;
        NatUtility.StartDiscovery();

        // call this at launch so it is cached for future calls and won't calculate this the first time we connect to a server
        // because calculating this the first time can take more than a second
        string s = SystemInfo.deviceUniqueIdentifier;

        //TODO: put this in a setting file
        Application.targetFrameRate = 60;

        connectionConfig.MaxSentMessageQueueSize = 256;
    }

    public override void OnStartServer() {
        base.OnStartServer();
        _masterController = MasterController.Instance;
        _masterController.Initialize();
    }

    public override void OnStopServer() {
        base.OnStopServer();
        Destroy(_masterController.gameObject);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {
        if (extraMessageReader == null)
            return;

        string uniqueId = extraMessageReader.ReadMessage<StringMessage>().value;
        if (string.IsNullOrEmpty(uniqueId)) {
            Debug.LogError("Unique ID is null");
            return;
        }

        // Check if player was already on the server before so they can use their old player controller.
        if (_players.ContainsKey(uniqueId)) {
            GameObject playerObject = _players[uniqueId];
            if (playerObject != null) {
                NetworkServer.AddPlayerForConnection(conn, _players[uniqueId], playerControllerId);

                var message = new EmptyMessage();
                NetworkServer.SendToClient(conn.connectionId, MsgConstants.PlayerSpawnedMessage, message);
                return;
            }
            _players.Remove(uniqueId);
        }

        if (playerPrefab != null) {
            GameObject player;
            Transform startPos = GetStartPosition();
            Tile tile;
            if (startPos != null) {
                player = (GameObject)Instantiate(playerPrefab, startPos.position, startPos.rotation);
                tile = startPos.GetComponent<TileObject>().Tile;
            }
            else {
                //player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                Debug.LogError("Spawnpoint not setup");
                return;
            }
            TileObject playerMob = player.GetComponent<Player>().TileObject;

            string mobData = ResourceManager.Instance.LoadString("data/human.json");
            var mob = JsonUtility.FromJson<Mob>(mobData);
            mob.name = "bob";
            playerMob.Atom = mob;

            playerMob.Tile = tile;
            tile.Add(playerMob.Atom);
            player.transform.SetParent(tile.transform.parent);

            _players.Add(uniqueId, player);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            var message = new EmptyMessage();
            NetworkServer.SendToClient(conn.connectionId, MsgConstants.PlayerSpawnedMessage, message);
        }
    }

    private static void DeviceFound(object sender, DeviceEventArgs args) {
        Debug.Log("NAT punchthrough");
        INatDevice device = args.Device;
        device.CreatePortMap(new Mapping(Protocol.Udp, 1313, 1313));
    }

    private static void DeviceLost(object sender, DeviceEventArgs args) {
        Debug.Log("NAT lost");
        INatDevice device = args.Device;
        device.DeletePortMap(new Mapping(Protocol.Udp, 1313, 1313));
    }

    public static Player GetCurrentPlayer() {
        return singleton.client.connection.playerControllers[0].gameObject.GetComponent<Player>();
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        // Don't delete the player character to allow reconnecting.
    }

    public override void OnServerSceneChanged(string sceneName) {
        base.OnServerSceneChanged(sceneName);
        MapManager.Instance.Initialize();
    }


    // http://forum.unity3d.com/threads/maximum-hosts-cannot-exceed-16.359579/
    public override void OnStopHost() {
        //NetworkTransport.RemoveHost(0);
    }

    public override void OnClientSceneChanged(NetworkConnection conn) {
        // debug setting; make the identifier in editor different from the one in final build even when they are run on the same device
#if UNITY_EDITOR
        var msg = new StringMessage("blblblbl");
#else
        var msg = new StringMessage(SystemInfo.deviceUniqueIdentifier);
#endif
        ClientScene.AddPlayer(conn, 0, msg);
    }
}