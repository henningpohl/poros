using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractionBehaviour))]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(MeshCollider))]
public class AnchoringSource : MonoBehaviour {
    private InteractionBehaviour leap;

    private Material material;
    public Color Color = Color.gray;
    public float FadeSpeed = 3f; // How fast the handle fades in/out when a hand gets close
    private float highlight = 0f; // goes to -1 for upper handle hover, 1 for lower handle, 0 for no hover
    public float HighlightSpeed = 3f; // How fast the highlighting changes
    private float HighlightSectionThreshold = 0.02f; // is the hand in the upper or lower half of the handle

    private Tether proxyTether, markTether, activeTether;
    private bool isDragging = false;
    private GameObject dropTarget;

    void Start() {
        material = GetComponent<Renderer>().material;

        proxyTether = transform.Find("ProxyTether").GetComponent<Tether>();
        markTether = transform.Find("MarkTether").GetComponent<Tether>();

        leap = GetComponent<InteractionBehaviour>();
        leap.OnGraspBegin += OnGraspBegin;
        leap.OnGraspEnd += OnGraspEnd;
        leap.OnGraspStay += OnGraspStay;
    }

    void Update() {
        var closestController = leap.manager.GetClosestHand(transform.position, out float controllerDistance);
        if(closestController != null && controllerDistance < 0.5f) {
            Color.a = Mathf.Min(1f, Color.a + Time.deltaTime * FadeSpeed);
            proxyTether.Alpha = markTether.Alpha = Color.a;
        } else {
            Color.a = Mathf.Max(0f, Color.a - Time.deltaTime * FadeSpeed);
            proxyTether.Alpha = markTether.Alpha = Color.a;
        }
        material.SetColor("Color_6ED4CD6", Color);

        if(leap.isHovered) {
            var hoverDelta = leap.closestHoveringController.position - transform.position;
            if(hoverDelta.y > HighlightSectionThreshold) {
                highlight = Mathf.Max(-1f, highlight - Time.deltaTime * HighlightSpeed);
                if(!isDragging) {
                    activeTether = markTether;
                }
            } else if(hoverDelta.y < -HighlightSectionThreshold) {
                highlight = Mathf.Min(1f, highlight + Time.deltaTime * HighlightSpeed);
                if(!isDragging) {
                    activeTether = proxyTether;
                }
            } else {
                if(!isDragging) {
                    FadeBackToNeutral();
                }
            }
        } else {
            if(!isDragging) {
                FadeBackToNeutral();
            }
        }
        material.SetFloat("Vector1_30E5B982", highlight);
    }

    private void OnGraspBegin() {
        activeTether.gameObject.SetActive(true);
        activeTether.Attachement = null;
        isDragging = true;
    }

    private void OnGraspEnd() {
        if(dropTarget != null) {
            activeTether.Attachement = dropTarget;
            if(activeTether == proxyTether) {
                transform.GetComponentInParent<ProxyNode>()?.Anchor(dropTarget);
            }
            // TODO: handle attaching of mark tether
        } else {
            activeTether.gameObject.SetActive(false);
            if(activeTether == proxyTether) {
                transform.GetComponentInParent<ProxyNode>()?.Detach();
            }
            // TODO: handle detaching of mark tether
        }
        isDragging = false;
    }

    private void OnGraspStay() {
        if(leap.graspingController.hoveredObjects.Count > 0) {
            dropTarget = null;
            foreach(var o in leap.graspingController.hoveredObjects) {
                // TODO: how to best filter things we can attach to?
                if(o.gameObject.name != "AnchorHandle") {
                    dropTarget = o.gameObject;
                }
            }
        } else {
            dropTarget = null;
        }

        activeTether.EndPoint = leap.graspingController.GetGraspPoint();
    }

    private void FadeBackToNeutral() {
        if(highlight < 0f) {
            highlight = Mathf.Min(0f, highlight + Time.deltaTime * HighlightSpeed);
        } else if(highlight > 0f) {
            highlight = Mathf.Max(0f, highlight - Time.deltaTime * HighlightSpeed);
        }
    }
}
