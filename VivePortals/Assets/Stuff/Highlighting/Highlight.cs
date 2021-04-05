using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Highlight : MonoBehaviour {
    
    void Awake() {
        RunHighlightingAnimation();
    }

    public void RunHighlightingAnimation() {
        var t = transform.GetChild(0);
        t.localScale = Vector3.zero;

        var sequence = DOTween.Sequence();
        sequence.Append(t.DOScale(1f, 0.15f).SetEase(Ease.InCubic));
        sequence.Append(t.DOPunchScale(new Vector3(0.1f, 0.3f, 0.1f), 0.1f));
        sequence.Append(t.DOLocalRotate(new Vector3(-5f, -5f, -10f), 0.25f).SetEase(Ease.InOutSine));
        sequence.Append(t.DOLocalRotate(new Vector3(5f, 5f, 10f), 0.5f).SetEase(Ease.InOutSine).SetLoops(10, LoopType.Yoyo));
        sequence.Append(t.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.25f).SetEase(Ease.InOutSine));
        sequence.Append(t.DOPunchScale(new Vector3(0.1f, 0.3f, 0.1f), 0.1f));
        sequence.Append(t.DOScale(0f, 0.15f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() => {
            Destroy(gameObject);
        });
    }
}
