using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using System;
using Leap.Unity;

[RequireComponent(typeof(InteractionBehaviour), typeof(LineRenderer), typeof(ProxyNode))]
public class Manipulation : MonoBehaviour {
    public event ManipulationModeChangedDelegate OnModeChange;
    public ManipulationMode Mode { get; private set; }

    private LineRenderer lineRenderer;
    private ProxyNode proxyNode;
    private InteractionBehaviour leap;
    private ProxyGraspedPoseHandler graspedPoseHandler;

    // Time users have to hold the hand against the proxy to start manipulation mode
    public const float ManipulationActivationTimeout = 0.5f;
    private float hoverTime = 0f;
    // When this much time has passed without interaction, go back to hover mode
    public const float InactiveTimeout = 5.0f;
    private float inactiveTime;
    // Anchoring timeouts and variables
    private Vector3 anchoringStart;
    private InteractionHand anchorHand;
    private float anchoringStopTimer;
    public const float AnchoringTimeout = 0.2f;
    private const float MINIMIZE_THRESHOLD = 0.05f;

    void Start() {
        Mode = ManipulationMode.Inactive;
        lineRenderer = GetComponent<LineRenderer>();
        proxyNode = GetComponent<ProxyNode>();
        leap = GetComponent<InteractionBehaviour>();
        graspedPoseHandler = new ProxyGraspedPoseHandler(leap);
        leap.graspedPoseHandler = graspedPoseHandler;
    }
    

    void Update() {
        switch(Mode) {
            case ManipulationMode.Inactive:
                CheckForNearbyControllers();
                break;
            case ManipulationMode.Hover:
                CheckForActivationStart();
                CheckWhetherHandsAreInside();
                // if we haven't changed mode yet, is there still a hand?
                if(Mode == ManipulationMode.Hover) {
                    CheckForNearbyControllers();
                }
                break;
            case ManipulationMode.Activating:
                CheckForContinuedActivation();
                break;
            case ManipulationMode.Active:
            case ManipulationMode.Translating:
            case ManipulationMode.ScalingAndRotating:
                HandleActiveMode();
                break;
            case ManipulationMode.Anchoring:
                HandleAnchoring();
                break;
            case ManipulationMode.Inside:
                CheckWhetherHandsAreStillInside();
                break;
            case ManipulationMode.Minimized:
                HandleMinimized();
                break;
        }
        //Debug.Log(Mode);
    }

    private void ChangeMode(ManipulationMode newMode) {
        if(newMode == Mode) {
            return;
        }
        var oldMode = Mode;
        Mode = newMode;
        OnModeChange?.Invoke(oldMode, newMode);
        //Debug.Log(newMode);
    }

    private void CheckForNearbyControllers() {
        if (proxyNode.State != ProxyNode.ProxyState.Normal) return;
        if(leap.hoveringControllers.Count > 0) {
            ChangeMode(ManipulationMode.Hover);
        } else {
            ChangeMode(ManipulationMode.Inactive);
        }
    }

    private void CheckForActivationStart() {
        var hasHoverHand = leap.hoveringControllers.Any(c => IsHandInActivationPose(c.intHand));
        if(hasHoverHand) {
            hoverTime = Time.deltaTime;
            ChangeMode(ManipulationMode.Activating);
        }
    }

    private void CheckWhetherHandsAreInside() {
        if(leap.hoveringControllers.Any(c => proxyNode.Contains(c.position))) {
            leap.ignoreGrasping = true;
            ChangeMode(ManipulationMode.Inside);
        }
    }

    private void CheckForContinuedActivation() {
        var hasHoverHand = leap.hoveringControllers.Any(c => IsHandInActivationPose(c.intHand));
        if(hasHoverHand) {
            hoverTime += Time.deltaTime;
            if(hoverTime > ManipulationActivationTimeout) {
                hoverTime = 0f;
                ChangeMode(ManipulationMode.Active);
                inactiveTime = 0f;
                graspedPoseHandler.SetEnabled(true);
            }
        } else {
            ChangeMode(ManipulationMode.Hover);
        }
    }

