using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerProxyStorage : MonoBehaviour
{
    private List<ProxyNode> proxies;
    private int maxProxies;
    private bool isShown;

    private const float upAngleThreashold = 60f;
    private const float headAngleThreashold = 60f;

    private const float minimizedProxySize = 0.05f;

    private int order = -1; // negative number means they are ordered to the top

    private void Start() {
        proxies = new List<ProxyNode>();
        maxProxies = transform.childCount;
    }

    private void Update() {
        transform.position = Player.Instance.InteractionHandLeft.position;
        transform.rotation = Player.Instance.InteractionHandLeft.rotation;
        float upAngle = Vector3.Angle(transform.forward, Vector3.up);
        float headAngle = Vector3.Angle(transform.up, Player.Instance.MainCamera.transform.forward);
        if(upAngle < upAngleThreashold && headAngle < headAngleThreashold) {
            Show();
        } else {
            Hide();
        }
    }

    public void Add(ProxyNode proxy) {
        if (proxies.Count == maxProxies) proxies.RemoveAt(0);
        proxies.Add(proxy);

        proxy.transform.SetParent(transform.GetChild(proxies.IndexOf(proxy)));
        proxy.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        proxy.transform.localRotation = Quaternion.identity;
        proxy.transform.localPosition = Vector3.zero;
    }

    public void Remove(ProxyNode proxy) {
        proxy.transform.SetParent(null, true); // Parent to scene's top
        order = proxies.IndexOf(proxy); // reorder the proxies next time they are shown
        proxies.Remove(proxy);
    }

    public void Hide() {
        if (!isShown) return;
        foreach (Transform child in transform) {
            child.transform.DOScale(0, 0.2f);
            child.gameObject.SetActive(false);
        }
        isShown = false;
    }

    public void Show() {
        if (isShown) return;
        if (order >= 0) OrderProxies(order);
        foreach (Transform child in transform) {           
            child.gameObject.SetActive(true);
            child.transform.DOScale(1f, 0.2f);
        }
        isShown = true;
    }

    private void OrderProxies(int start) {
        for(int i = start; i < transform.childCount; i++) {
            if (i >= proxies.Count) continue;
            proxies[i].transform.SetParent(transform.GetChild(i));
            proxies[i].transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            proxies[i].transform.localRotation = Quaternion.identity;
            proxies[i].transform.localPosition = Vector3.zero;
        }
        order = -1;
    }
}
