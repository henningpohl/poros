using Leap;
using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapWarpedServiceProvider : LeapProvider {
    public override event Action<Frame> OnUpdateFrame;
    public override event Action<Frame> OnFixedFrame;

    private ProxyNode proxyNodeLeftHand;
    private MarkNode markNodeLeftHand;
    private ProxyNode proxyNodeRightHand;
    private MarkNode markNodeRightHand;

    private bool leftHandEnabled;
    private bool rightHandEnabled;
    private bool leftHandRendererEnabled;
    private bool rightHandRendererEnabled;
    private bool leftHandPhysicsEnabled;
    private bool rightHandPhysicsEnabled;

    private LeapProvider ActualProvider;
    private Transform leapRoot;
    private Frame lastUpdateFrame;
    private Frame lastFixedFrame;

    public bool GetLeftHandEnabled() {
        return leftHandEnabled;
    }

    public bool GetRightHandEnabled() {
        return rightHandEnabled;
    }

    public void SetHandTransform(ProxyNode proxyNode, MarkNode markNode, HandChirality handChirality) {
        if (handChirality == HandChirality.Left) {
            proxyNodeLeftHand = proxyNode;
            markNodeLeftHand = markNode;
        } else {
            proxyNodeRightHand = proxyNode;
            markNodeRightHand = markNode;
        }
    }

    public void EnableHand(bool value, HandChirality handChirality) {
        if (handChirality == HandChirality.Left) {
            leftHandEnabled = value;
            leftHandRendererEnabled = value;
            leftHandPhysicsEnabled = value;
        } else {
            rightHandEnabled = value;
            rightHandRendererEnabled = value;
            rightHandPhysicsEnabled = value;
        }
    }

    public void LeftHandRendererEnabled(bool value) {
        leftHandRendererEnabled = value;
        if (leftHandRendererEnabled == leftHandPhysicsEnabled)
            leftHandEnabled = value;
    }

    public void RightHandRendererEnabled(bool value) {
        rightHandRendererEnabled = value;
        if (rightHandRendererEnabled == rightHandPhysicsEnabled)
            rightHandEnabled = value;
    }

    public void LeftHandPhysicsEnabled(bool value) {
        leftHandPhysicsEnabled = value;
        if (leftHandPhysicsEnabled == leftHandRendererEnabled)
            leftHandEnabled = value;
    }

    public void RightHandPhysicsEnabled(bool value) {
        rightHandPhysicsEnabled = value;
        if (rightHandPhysicsEnabled == rightHandRendererEnabled)
            rightHandEnabled = value;
    }

    public MarkNode GetLeftHandMarkNode() {
        return markNodeLeftHand;
    }

    public MarkNode GetRightHandMarkNode() {
        return markNodeRightHand;
    }

    public ProxyNode GetProxyNode(HandChirality chirality) {
        if(chirality == HandChirality.Left) {
            return proxyNodeLeftHand;
        }
        return proxyNodeRightHand;
    }

    private void Start() {
        ActualProvider = Player.Instance.GetActiveProvider();
        ActualProvider.OnFixedFrame += ActualProvider_OnFixedFrame;
        ActualProvider.OnUpdateFrame += ActualProvider_OnUpdateFrame;
        leapRoot = ActualProvider.transform;
    }

    private void TransformHand(Frame frame, ProxyNode proxyNode, MarkNode markNode, HandChirality handChirality) {
        if (proxyNode == null || markNode == null) return;

        var deltaRotation = Quaternion.FromToRotation(proxyNode.transform.forward, markNode.transform.forward);
        var localProxyPosition = proxyNode.transform.worldToLocalMatrix.MultiplyPoint(leapRoot.position);
        var markPosition = markNode.transform.localToWorldMatrix.MultiplyPoint(localProxyPosition);
        var scaleLeftHand = new Vector(
            markNode.transform.localScale.x / proxyNode.transform.localScale.x,
            markNode.transform.localScale.y / proxyNode.transform.localScale.y,
            markNode.transform.localScale.z / proxyNode.transform.localScale.z);

        if(handChirality == HandChirality.Left) {
            frame = frame.TransformLeft(new LeapTransform(-leapRoot.position.ToLeap(), LeapQuaternion.Identity));
            frame = frame.TransformLeft(new LeapTransform(Vector.Zero, deltaRotation.ToLeap(), scaleLeftHand));
            frame = frame.TransformLeft(new LeapTransform(markPosition.ToLeap(), LeapQuaternion.Identity));
        } else {
            frame = frame.TransformRight(new LeapTransform(-leapRoot.position.ToLeap(), LeapQuaternion.Identity));
            frame = frame.TransformRight(new LeapTransform(Vector.Zero, deltaRotation.ToLeap(), scaleLeftHand));
            frame = frame.TransformRight(new LeapTransform(markPosition.ToLeap(), LeapQuaternion.Identity));
        }
    }

    private void ActualProvider_OnUpdateFrame(Frame frame) {
        lastUpdateFrame = new Frame().CopyFrom(frame);
        // Remove disabled hands from the frame
        for(int i = lastUpdateFrame.Hands.Count - 1; i >= 0; i--) {
            if (!leftHandRendererEnabled && lastUpdateFrame.Hands[i].IsLeft) {
                lastUpdateFrame.Hands.RemoveAt(i);
            } else if (!rightHandRendererEnabled && lastUpdateFrame.Hands[i].IsRight) {
                lastUpdateFrame.Hands.RemoveAt(i);
            }
        }
        // Transform hands
        if(leftHandRendererEnabled)
            TransformHand(lastUpdateFrame, proxyNodeLeftHand, markNodeLeftHand, HandChirality.Left);
        if(rightHandRendererEnabled)
            TransformHand(lastUpdateFrame, proxyNodeRightHand, markNodeRightHand, HandChirality.Right);
        if (OnUpdateFrame != null) {
            OnUpdateFrame(lastUpdateFrame);
        }
    }

    private void ActualProvider_OnFixedFrame(Frame frame) {
        lastFixedFrame = new Frame().CopyFrom(frame);
        // Remove disabled hands from the frame
        for (int i = lastFixedFrame.Hands.Count - 1; i >= 0; i--) {
            if (!leftHandPhysicsEnabled && lastFixedFrame.Hands[i].IsLeft) {
                lastFixedFrame.Hands.RemoveAt(i);
            } else if (!rightHandPhysicsEnabled && lastFixedFrame.Hands[i].IsRight) {
                lastFixedFrame.Hands.RemoveAt(i);
            }
        }
        // Transform hands
        if(leftHandPhysicsEnabled)
            TransformHand(lastFixedFrame, proxyNodeLeftHand, markNodeLeftHand, HandChirality.Left);
        if(rightHandPhysicsEnabled)
            TransformHand(lastFixedFrame, proxyNodeRightHand, markNodeRightHand, HandChirality.Right);
        if (OnFixedFrame != null) {
            OnFixedFrame(lastFixedFrame);
        }
    }

    public override Frame CurrentFrame => lastUpdateFrame;
    public override Frame CurrentFixedFrame => lastFixedFrame;
}
