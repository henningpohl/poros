using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrossingMenu : MonoBehaviour {
    private ProxyNode proxy;
    private CrossingMenuItem[] items;
    private CrossingMenuItem selected;
    public string SelectedItem {
        get { return selected?.Action; }
    }

    void Start() {
        transform.DOScale(0f, 0.1f).From();

        items = GetComponentsInChildren<CrossingMenuItem>();
        foreach(var item in items) {
            item.OnSelectionChanged += OnSelectionChanged;
        }
    }

    private void OnSelectionChanged(object sender, bool isSelected) {
        if(isSelected) {
            if(selected != null) {
                selected.Selected = false;
            }
            selected = sender as CrossingMenuItem;
        } else {
            selected = null;
        }
    }

    void Update() {
        
    }

    public void SetOwner(ProxyNode proxyNode) {
        proxy = proxyNode;
    }

    public ProxyNode GetOwner() {
        return proxy;
    }

    public void SubmenuOpened(CrossingMenu submenu) {
        submenu.SetOwner(proxy);
        proxy.SetProxyMenu(submenu);
        Close();
    }

    public void Close() {
        transform.DOScale(0f, 0.1f).OnComplete(() => {
            Destroy(gameObject);
        });
    }
}
