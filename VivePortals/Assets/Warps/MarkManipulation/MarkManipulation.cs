using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Leap.Unity.Interaction;
using System;

[RequireComponent(typeof(MarkNode))]
public class MarkManipulation : MonoBehaviour {
    private enum MarkManipulationState {
        Inactive,
        Activating,
        Active
    }

    private MarkManipulationState state = MarkManipulationState.Inactive;
    private MarkHand leftHand;
    private MarkHand rightHand;
    private MarkNode mark;

    private GameObject grabSphere;
    private Material grabSphereMaterial;
    private InteractionBehaviour grabSphereLeap;
    private PinchAnchor grabSphereAnchoring;
    private List<InteractionBehaviour> deactivatedObjects = new List<InteractionBehaviour>();

    private float modeChangeTimer;
    public const float ManipulationActivationTimeout = 0.5f;
    public const float ManipulationDeactivationTimeout = 3.0f;

    private const float MinMarkSize = 0.1f;
    private const float MaxMarkSize = 15.0f;

    private Color GrabSphereDefaultColor = new Color(0.2f, 0.6f, 0.2f, 0.9f);
    private Color GrabSphereGrabbedColor = new Color(0.8f, 0.1f, 0.1f, 0.9f);

    void Start() {
        mark = GetComponent<MarkNode>();
        leftHand = mark.LeftHand;
        rightHand = mark.RightHand;
        grabSphere = transform.Find("GrabSphere").gameObject;
        grabSphere.SetActive(false);

        grabSphereMaterial = grabSphere.GetComponent<Renderer>().material;

        grabSphereAnchoring = grabSphere.GetComponent<PinchAnchor>();
        grabSphereAnchoring.AllowAnchoring = false;
        grabSphereAnchoring.OnAnchoringSelection += (s, e) => {
            if(e != null) {
                mark.AnchorToCenter(e);
            }
        };

        grabSphereLeap = grabSphere.GetComponent<InteractionBehaviour>();
        grabSphereLeap.OnGraspBegin += GrabSphereGraspBegin;
        grabSphereLeap.OnGraspStay += GrabSphereGraspStay;
        grabSphereLeap.OnGraspEnd += GrabSphereGraspedEnd;
    }

    void Update() {
        switch(state) {
            case MarkManipulationState.Inactive:
                CheckForActivation();
                break;
            case MarkManipulationState.Activating:
                CheckForContinuedActivation();
                break;
            case MarkManipulationState.Active:
                CheckForEndOfManipulation();
                break;
        }
        //Debug.Log(state);
    }

    #region Grab sphere interaction callbacks
    private InteractionHand grabHand;
    private Vector3 initialGrabOffset;

    private void GrabSphereGraspBegin() {
        grabHand = grabSphereLeap.graspingController.intHand;
        if(grabHand.NoFistPinchStrength() < 0.6f) {
            initialGrabOffset = grabHand.GetGraspPoint() - grabSphere.transform.position;
            grabSphereMaterial.DOColor(GrabSphereGrabbedColor, 0.1f);
        }
    }

    private void GrabSphereGraspedEnd() {
        grabHand = null;
        grabSphereMaterial.DOColor(GrabSphereDefaultColor, 0.1f);
    }

    private void GrabSphereGraspStay() {
        if(grabHand == leftHand.Leap) {
            ScaleMarkedSpace();
        } else if(grabHand == rightHand.Leap) {
            TranslateMarkedSpace();
        }
        modeChangeTimer = 0f;
    }

    private void ScaleMarkedSpace() {
        var grabVec = (grabHand.GetGraspPoint() - transform.position) - initialGrabOffset;
        var baseVec = grabSphere.transform.position - transform.position;

        var delta = grabVec.magnitude/ baseVec.magnitude - 1;
        delta = Mathf.Sign(delta) * CoolMath.SmoothStep(0.05f, 0.3f, Mathf.Abs(delta));
        delta = Time.deltaTime * delta * mark.Radius;
        mark.transform.localScale += new Vector3(delta, delta, delta);
        mark.transform.localScale.Clamp(MinMarkSize, MaxMarkSize);
    }

