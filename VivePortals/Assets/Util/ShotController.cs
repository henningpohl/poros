using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotController : MonoBehaviour {
    public int activeShot = 0;
    public List<GameObject> shots = new List<GameObject>();
    public Transform player;

    void Start() {
        if (shots.Count == 0) {
            return;
        }

        foreach(var child in transform) {
            (child as Transform)?.gameObject.SetActive(false);
        }
        SwitchShot(0, 0);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.KeypadPlus)) {
            SwitchShot(activeShot, (activeShot + 1) % shots.Count);
        }
    }

    private void SwitchShot(int from, int to) {
        shots[from].SetActive(false);
        activeShot = to;
        shots[activeShot].SetActive(true);
        var playerInShot = shots[activeShot].transform.Find("Player");
        var camOffset = Camera.main.transform.localPosition;

        player.position = new Vector3(playerInShot.position.x - camOffset.x, player.position.y, playerInShot.position.z - camOffset.z);
    }
}
