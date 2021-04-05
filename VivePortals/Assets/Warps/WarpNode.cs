using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent]
public class WarpNode : MonoBehaviour {

    public float Radius {
        get; private set;
    }

    public void Update() {
        float avgScale = Vector3.Dot(transform.localScale, Vector3.one) / 3f;
        Radius = avgScale * 0.5f;
    }

    public float DistanceTo(Vector3 pos) {
        return (pos - transform.position).magnitude;
    }

    public bool Contains(Vector3 pos) {
        var dist = (pos - transform.position).sqrMagnitude;
        return dist < Radius * Radius;
    }

    public virtual void Anchor(GameObject o) {
        transform.SetParent(o.transform, true);
    }

    public virtual void AnchorToCenter(GameObject o) {
        transform.DOMove(o.transform.position, 0.2f).OnComplete(() => Anchor(o));
    }

    public virtual void AnchorToPlayer(GameObject o) {
        transform.DOMove(Player.Instance.MainCamera.transform.position + 
            Player.Instance.MainCamera.transform.forward +
            Player.Instance.MainCamera.transform.right * -0.1f,
            0.2f).OnComplete(() => Anchor(o));
    }

    public virtual void Detach() {
        transform.SetParent(null, true);
    }

    /*
    private void OnDrawGizmos() {
        if(Type == NodeType.Unlinked) {
            Gizmos.color = Color.gray;
        } else if(Type == NodeType.Entry) {
            Gizmos.color = Color.green;
        } else {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireSphere(transform.position, transform.localScale.x/2);
    }
    */
}
