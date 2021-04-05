using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://thomasmountainborn.com/2016/05/25/materialpropertyblocks/

[RequireComponent(typeof(InteractionBehaviour))]
public class HighlightController : MonoBehaviour {
    private List<Renderer> renderers = new List<Renderer>();
    private MaterialPropertyBlock propBlock;
    private InteractionBehaviour leapBehavior;

    private float highlightValue = -1f;
    public bool Highlighted = false;
    public bool Disabled = false;
    [Range(0f, 100f)]
    public float Frequency = 12.0f;

    void Start() {
        propBlock = new MaterialPropertyBlock();        
        leapBehavior = GetComponent<InteractionBehaviour>();

        var renderer = GetComponent<Renderer>();
        if(renderer != null) {
            renderers.Add(renderer);
        } else {
            renderers.AddRange(GetComponentsInChildren<Renderer>());
        }
    }

    [ContextMenu("Ignore leap")]
    void DetachLeap() {
        leapBehavior = null;
    }

    void Update() {
        if(leapBehavior != null) {
            Highlighted = leapBehavior.isPrimaryHovered && !leapBehavior.isGrasped && !Disabled;
            //if(leapBehavior.isGrasped) {
            //    Frequency = Mathf.Max(Frequency - Time.deltaTime * 6f, 6f);
            //} else {
            //    Frequency = Mathf.Min(Frequency + Time.deltaTime * 6f, 12f);
            //}
        }

        if(!Highlighted) {
            highlightValue = -1f;
        } else if(highlightValue < 0f) {
            highlightValue = Time.time;
        }

        foreach(var renderer in renderers) {
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_Highlight", highlightValue);
            renderer.SetPropertyBlock(propBlock);
            foreach(var mat in renderer.materials) {
                mat.SetFloat("_HighlightFrequency", Frequency);
            }
        }
    }
}