    private void TranslateMarkedSpace() {
        var grabPos = grabHand.GetGraspPoint();
        var delta = grabPos - grabSphere.transform.position;
        delta /= mark.Radius;
        delta.x = Mathf.Sign(delta.x) * CoolMath.SmoothStep(0.1f, 0.2f, Mathf.Abs(delta.x));
        delta.y = Mathf.Sign(delta.y) * CoolMath.SmoothStep(0.1f, 0.2f, Mathf.Abs(delta.y));
        delta.z = Mathf.Sign(delta.z) * CoolMath.SmoothStep(0.1f, 0.2f, Mathf.Abs(delta.z));
        delta = Time.deltaTime * delta * mark.Radius;
        mark.transform.position += delta;
    }
    #endregion

    #region pinching functions
    public void BeginPinch(Vector3 pos) {
        grabSphereMaterial.DOColor(new Color(0.1f, 0.7f, 0.7f, 0.9f), 0.1f);
    }
    #endregion

    private void CheckForActivation() {
        if(IsIdleAndOnShell(leftHand) || IsIdleAndOnShell(rightHand)) {
            modeChangeTimer = Time.deltaTime;
            state = MarkManipulationState.Activating;
            mark.Flash(1);
        }
    }

    private void CheckForContinuedActivation() {
        if(IsIdleAndOnShell(leftHand) || IsIdleAndOnShell(rightHand)) {
            modeChangeTimer += Time.deltaTime;
            if(modeChangeTimer > ManipulationActivationTimeout) {
                ActivateManipulation();
            }
        } else {
            state = MarkManipulationState.Inactive;
        }
    }

    private void ActivateManipulation() {
        state = MarkManipulationState.Active;
        mark.Flash(2);

        var handPos = GetMeanHandPosition();
        var handVec = (transform.position - handPos).normalized;
        handPos = handPos + 0.3f * mark.Radius * handVec;

        leftHand.StartManipulationMode();
        rightHand.StartManipulationMode();

        grabSphere.SetActive(true);
        grabSphere.transform.position = handPos;
        grabSphere.transform.DOScale(0f, 0.05f).From().SetEase(Ease.InQuad);
        grabSphereAnchoring.AllowAnchoring = true;

        foreach(var obj in grabSphereLeap.manager.interactionObjects) {
            if(obj.gameObject == grabSphere) {
                continue;
            }
            if(obj.ignoreGrasping == true) {
                continue;
            }

            var behavior = obj.gameObject.GetComponent<InteractionBehaviour>();
            behavior.ignoreGrasping = true;
            deactivatedObjects.Add(behavior);
        }
    }

    private void CheckForEndOfManipulation() {
        if(grabSphereAnchoring.IsAnchoring) {
            return;
        }
        modeChangeTimer += Time.deltaTime;
        if(modeChangeTimer < ManipulationDeactivationTimeout) {
            return;
        }

        state = MarkManipulationState.Inactive;
        grabSphereAnchoring.AllowAnchoring = false;
        leftHand.StopManipulationMode();
        rightHand.StopManipulationMode();
        grabSphere.SetActive(false);
        mark.Flash(1);

        foreach(var obj in deactivatedObjects) {
            obj.ignoreGrasping = false;
        }
        deactivatedObjects.Clear();
    }

    private Vector3 GetMeanHandPosition() {
        if(leftHand.HandState == MarkHandState.Idle &&
          rightHand.HandState == MarkHandState.Idle) {
            var a = leftHand.Leap.leapHand.PalmPosition.ToUnity();
            var b = rightHand.Leap.leapHand.PalmPosition.ToUnity();
            return Vector3.Lerp(a, b, 0.5f);
        }

        if(leftHand.HandState == MarkHandState.Idle) {
            return leftHand.Leap.leapHand.PalmPosition.ToUnity();
        }

        if(rightHand.HandState == MarkHandState.Idle) {
            return rightHand.Leap.leapHand.PalmPosition.ToUnity();
        }

        return Vector3.zero;
    }

    private bool IsIdleAndOnShell(MarkHand hand) {
        if(hand.HandState != MarkHandState.Idle) {
            return false;
        }

        var centerToHand = hand.Leap.leapHand.PalmPosition.ToUnity() - transform.position;
        var angle = Vector3.Dot(centerToHand.normalized, hand.Leap.leapHand.PalmNormal.ToUnity());
        var shellDistance = centerToHand.magnitude / mark.Radius;

        //Debug.Log(shellDistance + " " + angle);
        if(angle > 0.8f && shellDistance > 0.8f && shellDistance < 1.05f) {
            return true;
        } else {
            return false;
        }
    }

    
}
