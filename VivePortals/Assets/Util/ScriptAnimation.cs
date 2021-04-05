using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptAnimation {
    public enum Kind {
        Linear,
        Eased,
        Exponential
    }

    public static IEnumerator CreateAnimation(float duration, Action<float> function) {
        return CreateAnimation(duration, Kind.Linear, function);
    }

    public static IEnumerator CreateAnimation(float duration, Kind kind, Action<float> function) {
        return CreateAnimation(duration, kind, function, () => { });
    }

    public static IEnumerator CreateAnimation(float duration, Kind kind, Action<float> function, Action finalize) {
        float time = 0.0f;
        AnimationCurve curve = GetCurve(kind, duration);

        while(time < duration) {
            time += Time.smoothDeltaTime;
            var t = curve.Evaluate(time);
            function(t);
            yield return null;
        }

        finalize();

        yield return null;
    }

    private static AnimationCurve GetCurve(Kind kind, float duration) {
        switch(kind) {
            case Kind.Eased:
                return AnimationCurve.EaseInOut(0f, 0f, duration, 1f);
            case Kind.Exponential:
                return new AnimationCurve(new Keyframe[] {
                    new Keyframe(0f, 0f, 0f, 0f),
                    new Keyframe(0.4f * duration, 0f, 0f, 0f),
                    new Keyframe(duration, 1f, 2f / duration, 0f)
                });
            case Kind.Linear:
            default:
                return AnimationCurve.Linear(0f, 0f, duration, 1f);
        }
    }
}

