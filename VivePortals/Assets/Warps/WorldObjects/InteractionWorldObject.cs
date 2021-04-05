using DG.Tweening;
using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractionBehaviour))]
public class InteractionWorldObject : MonoBehaviour
{
    private MarkNode lastMark;
    private InteractionBehaviour interactionBehaviour;
    private GameObject objectDuplicate = null;
    private Rigidbody duplicateRigidbody;
    private Transform duplicateOrigin;
    private Transform duplicateDestination;
    private Renderer objectRenderer;
    private Vector3 actualSize;

    private Tween shakingTween;

    // Transfer ownership (i.e., grasping)
    public void TransferOwnership() {
        // Only try to transfer ownership if you have it
        if (!interactionBehaviour.isGrasped) return;
        // Only transfer ownership if mark hand is grasping
        if (interactionBehaviour.graspingController.gameObject.tag != "BaseHand") {
            objectDuplicate.GetComponent<Rigidbody>().isKinematic = false;
            if (interactionBehaviour.graspingController.isLeft) {
                Player.Instance.InteractionHandLeft.TryGrasp(objectDuplicate.GetComponent<InteractionBehaviour>());
                if (objectDuplicate.transform.localScale != actualSize) objectDuplicate.GetComponent<InteractionWorldObject>().StartShaking();
            }
            if (interactionBehaviour.graspingController.isRight) {
                Player.Instance.InteractionHandRight.TryGrasp(objectDuplicate.GetComponent<InteractionBehaviour>());
                if (objectDuplicate.transform.localScale != actualSize) {
                    objectDuplicate.GetComponent<InteractionWorldObject>().StartShaking();
                }                   
            }
        }
    }

    public void InstantiateDuplicate() {
        if (objectDuplicate != null) {
            Debug.LogWarning("Couldn't instantiate a duplicate.");
            return;
        }
        objectDuplicate = Instantiate(gameObject, transform.parent);
        objectDuplicate.GetComponent<Rigidbody>().isKinematic = true;
        objectDuplicate.GetComponent<HighlightController>().Disabled = true;
        objectDuplicate.GetComponent<InteractionWorldObject>().SetDuplicate(gameObject);
    }

    public InteractionWorldObject InstantiateDuplicate(Transform originSpace, Transform destinationSpace) {
        if (objectDuplicate != null) {
            Debug.LogWarning("Couldn't instantiate a duplicate.");
            return null;
        }
        duplicateOrigin = originSpace;
        duplicateDestination = destinationSpace;
        objectDuplicate = Instantiate(gameObject, transform.parent);
        objectDuplicate.GetComponent<Rigidbody>().isKinematic = true;
        objectDuplicate.GetComponent<HighlightController>().Disabled = true;
        objectDuplicate.GetComponent<InteractionWorldObject>().SetDuplicate(gameObject);
        var duplicateInteractionWorldObject = objectDuplicate.GetComponent<InteractionWorldObject>();
        duplicateInteractionWorldObject.actualSize = actualSize;
        return duplicateInteractionWorldObject;
    }

    public GameObject GetDuplicate() {
        return objectDuplicate;
    }

    public void SetDuplicate(GameObject duplicate) {
        objectDuplicate = duplicate;
    }

    public void DestroyDuplicate() {
        if (objectDuplicate != null) Destroy(objectDuplicate);
    }

    public void Fade(float time) {
        Material zWritePass = Resources.Load("ZWriteMaterial", typeof(Material)) as Material;
        Material current = Resources.Load("TransparentLit", typeof(Material)) as Material;
        current.SetColor("_BaseColor", objectRenderer.material.GetColor("_BaseColor"));
        Material[] fadeMaterials = new Material[2] {
            zWritePass,
            current
        };
        objectRenderer.materials = fadeMaterials;
        StartCoroutine(FadeOut(time));
    }

    public void Destroy() {
        Destroy(gameObject);
    }

    private void Start()
    {
        actualSize = transform.localScale;
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        objectRenderer = GetComponent<Renderer>();
        interactionBehaviour.OnPerControllerGraspBegin += GraspBegin;
        interactionBehaviour.OnGraspStay += GraspStay;
        interactionBehaviour.OnPerControllerGraspEnd += GraspEnd;
    }

    private void GraspBegin(InteractionController interactionController) {
        GetComponent<HighlightController>().Disabled = true;
    }

