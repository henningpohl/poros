using System;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;

public class ProxyGraspedPoseHandler : IGraspedPoseHandler {
    private InteractionBehaviour interactionObject;
    private List<InteractionController> graspingControllers = new List<InteractionController>();
    private Quaternion initialRotation;
    private float initialScale;
    private Vector3 firstHandGraspOffset;
    private List<Vector3> bimanualGraspStartPos = new List<Vector3>();
    private List<float> bimanualGraspStartDist = new List<float>();

    private bool enabled = false;
    public int InteractingHands {
        get { return graspingControllers.Count; }
    }

    private const float OneEuroFcMin = 1.5f;
    private const float OneEuroBeta = 0.001f;
    private OneEuroFilter<Vector3> translationFilter = new OneEuroFilter<Vector3>(60.0f, OneEuroFcMin, OneEuroBeta);
    private OneEuroFilter<Quaternion> rotationFilter = new OneEuroFilter<Quaternion>(60.0f, OneEuroFcMin, OneEuroBeta);
    private OneEuroFilter scaleFilter = new OneEuroFilter(60.0f, OneEuroFcMin, OneEuroBeta);


    public ProxyGraspedPoseHandler(InteractionBehaviour interactionObject) {
        this.interactionObject = interactionObject;
    }

    public void SetEnabled(bool state) {
        if(state == enabled) {
            return;
        }
        enabled = state;
        if(enabled) {
            SetupInteractionStartState();
        }
    }

    public void AddController(InteractionController controller) {
        graspingControllers.Add(controller);
        SetupInteractionStartState();
    }

    private void SetupInteractionStartState() {
        if(graspingControllers.Count == 1) {
            firstHandGraspOffset = interactionObject.transform.position - graspingControllers[0].GetGraspPoint();
        }
        if(graspingControllers.Count == 2) {
            bimanualGraspStartPos.Clear();
            bimanualGraspStartPos.Add(graspingControllers[0].GetGraspPoint());
            bimanualGraspStartPos.Add(graspingControllers[1].GetGraspPoint());
            bimanualGraspStartDist.Clear();
            bimanualGraspStartDist.Add((interactionObject.transform.position - graspingControllers[0].GetGraspPoint()).magnitude);
            bimanualGraspStartDist.Add((interactionObject.transform.position - graspingControllers[1].GetGraspPoint()).magnitude);
            initialRotation = interactionObject.transform.rotation;
            initialScale = interactionObject.transform.localScale.x;
        }
    }

    public void ClearControllers() {
        graspingControllers.Clear();
    }

    public void GetGraspedPosition(out Vector3 position, out Quaternion rotation) {
        float scale;
        if(!enabled) {
            position = interactionObject.transform.position;
            rotation = interactionObject.transform.rotation;
            scale = interactionObject.transform.localScale.x;
            return;
        }

        if(graspingControllers.Count == 1) {
            HandleOneHanded(out position, out rotation, out scale);
        } else if(graspingControllers.Count == 2) {
            HandleTwoHanded(out position, out rotation, out scale);
        } else {
            // How the fuck did this happen?
            position = interactionObject.transform.position;
            rotation = interactionObject.transform.rotation;
            scale = interactionObject.transform.localScale.x;
        }

        position = translationFilter.Filter(position, Time.time);
        rotation = rotationFilter.Filter(rotation, Time.time).normalized;
        scale = scaleFilter.Filter(scale, Time.time);
        interactionObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void RemoveController(InteractionController controller) {
        graspingControllers.Remove(controller);
        if(graspingControllers.Count == 1) {
            firstHandGraspOffset = interactionObject.transform.position - graspingControllers[0].GetGraspPoint();
        }
    }

    private void HandleOneHanded(out Vector3 position, out Quaternion rotation, out float scale) {
        position = graspingControllers[0].GetGraspPoint() + firstHandGraspOffset;
        rotation = interactionObject.transform.rotation;
        scale = interactionObject.transform.localScale.x;
    }

    private void HandleTwoHanded(out Vector3 position, out Quaternion rotation, out float scale) {
        position = interactionObject.transform.position;
        //Debug.DrawLine(position, graspingControllers[0].GetGraspPoint(), Color.gray);
        //Debug.DrawLine(position, graspingControllers[1].GetGraspPoint(), Color.gray);
        //Debug.DrawLine(bimanualGraspStartPos[0], bimanualGraspStartPos[1], Color.gray, 10f);
        //Debug.DrawLine(graspingControllers[0].GetGraspPoint(), graspingControllers[1].GetGraspPoint(), Color.white, 10f);

        var initialDirection = (bimanualGraspStartPos[0] - bimanualGraspStartPos[1]).normalized;
        var currentDirection = (graspingControllers[0].GetGraspPoint() - graspingControllers[1].GetGraspPoint()).normalized;
        rotation = initialRotation * Quaternion.FromToRotation(initialDirection, currentDirection);

        var distA = (interactionObject.transform.position - graspingControllers[0].GetGraspPoint()).magnitude / bimanualGraspStartDist[0];
        var distB = (interactionObject.transform.position - graspingControllers[1].GetGraspPoint()).magnitude / bimanualGraspStartDist[1];
        scale = initialScale * Mathf.Lerp(distA, distB, 0.5f);
    }
}