using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RingLOD : MonoBehaviour {
    private Material material;
    public Color Color;

    void Start() {
        material = GetComponent<Renderer>().material;
    }

    
    void Update() {
        var dist = Vector3.Distance(Camera.main.transform.position, transform.position);

        float thickness = 0.04f + CoolMath.SmoothStep(1f, 15f, dist) * 0.36f;
        material.SetFloat("Vector1_11B6F0C9", thickness);

        float stripeCount = Mathf.Round(6f + CoolMath.SmoothStep(15f, 1f, dist) * 24f);
        material.SetFloat("Vector1_DD39A7BF", stripeCount);

        material.SetColor("Color_80BB22F3", Color);
    }
}
