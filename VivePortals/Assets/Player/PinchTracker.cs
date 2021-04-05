using Leap.Unity;
using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchTracker : MonoBehaviour {
    private const float pinchActivateThreashold = 0.5f;
    private const float pinchDeactivateThreashold = 0.4f;

    private InteractionManager manager;
    private InteractionHand pinchingHand;
    private IPinchable pinchedObject;

    private List<InteractionHand> candidateHands = new List<InteractionHand>();

    public IPinchable PinchedObject {
        get { return pinchedObject; }
    }

    void Start() {
        manager = InteractionManager.instance;
    }

    void Update() {
        if(pinchingHand == null || pinchedObject == null) {
            CheckForPinchAction();
            return;
        }

        if(!pinchingHand.isTracked) {
            pinchedObject.EndPinch(pinchingHand);
            pinchedObject = null;
            pinchingHand = null;
        } else {
            if(pinchingHand.leapHand.PinchStrength < pinchDeactivateThreashold) {
                pinchedObject.EndPinch(pinchingHand);
                pinchedObject = null;
                pinchingHand = null;
            } else {
                var pos = pinchingHand.leapHand.GetPinchPosition();
                pinchedObject.UpdatePinch(pinchingHand);
            }
        }
    }

    private void CheckForPinchAction() {
        candidateHands.Clear();
        foreach(var controller in manager.interactionControllers) {
            var hand = controller.intHand;
            if(!controller.isTracked || hand == null) {
                continue;
            }

            if(hand.NoFistPinchStrength() > pinchActivateThreashold) {
                candidateHands.Add(hand);
            }
        }

        if(candidateHands.Count == 0) {
            return;
        }

        var pinchResult = DetermineBestPinchable(candidateHands);
        if(pinchResult.Item1 != null) {
            pinchingHand = pinchResult.Item1;
            pinchedObject = pinchResult.Item2;
            pinchedObject.BeginPinch(pinchingHand);
        }
    }

    private Tuple<InteractionHand, IPinchable> DetermineBestPinchable(IEnumerable<InteractionHand> hands) {
        IPinchable best = null;
        InteractionHand bestHand = null;
        float minDistance = float.MaxValue;

        foreach(var hand in hands) {
            var pos = hand.leapHand.GetPinchPosition();

            foreach(var pinchComponent in pinchables) { 
                pinchComponent.CheckPinchStart(hand, out bool isValid, out float distance);
                if(isValid && distance < minDistance) {
                    minDistance = distance;
                    best = pinchComponent;
                    bestHand = hand;
                }
            }
        }

        return Tuple.Create<InteractionHand, IPinchable>(bestHand, best);
    }

    #region Global list of IPinchables
    private static List<IPinchable> pinchables = new List<IPinchable>();
    public static void RegisterPinchable(IPinchable pinchable) {
        pinchables.Add(pinchable);
    }
    public static void DeregisterPinchable(IPinchable pinchable) {
        pinchables.Remove(pinchable);
    }
    #endregion
}
