using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColissionAudio : MonoBehaviour {
    public AudioClip CollisionSound;
    public float HitVelocityThreshold = 3f;

    void Start() {

    }

    void Update() {
        
    }

    private void OnCollisionEnter(Collision collision) {
        var hitVelocity = collision.relativeVelocity.magnitude;
        if(hitVelocity >= HitVelocityThreshold) {
            var volume = CoolMath.SmoothStep(0.7f * HitVelocityThreshold, 2 * HitVelocityThreshold, hitVelocity);
            MirroredAudioSource.PlayClipAtPoint(CollisionSound, transform.position, volume);
        }
    }
}
