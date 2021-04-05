using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProxyAudio : MonoBehaviour {
    private ProxyNode proxy;
    private AudioSource audioSource;
    private Dictionary<AudioSource, AudioSource> remappedSources = new Dictionary<AudioSource, AudioSource>();

    // TODO: it might be good to organize this better, but it works okish this way for now
    public AudioClip MinimizeAudioClip;
    public AudioClip MaximizeAudioClip;
    public AudioClip AnchorAudioClip;
    public AudioClip UnanchorAudioClip;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        proxy = GetComponentInParent<ProxyNode>();
        proxy.OnProxyEvent += OnProxyEvent;
    }

    private void OnProxyEvent(object sender, ProxyEvent e) {
        switch(e) {
            case ProxyEvent.AnchoredProxy:
            case ProxyEvent.AnchoredMark:
                audioSource.PlayOneShot(AnchorAudioClip);
                break;
            case ProxyEvent.DetachedProxy:
            case ProxyEvent.DetachedMark:
                audioSource.PlayOneShot(UnanchorAudioClip);
                break;
            case ProxyEvent.Minimized:
                audioSource.PlayOneShot(MinimizeAudioClip);
                break;
            case ProxyEvent.Maximized:
                audioSource.PlayOneShot(MaximizeAudioClip);
                break;
            default:
                // Probably just ignore
                break;
        }
    }

    void Update() {
        var existingSources = new HashSet<AudioSource>(remappedSources.Keys);
        foreach(var mark in proxy.Marks) {
            if(mark == null) {
                continue;
            }

            var markPos = mark.transform.position;
            var markSqrRadius = mark.Radius * mark.Radius;
            foreach(var source in GameObject.FindObjectsOfType<AudioSource>()) {
                var relativePosition = source.transform.position - markPos;
                var distance = relativePosition.sqrMagnitude;
                if(distance < markSqrRadius) {
                    RemapAudioSource(source, relativePosition);
                    existingSources.Remove(source);
                }
            }
        }
        
        foreach(var toRemove in existingSources) {
            Destroy(remappedSources[toRemove].gameObject);
            remappedSources.Remove(toRemove);
        }
    }

    private void RemapAudioSource(AudioSource source, Vector3 localPosition) {
        if(remappedSources.ContainsKey(source)) {
            var remappedSource = remappedSources[source];
            remappedSource.transform.localPosition = localPosition;
            remappedSource.time = source.time;
            if(source.isPlaying && !remappedSource.isPlaying) {
                remappedSource.Play();
            }
            if(!source.isPlaying && remappedSource.isPlaying) {
                remappedSource.Stop();
            }

            return;
        }

        var copiedSource = AudioSource.Instantiate<AudioSource>(source, transform, false);
        foreach(var component in copiedSource.GetComponents<Component>()) {
            if(component.GetType() == typeof(Transform)) {
                continue;
            }
            if(component.GetType() == typeof(AudioSource)) {
                continue;
            }
            Destroy(component);
        }
        copiedSource.transform.localPosition = localPosition;

        remappedSources[source] = copiedSource;
    }
}
