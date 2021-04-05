using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrbit : MonoBehaviour {
    public float Radius = 1.0f;
    private AnimationCurve captureCurve = AnimationCurve.EaseInOut(0f, 0f, 2f, 1f);

    private struct OrbitData {
        public float phase;
        public float inclication;
        public float speed;
        public float time;

        public OrbitData(GameObject g) {
            Random.InitState(g.GetHashCode());
            phase = Mathf.Atan(g.transform.localPosition.z / g.transform.localPosition.x);
            inclication = Random.Range(-0.3f, 0.3f);
            speed = Random.Range(1.0f, 1.2f);
            time = 0f;
        }
    }

    private Dictionary<GameObject, OrbitData> phaseData = new Dictionary<GameObject, OrbitData>();

    void Start() {
        for(int i = 0; i < transform.childCount; ++i) {
            var child = transform.GetChild(i);
            phaseData.Add(child.gameObject, new OrbitData(child.gameObject));
        }
    }

    public void Capture(GameObject go) {
        go.transform.SetParent(transform, true);
        phaseData.Add(go, new OrbitData(go));
    }

    public void Release(GameObject go) {
        go.transform.SetParent(null, true);
    }

    void Update() {
        /*
        if(Time.frameCount == 20) {
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tmp.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            tmp.transform.position = 5f * Random.onUnitSphere;
            Capture(tmp);
        }
        */

        for(int i = 0; i < transform.childCount; ++i) {
            var child = transform.GetChild(i);
            var orbit = phaseData[child.gameObject];
            // If hand is nearby slow down orbiting
            if (IsHandNearby(child)) {
                orbit.phase += Time.deltaTime * orbit.speed/3;
            } else {
                orbit.phase += Time.deltaTime * orbit.speed;
            }         
            orbit.time += Time.deltaTime;

            Vector3 orbitPosition = new Vector3(Radius * Mathf.Cos(orbit.phase), orbit.inclication * Mathf.Sin(orbit.phase), Radius * Mathf.Sin(orbit.phase));
            var t = captureCurve.Evaluate(orbit.time);
            child.localPosition = Vector3.Lerp(child.localPosition, orbitPosition, t);

            phaseData[child.gameObject] = orbit;
        }
    }

    private bool IsHandNearby(Transform go) {
        if (go.GetComponent<InteractionBehaviour>().isPrimaryHovered) {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos() {
        int steps = Mathf.RoundToInt(Mathf.Max(Radius * 16, 12f));
        var from = transform.position + new Vector3(Radius * Mathf.Cos(0), 0f, Radius * Mathf.Sin(0));
        for(int i = 0; i <= steps; ++i) {
            var to = transform.position + new Vector3(Radius * Mathf.Cos(2f * Mathf.PI * i / steps), 0f, Radius * Mathf.Sin(2f * Mathf.PI * i / steps));
            Gizmos.DrawLine(from, to);
            from = to;
        }
    }
}
