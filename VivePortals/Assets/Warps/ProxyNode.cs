using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using Leap.Unity;
using UnityEditor;

[RequireComponent(typeof(Manipulation))]
public class ProxyNode : WarpNode, IPinchable {
    public enum ProxyState {
        Normal,
        Pinched,
        Minimized,
        Changing
    }

    public List<MarkNode> Marks = new List<MarkNode>(1);
    public event EventHandler<ProxyEvent> OnProxyEvent;

    private static List<ProxyNode> instances = new List<ProxyNode>();
    public static IEnumerable<ProxyNode> Instances {
        get => instances;
    }

    private ProxySphere sphere;
    public Color Color;
    public bool UseEditorColor = false;
    private VisualEffect vfxGraph;

    public ProxyState State { get; private set; }
    private float originalSize;
    public float ProxyScaleFactor { get; private set; }

    public bool HeadEntered { get; private set; }

    private Manipulation manipulation;

    public CrossingMenu ProxyMenuPrefab;
    private CrossingMenu proxyMenu;
    private Vector3 pinchStart;

    // Distance to drag to activate menu
    private const float menuActivateThreshold = 0.15f;
    // Offset from the direction of dragging
    private const float menuLocationOffset = 0.1f;

    void Awake() {
        State = ProxyState.Normal;
        ProxyScaleFactor = 1f;
        if(!UseEditorColor) {
            Color = WarpColorPalette.GetColor();
        }
        sphere = GetComponentInChildren<ProxySphere>();
        sphere.Color = Color;
        vfxGraph = GetComponentInChildren<VisualEffect>();

        //transform.DOScale(0f, 0.2f).From();

        manipulation = GetComponent<Manipulation>();
        manipulation.OnModeChange += OnManipulationModeChange;

        PinchTracker.RegisterPinchable(this);
        instances.Add(this);
    }

    private void OnDestroy() {
        PinchTracker.DeregisterPinchable(this);
        instances.Remove(this);
    }

    public void SetCreationMode(bool isBeingCreated) {
        if(isBeingCreated) {
            manipulation.enabled = false;
            sphere.ChangeHandOpening(0f, 0.5f);

        } else {
            manipulation.enabled = true;
            sphere.ChangeHandOpening(1f, 0.5f);
        }
    }

    private void OnManipulationModeChange(ManipulationMode from, ManipulationMode to) {
        if(to == ManipulationMode.Activating) {
            sphere.BeginInteractionTimerAnimation(Manipulation.ManipulationActivationTimeout);
        }
        if(from == ManipulationMode.Activating && to == ManipulationMode.Active) {
            OnProxyEvent?.Invoke(this, ProxyEvent.StartManipulation);
            sphere.EndInteractionTimerAnimation(true);
        } else if(from == ManipulationMode.Activating) {
            sphere.EndInteractionTimerAnimation(false);
            sphere.ChangeHandOpening(1f, 0.5f);
        }
        if(from == ManipulationMode.Active && (to == ManipulationMode.Hover || to == ManipulationMode.Inactive )) {
            OnProxyEvent?.Invoke(this, ProxyEvent.EndManipulation);
            sphere.ChangeHandOpening(1f, 0.5f);
            sphere.ChangeSolidness(0.0f, 0.5f);
        }
        if(to == ManipulationMode.Translating) {
            OnProxyEvent?.Invoke(this, ProxyEvent.StartMoving);
        }
        if(from == ManipulationMode.Translating) {
            OnProxyEvent?.Invoke(this, ProxyEvent.EndMoving);
        }
        if(to == ManipulationMode.ScalingAndRotating) {
            originalSize = transform.localScale.x;
            State = ProxyState.Changing;
            OnProxyEvent?.Invoke(this, ProxyEvent.StartScalingAndRotating);
        }
        if(from == ManipulationMode.ScalingAndRotating) {
            OnProxyEvent?.Invoke(this, ProxyEvent.EndScalingAndRotating);
        }
    }

    new void Update() {
        base.Update();

        HandleHeadEnter();

        foreach(var mark in Marks) {
            if(mark == null) {
                continue;
            }
            vfxGraph.SetVector3("EndPos", mark.transform.position);
            vfxGraph.SetFloat("EndRadius", mark.Radius);
            mark.Color = Color;
        }

        // A bit of a stupid hack because the cone effect requires world-space size of the
        // sphere, yet the whole ProxySphere otherwise operates oblivious to its actual size
        sphere.Radius = Radius;
    }

