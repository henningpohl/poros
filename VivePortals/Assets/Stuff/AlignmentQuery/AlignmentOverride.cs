using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentOverride : MonoBehaviour {
    public List<Vector3> PreferredOrientations = new List<Vector3>();
    public List<float> PreferredXPositions = new List<float>();
    public List<float> PreferredYPositions = new List<float>();
    public List<float> PreferredZPositions = new List<float>();
    public Vector3 Forward;
}
