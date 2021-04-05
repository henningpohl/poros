using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapMotionDataSource : MonoBehaviour, IHandDataSource {
    private LeapServiceProvider leapService;

    private Quaternion TransformJointRotation(Quaternion inQuat){
        Quaternion outQuat;// = inQuat;
        Quaternion invertQuat =  Quaternion.AngleAxis(180, Vector3.forward) * Quaternion.AngleAxis(-90, Vector3.up);
        invertQuat = Quaternion.Inverse(invertQuat);

        outQuat = inQuat * invertQuat;
        //outQuat *= Quaternion.AngleAxis(-90, Vector3.up);inQuat.
        //outQuat *= Quaternion.AngleAxis(180, Vector3.forward);
        //outQuat = new Quaternion(inQuat.z, -inQuat.y, -inQuat.x, inQuat.w);
        return outQuat;
    }

        private Quaternion TransformJointRotationBase(Quaternion inQuat){
        Quaternion outQuat = inQuat;
        //outQuat *= Quaternion.AngleAxis(-90, Vector3.up);inQuat.
        //outQuat *= Quaternion.AngleAxis(180, Vector3.forward);
        outQuat = new Quaternion(inQuat.y, inQuat.z, inQuat.x, inQuat.w);
        return outQuat;
    }

    void IHandDataSource.UpdatePose(Hand hand) {
        if(leapService == null || leapService.IsConnected() == false) return;

        var handData = leapService.CurrentFrame.Hands.Find(h => (h.IsLeft && hand.HandID == Hand.HandType.Left) || (h.IsRight && hand.HandID == Hand.HandType.Right));

        if(handData == null) return;

        //foreach(Hand.HandJoint joint in System.Enum.GetValues(typeof(Hand.HandJoint))){

        //Update all joint spheres in the fingers
        foreach (var finger in handData.Fingers) {
            for (int j = 1; j < 4; j++) {
                
                Hand.HandJoint joint = hand.getFingerJointIndex((int)finger.Type, j-1);
                Transform jointTransform = hand.GetJoint(joint);
                Vector3 initialPosition = jointTransform.position;

                Leap.Bone currentBone = finger.Bone((Leap.Bone.BoneType)j);
                
                //Vector3.AngleBetween  currentBone.Direction;

                Vector3 nextJointPos = currentBone.NextJoint.ToVector3();
                Vector3 prevJointPos = currentBone.PrevJoint.ToVector3();

                // if(finger.Type == Leap.Finger.FingerType.TYPE_INDEX && j == 1){
                //     Debug.Log(joint.ToString() + " " + angle);
                // }


                //jointTransform.RotateAroundLocal()
                

                //currentBone.
                

                //Quaternion newRot = jointTransform.rotation;
                jointTransform.rotation = Quaternion.LookRotation(nextJointPos - prevJointPos);
                //jointTransform.rotation = Quaternion.LookRotation(nextJointPos - prevJointPos, Vector3.up);

                //jointTransform.position = initialPosition
                //jointTransform.rotation = newRot;

            }
        }

    }

    private int getFingerJointIndex(int fingerIndex, int jointIndex) {
      return fingerIndex * 4 + jointIndex;
    }

    void IHandDataSource.UpdatePose(Hand hand, Hand.HandJoint joint) {
        if(leapService == null || leapService.IsConnected() == false) return;

        var handData = leapService.CurrentFrame.Hands.Find(h => (h.IsLeft && hand.HandID == Hand.HandType.Left) || (h.IsRight && hand.HandID == Hand.HandType.Right));

        if(handData == null) return;

        var transform = hand.GetJoint(joint);

        switch(joint) {
            case Hand.HandJoint.Base:
                //transform.rotation = handData.Rotation.ToQuaternion() * hand.HandBaseOrientation;
                //transform.rotation = TransformJointRotationBase(handData.Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.IndexStart:
                //transform.localRotation = handData.GetIndex().bones[0].Rotation.ToQuaternion();
                //transform.rotation = handData.GetIndex().bones[0].Rotation.ToQuaternion();
                transform.localRotation = TransformJointRotation(handData.GetIndex().bones[1].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.IndexMid:
                transform.localRotation = TransformJointRotation(handData.GetIndex().bones[2].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.IndexEnd:
                transform.localRotation = TransformJointRotation(handData.GetIndex().bones[3].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.MiddleStart:
                transform.localRotation = TransformJointRotation(handData.GetMiddle().bones[1].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.MiddleMid:
                transform.localRotation = TransformJointRotation(handData.GetMiddle().bones[2].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.MiddleEnd:
                transform.localRotation = TransformJointRotation(handData.GetMiddle().bones[3].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.RingStart:
                transform.localRotation = TransformJointRotation(handData.GetRing().bones[1].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.RingMid:
                transform.localRotation = TransformJointRotation(handData.GetRing().bones[2].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.RingEnd:
                transform.localRotation = TransformJointRotation(handData.GetRing().bones[3].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.PinkyStart:
                transform.localRotation = TransformJointRotation(handData.GetPinky().bones[1].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.PinkyMid:
                transform.localRotation = TransformJointRotation(handData.GetPinky().bones[2].Rotation.ToQuaternion());
                break;
            case Hand.HandJoint.PinkyEnd:
                transform.localRotation = TransformJointRotation(handData.GetPinky().bones[3].Rotation.ToQuaternion());
                break;
        }

    }

    void Start() {
        leapService = GetComponent<LeapServiceProvider>();

    }

    void Update() {

    }

    public bool IsTracked(Hand.HandType hand) {
        if(leapService == null) return false;

        var handData = leapService.CurrentFrame.Hands.Find(h => (h.IsLeft && hand == Hand.HandType.Left) || (h.IsRight && hand == Hand.HandType.Right));
        if(handData == null) return false;

        return true;
    }
}
