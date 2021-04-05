using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandFeature {
        Openness,
        Spread,
        IndexPinch,
        RingMiddlePinkyClosed
    }

public class Hand : MonoBehaviour {
    public enum HandType {
        Left, Right
    }
    public HandType HandID;
    private float pinchThreshold = 0.04f;
    private float bentThreshold = 0.6f;
    public bool isActive;

    public enum HandModelType {
        Oculus,
        SteamVR,
        Leap
    }

    public enum Fingers{
        Index,
        Middle,
        Ring,
        Pinky,
        Thumb
    }

    public HandModelType HandModel;

    public enum HandJoint {
        Base,
        ThumbStart,
        ThumbMid,
        ThumbEnd,
        IndexStart,
        IndexMid,
        IndexEnd,
        MiddleStart,
        MiddleMid,
        MiddleEnd,
        RingStart,
        RingMid,
        RingEnd,
        PinkyStart,
        PinkyMid,
        PinkyEnd
    };

    public enum HandBone {
        Wrist,
        Palm,
        IndexTip,
        MiddleTip,
        RingTip,
        PinkyTip,
        ThumbTip,
        ThumbMetaCarpal,
        IndexMetaCarpal,
        MiddleMetaCarpal,
        RingMetaCarpal,
        PinkyMetaCarpal
    };

    public Vector3 GetJointPosition(HandJoint joint) {
        if(jointMap.ContainsKey(joint)) {
            return jointMap[joint].position;
        } else {
            return Vector3.zero;
        }
    }

    

    // Finger bending from 0-1 
    private float GetFingerBend(Fingers finger){

        float angle = 0;

        switch(finger){
            case Fingers.Index:
                angle = Vector3.Angle(GetBone(HandBone.IndexTip).up, GetBone(HandBone.IndexMetaCarpal).up);
                break;
            case Fingers.Middle:
                angle = Vector3.Angle(GetBone(HandBone.MiddleTip).up, GetBone(HandBone.MiddleMetaCarpal).up);
                break;
            case Fingers.Ring:
                angle = Vector3.Angle(GetBone(HandBone.RingTip).up, GetBone(HandBone.RingMetaCarpal).up);
                break;
            case Fingers.Pinky:
                angle = Vector3.Angle(GetBone(HandBone.PinkyTip).up, GetBone(HandBone.PinkyMetaCarpal).up);
                break;
            case Fingers.Thumb:
                angle = Vector3.Angle(GetBone(HandBone.ThumbTip).forward, GetBone(HandBone.ThumbMetaCarpal).forward);
                break;   
        }

        return angle / 180f;

    }

    public bool MiddleRingPinkyOpen(){

        float middleBend = GetFingerBend(Fingers.Middle);
        float ringBend = GetFingerBend(Fingers.Ring);
        float pinkyBend = GetFingerBend(Fingers.Pinky);

        //Debug.Log("middle bend: " + middleBend);
        //Debug.Log("ring bend: " + ringBend);
        //Debug.Log("pinky bend: " + pinkyBend);

        if (middleBend < 0.2f &&
            ringBend   < 0.2f &&
            pinkyBend  < 0.2f)
            return true;
        else
            return false;

    }

    public bool MiddleRingPinkyClosed(){

        float middleBend = GetFingerBend(Fingers.Middle);
        float ringBend = GetFingerBend(Fingers.Ring);
        float pinkyBend = GetFingerBend(Fingers.Pinky);

        //Debug.Log("middle bend: " + middleBend);
        //Debug.Log("ring bend: " + ringBend);
        //Debug.Log("pinky bend: " + pinkyBend);

        if (middleBend > bentThreshold &&
            ringBend   > bentThreshold &&
            pinkyBend  > bentThreshold)
            return true;
        else
            return false;

    }

    public bool IndexPointing()
    {
        float bendAmount = GetFingerBend(Fingers.Index);
        if (bendAmount < 1.0f - bentThreshold)
            return true;
        else
            return false;

    }


    // TODO: Check that thumb is working
    private float HandOpenness(){
        float indexBend =  GetFingerBend(Fingers.Index);
        float middleBend = GetFingerBend(Fingers.Middle);
        float ringBend = GetFingerBend(Fingers.Ring);
        float pinkyBend = GetFingerBend(Fingers.Pinky);
        //float thumbBend = GetFingerBend(Fingers.Thumb);
        return 1.0f - ((indexBend + middleBend + ringBend + pinkyBend) / 4.0f);
    }

    public float GetFeature(HandFeature feature) {

        switch(feature) {
            case HandFeature.RingMiddlePinkyClosed:

                break;
            case HandFeature.Openness:
                return HandOpenness();
                break;
            case HandFeature.Spread:
                // TODO
                break;
        }
        return 0f;

    }

