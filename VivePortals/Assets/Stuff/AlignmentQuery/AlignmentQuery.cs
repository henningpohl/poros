using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlignmentQuery {
    public static Vector3 GetPreferredOrientation(Vector3 pos, float radius, GameObject ignore = null) {
        HashSet<AlignmentOverride> objects = new HashSet<AlignmentOverride>();
        foreach(var collider in Physics.OverlapSphere(pos, radius)) {
            var align = collider.gameObject.GetComponent<AlignmentOverride>();
            if(align != null) {
                objects.Add(align);
            }
        }

        if(objects.Count == 0) {
            return Vector3.zero;
        }

        foreach(var o in objects) {
            var forward = o.transform.TransformVector(o.Forward);
            // TODO: we just use the first. Could be improved
            return forward;
        }
        return Vector3.zero;
    }

    public static AlignmentData Scan(Vector3 pos, float radius, GameObject ignore = null) {
        var data = new AlignmentData();
        foreach(var collider in Physics.OverlapSphere(pos, radius)) {
            if(ignore != null && ignore.GetComponentsInChildren<Collider>().Contains(collider)) {
                continue;
            }

            switch(collider) {
                case BoxCollider b:
                    HandleBoxCollider(b, ref data);
                    break;
                case SphereCollider s:
                    HandleSphereCollider(s, ref data);
                    break;
                case CapsuleCollider c:
                    HandleCapsuleCollider(c, ref data);
                    break;
                case MeshCollider m:
                    HandleMeshCollider(m, ref data);
                    break;
            }
        }

        /*
        Dictionary<Vector3, int> histogram = new Dictionary<Vector3, int>(new DirectionComparer());
        foreach(var dir in orientations) {
            if(histogram.TryGetValue(dir, out int count)) {
                histogram[dir] = count + 1;
            } else {
                histogram[dir] = 1;
            }
        }*/

        return data;
    }

    private static void HandleBoxCollider(BoxCollider c, ref AlignmentData data) {
        var m = c.transform.localToWorldMatrix;
        data.AddOrientation(m.MultiplyVector(Vector3.forward));
        data.AddOrientation(m.MultiplyVector(Vector3.back));
        data.AddOrientation(m.MultiplyVector(Vector3.up));
        data.AddOrientation(m.MultiplyVector(Vector3.down));
        data.AddOrientation(m.MultiplyVector(Vector3.right));
        data.AddOrientation(m.MultiplyVector(Vector3.left));

        var cornerA = m.MultiplyPoint(c.center + 0.5f * c.size);
        var cornerB = m.MultiplyPoint(c.center - 0.5f * c.size);
        data.AddLevelX(cornerA.x);
        data.AddLevelX(cornerB.x);
        data.AddLevelY(cornerA.y);
        data.AddLevelY(cornerB.y);
        data.AddLevelZ(cornerA.z);
        data.AddLevelZ(cornerB.z);
    }

    private static void HandleCapsuleCollider(CapsuleCollider c, ref AlignmentData data) {
        /*
        var m = c.transform.localToWorldMatrix;
        switch(c.direction) {
            case 0: // aligned along x-axis
                list.Add(m.MultiplyVector(Vector3.right));
                list.Add(m.MultiplyVector(Vector3.left));
                break;
            case 1: // aligned along y-axis
                list.Add(m.MultiplyVector(Vector3.up));
                list.Add(m.MultiplyVector(Vector3.down));
                break;
            case 2: // aligned along z-axis
                list.Add(m.MultiplyVector(Vector3.forward));
                list.Add(m.MultiplyVector(Vector3.back));
                break;
        }*/
    }

    private static void HandleSphereCollider(SphereCollider c, ref AlignmentData data) {

    }

    private static void HandleMeshCollider(MeshCollider c, ref AlignmentData data) {
        // TODO: I don't think there's an efficient solution for those. 
        // The convex collider mesh doesn't seem accessible by script.
        // We could access the original mesh, but that all is pretty inefficient.
        // Best solution for now probably is to just restrict ourselves to the other colliders.
    }
}
