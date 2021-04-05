using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// A stupid pseudo-implementation of ICP
// https://en.wikipedia.org/wiki/Iterative_closest_point
public class AlignmentEstimator {
    public static Quaternion GetRotation(List<Vector3> a, List<Vector3> b) {
        var closest = ComputePointMapping(a, b);
        var c = (from i in closest.Values select b[i]).ToList();

        KabschSolver solver = new KabschSolver();
        var matrix = solver.SolveKabsch(a, c);
        return matrix.GetQuaternion();
    }

    private static Dictionary<int, int> ComputePointMapping(List<Vector3> a, List<Vector3> b) {
        Dictionary<int, int> closest = new Dictionary<int, int>();
        for(int i = 0; i < a.Count; ++i) {
            float minDistance = float.MaxValue;
            int minId = -1;
            for(int j = 0; j < b.Count; ++j) {
                var distance = (a[i] - b[j]).sqrMagnitude;
                if(distance < minDistance) {
                    minId = j;
                    minDistance = distance;
                }
            }
            closest[i] = minId;
        }
        return closest;
    }
}