    #region Functions dealing with pinching on the proxy for crossing menu
    public void CheckPinchStart(InteractionHand hand, out bool isValid, out float distance) {
        var pos = hand.leapHand.GetPinchPosition();
        distance = (pos - transform.position).magnitude - Radius;
        // is the hand within 5cm of the proxy surface?
        isValid = distance > -0.025f && distance < 0.025f;
        // is the proxy not being manipulated
        ManipulationMode allowedToPinch = ManipulationMode.Inactive | ManipulationMode.Hover | ManipulationMode.None;
        isValid &= ((allowedToPinch & manipulation.Mode) == manipulation.Mode);
    }

    public void BeginPinch(InteractionHand hand) {
        var pos = hand.leapHand.GetPinchPosition();
        pinchStart = pos;
        State = ProxyState.Pinched;
        sphere.PinchPosition = pos;
        sphere.ChangeHandOpening(0f, 0.1f);
    }

    public void UpdatePinch(InteractionHand hand) {
        var pos = hand.leapHand.GetPinchPosition();
        sphere.PinchPosition = pos;

        if(proxyMenu == null) {
            var pinchDirection = pos - pinchStart;
            var pinchDistance = pinchDirection.magnitude;
            if(pinchDistance > menuActivateThreshold) {
                var menuPosition = pos + pinchDirection.normalized * menuLocationOffset;
                var menuOrientation = Quaternion.LookRotation(pinchDirection);
                proxyMenu = Instantiate<CrossingMenu>(ProxyMenuPrefab, menuPosition, menuOrientation);
                proxyMenu.SetOwner(this);
            }
        }
    }

    public void EndPinch(InteractionHand hand) {
        if(proxyMenu != null) {
            switch (proxyMenu.SelectedItem) {
                case "Clone":
                    Vector3 offset = Player.Instance.MainCamera.transform.forward * transform.localScale.x * 0.5f;
                    Clone(sphere.PinchPosition + offset);
                    break;
                case "Split":
                    Vector3 direction = (sphere.PinchPosition - transform.position).normalized;
                    Split(direction);
                    break;
                case "Merge":
                    Merge(GetNodeAtPosition(sphere.PinchPosition));
                    break;
                case "Highlight":
                    HighlightQuery(GetNodeAtPosition(sphere.PinchPosition));
                    break;
                case "AlignToWorld":
                    AlignToWorld();
                    break;
                case "AlignForConvenience":
                    ConvenienceAlign(GetNodeAtPosition(sphere.PinchPosition));
                    break;
                case "AlignToOther":
                    AlignWith(GetNodeAtPosition(sphere.PinchPosition));
                    break;
            }
            proxyMenu.Close();
            proxyMenu = null;
        }
        sphere.PinchPosition = Vector3.zero;
        State = ProxyState.Normal;
        sphere.ChangeHandOpening(1f, 0.1f);
    }

    public void SetProxyMenu(CrossingMenu menu) {
        proxyMenu = menu;
    }
    #endregion

    #region Functions for minimizing a proxy
    [ContextMenu("Minimize")]
    public void Minimize() {
        //originalSize = transform.localScale.x;
        OnProxyEvent?.Invoke(this, ProxyEvent.Minimized);
        StartCoroutine(MinimizeCoroutine());
    }

    private IEnumerator MinimizeCoroutine() {
        transform.DOScale(0.0f, 0.5f);
        Tween myTween = transform.DOJump(Player.Instance.InteractionHandLeft.position, 0.2f, 1, 0.5f);
        yield return myTween.WaitForCompletion();
        // Finished minimizing
        sphere.ChangeSolidness(1f, 0.15f);
        sphere.ChangeHandOpening(0f, 0.15f);
        State = ProxyState.Minimized;
        Player.Instance.ProxyStorage.Add(this);
        OnProxyEvent?.Invoke(this, ProxyEvent.Minimized);
    }

    [ContextMenu("Maximize")]
    public void Maximize() {
        OnProxyEvent?.Invoke(this, ProxyEvent.Maximized);
        StartCoroutine(MaximizeCoroutine());
    }