    public bool IsPinching(){
        if (Vector3.Distance(GetBone(HandBone.IndexTip).position, GetBone(HandBone.ThumbTip).position) < pinchThreshold)
            return true;
        else
            return false;
    }

    public Quaternion HandBaseOrientation = Quaternion.identity;

    public static Hand LeftHand = null;
    public static Hand RightHand = null;

    private Dictionary<HandJoint, Transform> jointMap = new Dictionary<HandJoint, Transform>();
    private Dictionary<HandBone, Transform> boneMap = new Dictionary<HandBone, Transform>();

    private Dictionary<HandModelType, string> HAND_BASE_QUERY = new Dictionary<HandModelType, string>() {
        {HandModelType.Oculus, "Offset/{0}_hand_skeletal_lowres/hands:{0}_hand_world/hands:b_{0}_hand"},
        {HandModelType.SteamVR, "Root/wrist_{0}"},
        {HandModelType.Leap, "{0}_Wrist"}
    };
    public HandJoint getFingerJointIndex(int fingerIndex, int jointIndex){
        return (HandJoint)(fingerIndex * 3 + jointIndex + 1);
    }

    private Dictionary<HandModelType, List<KeyValuePair<HandJoint, string>>> HAND_JOINT_QUERIES = new Dictionary<HandModelType, List<KeyValuePair<HandJoint, string>>>() {
        {HandModelType.Oculus, new List<KeyValuePair<HandJoint, string>>() {
            new KeyValuePair<HandJoint, string>(HandJoint.ThumbStart,  "hands:b_{0}_thumb1"),
            new KeyValuePair<HandJoint, string>(HandJoint.ThumbMid,    "hands:b_{0}_thumb1/hands:b_{0}_thumb2"),
            new KeyValuePair<HandJoint, string>(HandJoint.ThumbEnd,    "hands:b_{0}_thumb1/hands:b_{0}_thumb2/hands:b_{0}_thumb3"),
            new KeyValuePair<HandJoint, string>(HandJoint.IndexStart,  "hands:b_{0}_index1"),
            new KeyValuePair<HandJoint, string>(HandJoint.IndexMid,    "hands:b_{0}_index1/hands:b_{0}_index2"),
            new KeyValuePair<HandJoint, string>(HandJoint.IndexEnd,    "hands:b_{0}_index1/hands:b_{0}_index2/hands:b_{0}_index3"),
            new KeyValuePair<HandJoint, string>(HandJoint.MiddleStart, "hands:b_{0}_middle1"),
            new KeyValuePair<HandJoint, string>(HandJoint.MiddleMid,   "hands:b_{0}_middle1/hands:b_{0}_middle2"),
            new KeyValuePair<HandJoint, string>(HandJoint.MiddleEnd,   "hands:b_{0}_middle1/hands:b_{0}_middle2/hands:b_{0}_middle3"),
            new KeyValuePair<HandJoint, string>(HandJoint.RingStart,   "hands:b_{0}_ring1"),
            new KeyValuePair<HandJoint, string>(HandJoint.RingMid,     "hands:b_{0}_ring1/hands:b_{0}_ring2"),
            new KeyValuePair<HandJoint, string>(HandJoint.RingEnd,     "hands:b_{0}_ring1/hands:b_{0}_ring2/hands:b_{0}_ring3"),
            new KeyValuePair<HandJoint, string>(HandJoint.PinkyStart,  "hands:b_{0}_pinky0/hands:b_{0}_pinky1"),
            new KeyValuePair<HandJoint, string>(HandJoint.PinkyMid,    "hands:b_{0}_pinky0/hands:b_{0}_pinky1/hands:b_{0}_pinky2"),
            new KeyValuePair<HandJoint, string>(HandJoint.PinkyEnd,    "hands:b_{0}_pinky0/hands:b_{0}_pinky1/hands:b_{0}_pinky2/hands:b_{0}_pinky3"),
        }},
        {HandModelType.SteamVR, new List<KeyValuePair<HandJoint, string>>() {
            new KeyValuePair<HandJoint, string>(HandJoint.ThumbStart,  "finger_thumb_0_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.ThumbMid,    "finger_thumb_0_{0}/finger_thumb_1_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.ThumbEnd,    "finger_thumb_0_{0}/finger_thumb_1_{0}/finger_thumb_2_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.IndexStart,  "finger_index_meta_{0}/finger_index_0_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.IndexMid,    "finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.IndexEnd,    "finger_index_meta_{0}/finger_index_0_{0}/finger_index_1_{0}/finger_index_2_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.MiddleStart, "finger_middle_meta_{0}/finger_middle_0_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.MiddleMid,   "finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.MiddleEnd,   "finger_middle_meta_{0}/finger_middle_0_{0}/finger_middle_1_{0}/finger_middle_2_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.RingStart,   "finger_ring_meta_{0}/finger_ring_0_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.RingMid,     "finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.RingEnd,     "finger_ring_meta_{0}/finger_ring_0_{0}/finger_ring_1_{0}/finger_ring_2_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.PinkyStart,  "finger_pinky_meta_{0}/finger_pinky_0_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.PinkyMid,    "finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}"),
            new KeyValuePair<HandJoint, string>(HandJoint.PinkyEnd,    "finger_pinky_meta_{0}/finger_pinky_0_{0}/finger_pinky_1_{0}/finger_pinky_2_{0}"),
        }}
    };

