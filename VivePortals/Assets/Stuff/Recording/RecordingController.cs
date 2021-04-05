using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordingController : MonoBehaviour
{
    private bool isEnabled = true;
    private List<Camera> Cameras;
    private int currentCamera = -1;

    private void Start() {
        isEnabled = Player.Instance.EnableVR;
        if(isEnabled) Cursor.visible = false;
        Cameras = new List<Camera>();
        GameObject[] extraCameras = GameObject.FindGameObjectsWithTag("ExtraCamera");
        for (int i = 0; i < extraCameras.Length; i++) {
            Cameras.Add(extraCameras[i].GetComponent<Camera>());
        }
    }

    private void Update() {
        if (!isEnabled) return;
        // Next camera
        if (Input.GetMouseButtonDown(0)) {
            currentCamera++;
            if (currentCamera >= Cameras.Count) currentCamera = 0;
            EnableCamera(currentCamera);
        }
        // Previous camera
        if (Input.GetMouseButtonDown(1)) {
            currentCamera--;
            if (currentCamera < 0) currentCamera = Cameras.Count - 1;
            EnableCamera(currentCamera);
        }
        // Default camera
        if (Input.GetMouseButtonDown(2)) {
            // Disable all aditional cameras
            foreach(Camera cam in Cameras) {
                cam.enabled = false;
            }
            currentCamera = -1;
        }
    }

    private void EnableCamera(int index) {
        for(int i=0; i < Cameras.Count; i++) {
            if(i == index) {
                Cameras[i].enabled = true;
            } else {
                Cameras[i].enabled = false;
            }
        }
    }
}
