using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;
using DG.Tweening;

public class MarkNode : WarpNode {
    public enum MarkState
    {
        Normal,
        Changing
    }

    public MarkHand LeftHand;
    public MarkHand RightHand;

    public MarkState State { get; private set; }

    public Color Color;
    private RingLOD ring;
    private Material sphereMaterial;

    private Transform debugSphere;
    public GameObject HighlightPrefab;

    void Start() {
        State = MarkState.Normal;
        ring = GetComponentInChildren<RingLOD>();
        var sphere = GetComponentInChildren<MarkingSphereShaderSwitcher>().gameObject;
        sphereMaterial = sphere.GetComponent<Renderer>().material;
    }

    new void Update() {
        base.Update();
        ring.Color = Color;
    }

    public IEnumerable<GameObject> ContainedItems() {
        // Hacky fix because Physics.OverlapSphere does stupid things
        foreach(var obj in HighlightData.GetAll()) {
            var renderer = obj.GetComponent<Renderer>();
            if(renderer == null) {
                continue;
            }

            if(this.Contains(renderer.bounds.center)) {
                yield return obj;
            }
        }
    }

    public void Highlight(List<string> query) {
        // Hacky fix because Physics.OverlapSphere does stupid things
        foreach(var obj in HighlightData.GetAll(query)) {
            var renderer = obj.GetComponent<Renderer>();
            if(renderer == null) {
                continue;
            }

            if(!this.Contains(renderer.bounds.center)) {
                continue;
            }

            var size = renderer.bounds.extents.Average() * 1.5f;
            var highlight = Instantiate(HighlightPrefab, renderer.bounds.center, Quaternion.identity);
            highlight.transform.localScale = new Vector3(size, size, size);
            highlight.name = obj.name + " highlight";
        }
    }

    [ContextMenu("Highlight all book2")]
    public void HighlightTest() {
        Highlight(new List<string>() {"Book2"});
    }

    [ContextMenu("Flash")]
    public void Flash(int count = 2) {
        var flashColor = new Color(1f, 1f, 1f, 1f);
        sphereMaterial.DOColor(flashColor, "_OutsideColor", 0.1f)
            .SetOptions(true)
            .SetLoops(count * 2, LoopType.Yoyo)
            .SetEase(Ease.InCubic);
    }
}
