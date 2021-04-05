using UnityEngine;

public class ProcessHands : SingletonMonoBehaviour<ProcessHands>
{
    public bool disablePortalCreation;

    public Hand leftHand;
    public Hand rightHand;

    public Vector3 leftHandPos;
    public Vector3 rightHandPos;

    public GameObject Eye;

    private GameObject EntryWarp;
    private GameObject ExitWarp;

    private bool bothPinching = false;
    private bool creatingSphere = false;
    private bool bothHandsActive = false;

    public float minimumProxySize = 0.10f;
    public float maximumProxySize = 0.9f;

    public float markSizeLimit = 10.0f;
    public float markScaleFactor = 1.0f;
    public float markScaleExponent = 1.0f;
    public float reachFactor = 7.0f;

    private GameObject markedSpace;
    public GameObject proxySpace;
    private GameObject floorMarker;

    public bool clickGesture;

    private ProxyNode proxyNode;
    private MarkNode markNode;

    public CreationMode mode;

    public float distanceBetweenPinches { get; private set; }

    private bool pointingToEachOther = false;

    public Vector3 lIndexTipPos { get; private set; }
    public Vector3 rIndexTipPos { get; private set; }

    private float timeClickGestureStarted = 0;
    private float timeSinceLastClickGesture = 0;
    private float timeCreated = 0;

    public bool leftHandGrabbing = false;
    public bool rightHandGrabbing = false;

    OneEuroFilter<Vector3> markPosFilter = new OneEuroFilter<Vector3>(30.0f, 0.3f);
    OneEuroFilter<Vector3> proxyPosFilter = new OneEuroFilter<Vector3>(30.0f, 0.3f);

    OneEuroFilter markScaleFilter = new OneEuroFilter(30.0f);
    OneEuroFilter proxyScaleFilter = new OneEuroFilter(30.0f);

    public enum CreationMode{
        Ready,
        HoveringProxy,
        Interacting,
        CreatingProxy,
        MovingProxy,
        MovingMarked

    }

    private enum EditingMode{
        Translating,
        Rotating,
        Scaling,
        All
    }

    void Start()
    {
        clickGesture = false;
        distanceBetweenPinches = 1.0f;
        mode = CreationMode.Ready;
        EntryWarp = Resources.Load("Prefabs/ProxyNode", typeof(GameObject)) as GameObject;
        ExitWarp = Resources.Load("Prefabs/MarkNode", typeof(GameObject)) as GameObject;
    }


    private void SetActiveNodes(bool active)
    {
        //proxyNode.GetComponent<ProxyNode>().enabled = active;
        proxyNode.GetComponent<Manipulation>().enabled = active;
        proxyNode.GetComponent<HandsManager>().enabled = active;

        //markNode.GetComponent<MarkNode>().enabled = active;
        markNode.GetComponent<MarkManipulation>().enabled = active;
    }

    private void CreateProxy(){
        creatingSphere = true;

        //Destroy(markedSpace);
        //Destroy(proxySpace);
        //Destroy(proxyNode);
        //Destroy(markNode);

        markedSpace = Instantiate(ExitWarp);
        proxySpace = Instantiate(EntryWarp);

        proxyNode = proxySpace.GetComponent<ProxyNode>();
        markNode = markedSpace.GetComponent<MarkNode>();
        floorMarker = markedSpace.transform.Find("Cylinder").gameObject;
        floorMarker.SetActive(true);


        proxyNode.Marks.Add(markNode);

        proxyNode.SetCreationMode(true);

        SetActiveNodes(false);

        

        Vector3 pos = MarkPos();
        float size = MarkSize();
        Vector3 midPoint = (lIndexTipPos + rIndexTipPos) / 2.0f;

        float proxyFilteredSize = 0, markFilteredScale = 0;

        for (int i = 0; i < 100; i++)
        {
            markPosFilter.Filter(pos);
            proxyPosFilter.Filter(midPoint);

            proxyFilteredSize = proxyScaleFilter.Filter(0.01f);
            markFilteredScale = markScaleFilter.Filter(0.01f);
        }

        markedSpace.transform.position = pos;
        markedSpace.transform.localScale = new Vector3(markFilteredScale, markFilteredScale, markFilteredScale);

        proxySpace.transform.position = midPoint;
        proxySpace.transform.localScale = new Vector3(proxyFilteredSize, proxyFilteredSize, proxyFilteredSize);
    }

