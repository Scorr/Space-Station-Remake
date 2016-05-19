using Controller;

public class ShuttleComputer : Machinery, IInteractive {

    public void Interact() {
        MasterController.Instance.RoundController.CallShuttle();
    }
}