using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class AlignmentVisualizer : MonoBehaviour {
    private float radius;
    private AlignmentData data;

    void Start() {
        
    }

    void Update() {
        radius = transform.localScale.Average();
        data = AlignmentQuery.Scan(transform.position, radius);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);

        if(data == null || data.Orientations.Count == 0) {
            return;
        }

        data.BuildHistograms();

        float maxCount = (float)data.OrientationHistogram.Values.Max();
        foreach(var dir in data.OrientationHistogram) {
            Gizmos.color = Color.Lerp(Color.green, Color.red, dir.Value / maxCount);
            Gizmos.DrawRay(transform.position, dir.Key);
        }

        LevelGizmo(data.XLevelHistogram, Vector3.right, 0.1f * Vector3.up);
        LevelGizmo(data.YLevelHistogram, Vector3.up, 0.1f * Vector3.right);
        LevelGizmo(data.ZLevelHistogram, Vector3.forward, 0.1f * Vector3.up);
    }

    private void LevelGizmo(Dictionary<float, int> hist, Vector3 axis, Vector3 offaxis) {
        float maxCount = (float)hist.Values.Max();
        var center = transform.position.ComponentMultiply(Vector3.one - axis);
        foreach(var i in hist) {
            Gizmos.color = Color.Lerp(Color.green, Color.red, i.Value / maxCount);
            var pos = i.Key * axis;
            Gizmos.DrawLine(center + pos - offaxis, center + pos + offaxis);
        }
    }
}
