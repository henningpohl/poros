using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Leap.Unity.Interaction;

public class CrossingMenuItem : MonoBehaviour {
    public string Action;
    public bool Selected = false;

    public Color Color;
    public Color SelectColor;
    private Color curColor;

    private ContactBone usedBone;
    private Material sphereMaterial;

    public CrossingMenu SubmenuPrefab;
    private CrossingMenu submenu;

    public event EventHandler<bool> OnSelectionChanged;

    void Start() {
        curColor = Color;
        sphereMaterial = GetComponent<Renderer>().materials[0];
    }

    private void OnTriggerEnter(Collider other) {
        var bone = other.GetComponent<ContactBone>();
        if(bone == null) {
            return;
        }

        if(Selected) {
            // deselection has to happen with the same finger bone that selected
            if(usedBone == bone) {
                usedBone = null;
                Selected = false;
                OnSelectionChanged?.Invoke(this, Selected);
            }
        } else {
            // only run selection for the first finger bone that makes contact
            if(usedBone == null) {
                usedBone = bone;
                Selected = true;
                OnSelectionChanged?.Invoke(this, Selected);

                if(SubmenuPrefab != null) {
                    OpenSubmenu();
                }
            }
        }     
    }

    private void OnTriggerExit(Collider other) {
        // can be ignored
    }

    void Update() {
        if(Selected && curColor == Color) {
            transform.DOShakeScale(0.2f, 0.1f);
            DOTween.To(() => curColor, x => curColor = x, SelectColor, 0.2f);
        }
        if(!Selected && curColor == SelectColor) {
            DOTween.To(() => curColor, x => curColor = x, Color, 0.2f);
        }

        sphereMaterial.SetColor("_BaseColor", curColor);
    }

    [ContextMenu("Open submenu")]
    private void OpenSubmenu() {
        Quaternion orientation = Quaternion.LookRotation(Player.Instance.MainCamera.transform.position - transform.position);
        submenu = Instantiate<CrossingMenu>(SubmenuPrefab, transform.position, orientation);
        // Tell to proxy that the submenu was opened (i.e., switch the submenu for topmenu)
        CrossingMenu topMenu = transform.parent.GetComponent<CrossingMenu>();
        topMenu.SubmenuOpened(submenu);
    }
}
