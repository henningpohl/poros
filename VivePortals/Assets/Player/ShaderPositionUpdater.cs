using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ShaderPositionUpdater : MonoBehaviour {
    public enum TrackedObjectType {
        LeftHand, RightHand, Head
    }
    public TrackedObjectType TrackedObject = TrackedObjectType.Head;

    private int propertyID = -1;

    void Start() {
        switch(TrackedObject) {
            case TrackedObjectType.Head:
                propertyID = Shader.PropertyToID("HeadPosVec3");
                break;
            case TrackedObjectType.LeftHand:
                propertyID = Shader.PropertyToID("LeftHandPosVec3");
                break;
            case TrackedObjectType.RightHand:
                propertyID = Shader.PropertyToID("RightHandPosVec3");
                break;
        }
        Shader.SetGlobalVector(propertyID, new Vector4(float.MaxValue, float.MaxValue, float.MaxValue));
    }

    void Update() {
        if(propertyID != -1) {
            Shader.SetGlobalVector(propertyID, transform.position);
        }
    }

    private void OnDisable() {
        if(propertyID != -1) {
            Shader.SetGlobalVector(propertyID, new Vector4(float.MaxValue, float.MaxValue, float.MaxValue));
        }
    }
}