    private void UpdateMarkedOnly(){

        

        Vector3 midPoint = (lIndexTipPos + rIndexTipPos) / 2.0f;

        float size = 0.2f + Mathf.Pow(6.0f * distanceBetweenPinches, 3.0f);

        float distanceBetweenEyeMidpoint = Vector3.Distance(Eye.transform.position, midPoint);

        // Offset the eye position downwards a bit to make it easy on the arms
        Vector3 vectorEyeToMidpoint = midPoint - (Eye.transform.position - new Vector3(0.0f, 0.2f));

        Vector3 pos = midPoint + (Mathf.Pow(5.0f * distanceBetweenEyeMidpoint, 3.0f) * vectorEyeToMidpoint);

        // create a shape with size scaled exponentially in front of the user.
        markedSpace.transform.position = pos;
        markedSpace.transform.localScale = new Vector3(size, size, size);

        //Debug.DrawRay(midPoint, selectionSphere.transform.position - midPoint, Color.red, 1.0f, false);
        
    }

    private Vector3 MarkPos()
    {
        Vector3 midPoint = (lIndexTipPos + rIndexTipPos) / 2.0f;
        float distanceBetweenEyeMidpoint = Vector3.Distance(Eye.transform.position, midPoint);

        // Offset the eye position downwards a bit to make it easy on the arms
        Vector3 vectorEyeToMidpoint = midPoint - (Eye.transform.position - new Vector3(0.0f, 0.2f));

        Vector3 pos = midPoint + (Mathf.Pow(reachFactor * distanceBetweenEyeMidpoint, 2.6f) * vectorEyeToMidpoint);
        return pos;
    }

    private float SphereScale()
    {
        float scale = distanceBetweenPinches;
        if(scale < minimumProxySize) scale = minimumProxySize;
        if(scale > maximumProxySize) scale = maximumProxySize;
        return scale;
    }

    private float MarkSize()
    {
        float size = Mathf.Pow(markScaleFactor * SphereScale(), markScaleExponent);
        if (size > markSizeLimit) size = markSizeLimit;
        return size;
    }

    private void UpdateProxyAndMarked(){

        //if (distanceBetweenPinches < 30.0f && distanceBetweenPinches > 0.02f){
        Vector3 midPoint = (lIndexTipPos + rIndexTipPos) / 2.0f;

        // create a shape with size scaled exponentially in front of the user.

        Vector3 pos = MarkPos();
        float size = MarkSize();

        Vector3 markFilteredPos = markPosFilter.Filter(pos, Time.time);
        float markFilteredScale = markScaleFilter.Filter(size, Time.time);

       
        float proxyFilteredSize = proxyScaleFilter.Filter(SphereScale(), Time.time);
        Vector3 proxyFilteredPos = proxyPosFilter.Filter(midPoint, Time.time);


        // Non filtered:
        if (Time.time - timeCreated < 0.2f)
        {
            //Debug.Log("using non-filtered");
            markedSpace.transform.position = pos;
            markedSpace.transform.localScale = new Vector3(markFilteredScale, markFilteredScale, markFilteredScale);

            proxySpace.transform.position = midPoint;
            proxySpace.transform.localScale = new Vector3(proxyFilteredSize, proxyFilteredSize, proxyFilteredSize);
        }
        else
        {
            markedSpace.transform.position = markFilteredPos;
            markedSpace.transform.localScale = new Vector3(markFilteredScale, markFilteredScale, markFilteredScale);

            proxySpace.transform.position = proxyFilteredPos;   // + vectorEyeToMidpoint*0.5f;
            proxySpace.transform.localScale = new Vector3(proxyFilteredSize, proxyFilteredSize, proxyFilteredSize);
        }
        
        
        

    }

    public void UpdateProxyOnly(){
        
        Vector3 midPoint = (lIndexTipPos + rIndexTipPos) / 2.0f;

        float size = 0.2f + Mathf.Pow(5.0f * distanceBetweenPinches, 3.0f);

        float distanceBetweenEyeMidpoint = Vector3.Distance(Eye.transform.position, midPoint);

        // Offset the eye position downwards a bit to make it easy on the arms
        Vector3 vectorEyeToMidpoint = midPoint - (Eye.transform.position - new Vector3(0.0f, 0.2f));

        Vector3 pos = midPoint + (Mathf.Pow(10.0f * distanceBetweenEyeMidpoint, 3.0f) * vectorEyeToMidpoint);

        proxySpace.transform.position = midPoint;
        proxySpace.transform.localScale = new Vector3(distanceBetweenPinches, distanceBetweenPinches, distanceBetweenPinches);

        // To change rotation, set look rotation to be 

        Vector3 leftTipToRight = rIndexTipPos - lIndexTipPos;
        float angle = Vector3.Angle(leftTipToRight, vectorEyeToMidpoint);

        //proxySpace.transform.localRotation.SetLookRotation(leftTipToRight);
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
        proxySpace.transform.localRotation = rot;

        //Debug.DrawRay(midPoint, selectionSphere.transform.position - midPoint, Color.red, 1.0f, false);
        
    }


