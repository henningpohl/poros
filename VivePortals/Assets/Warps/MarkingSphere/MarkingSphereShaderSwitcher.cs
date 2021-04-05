using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkingSphereShaderSwitcher : MonoBehaviour {
    private Material mat;
    public Shader InsideShader;
    public Shader OutsideShader;

    void Start() {
        mat = GetComponent<MeshRenderer>().material;
    }

    void Update() {
        var localCameraPosition = transform.worldToLocalMatrix.MultiplyPoint(Camera.main.transform.position);
        var distance = localCameraPosition.sqrMagnitude;
        if(distance <= (0.5f * 0.5f)) {
            mat.shader = InsideShader;
        } else {
            mat.shader = OutsideShader;
        }
    }
}
