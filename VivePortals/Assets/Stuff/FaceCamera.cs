using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class FaceCamera : MonoBehaviour {
    private Camera camera;

    private void Start() {
        if(ClipSphereRenderPass.OnProxyPass != null)
            ClipSphereRenderPass.OnProxyPass += OnProxyPass;
    }

    void Update() {
#if UNITY_EDITOR
        if(EditorApplication.isPlaying) {
            if (Camera.main == null) return;
            camera = Camera.main;
        } else {
            camera = SceneView.lastActiveSceneView.camera;
        }
#else
        camera = Camera.main;
#endif
    }

    void LateUpdate() {
        if (camera == null) return;
        transform.LookAt(camera.transform);
    }

    private void OnProxyPass(object sender, Vector3 camPos) {
        transform.LookAt(camPos);
    }
}
