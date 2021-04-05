using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolMath {

    // https://en.wikipedia.org/wiki/Smoothstep
    public static float SmoothStep(float edge0, float edge1, float x) {
        x = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return x * x * (3f - 2f * x);
    }

}