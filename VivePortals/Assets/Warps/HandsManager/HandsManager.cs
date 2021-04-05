using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProxyNode), typeof(InteractionBehaviour), typeof(Manipulation))]
public class HandsManager : MonoBehaviour
{
    private ProxyNode proxyNode;
    private InteractionBehaviour interactionBehaviour;
    private Manipulation manipulation;

    private void Start() {
        proxyNode = GetComponent<ProxyNode>();
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        manipulation = GetComponent<Manipulation>();
        manipulation.OnModeChange += OnManipulationModeChange;
    }

    private void Update() {
        if(manipulation.Mode == ManipulationMode.Inside) {
            // Check which of the hands is inside
            foreach (var hController in interactionBehaviour.hoveringControllers) {
                if (hController.gameObject.tag == "BaseHand" && proxyNode.Contains(hController.position)) {
                    // Enable all the mark hands for the base hand that is inside
                    foreach (var mark in proxyNode.Marks) {
                        if (hController.isLeft) {
                            mark.LeftHand.Enable(proxyNode);
                        } else {
                            mark.RightHand.Enable(proxyNode);
                        }
                    }
                } else if (hController.gameObject.tag == "BaseHand") {
                    // Disable all the mark hands for the base hand that is inside
                    foreach (var mark in proxyNode.Marks) {
                        if (hController.isLeft) {
                            mark.LeftHand.Disable();
                        } else {
                            mark.RightHand.Disable();
                        }
                    }
                }
            }
        }
    }

    private void OnManipulationModeChange(ManipulationMode from, ManipulationMode to) {
        // Disable all the hands when the ManipulationMode goes out of the (hands) Inside
        if (from == ManipulationMode.Inside && to != ManipulationMode.Inside) {
            foreach(var mark in proxyNode.Marks) {
                mark.LeftHand.Disable();
                mark.RightHand.Disable();
            }
        }
    }
}