    private Dictionary<HandModelType, List<KeyValuePair<HandBone, string>>> HAND_BONE_QUERIES = new Dictionary<HandModelType, List<KeyValuePair<HandBone, string>>>() {
        {HandModelType.Leap, new List<KeyValuePair<HandBone, string>>() {
            new KeyValuePair<HandBone, string>(HandBone.Palm,  "{0}_Palm"),
            new KeyValuePair<HandBone, string>(HandBone.IndexTip,  "{0}_Palm/{0}_index_meta/{0}_index_a/{0}_index_b/{0}_index_c/{0}_index_end"),
            new KeyValuePair<HandBone, string>(HandBone.MiddleTip,  "{0}_Palm/{0}_middle_meta/{0}_middle_a/{0}_middle_b/{0}_middle_c/{0}_middle_end"),
            new KeyValuePair<HandBone, string>(HandBone.RingTip,  "{0}_Palm/{0}_ring_meta/{0}_ring_a/{0}_ring_b/{0}_ring_c/{0}_ring_end"),
            new KeyValuePair<HandBone, string>(HandBone.PinkyTip,  "{0}_Palm/{0}_pinky_meta/{0}_pinky_a/{0}_pinky_b/{0}_pinky_c/{0}_pinky_end"),
            new KeyValuePair<HandBone, string>(HandBone.ThumbTip,  "{0}_Palm/{0}_thumb_meta/{0}_thumb_a/{0}_thumb_b/{0}_thumb_end"),
            new KeyValuePair<HandBone, string>(HandBone.IndexMetaCarpal,  "{0}_Palm/{0}_index_meta"),
            new KeyValuePair<HandBone, string>(HandBone.MiddleMetaCarpal,  "{0}_Palm/{0}_middle_meta"),
            new KeyValuePair<HandBone, string>(HandBone.RingMetaCarpal,  "{0}_Palm/{0}_ring_meta"),
            new KeyValuePair<HandBone, string>(HandBone.PinkyMetaCarpal,  "{0}_Palm/{0}_pinky_meta"),
            new KeyValuePair<HandBone, string>(HandBone.ThumbMetaCarpal,  "{0}_Palm/{0}_thumb_meta"),
        }}
    };


    public Transform GetJoint(HandJoint joint) {
        return jointMap[joint];
    }

    public Transform GetBone(HandBone bone){
        return boneMap[bone];
    }

    private void Awake() {
        if (this.HandID == HandType.Left) {
            LeftHand = this;
        } else if (this.HandID == HandType.Right) {
            RightHand = this;
        }
    }

    void Start() {

        var handBase = transform.Find(string.Format(HAND_BASE_QUERY[HandModel], HandID == HandType.Left ? "L" : "R"));

        //jointMap[HandJoint.Base] = handBase;
        boneMap[HandBone.Wrist] = handBase;

        // foreach(var v in HAND_JOINT_QUERIES[HandModel]) {
        //     jointMap[v.Key] = handBase.Find(string.Format(v.Value, HandID == HandType.Left ? "L" : "R"));
        // }
        foreach(var v in HAND_BONE_QUERIES[HandModel]) {
            boneMap[v.Key] = handBase.Find(string.Format(v.Value, HandID == HandType.Left ? "L" : "R"));
        }

        switch(HandModel) {
            case HandModelType.SteamVR:
                HandBaseOrientation = Quaternion.Inverse(Quaternion.LookRotation(Vector3.forward, Vector3.up));
                break;
            default:
                HandBaseOrientation = Quaternion.Inverse(Quaternion.LookRotation(Vector3.forward, Vector3.up));
                break;
        }
    }

    void Update() {
        isActive = gameObject.activeSelf;
    }
}
