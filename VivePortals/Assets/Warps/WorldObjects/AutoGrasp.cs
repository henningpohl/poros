using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGrasp : MonoBehaviour
{
    public InteractionBehaviour interactionBehaviour;
    public InteractionController interactionController;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) {
            //interactionController.intHand.SwapGrasp
            //interactionController.intHand.manager
            interactionController.intHand.TryGrasp(interactionBehaviour);
            //interactionBehaviour.BeginGrasp(new List<InteractionController> { interactionController });
        }
    }
}
