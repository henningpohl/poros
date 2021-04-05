using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightData : MonoBehaviour {
    public string ID;

    private static Dictionary<string, List<GameObject>> sceneObjects = new Dictionary<string, List<GameObject>>();

    void Start() {
        if(sceneObjects.ContainsKey(ID)) {
            sceneObjects[ID].Add(gameObject);
        } else {
            sceneObjects[ID] = new List<GameObject>();
            sceneObjects[ID].Add(gameObject);
        }
    }

    private void OnDestroy() {
        if(sceneObjects.ContainsKey(ID)) {
            sceneObjects[ID].Remove(gameObject);
        }
    }

    public static IEnumerable<GameObject> Get(string id) {
        if(sceneObjects.ContainsKey(id)) {
            return sceneObjects[id];
        } else {
            return new GameObject[0];
        }
    }

    public static IEnumerable<GameObject> GetAll() {
        foreach(var collection in sceneObjects.Values) {
            foreach(var item in collection) {
                yield return item;
            }
        }
    }

    public static IEnumerable<GameObject> GetAll(List<string> ids) {
        foreach(var id in ids) {
            foreach(var obj in Get(id)) {
                yield return obj;
            }
        }
    }
}
