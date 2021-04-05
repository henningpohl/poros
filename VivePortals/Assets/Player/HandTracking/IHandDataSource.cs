using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHandDataSource {
    void UpdatePose(Hand hand, Hand.HandJoint joint);

    void UpdatePose(Hand hand);

    bool IsTracked(Hand.HandType hand);
}