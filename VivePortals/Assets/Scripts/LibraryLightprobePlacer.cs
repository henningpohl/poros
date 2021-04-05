using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://docs.unity3d.com/Manual/LightProbes-Placing-Scripting.html

[RequireComponent(typeof(LightProbeGroup))]
public class LibraryLightprobePlacer : MonoBehaviour {
#if UNITY_EDITOR
    [ContextMenu("Generate light probes")]
    public void Generate() {
        var lightProbes = GetComponent<LightProbeGroup>();
        List<Vector3> positions = new List<Vector3>();

        float[] rows = new float[] { -11.3f, -9.2f, -8.0f, -5.7f, -4.0f, -1.4f, 0.0f, 2.5f, 4.0f, 6.8f, 7.5f, 9.0f, 11.0f };
        float[] cols = new float[] { -5.8f, -4.0f, -2.2f, -0.7f, 0.7f, 2.4f, 4.0f, 5.8f };
        float[] height = new float[] { 0.05f, 0.8f, 1.8f, 2.8f};

        foreach(var x in rows) {
            foreach(var y in height) {
                foreach(var z in cols) {
                    var pos = new Vector3(x, y, z);
                    positions.Add(pos);

                }
            }
        }

        lightProbes.probePositions = positions.ToArray();
    }
#endif
}
