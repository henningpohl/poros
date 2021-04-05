using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DwellIndicator : MonoBehaviour {
    private Image indicatorImage;

    void Start() {
        indicatorImage = GetComponentInChildren<Image>();
    }

    void Update() {
        // http://wiki.unity3d.com/index.php?title=CameraFacingBillboard
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        //indicatorImage.fillAmount = 0.5f;
    }
}
