using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Controller {
    internal class MasterController : Singleton<MasterController> {

        private const float TicksPerSecond = 20f;
        public const float TickRate = 1f / TicksPerSecond;
        private float _currentTickProgress;

        private AtmosController _atmosController;
        public AtmosController AtmosController {
            get { return _atmosController ?? (_atmosController = GetComponent<AtmosController>() ?? gameObject.AddComponent<AtmosController>()); }
        }

        private RoundController _roundController;
        public RoundController RoundController {
            get { return _roundController ?? (_roundController = GetComponent<RoundController>() ?? gameObject.AddComponent<RoundController>()); }
        }
        
        private Chat _chat;
        public Chat Chat {
            get {
                // not ?? because == null operator is overloaded to check if the gameobject was destroyed
                if (_chat == null)
                    _chat = FindObjectOfType<Chat>();
                return _chat;
            }
        }

        private readonly List<IUpdate> _updateList = new List<IUpdate>(); 

        public void Initialize() {
            Destroy(_roundController);
            _roundController = gameObject.AddComponent<RoundController>();
        }

        private void Update() {
            // Use Chat's NetworkBehaviour to check if we are the server.
            // Should not rely on Chat in the future and use a more elegant solution.
            if (Chat == null || !Chat.isServer) {
                return;
            }

            _currentTickProgress += Time.deltaTime;
            if (_currentTickProgress >= TickRate)
                _currentTickProgress -= TickRate;
            else
                return;

            for (int i = 0; i < _updateList.Count; i++) {
                _updateList[i].Update();
            }
        }

        public void AddUpdate(IUpdate updateObject) {
            _updateList.Add(updateObject);
        }

        public void RemoveUpdate(IUpdate updateObject) {
            _updateList.Remove(updateObject);
        }
    }
}
