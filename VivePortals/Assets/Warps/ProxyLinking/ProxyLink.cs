using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ProxyLink : MonoBehaviour {
    public ProxyNode A;
    public ProxyNode B;

    void Start() {
        
    }

    void Update() {
        if(A == null || B == null) {
            return;
        }

        var q = Quaternion.FromToRotation(Vector3.forward, (B.transform.position - A.transform.position).normalized);
        var m = Matrix4x4.Rotate(q);

        var renderer = GetComponent<MeshRenderer>();
        renderer.sharedMaterial.SetMatrix("_ConnectionMatrix", m);
        renderer.sharedMaterial.SetVector("_From", A.transform.position);
        renderer.sharedMaterial.SetVector("_To", B.transform.position);
        renderer.sharedMaterial.SetFloat("_FromRadius", A.transform.localScale.x);
        renderer.sharedMaterial.SetFloat("_ToRadius", B.transform.localScale.x);
        renderer.sharedMaterial.SetColor("_FromColor", A.Color);
        renderer.sharedMaterial.SetColor("_ToColor", B.Color);

        GetComponent<MeshFilter>().sharedMesh.bounds = new Bounds(transform.position, new Vector3(100, 100, 100));

        /*
        var bounds = new Bounds();
        bounds.SetMinMax(Vector3.Min(A.transform.position, B.transform.position), Vector3.Max(A.transform.position, B.transform.position));
        GetComponent<MeshFilter>().sharedMesh.bounds = bounds;
        */
    }
}
