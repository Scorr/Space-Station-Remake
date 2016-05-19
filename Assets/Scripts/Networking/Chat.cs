using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

internal class Chat : NetworkBehaviour {
    NetworkClient _client;

    [SerializeField]
    private Text _chatline;
    [SerializeField]
    private SyncListString _chatLog = new SyncListString();
    [SerializeField]
    private InputField _input;

    public override void OnStartClient() {
        _chatLog.Callback = OnChatUpdated;
    }

    public void Start() {
        _client = NetworkManager.singleton.client;
        NetworkServer.RegisterHandler(MsgConstants.ChatMsg, OnServerPostChatMessage);
        _input.onEndEdit.AddListener(delegate { PostChatMessage(_input.text); });
    }

    [Client]
    public void PostChatMessage(string message) {
        if (message.Length == 0) return;
        var msg = new StringMessage(NetworkManager.singleton.client.connection.connectionId + ": " + message);
        _client.Send(MsgConstants.ChatMsg, msg);

        _input.text = "";
    }
    
    public void PostServerMessage(string message) {
#if SERVER
        if (!isServer)
            return;

        if (message.Length == 0) return;
        _chatLog.Add(message);
#endif
    }

    [Server]
    private void OnServerPostChatMessage(NetworkMessage netMsg) {
        string message = netMsg.ReadMessage<StringMessage>().value;
        _chatLog.Add(message);
    }

    private void OnChatUpdated(SyncListString.Operation op, int index) {
        _chatline.text += "\n" + _chatLog[_chatLog.Count - 1];
    }

    public void AddMessage(string text) {
        _chatline.text += "\n" + text;
    }

    private void Update() {
        if (Input.GetKeyDown("return")) {
            _input.Select();
        }
    }
}