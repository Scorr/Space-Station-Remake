using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

internal class Hands : NetworkBehaviour {

    private Inventory inventory;
    [SerializeField]
    private Sprite lHandSprite;
    [SerializeField]
    private Sprite lHandActiveSprite;
    [SerializeField]
    private Sprite rHandSprite;
    [SerializeField]
    private Sprite rHandActiveSprite;

    [SerializeField]
    private GameObject leftHandPrefab;
    [SerializeField]
    private GameObject rightHandPrefab;

    private Image _leftHandImage;
    private Image _rightHandImage;

    private void Awake() {
        _leftHandImage = leftHandPrefab.GetComponent<Image>();
        _rightHandImage = rightHandPrefab.GetComponent<Image>();
    }

    private void ToggleCurrentHand() {
        inventory.ToggleHands();
        SetCurrentHand(inventory.CurrentHand == Inventory.Hand.Left);
    }

    public void SetCurrentHand(bool leftHand) {
        Inventory.Hand hand;
        if (leftHand) {
            hand = Inventory.Hand.Left;
            _leftHandImage.sprite = lHandActiveSprite;
            _rightHandImage.sprite = rHandSprite;
        }
        else {
            hand = Inventory.Hand.Right;
            _rightHandImage.sprite = rHandActiveSprite;
            _leftHandImage.sprite = lHandSprite;
        }
        CoreNetworkManager.GetCurrentPlayer().CmdSetActiveHand(leftHand);
        inventory.SetActiveHand(hand);
    }

    public override void OnStartClient() {
        NetworkManager.singleton.client.RegisterHandler(MsgConstants.PlayerSpawnedMessage, Initialize);
    }

    private void Initialize(NetworkMessage message) {
        Invoke("Initialize2", 0.01f);
    }

    //TODO: handchanged is null during Initialize(), so wait 0.01f before starting
    private void Initialize2() {
        inventory = CoreNetworkManager.GetCurrentPlayer().Character.inventory;
        inventory.HandChanged += OnHandChanged;

        CoreNetworkManager.GetCurrentPlayer().ToggleHands += ToggleCurrentHand;

        Item rightItem = inventory.RightHand;
        if (rightItem != null) {
            OnHandChanged(Inventory.Hand.Right, rightItem);
        }

        Item leftItem = inventory.LeftHand;
        if (leftItem != null) {
            OnHandChanged(Inventory.Hand.Left, leftItem);
        }
    }

    public void OnHandChanged(Inventory.Hand hand, Item item) {
        switch (hand) {
            case Inventory.Hand.Left:
                if (item == null && leftHandPrefab.transform.childCount > 0) {
                    Destroy(leftHandPrefab.transform.GetChild(0).gameObject);
                }
                break;
            case Inventory.Hand.Right:
                if (item == null && rightHandPrefab.transform.childCount > 0) {
                    Destroy(rightHandPrefab.transform.GetChild(0).gameObject);
                }
                break;
        }

        if (item == null)
            return;

        GameObject itemPrefab = new GameObject(item.name);
        Image image = itemPrefab.AddComponent<Image>();
        image.sprite = SpriteRepository.GetSpriteFromSheet(item.Sprites[0]);

        switch (hand) {
            case Inventory.Hand.Left:
                itemPrefab.transform.SetParent(leftHandPrefab.transform, false);
                break;
            case Inventory.Hand.Right:
                itemPrefab.transform.SetParent(rightHandPrefab.transform, false);
                break;
        }

        itemPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
    }
}