    private void ProcessHandFeatures(){

        bothHandsActive = (leftHand.isActive && rightHand.isActive)? true : false;

        if(leftHand.isActive){
            leftHandPos = leftHand.GetBone(Hand.HandBone.Palm).position;
            lIndexTipPos = leftHand.GetBone(Hand.HandBone.IndexTip).position;
            leftHandGrabbing = leftHand.GetFeature(HandFeature.Openness) < 0.3f;
            //if(leftHandGrabbing)
            //    Debug.Log("leftHandGrabbing :  " + leftHand.GetFeature(HandFeature.Openness));
        }
        if(rightHand.isActive){
            rightHandPos = rightHand.GetBone(Hand.HandBone.Palm).position;
            rIndexTipPos = rightHand.GetBone(Hand.HandBone.IndexTip).position;
            rightHandGrabbing = rightHand.GetFeature(HandFeature.Openness) < 0.3f;
        }


        if(!bothHandsActive) return;

        distanceBetweenPinches = Vector3.Distance(lIndexTipPos, rIndexTipPos);

        float pointingAngle = Vector3.Angle(leftHand.GetBone(Hand.HandBone.IndexTip).forward,
                                        rightHand.GetBone(Hand.HandBone.IndexTip).forward);

        if (pointingAngle > 140.0f) pointingToEachOther = true;
        else pointingToEachOther = false;

        // if (leftHand.IsPinching() && rightHand.IsPinching() && distanceBetweenPinches < 0.2f)
        //     bothPinching = true;
        // else
        //     bothPinching = false;

        if (leftHand.MiddleRingPinkyClosed() && rightHand.MiddleRingPinkyClosed() &&
            leftHand.IndexPointing() && rightHand.IndexPointing())
        {
            if (!clickGesture) timeClickGestureStarted = Time.time;
            timeSinceLastClickGesture = Time.time;
            clickGesture = true;
        }
        else
            clickGesture = false;


    }

    // State system
    void ProcessForPortals(){

        if (!bothHandsActive) return;

        switch(mode){
            case(CreationMode.Ready):
                if(clickGesture && 
                distanceBetweenPinches < 0.1f && 
                Time.time - timeClickGestureStarted > 1.0f &&
                pointingToEachOther){
                    // If gestures are on, create proxy if ready (i.e., not interacting)
                    mode = CreationMode.CreatingProxy;
                    CreateProxy();
                    Debug.Log("Creating proxy...");
                    timeCreated = Time.time;
                }
                break;

            case(CreationMode.CreatingProxy):

                

                // If gesture is no longer, for a period of time, finish creation
                if (!clickGesture && Time.time - timeSinceLastClickGesture > 0.7f)
                {
                    mode = CreationMode.Ready;
                    //proxySpace.GetComponentInChildren<InteractionZone>().enabled = true;
                    //portals.Add(new Tuple<GameObject, GameObject>(selectionSphere, proxySphere));
                    Debug.Log("Proxy finished");
                    SetActiveNodes(true);
                    floorMarker.SetActive(true);
                    proxyNode.SetCreationMode(false);
                }
                if(clickGesture && Time.time - timeSinceLastClickGesture < 0.05f)
                    UpdateProxyAndMarked();

                // Update spheres during creation
                //if (Time.time - timeSinceLastClickGesture < 0.1f)

                break;

            case (CreationMode.HoveringProxy):
                break;

            case (CreationMode.MovingProxy):

                // Update proxy after creation
                //UpdateProxyOnly();
                break;

            case (CreationMode.MovingMarked):
                Debug.Log("Moving marked");
                //UpdateMarkedOnly();
                break;

        }

    }

    // Update is called once per frame
    void Update()
    {
        ProcessHandFeatures();
        if(!disablePortalCreation)
            ProcessForPortals();
    }
}
