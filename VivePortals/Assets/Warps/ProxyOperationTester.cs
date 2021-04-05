using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxyOperationTester : MonoBehaviour {
    public ProxyNode ProxyA;
    public ProxyNode ProxyB;

    [ContextMenu("Align for convenience")]
    void AlignForConvenience() {
        ProxyA.ConvenienceAlign(ProxyB);
    }

    [ContextMenu("Align")]
    void Align() {
        ProxyA.AlignWith(ProxyB);
    }

    [ContextMenu("Merge")]
    void Merge() {
        ProxyA.Merge(ProxyB);
    }

    [ContextMenu("Highlight")]
    void Highlight() {
        ProxyA.HighlightQuery(ProxyB);
    }
}
