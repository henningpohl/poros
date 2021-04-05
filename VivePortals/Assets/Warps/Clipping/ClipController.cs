using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ClipController : MonoBehaviour {

    //private int clipPositionID;
    public Vector3 ClipPosition = new Vector3(0, 0, 0);
    public Vector3 ClipScale = new Vector3(1, 1, 1);
    [Range(0.0f, 0.1f)]
    public float ClipEdgeThickness = 0.01f;
    public Color ClipEdgeColor = Color.red;

    void Start() {
        //clipPositionID = Shader.PropertyToID("_ClipObjPosition");
    }

    void Update() {
        Shader.SetGlobalVector("_ClipObjPosition", ClipPosition);
        Shader.SetGlobalVector("_ClipObjScale", ClipScale);
        Shader.SetGlobalFloat("_ClipObjEdgeThickness", ClipEdgeThickness);
        Shader.SetGlobalColor("_ClipObjEdgeColor", ClipEdgeColor);
        transform.localPosition = ClipPosition;
        transform.localScale = ClipScale;
    }
}
