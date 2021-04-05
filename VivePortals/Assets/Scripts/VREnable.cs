using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Leap.Unity;

public class VREnable : MonoBehaviour
{
    public bool enableVR;

    void Awake()
    {
        LeapXRServiceProvider provider = FindObjectOfType<LeapXRServiceProvider>();
        XRSettings.enabled = enableVR;
        if (enableVR) {
            provider.deviceOrigin = GameObject.Find("VRLeapOrigin").transform;
        } else {
            provider.deviceOrigin = GameObject.Find("DesktopLeapOrigin").transform;
        }
    }
}
