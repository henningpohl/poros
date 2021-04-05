using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCButton : MonoBehaviour
{
    public MeshRenderer PCScreen;
    public Material ScreenMaterialOn;
    public Material ScreenMaterialOff;

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Button touch");
        PCScreen.material = ScreenMaterialOn;
    }
}
