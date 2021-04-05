using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {
    public Hand LeftHand;
    public Hand RightHand;

    public GameObject DataSourceGameObject;
    private IHandDataSource DataSource = null;

    void Start() {
        DataSource = DataSourceGameObject?.GetComponent<IHandDataSource>();
    }

    void Update() {
        if(DataSource == null) {
            return;
        }

        if(LeftHand) {
            if(DataSource.IsTracked(LeftHand.HandID)) {
                LeftHand.gameObject.SetActive(true);
                // foreach(Hand.HandJoint joint in Enum.GetValues(typeof(Hand.HandJoint))) {
                //     DataSource.UpdatePose(LeftHand, joint);
                // }
                DataSource.UpdatePose(LeftHand);
            } else {
                LeftHand.gameObject.SetActive(false);
            }
        }

        if(RightHand) {
            if(DataSource.IsTracked(RightHand.HandID)) {
                RightHand.gameObject.SetActive(true);
                // foreach(Hand.HandJoint joint in Enum.GetValues(typeof(Hand.HandJoint))) {
                    // DataSource.UpdatePose(RightHand, joint); 
                // }
                DataSource.UpdatePose(RightHand);
            } else {
                RightHand.gameObject.SetActive(false);
            }
        }
    }
}
