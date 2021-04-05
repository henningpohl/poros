using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//From VRTK: https://github.com/artcom/HeadStartVR/blob/master/Assets/VRTK/Source/Scripts/Presence/VRTK_HipTracking.cs
public class PlayerFollowHips : MonoBehaviour
{
    [Tooltip("Distance of hips from the player's head (negative number).")]
    public float HipsHight;

    private Transform playerHead;
    private Vector3 initialObjectRotation;

    private void Start() {
        initialObjectRotation = transform.localEulerAngles;
        playerHead = Player.Instance.MainCamera.transform;
    }

    protected virtual void LateUpdate() {
        if (playerHead == null) {
            return;
        }
        Vector3 up = Vector3.up;

        transform.position = playerHead.position + (HipsHight * up);

        Vector3 forward = playerHead.forward;
        Vector3 forwardLeveld1 = forward;
        forwardLeveld1.y = 0;
        forwardLeveld1.Normalize();
        Vector3 mixedInLocalForward = playerHead.up;
        if (forward.y > 0) {
            mixedInLocalForward = -playerHead.up;
        }
        mixedInLocalForward.y = 0;
        mixedInLocalForward.Normalize();

        float dot = Mathf.Clamp(Vector3.Dot(forwardLeveld1, forward), 0f, 1f);
        Vector3 finalForward = Vector3.Lerp(mixedInLocalForward, forwardLeveld1, dot * dot);
        transform.rotation = Quaternion.LookRotation(finalForward, up);
        transform.Rotate(initialObjectRotation, Space.Self);
    }
}
