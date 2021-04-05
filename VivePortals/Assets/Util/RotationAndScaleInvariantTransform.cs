using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAndScaleInvariantTransform : MonoBehaviour {
    private Vector3 initialParentScale;
    private Vector3 initialScale;
    private Quaternion initialRotation;
    private Vector3 initialPosition;

    void Start() {
        initialParentScale = transform.parent.localScale;
        initialScale = transform.localScale;
        initialRotation = transform.rotation;
        initialPosition = transform.parent.worldToLocalMatrix.MultiplyPoint(transform.position);
        initialPosition = initialPosition.ComponentMultiply(initialParentScale);
    }

    void LateUpdate() {
        var scaleDiff = transform.parent.localScale.ComponentDivide(initialParentScale);
        transform.localScale = initialScale.ComponentDivide(scaleDiff);
        transform.rotation = initialRotation;
        transform.position = transform.parent.position + initialPosition.ComponentMultiply(scaleDiff);
    }
}
