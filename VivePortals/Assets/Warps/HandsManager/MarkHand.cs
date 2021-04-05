using Leap.Unity.Interaction;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MarkHand : MonoBehaviour
{
    public HandChirality HandChirality;
    public MarkHandState HandState = MarkHandState.Disabled;
    public MarkNode Mark;
    public LeapWarpedServiceProvider LeapProvider;
    public Renderer HandModel;

    public InteractionHand Leap {
        get { return interactionHand; }
    }

    private InteractionHand interactionHand;
    private bool isEnabled;
    private InteractionWorldObject baseHandObject;
    private InteractionWorldObject markHandObject;
    private enum Ownership {
        BaseHand,
        MarkHand,
        None
    };
    private Ownership ownership = Ownership.None;

    private Coroutine handFadeOutCoroutine;

    #region Public Methods
    public void Enable(ProxyNode proxy) {
        LeapProvider.SetHandTransform(proxy, Mark, HandChirality);
        isEnabled = true;
    }

    public void Disable() {
        isEnabled = false;
    }

    public void StartManipulationMode() {
        ChangeState(MarkHandState.ManipulationMode);
    }

    public void StopManipulationMode() {
        ChangeState(MarkHandState.Idle);
    }
    #endregion

    #region Unity Functions
    private void Start() {
        StartCoroutine(InitializeInteractionHandLater());
    }

    private void Update() {
        switch (HandState) {
            case MarkHandState.Disabled:
                DisabledStateHandler();
                break;
            case MarkHandState.Activating:
                ActivatingStateHandler();
                break;
            case MarkHandState.Deactivating:
                DeactivatingStateHandler();
                break;
            case MarkHandState.Idle:
                IdleStateHandler();
                break;
            case MarkHandState.GraspingObject:
                GraspingObjectStateHandler();
                break;
            case MarkHandState.DuplicateObject:
                DuplicateObjectStateHandler();
                break;
            case MarkHandState.ManipulationMode:
                // Do nothing
                break;
        }
    }
    #endregion

    #region State Handlers
    private void ChangeState(MarkHandState to) {
        if (HandState == to) {
            return;
        }
        //Debug.Log("MarkHandState from: " + HandState + " to: " + to);
        HandState = to;
    }

    private void DisabledStateHandler() {
        if (isEnabled) {
            ChangeState(MarkHandState.Activating);
            LeapProvider.EnableHand(true, HandChirality);
        }
    }

    private void ActivatingStateHandler() {
        // Fade in the hand
        Color opaqe = new Color(0, 0, 0, 1f);
        Tween tween = HandModel.materials[1].DOColor(opaqe, "_BaseColor", 0.5f).SetOptions(true);
        // Check if base hand was grasping an object when it entered the proxy
        if (Player.Instance.GetInteractionHand(HandChirality).isGraspingObject) {
            // Create a duplicate at mark hand location if the base hand was grasping an object
            baseHandObject = Player.Instance.GetInteractionHand(HandChirality).graspedObject.gameObject.GetComponent<InteractionWorldObject>();
            ProxyNode proxy = LeapProvider.GetProxyNode(HandChirality);
            // Scale the grasped object to appropriate size
            Vector3 relativeScale = new Vector3(
                proxy.transform.localScale.x / Mark.transform.localScale.x,
                proxy.transform.localScale.y / Mark.transform.localScale.y,
                proxy.transform.localScale.z / Mark.transform.localScale.z
                );
            baseHandObject.ScaleAroundGraspRel(relativeScale);
            // Instatniate a duplicate at mark location
            markHandObject = baseHandObject.InstantiateDuplicate(proxy.transform, Mark.transform);
            ownership = Ownership.BaseHand;
            ChangeState(MarkHandState.DuplicateObject);
        } else { // If the base hand wasn not grasping an object go to Idle state
            ChangeState(MarkHandState.Idle);
        }     
    }

    private void DeactivatingStateHandler() {
        // Fade out the hand coroutine
        if (!isEnabled && handFadeOutCoroutine == null) {
            handFadeOutCoroutine = StartCoroutine(HandFadeOut());
            // Fade out the mark object
            if (markHandObject != null) {
                markHandObject.Fade(0.8f);
            }
        }
        // If hand has re-entered during deactivating start activating again
        if (isEnabled) {
            StopCoroutine(handFadeOutCoroutine);
            handFadeOutCoroutine = null;
            ChangeState(MarkHandState.Activating);
        }
    }

    private void IdleStateHandler() {
        if (!isEnabled) {
            ChangeState(MarkHandState.Deactivating);
        }
        if (interactionHand.isGraspingObject) {
            // Duplicate the object at base hand's location
            markHandObject = interactionHand.graspedObject.transform.GetComponent<InteractionWorldObject>();
            baseHandObject = markHandObject.InstantiateDuplicate(Mark.transform, LeapProvider.GetProxyNode(HandChirality).transform);
            ownership = Ownership.MarkHand;
            ChangeState(MarkHandState.GraspingObject);
        }
    }

    private void GraspingObjectStateHandler() {
        if (!isEnabled) {
            ChangeState(MarkHandState.Deactivating);
        } else if (interactionHand.graspedObject == null) {
            // Destroy the object at base hand's location
            baseHandObject.Destroy();
            baseHandObject = null;
            markHandObject = null;
            ownership = Ownership.None;
            ChangeState(MarkHandState.Idle);
        }
    }

    private void DuplicateObjectStateHandler() {
        // Base hand releases the grasped object while in the proxy
        if (!Player.Instance.GetInteractionHand(HandChirality).isGraspingObject) {
            // Destroy the object that was grasped by the base hand and keep the duplicate
            markHandObject.GetComponent<Rigidbody>().isKinematic = false;
            //markHandObject.GetComponent<HighlightController>().enabled = true;
            markHandObject = null;       
            baseHandObject.Destroy();
            baseHandObject = null;
            ownership = Ownership.None;
            ChangeState(MarkHandState.Idle);
        }
        if (!isEnabled) {
            ChangeState(MarkHandState.Deactivating);
        }
    }
    #endregion

    #region Coroutines
    // Hacky, basically this one waits for the fading to be completed
    // and then handles everything else.
    IEnumerator HandFadeOut() {
        // Fade out the hand
        Color transparent = new Color(0, 0, 0, 0f);
        Tween tween = HandModel.materials[1].DOColor(transparent, "_BaseColor", 0.8f).SetOptions(true);
        yield return tween.WaitForCompletion();
        /* Fading finished */
        // If marked hand is grasping an object destroy it
        if (markHandObject != null) {
            // Transfer ownership first
            if(ownership == Ownership.MarkHand) {
                markHandObject.TransferOwnership();
            }    
            markHandObject.Destroy();
        }
        baseHandObject = null;
        LeapProvider.EnableHand(false, HandChirality);
        handFadeOutCoroutine = null;
        ChangeState(MarkHandState.Disabled);
    }

    
    private IEnumerator InitializeInteractionHandLater() {
        //yield return new WaitForFixedUpdate();

        var leapInteractionManger = Object.FindObjectOfType<InteractionManager>();
        // Create new InteractionHand and add it to InteractionManager
        var go = new GameObject("MarkHand" + HandChirality);
        interactionHand = go.AddComponent<InteractionHand>();
        interactionHand.leapProvider = LeapProvider;
        interactionHand.manager = leapInteractionManger;
        interactionHand.handDataMode = (HandChirality == HandChirality.Left) ? HandDataMode.PlayerLeft : HandDataMode.PlayerRight;
        go.transform.SetParent(leapInteractionManger.transform);

        yield return null; // yield's fucked yo
    }
    #endregion
}
