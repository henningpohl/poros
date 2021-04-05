using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Tether : MonoBehaviour {
    public Vector3 StartPoint = Vector3.zero;
    public Vector3 EndPoint = Vector3.zero;
    public GameObject Attachement = null;
    public float Thickness = 0.1f;
    public float Alpha = 1f;
    private Material material;

    void Start() {
        material = GetComponent<Renderer>().material;
    }

    void Update() {
        Vector3 localEndPoint;
        if(Attachement == null) {
            localEndPoint = transform.parent.worldToLocalMatrix.MultiplyPoint(EndPoint);
        } else {
            localEndPoint = transform.parent.worldToLocalMatrix.MultiplyPoint(Attachement.transform.position);
        }
        
        transform.localPosition = StartPoint + 0.5f * (localEndPoint - StartPoint);
        transform.localScale = new Vector3(Thickness, localEndPoint.magnitude * 0.5f, Thickness);
        transform.localRotation = Quaternion.FromToRotation(Vector3.up, localEndPoint - StartPoint);

        material.SetFloat("Vector1_BF243A1", Alpha);
    }
}
