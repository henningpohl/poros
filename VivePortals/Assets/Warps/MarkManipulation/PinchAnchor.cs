using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Leap.Unity;
using Leap.Unity.Interaction;
using System;

public class PinchAnchor : MonoBehaviour, IPinchable {
    private LineRenderer tether;

    public EventHandler OnAnchoringStart;
    public EventHandler<GameObject> OnAnchoringSelection;
    public bool AllowAnchoring = true;

    public bool IsAnchoring {
        get { return tether.enabled; }
    }

    public float Radius {
        get; private set;
    }

    void Start() {
        tether = GetComponent<LineRenderer>();
        tether.enabled = false;
    }

    void Awake() {
        PinchTracker.RegisterPinchable(this);
    }

    void OnDestroy() {
        PinchTracker.DeregisterPinchable(this);
    }

    void Update() {
        var outside = transform.TransformPoint(0.5f, 0.0f, 0.0f);
        Radius = (outside - transform.position).magnitude;
    }

    public void CheckPinchStart(InteractionHand hand, out bool isValid, out float distance) {
        var pos = hand.leapHand.GetPinchPosition();
        distance = (pos - transform.position).magnitude;
        isValid = AllowAnchoring && (distance < Radius * 1.5f);
        //Debug.Log(distance + " " + isValid);
    }

    public void BeginPinch(InteractionHand hand) {
        tether.enabled = true;
        UpdatePinch(hand);
        OnAnchoringStart?.Invoke(this, null);
    }

    public void EndPinch(InteractionHand hand) {
        if(hand.hoveredObjects.Count > 0) {
            var iter = hand.hoveredObjects.GetEnumerator();
            GameObject obj = null;
            if(iter.MoveNext()) {
                obj = iter.Current.gameObject;
            }
            OnAnchoringSelection?.Invoke(this, obj);
        }

        // TODO: fade out tether
        tether.enabled = false;

    }

    public void UpdatePinch(InteractionHand hand) {
        tether.SetPosition(0, transform.position);
        tether.SetPosition(1, hand.leapHand.GetPinchPosition());
        tether.startWidth = 0.25f * Radius;
        tether.endWidth = 0.01f * Radius;
    }
}
