using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PreviewSphere : MonoBehaviour {
    public float Radius = 1.0f;

    void Start() {
        
    }

    void Update() {
        this.transform.localScale = new Vector3(Radius, Radius, Radius);
    }
}
