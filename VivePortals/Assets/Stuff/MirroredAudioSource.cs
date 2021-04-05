using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirroredAudioSource {
    public static void PlayClipAtPoint(AudioClip clip, Vector3 position) {
        PlayClipAtPoint(clip, position, 1f);
    }

    public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume) {
        AudioSource.PlayClipAtPoint(clip, position, volume);
        foreach(var proxy in ProxyNode.Instances) {
            foreach(var mark in proxy.Marks) {
                if(mark == null) {
                    continue;
                }

                if(mark.Contains(position)) {
                    var lPos = mark.transform.worldToLocalMatrix.MultiplyPoint(position);
                    var pPos = proxy.transform.localToWorldMatrix.MultiplyPoint(lPos);
                    AudioSource.PlayClipAtPoint(clip, pPos, volume);
                }
            }
        }

    }
}
