using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// https://docs.unity3d.com/Manual/nav-AgentPatrol.html
// https://docs.unity3d.com/Manual/nav-CouplingAnimationAndNavigation.html
[RequireComponent(typeof(NavMeshAgent))]
public class CatBehavior : MonoBehaviour {
    public Vector3[] Points;
    private int currentPoint = 0;

    public float RoarLikelihood = 0.001f;
    public float PauseLikelihood = 0.01f;
    private float pauseTime;
    private bool shouldRoar = false;

    public AudioClip RoarSound;
    public AudioClip StepSound1;
    public AudioClip StepSound2;
    private bool stepSwitch;
    public float StepVolume = 0.5f;

    private NavMeshAgent agent;
    private Animator anim;

    private int idleState = Animator.StringToHash("Idle");
    private int walkState = Animator.StringToHash("Walk");
    private int roarState = Animator.StringToHash("Roar");

    void Start() {
        //InteractionBehaviour interactionBehaviour = GetComponent<>
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        agent.destination = Points[0];
        agent.isStopped = true;
        anim = GetComponent<Animator>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            anim.SetTrigger("roar");
            StartCoroutine(WaitAndWalk());
        }
        //pauseTime = Mathf.Max(pauseTime - Time.deltaTime, 0f);
        //int currentAnimation = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
        //if(currentAnimation == idleState) {
        //    if(pauseTime == 0f) {
        //        agent.isStopped = false;
        //        anim.SetBool("walking", true);
        //    }

        //} else if(currentAnimation == walkState) {
        //    if(Random.value < RoarLikelihood) {
        //        agent.isStopped = true;
        //        shouldRoar = true;
        //    } else if(Random.value < PauseLikelihood) {
        //        agent.isStopped = true;
        //        pauseTime = Random.Range(3f, 10f);
        //    }
        //    if(agent.velocity.sqrMagnitude == 0f) {
        //        anim.SetBool("walking", false);
        //        if(shouldRoar) {
        //            anim.SetTrigger("roar");
        //        }
        //    }
        //} else if(currentAnimation == roarState) {
        //    // do nothing
        //}       

        //if(!agent.pathPending && agent.remainingDistance < 0.5f) {
        //    agent.destination = Points[currentPoint];
        //    currentPoint = (currentPoint + 1) % Points.Length;
        //}
    }

    private void OnStep() {
        MirroredAudioSource.PlayClipAtPoint(stepSwitch ? StepSound1 : StepSound2, transform.position, StepVolume);
        stepSwitch = !stepSwitch;
    }

    private void OnRoar() {
        MirroredAudioSource.PlayClipAtPoint(RoarSound, transform.position);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        for(int i = 0; i < Points.Length; ++i) {
            Gizmos.DrawLine(Points[i], Points[(i + 1) % Points.Length]);
        }
    }

    private IEnumerator WaitAndWalk() {
        yield return new WaitForSeconds(2.5f);
        agent.isStopped = false;
        anim.SetBool("walking", true);      
    }
}
