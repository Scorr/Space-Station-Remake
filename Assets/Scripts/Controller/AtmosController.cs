using System.Collections.Generic;
using UnityEngine;

namespace Controller {
    internal class AtmosController : MonoBehaviour {
        
        private float _currentTickProgress;

        private readonly List<Gas> _gasList = new List<Gas>();

        private void Update() {
            _currentTickProgress += Time.deltaTime;
            if (_currentTickProgress >= MasterController.TickRate)
                _currentTickProgress -= MasterController.TickRate;
            else
                return;

            for (int i = 0; i < _gasList.Count; i++) {
                Gas gas = _gasList[i];
                if (gas.TileObject == null) { // check if tileobject wasn't destroyed
                    _gasList.Remove(gas);
                    continue;
                }

                List<Tile> tiles = MapManager.Instance.GetSurroundingTiles(gas.TileObject.Tile, 1, false, true);
                // check what neighbours are valid
                for (int j = 0; j < tiles.Count; j++) {
                    Tile adjacentTile = tiles[j];
                    if (adjacentTile == null) {
                        tiles.RemoveAt(j);
                        continue;
                    }

                    // check if non blocking
                    if (!MapManager.Instance.CheckAdjacentGas(gas.TileObject.Tile, adjacentTile)) {
                        tiles.RemoveAt(j);
                    }
                }

                //gas.NextTickValue = gas.Value / tiles.Count + 1;

                for (int k = 0; k < tiles.Count; k++) {
                    Tile adjacentTile = tiles[k];
                    // calculate gas difference
                    if (adjacentTile.gas != null) {
                        Gas otherGas = adjacentTile.gas;
                        if (otherGas != null) {
                            float difference = (gas.Value - otherGas.Value)*0.5f;

                            //gas.NextTickValue += otherGas.Value/9;
                            gas.Value -= difference;
                            otherGas.Value += difference;
                        }
                    }
                    else if (adjacentTile.turf is Space) {
                        float difference = gas.Value*0.5f; // if it's a spacetile leak gas into the void
                        gas.Value -= difference;
                    }
                }
            }

            for (int j = 0; j < _gasList.Count; j++) {
                Gas gas = _gasList[j];
                //gas.Value = gas.NextTickValue;
            }
        }

        public void AddGas(Gas newGas) {
            _gasList.Add(newGas);
        }
    }
}