using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentData {
    private class DirectionComparer : IEqualityComparer<Vector3> {
        private float angleThreshold = 10f; // if vectors are less than 10 degrees apart they are the same

        public bool Equals(Vector3 x, Vector3 y) {
            var angle = Vector3.Angle(x, y);
            return angle < angleThreshold;
        }

        public int GetHashCode(Vector3 obj) {
            return obj.GetHashCode();
        }
    }

    private class LevelComparer : IEqualityComparer<float> {
        private float levelThreshold = 0.01f; // if values are less than a centimeter apart they are the same

        public bool Equals(float x, float y) {
            return Mathf.Abs(x - y) < levelThreshold;
        }

        public int GetHashCode(float obj) {
            return obj.GetHashCode();
        }
    }

    public List<Vector3> Orientations = new List<Vector3>();
    public List<float> XLevels = new List<float>();
    public List<float> YLevels = new List<float>();
    public List<float> ZLevels = new List<float>();

    public Dictionary<Vector3, int> OrientationHistogram;
    public Dictionary<float, int> XLevelHistogram;
    public Dictionary<float, int> YLevelHistogram;
    public Dictionary<float, int> ZLevelHistogram;

    public AlignmentData() {
        OrientationHistogram = new Dictionary<Vector3, int>(new DirectionComparer());
        var levelComparer = new LevelComparer();
        XLevelHistogram = new Dictionary<float, int>(levelComparer);
        YLevelHistogram = new Dictionary<float, int>(levelComparer);
        ZLevelHistogram = new Dictionary<float, int>(levelComparer);
    }

    public void BuildHistograms() {
        BuildHistogram(ref Orientations, ref OrientationHistogram);
        BuildHistogram(ref XLevels, ref XLevelHistogram);
        BuildHistogram(ref YLevels, ref YLevelHistogram);
        BuildHistogram(ref ZLevels, ref ZLevelHistogram);
    }

    private void BuildHistogram<T>(ref List<T> data, ref Dictionary<T, int> hist) {
        hist.Clear();
        foreach(var x in data) {
            if(hist.TryGetValue(x, out int count)) {
                hist[x] = count + 1;
            } else {
                hist[x] = 1;
            }
        }
    }

    public void AddOrientation(Vector3 orientation) {
        Orientations.Add(orientation);
    }

    public void AddLevelX(float x) {
        XLevels.Add(x);
    }

    public void AddLevelY(float y) {
        YLevels.Add(y);
    }

    public void AddLevelZ(float z) {
        ZLevels.Add(z);
    }

    
    public Quaternion TryMatchOrientation(AlignmentData other) {
        return AlignmentEstimator.GetRotation(Orientations, other.Orientations);
    }
}
