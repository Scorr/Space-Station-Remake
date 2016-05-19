using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Movable : NetworkBehaviour {
    
    public float speed = 1f;
    public bool moving = false;

    public void Move(Tile tile, Action<Tile> callback = null) {
#if SERVER
        StartCoroutine(DoMove(tile, callback));
#endif
    }

    public IEnumerator DoMove(Tile tile, Action<Tile> callback = null) {
#if SERVER
        moving = true;
        Vector3 newPos = tile.transform.localPosition;

        RpcMove(newPos);
        yield return StartCoroutine(LerpMove(newPos));
        
        moving = false;

        if (callback != null)
            callback.Invoke(tile);
#endif
    }

    public IEnumerator LerpMove(Vector3 newPosition) {
        while (Vector2.Distance(transform.localPosition, newPosition) > 0.01f) {
            Vector2 dir = ((Vector2)newPosition - (Vector2)transform.localPosition).normalized;
            dir *= speed * Time.fixedDeltaTime;
            transform.Translate(dir);

            yield return new WaitForFixedUpdate();
        }
    }

    [ClientRpc]
    public void RpcMove(Vector3 position) {
        if (!isServer) {
            StopAllCoroutines();
            StartCoroutine(LerpMove(position));
        }
    }
}
