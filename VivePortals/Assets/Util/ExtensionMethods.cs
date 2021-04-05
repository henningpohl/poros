using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExtensionMethods {
    // Adapted from https://stackoverflow.com/questions/469202/best-way-to-remove-multiple-items-matching-a-predicate-from-a-c-sharp-dictionary
    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, bool> predicate) {
        var keys = dict.Keys.Where(k => predicate(k)).ToList();
        foreach(var key in keys) {
            dict.Remove(key);
        }
    }

    public static Leap.Vector ToLeap(this Vector3 v) {
        return new Leap.Vector(v.x, v.y, v.z);
    }

    public static Leap.LeapQuaternion ToLeap(this Quaternion q) {
        return new Leap.LeapQuaternion(q.x, q.y, q.z, q.w);
    }

    public static Vector3 ToUnity(this Leap.Vector v) {
        return new Vector3(v.x, v.y, v.z);
    }

    public static Quaternion ToUnity(this Leap.LeapQuaternion q) {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }

    public static Vector3 ComponentMultiply(this Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 ComponentDivide(this Vector3 a, Vector3 b) {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static float Average(this Vector3 v) {
        return (v.x + v.y + v.z) / 3f;
    }

    public static Color WithAlpha(this Color c, float alpha) {
        return new Color(c.r, c.g, c.b, alpha);
    }

    public static InteractionController GetClosestHand(this InteractionManager manager, Vector3 position, out float distance) {
        distance = float.PositiveInfinity;
        InteractionController closestController = null;
        if(manager == null) {
            return closestController;
        }

        foreach(var controller in manager.interactionControllers) {
            if(!controller.isActiveAndEnabled) {
                continue;
            }
            if(!controller.isTracked) {
                continue;
            }
            var controllerDistance = Vector3.Distance(controller.position, position);
            if(controllerDistance < distance) {
                distance = controllerDistance;
                closestController = controller;
            }
        }
        return closestController;
    }

    public static InteractionController GetClosest(this ReadonlyHashSet<InteractionController> set, Vector3 pos, float offset = 0f) {
        float minDist = float.PositiveInfinity;
        InteractionController closest = null;
        foreach(var controller in set) {
            var dist = Mathf.Abs((controller.position - pos).magnitude - offset);
            if(dist < minDist) {
                minDist = dist;
                closest = controller;
            }
        }
        return closest;
    }

    public static bool Any<T>(this ReadonlyHashSet<T> set, Func<T, Boolean> func) {
        foreach(var x in set) {
            if(func(x)) {
                return true;
            }
        }
        return false;
    }

    public static float NoFistPinchStrength(this InteractionHand hand) {
        if(!hand.isTracked) {
            return -1f;
        }

        var fist = hand.leapHand.GetFistStrength();
        var pinch = hand.leapHand.PinchStrength;
        return (1f - fist) * pinch;
    }

    public static Vector3 Clamp(this Vector3 vec, float min, float max) {
        vec.x = Mathf.Clamp(vec.x, min, max);
        vec.y = Mathf.Clamp(vec.y, min, max);
        vec.z = Mathf.Clamp(vec.z, min, max);
        return vec;
    }
}