    private void HandleActiveMode() {
        bool hasHoverHand = false;
        foreach (var controller in leap.hoveringControllers) {
            if(controller.isGraspingObject) continue;
            hasHoverHand |= IsHandInActivationPose(controller.intHand);
        }
        if(hasHoverHand) {
            hoverTime += Time.deltaTime;
        } else {
            hoverTime = 0f;
        }

        if (graspedPoseHandler.InteractingHands == 0) {
            inactiveTime += Time.deltaTime;
            if(inactiveTime > InactiveTimeout) {
                ChangeMode(ManipulationMode.Hover);
                graspedPoseHandler.SetEnabled(false);
            } else {
                ChangeMode(ManipulationMode.Active);
            }
        } else if(graspedPoseHandler.InteractingHands == 1) {
            inactiveTime = 0f;
            // Don't allow going from Translating into anchoring
            if(Mode == ManipulationMode.Translating) hasHoverHand = false;
            // Have slight delay on anchoring start as not to trigger it on rotation/scaling
            if(hasHoverHand && hoverTime > 0.2f) {
                anchorHand = leap.graspingController.intHand;
                anchoringStart = anchorHand.isGraspingObject ? anchorHand.GetGraspPoint() : anchorHand.position;
                anchoringStopTimer = 0f;
                lineRenderer.enabled = true;
                ChangeMode(ManipulationMode.Anchoring);
                Player.Instance?.ShowAnchoringTarget();
                graspedPoseHandler.SetEnabled(false);
            } else {
                ChangeMode(ManipulationMode.Translating);
            }
        } else if(graspedPoseHandler.InteractingHands == 2) {
            inactiveTime = 0f;
            if (proxyNode.Radius < MINIMIZE_THRESHOLD) {
                graspedPoseHandler.SetEnabled(false);
                proxyNode.Minimize();
                ChangeMode(ManipulationMode.Minimized);
            } else {
                ChangeMode(ManipulationMode.ScalingAndRotating);
            }
        }
    }

    private void HandleAnchoring() {
        if(!anchorHand.isTracked) {
            anchoringStopTimer += Time.deltaTime;
        } else if(anchorHand.intHand.leapHand.GrabStrength < 0.8f) {
            var dropTargets = GetValidDropTargets(anchorHand.hoveredObjects);
            if(dropTargets.Count == 0) {
                anchoringStopTimer += Time.deltaTime;
            } else {
                // TODO: we just use the first. Could do filtering.
                proxyNode.Anchor(dropTargets[0]);
                anchoringStopTimer = AnchoringTimeout;
            }
        } else { 
            anchoringStopTimer = 0f;
        }

        if(anchoringStopTimer >= AnchoringTimeout) {
            lineRenderer.enabled = false;
            ChangeMode(ManipulationMode.Hover);
            Player.Instance?.HideAnchoringTarget();
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, anchoringStart);
        lineRenderer.SetPosition(1, anchorHand.isGraspingObject ? anchorHand.GetGraspPoint() : anchorHand.position);
    }

    private void HandleMinimized() {
        if(leap.contactingControllers.Any(c => c.isRight)) {
            proxyNode.Maximize();
            ChangeMode(ManipulationMode.Inactive);
        }
    }

    private void CheckWhetherHandsAreStillInside() {
        if(!leap.hoveringControllers.Any(c => proxyNode.Contains(c.position))) {
            leap.ignoreGrasping = false;
            ChangeMode(ManipulationMode.Hover);
        }
    }

    private bool IsHandInActivationPose(InteractionHand hand) {
        var proxyToHand = hand.leapHand.PalmPosition.ToUnity() - proxyNode.transform.position;
        var palmNormal = -hand.leapHand.PalmNormal.ToUnity();

        var alignment = Vector3.Dot(proxyToHand.normalized, palmNormal.normalized);
        var distance = proxyToHand.magnitude - proxyNode.Radius;
        var pinch = hand.leapHand.PinchStrength;

        // is the hand parallel to the sphere, within 5cm of the surface, and not pinching?
        return alignment > 0.9 && distance > 0.0f && distance < 0.05f && pinch < 0.35f;
    }

    private List<GameObject> GetValidDropTargets(ReadonlyHashSet<IInteractionBehaviour> set) {
        var output = new List<GameObject>();
        foreach(var obj in set) {
            if(obj.gameObject == proxyNode.gameObject) {
                continue;
            }
            output.Add(obj.gameObject);
        }
        return output;
    }
}
