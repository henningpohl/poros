using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public float Speed = 1f;
    private float step = 6f; // The higher the nuber the shorter the step

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow)) {
            Vector3 direction = Player.Instance.MainCamera.transform.forward;
            direction = new Vector3(direction.x, 0f, direction.z);
            transform.position = transform.position +
                direction * Time.deltaTime * (Speed + ((1 + Mathf.Sin(Time.time * step)) / 2));
        }
    }
}