    private IEnumerator MaximizeCoroutine() {
        Player.Instance.ProxyStorage.Remove(this);
        Vector3 endLocation = Player.Instance.InteractionHandLeft.position +
            Player.Instance.MainCamera.transform.forward * originalSize * 0.7f;
        //endLocation.y = 1.2f;
        Tween myTween = transform.DOJump(endLocation, 0.2f, 1, 0.5f);
        transform.DOScale(originalSize, 0.15f);
        yield return myTween.WaitForCompletion();
        // Finished maximizing
        transform.localRotation = Quaternion.identity;
        sphere.ChangeSolidness(0f, 0.15f);
        sphere.ChangeHandOpening(1f, 0.15f);
        State = ProxyState.Normal;
        OnProxyEvent?.Invoke(this, ProxyEvent.Maximized);
    }
    #endregion

    #region Functions for heads poking in and out of a proxy
    private void HandleHeadEnter() {
        // Distance of the head to the center of the proxy
        float headToProxyDistance = (transform.position - Camera.main.transform.position).magnitude;
        // Distance to scale function
        float smoothStep = CoolMath.SmoothStep(1f, 0.2f, headToProxyDistance / (transform.localScale.x / 2));
        ProxyScaleFactor = 1 + smoothStep * 10;

        foreach (var mark in Marks) {
            if(smoothStep > 0.75f) {
                if(!HeadEntered) {
                    HeadEnter();
                }
            } else {
                if(HeadEntered) {
                    HeadExit();
                }
            }
        }
    }

    private void HeadEnter() {
        HeadEntered = true;
        OnProxyEvent?.Invoke(this, ProxyEvent.HeadEntered);
        Player.Instance.EnableTeleportCamera(this, Marks[0]);
    }

    private void HeadExit() {
        HeadEntered = false;
        OnProxyEvent?.Invoke(this, ProxyEvent.HeadExited);
        Player.Instance.DisableTeleportCamera();
    }
    #endregion

    #region Functions for anchoring and detaching a proxy
    public override void AnchorToPlayer(GameObject o) {
        base.AnchorToPlayer(o);
        OnProxyEvent?.Invoke(this, ProxyEvent.AnchoredProxy);
    }

    public override void Anchor(GameObject o) {
        base.Anchor(o);
        OnProxyEvent?.Invoke(this, ProxyEvent.AnchoredProxy);
    }

    public override void Detach() {
        base.Detach();
        OnProxyEvent?.Invoke(this, ProxyEvent.AnchoredProxy);
    }
    #endregion

    [ContextMenu("Change size")]
    public void ChangeSize() {
        if(State == ProxyState.Normal) {
            Minimize();
        } else if(State == ProxyState.Minimized) {
            Maximize();
        }
    }

    [ContextMenu("Align to world")]
    public void AlignToWorld() {
        if(Marks.Count == 0) {
            return;
        }

        var mark = Marks[0]; // TODO: maybe also make this work with multiple marked spaces
        var markData = AlignmentQuery.Scan(mark.transform.position, mark.Radius, mark.gameObject);
        var worldData = AlignmentQuery.Scan(transform.position, Radius * 2, gameObject);

        var rotation = markData.TryMatchOrientation(worldData);
        transform.DORotateQuaternion(rotation, 0.3f);
    }

    [ContextMenu("Align within")]
    public void AlignWithin() {
        if(Marks.Count < 2) {
            return;
        }

        // Only handles 2 linked marks right now
        var markA = AlignmentQuery.Scan(Marks[0].transform.position, Marks[0].Radius, Marks[0].gameObject);
        var markB = AlignmentQuery.Scan(Marks[1].transform.position, Marks[1].Radius, Marks[1].gameObject);

        var rotation = markA.TryMatchOrientation(markB);
        transform.DORotateQuaternion(rotation, 0.3f);
    }

    // TODO: only works for small misalignment right now, not when things are off 90� or so
    public void AlignWith(ProxyNode other) {
        if(Marks.Count == 0 || other.Marks.Count == 0) {
            return;
        }

        var markA = Marks[0]; // TODO: maybe also make this work with multiple marked spaces
        var markB = other.Marks[0];

        var markDataA = AlignmentQuery.Scan(markA.transform.position, markA.Radius, markA.gameObject);
        var markDataB = AlignmentQuery.Scan(markB.transform.position, markB.Radius, markB.gameObject);

        var rot = markDataA.TryMatchOrientation(markDataB);
        transform.DORotateQuaternion(rot, 0.3f);
    }

