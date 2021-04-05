using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPinchable {
    void CheckPinchStart(InteractionHand hand, out bool isValid, out float distance);
    void BeginPinch(InteractionHand hand);
    void UpdatePinch(InteractionHand hand);
    void EndPinch(InteractionHand hand);
}
