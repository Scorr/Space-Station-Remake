using UnityEngine;
using System.Collections;

public class Podracing : MonoBehaviour {

    new Rigidbody2D rigidbody;

    void Awake() {
        rigidbody = gameObject.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0;
    }

	void Update () {
        float xAxisValue = Input.GetAxis("Horizontal");
        float yAxisValue = Input.GetAxis("Vertical");
        rigidbody.AddForce(new Vector2(xAxisValue * 0.05f, yAxisValue * 0.05f));
    }
}