    public void ConvenienceAlign(ProxyNode other) {
        if(Marks.Count == 0 || other.Marks.Count == 0) {
            return;
        }

        var markA = Marks[0]; // TODO: maybe also make this work with multiple marked spaces
        var markB = other.Marks[0];

        var forwardA = AlignmentQuery.GetPreferredOrientation(markA.transform.position, markA.Radius);
        var forwardB = AlignmentQuery.GetPreferredOrientation(markB.transform.position, markB.Radius);
        if(forwardA == Vector3.zero || forwardB == Vector3.zero) {
            return;
        }

        var middlePoint = Vector3.Lerp(transform.position, other.transform.position, 0.5f);
        middlePoint = Vector3.Lerp(middlePoint, Player.Instance.MainCamera.transform.position, 0.6f);

        var rotA = Quaternion.FromToRotation(forwardA, middlePoint - transform.position);
        var rotB = Quaternion.FromToRotation(forwardB, middlePoint - other.transform.position);

        transform.DORotateQuaternion(rotA, 0.3f);
        other.transform.DORotateQuaternion(rotB, 0.3f);
    }
    
    public void Clone(Vector3 pos) {
        GameObject newProxy = Instantiate(Resources.Load<GameObject>("Prefabs/ProxyNode"));
        newProxy.GetComponent<ProxyNode>().Marks = Marks;
        newProxy.transform.localRotation = transform.localRotation;
        newProxy.transform.DOScale(transform.localScale, 0.2f);
        newProxy.transform.DOMove(pos, 0.2f);
    }

    [ContextMenu("Clone proxy")]
    public void Clone() {
        Clone(transform.position + Vector3.right * Radius * 3);
    }

    public void Merge(ProxyNode other) {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(transform.position, 0.3f).SetEase(Ease.InBack));
        sequence.Insert(0f, transform.DOScale(Radius * 2, 0.3f).SetEase(Ease.InExpo));
        sequence.Insert(0f, transform.DORotateQuaternion(transform.rotation, 0.3f));
        sequence.Append(other.transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0.4f), 0.1f));
        sequence.InsertCallback(0.3f, () => {
            foreach(var mark in Marks) {
                if(!other.Marks.Contains(mark)) {
                    other.Marks.Add(mark);
                }
            }
            Destroy(gameObject);
        });
    }

    [ContextMenu("Split")]
    public void Split(Vector3 splitDir) {
        if(Marks.Count < 2) {
            return;
        }

        var playerToProxy = transform.position - Player.Instance.transform.position;
        //var splitDir = Vector3.Cross(playerToProxy.normalized, Vector3.up);
        //Debug.Log(splitDir);
        //Debug.Log(splitDir.magnitude);

        for(int i = 0; i < Marks.Count; ++i) {
            var newProxy = Instantiate(Resources.Load<GameObject>("Prefabs/ProxyNode"));
            var newProxyMarks = newProxy.GetComponent<ProxyNode>().Marks;
            //newProxyMarks.Clear();
            newProxyMarks.Add(Marks[i]);

            var splitDirFactor = Radius * 2.5f * (0.5f + (i - (i % 2)) / 2);
            splitDirFactor *= i % 2 == 0 ? 1f : -1f;
            var newProxyLoc = transform.position + splitDir * splitDirFactor;
            newProxy.transform.DOMove(newProxyLoc, 0.2f).SetEase(Ease.OutBack);
        }

        transform.DOScale(0f, 0.1f).SetEase(Ease.InBack).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    public void HighlightQuery(ProxyNode other) {
        HashSet<string> querySet = new HashSet<string>();
        foreach(var mark in Marks) {
            if(mark?.enabled == true) {
                foreach(var obj in mark.ContainedItems()) {
                    querySet.Add(obj.GetComponent<HighlightData>().ID);
                }
            }
        }

        if(querySet.Count == 0) {
            return;
        }

        var query = querySet.ToList();
        foreach(var mark in other.Marks) {
            mark?.Highlight(query);
        }
    }

    // TODO: do we want these particles?
    private bool active = false;
    [ContextMenu("Particles")]
    public void Test() {
        if(active) {
            vfxGraph.Stop();
        } else {
            vfxGraph.Play();
        }
        active = !active;
    }

    static ProxyNode GetNodeAtPosition(Vector3 pos) {
        foreach (var n in instances) {
            if (n.DistanceTo(pos) < n.Radius) {
                return n;
            }
        }
        return null;
    }
}
