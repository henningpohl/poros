using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    public ProxyNode proxy;
    public bool HandsInside;

    private bool leftHandInside;
    private bool rightHandInside;

    private float startTime;
    private float hoverTime = 1f;

    void Update()
    {
        if (rightHandInside && leftHandInside) {
            if(!HandsInside) {
                HandsInside = true;
                startTime = Time.time;
                ProcessHands.Instance.mode = ProcessHands.CreationMode.HoveringProxy;
            }          

            float timeDelta = Time.time - startTime;

            if (timeDelta >= hoverTime && ProcessHands.Instance.mode == ProcessHands.CreationMode.HoveringProxy) {
                if (ProcessHands.Instance.clickGesture) {
                    ProcessHands.Instance.mode = ProcessHands.CreationMode.MovingProxy;
                    ProcessHands.Instance.proxySpace = proxy.gameObject;
                }
            }

            if (ProcessHands.Instance.mode == ProcessHands.CreationMode.MovingProxy) {
                if (!ProcessHands.Instance.clickGesture) {
                    ProcessHands.Instance.mode = ProcessHands.CreationMode.Ready;
                }
            }
            

        } else {
            if (HandsInside) {
                ProcessHands.Instance.mode = ProcessHands.CreationMode.Ready;
                HandsInside = false;
            }        
        }

        
    }
}
