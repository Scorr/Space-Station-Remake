using System;

public class Inventory {

    public enum Hand { Left, Right }

    private Item _leftHand;
	public Item LeftHand {
        get {
            return _leftHand;
        }
        set {
            _leftHand = value;
            if (HandChanged != null) {
                HandChanged.Invoke(Hand.Left, value);
            }
        }
    }
    private Item _rightHand;
    public Item RightHand {
        get {
            return _rightHand;
        }
        set {
            _rightHand = value;
            if (HandChanged != null)
                HandChanged.Invoke(Hand.Right, value);
        }
    }

    public Hand CurrentHand = Hand.Left;
    public Item ActiveHand {
        get {
            return CurrentHand == Hand.Left ? LeftHand : RightHand;
        }
        set {
            if (CurrentHand == Hand.Left)
                LeftHand = value;
            else
                RightHand = value;
        }
    }
    
    public event Action<Hand, Item> HandChanged;
    
    public void SetActiveHand(Hand hand) {
        CurrentHand = hand;
    }

    public bool Add(Item item) {
        if (ActiveHand == null) {
            ActiveHand = item;

            return true;
        }
        if (item != null && ActiveHand.name == item.name && ActiveHand.currentStack < ActiveHand.maxStack) {
            ActiveHand.currentStack += item.currentStack;

            return true;
        }

        return false;
    }

    public void RemoveActiveItem() {
        if (ActiveHand == null) return;

        ActiveHand.currentStack--;

        if (ActiveHand.currentStack <= 0) {
            ActiveHand = null;
        }
    }

    public void ToggleHands() {
        CurrentHand = CurrentHand == Hand.Left ? Hand.Right : Hand.Left;
    }
}
