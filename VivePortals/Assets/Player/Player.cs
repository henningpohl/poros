using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.XR;

public class Player : SingletonMonoBehaviour<Player> {
    public bool EnableVR;
    public Camera MainCamera;
    public Camera TeleportCamera;
    public PlayerOrbit Orbit { get; private set; }

    public LeapProvider LeapDesktopProvider;
    public LeapXRServiceProvider LeapVRProvider;
    public HandModelManager HandModelManager;

    public InteractionHand InteractionHandLeft;
    public InteractionHand InteractionHandRight;

    public PlayerProxyStorage ProxyStorage;
    public GameObject AnchoringTarget;

    private LeapProvider activeProvider;
    private ProxyNode proxyNode;
    private MarkNode markNode;

    private void Awake() {
        XRSettings.enabled = EnableVR;
        if (EnableVR) {
            LeapVRProvider.enabled = true;
            LeapDesktopProvider.enabled = false;
            activeProvider = LeapVRProvider;
        } else {
            LeapVRProvider.enabled = false;
            LeapDesktopProvider.enabled = true;
            activeProvider = LeapDesktopProvider;
        }

        Orbit = GetComponentInChildren<PlayerOrbit>();
        HandModelManager.leapProvider = activeProvider;
        InteractionHandLeft.leapProvider = activeProvider;
        InteractionHandRight.leapProvider = activeProvider;
        TeleportCamera.enabled = false;
        //HideAnchoringTarget();
    }

    private void Update() {
        UpdateTeleportCamera();
    }

    private void UpdateTeleportCamera() {
        if (proxyNode == null || markNode == null) return;
        // Copy main camera transform if VR is not enabled
        if(!EnableVR) {
            TeleportCamera.transform.localPosition = MainCamera.transform.localPosition;
            TeleportCamera.transform.localRotation = MainCamera.transform.localRotation;
        }
        // Reset position of teleport camera parent
        TeleportCamera.transform.parent.localPosition = Vector3.zero;
        // Rotate
        Quaternion deltaRotation = Quaternion.FromToRotation(proxyNode.transform.forward, markNode.transform.forward);
        TeleportCamera.transform.parent.localRotation = deltaRotation;
        // Scale
        TeleportCamera.transform.parent.localScale = new Vector3(
            markNode.transform.localScale.x / proxyNode.transform.localScale.x,
            markNode.transform.localScale.y / proxyNode.transform.localScale.y,
            markNode.transform.localScale.z / proxyNode.transform.localScale.z);
        // Move
        var camParentProxySpace = proxyNode.transform.InverseTransformPoint(TeleportCamera.transform.parent.position);
        var camParentMarkSpace = markNode.transform.TransformPoint(camParentProxySpace);
        TeleportCamera.transform.parent.position = camParentMarkSpace;
    }

    public void EnableTeleportCamera(ProxyNode proxy, MarkNode mark) {
        proxyNode = proxy;
        markNode = mark;
        Player.Instance.MainCamera.targetDisplay = 2; // Set main camera to some non-VR display
        Player.Instance.TeleportCamera.enabled = true;
    }

    public void DisableTeleportCamera() {
        proxyNode = null;
        markNode = null;
        Player.Instance.MainCamera.targetDisplay = 0; // Set main camera to the VR display
        Player.Instance.TeleportCamera.enabled = false;
    }

    public LeapProvider GetActiveProvider() {
        return activeProvider;
    }

    public InteractionHand GetInteractionHand(HandChirality handChirality) {
        if (handChirality == HandChirality.Left) {
            return InteractionHandLeft;
        }
        return InteractionHandRight;
    }

    [ContextMenu("Show anchoring target")]
    public void ShowAnchoringTarget() {
        AnchoringTarget.SetActive(true);
    }

    [ContextMenu("Hide anchoring target")]
    public void HideAnchoringTarget() {
        if (transform.childCount > 1) return;
        AnchoringTarget.SetActive(false);
    }
}