    private void GraspStay() {
        TransformDuplicateObject();
    }

    private void GraspEnd(InteractionController interactionController) {
        StartCoroutine(TurnOffKinematic());
        if (interactionController.tag != "BaseHand") return;
        if (objectDuplicate != null) return;
        if(shakingTween != null && shakingTween.IsPlaying()) {
            shakingTween.Kill(true);
        }
        // Always scale the object to it's actual size when released
        ScaleToActualSize(interactionController);
    }

    public void ScaleToActualSize(InteractionController iController) {
        if (transform.localScale == actualSize) return;
        //if (interactionBehaviour.isGrasped) {
        //    ScaleAroundGraspAbs(actualSize);
        //    return;
        //}
        interactionBehaviour.rigidbody.velocity = Vector3.zero;
        transform.DOScale(actualSize, 0.3f);
        transform.DORotate(Vector3.zero, 0.3f);
        Vector3 endPosition = iController.position + Player.Instance.MainCamera.transform.forward;
        endPosition = new Vector3(endPosition.x, 0f, endPosition.z);
        transform.DOMove(iController.position + Player.Instance.MainCamera.transform.forward, 0.3f).OnComplete(() => GetComponent<Rigidbody>().isKinematic = false);
    }

    //public void ScaleToActualSize() {
    //    if (transform.localScale == actualSize) return;
    //    if (interactionBehaviour.isGrasped) {
    //        ScaleAroundGraspAbs(actualSize);
    //        return;
    //    }
    //    transform.localScale = actualSize;
    //}

    public void ScaleAroundGraspAbs(Vector3 absoluteScale) {
        if (!interactionBehaviour.isGrasped) return;
        InteractionController interactionController = interactionBehaviour.graspingController;

        Vector3 graspingPoint = interactionController.GetGraspPoint();
        interactionBehaviour.graspedPoseHandler.ClearControllers();
        Vector3 pivotPointInLocalSpace = transform.InverseTransformPoint(graspingPoint);
        transform.localScale = absoluteScale;   
        Vector3 scaledPivotPointWorldSpace = transform.TransformPoint(pivotPointInLocalSpace);
        Vector3 diff = graspingPoint - scaledPivotPointWorldSpace;
        transform.position = transform.position + diff;
        interactionBehaviour.rigidbody.position = interactionBehaviour.rigidbody.position + diff;

        interactionBehaviour.graspedPoseHandler.ClearControllers();
        interactionBehaviour.graspedPoseHandler.AddController(interactionController);
    }

    public void ScaleAroundGraspRel(Vector3 relativeScale) {
        Vector3 absoluteScale = new Vector3(
            relativeScale.x * actualSize.x,
            relativeScale.y * actualSize.y,
            relativeScale.z * actualSize.z
            );
        ScaleAroundGraspAbs(absoluteScale);
    }

    private void TransformDuplicateObject() {
        if (objectDuplicate == null || duplicateDestination == null || duplicateOrigin == null) return;
        // Rotate
        var deltaRotation = Quaternion.FromToRotation(duplicateOrigin.transform.forward, duplicateDestination.transform.forward);
        objectDuplicate.transform.rotation = deltaRotation * transform.rotation;
        // Scale
        objectDuplicate.transform.localScale = new Vector3(
            transform.localScale.x * duplicateDestination.localScale.x / duplicateOrigin.localScale.x,
            transform.localScale.y * duplicateDestination.localScale.y / duplicateOrigin.localScale.y,
            transform.localScale.z * duplicateDestination.localScale.z / duplicateOrigin.localScale.z);
        // Move
        var objectOriginSpacePos = duplicateOrigin.InverseTransformPoint(transform.position);
        var objectDestSpacePos = duplicateDestination.TransformPoint(objectOriginSpacePos);
        objectDuplicate.transform.position = objectDestSpacePos;
    }

    private void StartShaking() {
        shakingTween = transform.DOShakeScale(0.5f, 0.1f).SetLoops(-1).SetEase(Ease.InOutSine);
    }

    IEnumerator FadeOut(float time) {
        // Fade out the hand
        Color transparent = new Color(0, 0, 0, 0f);
        Tween tween = objectRenderer.materials[1].DOColor(transparent, "_BaseColor", time).SetOptions(true);
        yield return tween.WaitForCompletion();
    }

    IEnumerator TurnOffKinematic() {
        yield return new WaitForFixedUpdate();
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
