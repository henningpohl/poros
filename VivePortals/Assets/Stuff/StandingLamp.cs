using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandingLamp : MonoBehaviour {
    private Light light;
    public GameObject cord;
    public MeshRenderer shade;
    private float lastPosition;
    private float triggerHeight = -0.05f;

    private Color onColor = new Color(1f, 1f, 1f);
    private Color offColor = new Color(0.5f, 0.5f, 0.5f);

    void Start() {
        light = GetComponentInChildren<Light>();
        light.enabled = false;
        lastPosition = cord.transform.position.y;
    }

    void Update() {
        float position = cord.transform.position.y;
        if(position < triggerHeight && lastPosition >= triggerHeight) {
            light.enabled = !light.enabled;
            if(light.enabled) {
                shade.material.SetColor("_BaseColor", onColor);
            } else {
                shade.material.SetColor("_BaseColor", offColor);
            }
        }
        lastPosition = position;
    }
}
