using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class ProxySphere : MonoBehaviour {
    [Range(0f, 1f)]
    public float InteractionOpening = 1f;
    [Range(0f, 1f)]
    public float Solidness = 0f;

    public Color Color;

    public Renderer Cone;
    public Vector3 PinchPosition = Vector3.zero;

    [HideInInspector]
    public float Radius = 0.5f;

    private Material sphereInnerMaterial;
    private Material sphereOuterMaterial;
    private Material coneMaterial;

    private Sequence interactionTimerSequence;

    void Start() {
        // hack to ensure cones do not get culled that early
        var bounds = new Bounds(transform.position, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
#if UNITY_EDITOR
        if(UnityEditor.EditorApplication.isPlaying) {
            sphereInnerMaterial = GetComponent<Renderer>().materials[0];
            sphereOuterMaterial = GetComponent<Renderer>().materials[1];
            coneMaterial = Cone.GetComponent<Renderer>().material;
            Cone.GetComponent<MeshFilter>().mesh.bounds = bounds;
        } else {
            sphereInnerMaterial = GetComponent<Renderer>().sharedMaterials[0];
            sphereOuterMaterial = GetComponent<Renderer>().sharedMaterials[1];
            coneMaterial = Cone.GetComponent<Renderer>().sharedMaterial;
            Cone.GetComponent<MeshFilter>().sharedMesh.bounds = bounds;
        }
#else
        sphereInnerMaterial = GetComponent<Renderer>().materials[0];
        sphereOuterMaterial = GetComponent<Renderer>().materials[1];
        coneMaterial = Cone.GetComponent<Renderer>().material;
        Cone.GetComponent<MeshFilter>().mesh.bounds = bounds;
#endif
    }
    
    void Update() {
        sphereOuterMaterial.SetFloat("Solidness", Solidness);
        sphereOuterMaterial.SetFloat("OpeningFactor", InteractionOpening);
        sphereOuterMaterial.SetColor("ProxyColor", Color);
        sphereInnerMaterial.SetColor("_BaseColor", Color.WithAlpha(0.05f));
        coneMaterial.SetColor("_Color", Color);

        if(PinchPosition == Vector3.zero) {
            Cone.enabled = false;
        } else {
            Cone.enabled = true;
            var coneDirection = (PinchPosition - transform.position).normalized;
            var coneOrientation = Quaternion.FromToRotation(Vector3.forward, coneDirection);
            var coneMatrix = Matrix4x4.Rotate(coneOrientation);
            coneMaterial.SetMatrix("_ConeMatrix", coneMatrix);
            coneMaterial.SetVector("_From", transform.position + Radius * coneDirection);
            coneMaterial.SetVector("_To", PinchPosition);
            coneMaterial.SetFloat("_FromRadius", Radius);
        }
    }

    public void ChangeHandOpening(float to, float duration) {
        DOTween.To(() => InteractionOpening, x => InteractionOpening = x, to, duration);
    }

    public void ChangeSolidness(float to, float duration) {
        DOTween.To(() => Solidness, x => Solidness = x, to, duration);
    }

    public void BeginInteractionTimerAnimation(float duration) {
        ChangeHandOpening(0f, duration * 0.75f);
        if(interactionTimerSequence != null) {
            interactionTimerSequence.Kill();
        }
        interactionTimerSequence = DOTween.Sequence();
        interactionTimerSequence.Append(sphereOuterMaterial.DOColor(Color.white, "ProxyColor", duration));
        interactionTimerSequence.Append(sphereOuterMaterial.DOFloat(0.5f, "Solidness", duration));
    }

    public void EndInteractionTimerAnimation(bool success) {
        if(interactionTimerSequence != null) {
            interactionTimerSequence.Kill();
            interactionTimerSequence = null;
        }

        if(success) {
            transform.DOShakeScale(0.3f, 0.1f);
            ChangeSolidness(0.5f, 0.5f);
        } else {
            sphereOuterMaterial.DOFloat(InteractionOpening, "OpeningFactor", 0.01f);
        }
        sphereOuterMaterial.DOColor(Color, "ProxyColor", 0.2f);
    }

    [ContextMenu("Flash")]
    public void Flash() {
        var sequence = DOTween.Sequence();
        sequence.Append(sphereOuterMaterial.DOColor(Color.white, "ProxyColor", 0.05f));
        sequence.Append(sphereOuterMaterial.DOColor(Color, "ProxyColor", 0.05f));
        sequence.AppendInterval(0.025f);
        sequence.Append(sphereOuterMaterial.DOColor(Color.white, "ProxyColor", 0.05f));
        sequence.Append(sphereOuterMaterial.DOColor(Color, "ProxyColor", 0.05f));
        sequence.AppendInterval(0.02f);
        sequence.Append(sphereOuterMaterial.DOColor(Color.white, "ProxyColor", 0.05f));
        sequence.Append(sphereOuterMaterial.DOColor(Color, "ProxyColor", 0.05f));
        //sequence.Insert(0, sphereMaterial.DOFloat(0.5f, "ProxyState", 0.05f));
        //sequence.Insert(sequence.Duration() - 0.05f, sphereMaterial.DOFloat(Solidness, "ProxyState", 0.05f));
    }
}
