using UnityEngine;

namespace Controller {

    internal class RoundController : MonoBehaviour {

        private float _startTime;

        private enum ShuttleState { None, Called, Recalled }
        private ShuttleState _currentShuttleState;
        private float _arrivalTime; // The point in time that the shuttle is expected to reach its destination.
        private const float ShuttleTravelTime = 6f; // Time in seconds it takes for the shuttle to arrive.

        private float _currentSecondProgress;

        private int _deadPlayers;

        public float CurrentTime {
            get { return Time.time - _startTime; }
        }

        private void Start() {
            _startTime = Time.time;
        }

        private void Update() {
            if (_currentShuttleState == ShuttleState.None) return;

            float timeRemaining = _arrivalTime - CurrentTime;
            _currentSecondProgress += Time.deltaTime;

            if (_currentSecondProgress >= 1f) {
                _currentSecondProgress = 0f;


                if (timeRemaining <= 5) {
                    string shuttleVerb = "";
                    if (_currentShuttleState == ShuttleState.Called)
                        shuttleVerb = "to arrive";
                    else if (_currentShuttleState == ShuttleState.Recalled)
                        shuttleVerb = "to recall";

                        MasterController.Instance.Chat.PostServerMessage(Mathf.CeilToInt(timeRemaining) +
                                                                         " seconds remaining for shuttle " + shuttleVerb + ".");
                }
                // Shuttle reached
                if (timeRemaining <= 0) {
                    if (_currentShuttleState == ShuttleState.Called) {
                        // TODO: find better way to call sounds
                        CoreNetworkManager.singleton.client.connection.playerControllers[0].gameObject
                            .GetComponent<Player>()
                            .RpcPlaySound("sound/shuttledock");
                        EndRound();
                    }
                    else if (_currentShuttleState == ShuttleState.Recalled) {

                    }

                    _currentShuttleState = ShuttleState.None;
                }
            }
        }

        public void PlayerDeath() {
            ++_deadPlayers;
            if (CoreNetworkManager.singleton.numPlayers <= _deadPlayers)
                EndRound();
        }

        private void EndRound() {
            CoreNetworkManager.singleton.ServerChangeScene("SpaceStation");
            //CoreNetworkManager.singleton.StopHost();
        }

        /// <summary>
        /// Call the emergency shuttle to escape.
        /// </summary>
        /// <returns>Returns false if the state hasn't changed (shuttle was already recalled for example).</returns>
        public void CallShuttle() {
            if (_currentShuttleState == ShuttleState.None) {
                _currentShuttleState = ShuttleState.Called;

                _arrivalTime = CurrentTime + ShuttleTravelTime;
                _currentSecondProgress = 0f;

                // TODO: find better way to call sounds
                CoreNetworkManager.singleton.client.connection.playerControllers[0].gameObject.GetComponent<Player>().RpcPlaySound("sound/shuttlecalled");
                MasterController.Instance.Chat.PostServerMessage("The escape shuttle has been called.");
            }
            else if (_currentShuttleState == ShuttleState.Called) {
                _currentShuttleState = ShuttleState.Recalled;
                
                _arrivalTime = CurrentTime + (CurrentTime + ShuttleTravelTime - _arrivalTime);
                _currentSecondProgress = 0f;
                CoreNetworkManager.singleton.client.connection.playerControllers[0].gameObject.GetComponent<Player>().RpcPlaySound("sound/shuttlerecalled");
                MasterController.Instance.Chat.PostServerMessage("The escape shuttle has been recalled.");
            }
        }
    }
}
