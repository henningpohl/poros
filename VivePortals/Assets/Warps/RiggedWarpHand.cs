using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Copies the transforms from the rigged leap (origin) hand
/// to a warp hand. L/R_Palm is considered to be the base.
/// </summary>
public class RiggedWarpHand : MonoBehaviour
{
    public enum HandType {
        Left,
        Right
    }
    public HandType handType;
    private List<Transform> jointList;
    private Transform originHandPalm;

    private void Start() {
        // Store joint references of the hand this script is attached to
        if(jointList == null) {
            jointList = new List<Transform>();
            Transform warpHandPalm = transform.GetChild(0).GetChild(0);
            ListJoints(warpHandPalm, jointList);
        }
    }

    private void Update() {
        UpdateHand();
    }

    // Find joints and store references to them in a list (recursive)
    private void ListJoints(Transform hand, List<Transform> jointList) {
        foreach (Transform joint in hand) {
            jointList.Add(joint);
            ListJoints(joint, jointList);
        }
    }

    private void UpdateHand() {
        // Get the transform of the origin hand
        Transform originHand = null;
        if (handType == HandType.Left && Hand.LeftHand != null && Hand.LeftHand.isActive) {
            originHand = Hand.LeftHand.GetBone(Hand.HandBone.Palm);
        } else if (Hand.RightHand != null && Hand.RightHand.isActive) {
            originHand = Hand.RightHand.GetBone(Hand.HandBone.Palm);
        }
        // Update warp hand transforms
        if (originHand != null) {
            int i = 0; 
            UpdateJoints(ref i, originHand, jointList);
        }
    }

    // Go through each joint of origin hand and the warp hand (recursive)
    private void UpdateJoints(ref int i, Transform originHand, List<Transform> jointList) {
        foreach (Transform joint in originHand) {           
            jointList[i].transform.localPosition = joint.localPosition;
            jointList[i].transform.localRotation = joint.localRotation;
            i++;
            UpdateJoints(ref i, joint, jointList);
        }
    }
}
