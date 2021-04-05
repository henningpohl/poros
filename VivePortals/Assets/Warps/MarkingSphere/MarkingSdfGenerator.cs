using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

// Based on https://github.com/danielshervheim/Signed-Distance-Field-Generator/blob/master/Assets/Signed%20Distance%20Field%20Generator/Scripts/Editor/SDF.cs
// Also see https://github.com/aman-tiwari/MeshToSDF
// And https://github.com/xraxra/SDFr
public class MarkingSdfGenerator : EditorWindow {

    [MenuItem("Signed Distance Field/Generate Marking SDF")]
    static void Window() {
        //MarkingSdfGenerator window = CreateInstance<MarkingSdfGenerator>();
        //window.ShowUtility();
        CreateSDF(64);
    }

    private void OnGUI() {
        bool compute = SystemInfo.supportsComputeShaders;
        EditorGUILayout.HelpBox("Compute support is " + compute, MessageType.Info);
    }


    static void CreateSDF(int resolution) {
        var texture = new Texture3D(resolution, resolution, resolution, TextureFormat.RHalf, false);
        texture.anisoLevel = 1;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float maxDistance = Mathf.Sqrt(3);

        for(int i = 0; i < resolution; ++i) {
            float x = (i - resolution/2f) / resolution;
            for(int j = 0; j < resolution; ++j) {
                float y = (j - resolution/2f) / resolution;
                for(int k = 0; k < resolution; ++k) {
                    float z = (k - resolution/2f) / resolution;

                    float da = DistanceToCircle(new Vector3(x, y, z), Vector3.zero, 0.4f, Vector3.left);
                    float db = DistanceToCircle(new Vector3(x, y, z), Vector3.zero, 0.4f, Vector3.up);
                    float dc = DistanceToCircle(new Vector3(x, y, z), Vector3.zero, 0.4f, Vector3.forward);

                    float dist = Mathf.Min(da, Mathf.Min(db, dc));
                    texture.SetPixel(i, j, k, new Color(dist / maxDistance, 0f, 0f, 0f), 0);
                }
            }
        }


        texture.Apply();

        AssetDatabase.CreateAsset(texture, "Assets/MarkingSphere/MarkingSDF.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // https://www.geometrictools.com/Documentation/DistanceToCircle3.pdf
    private static float DistanceToCircle(Vector3 point, Vector3 center, float radius, Vector3 planeNormal) {
        var d = point - center;
        var q = Vector3.ProjectOnPlane(point, planeNormal);
        var dist = Mathf.Pow(Vector3.Dot(planeNormal, d), 2f) + Mathf.Pow(Vector3.Cross(planeNormal, d).magnitude - radius, 2f);
        return Mathf.Sqrt(dist);
    }
}

#endif