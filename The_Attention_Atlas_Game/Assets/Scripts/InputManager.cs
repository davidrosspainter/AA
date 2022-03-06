using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using DataStructures;
using System;

public class InputManager : MonoBehaviour
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // InputManager must precede InputManager in the script execution order, or the pointer will be transform

    public enum Sources
    {
        headset = 0,
        controllerLeft = 1,
        controllerRight = 2
    }

    public PointerSystem.Pointer.PointerID pointerToUse = PointerSystem.Pointer.PointerID.right; // default

    List<string> names = new List<string> { "[CameraRig]/Camera", "[CameraRig]/Controller (left)", "[CameraRig]/Controller (right)" };

    public static List<Transform> transforms = new List<Transform>();
    public static List<EssentialTransform> essentialTransforms = new List<EssentialTransform>();

    public static Headset headset = new Headset();
    public Headset headsetCopy = new Headset(); // for viewing in inspector
    public static List<Controller> controllers = new List<Controller>();
    public List<Controller> controllersCopy = new List<Controller>(); // for viewing in inspector

    [Serializable]
    public class Headset
    {
        public Transform transform;
        public EssentialTransform essentialTransform; // serializable
        public bool isProximitySensorActivated = false;
        public bool isTracked = false;

        public void Read()
        {
            transform = transforms[(int)Sources.headset];
            essentialTransform = essentialTransforms[0];

            isProximitySensorActivated = SteamVR_Actions.default_HeadsetOnHead.state; // headsetOnHead binding

            // check tracking
            List<UnityEngine.XR.XRNodeState> nodes = new List<UnityEngine.XR.XRNodeState>();
            UnityEngine.XR.InputTracking.GetNodeStates(nodes);

            if (nodes.Count == 0)
            {
                isTracked = false;
            }
            else
            {
                UnityEngine.XR.XRNodeState ns = nodes[(int)UnityEngine.XR.XRNode.Head];
                isTracked = ns.tracked;
            }
        }
    }

    [Serializable]
    public class Controller
    {
        public string name;
        public int index;
        public SteamVR_Input_Sources source;

        public Transform transform;
        public EssentialTransform essentialTransform; // serializable and for error checking alignment and tracking state

        public SerializableVector2 trackpadPosition;
        public bool trackpadTouch;
        public bool trackpadClick;

        public bool menu;
        public bool grip;

        public float triggerPull;
        public bool triggerClick;

        public bool system;

        public Controller(string name, int index, Transform transform, EssentialTransform essentialTransform)
        {
            this.name = name;
            this.index = index;
            this.transform = transform;
            this.essentialTransform = essentialTransform;

            if (index == 1)
                source = SteamVR_Input_Sources.LeftHand;
            else if (index == 2)
                source = SteamVR_Input_Sources.RightHand;
        }

        public void Read()
        {
            //bool isControllerOn;

            //if (transform.position == transforms[index].position)
            //    isControllerOn = false;
            //else
            //    isControllerOn = true;

            transform = transforms[index];
            essentialTransform = essentialTransforms[index];

            //essentialTransform.isTracked = isControllerOn; // hack solution

            // works even when not tracking
            trackpadPosition = SteamVR_Actions.drp_trackpadPosition.GetAxis(source);
            trackpadTouch = SteamVR_Actions.drp_trackpadTouch.GetState(source);
            trackpadClick = SteamVR_Actions.drp_trackpadClick.GetState(source);

            menu = SteamVR_Actions.drp_menu.GetState(source);
            grip = SteamVR_Actions.drp_grip.GetState(source);

            triggerPull = SteamVR_Actions.drp_triggerPull.GetAxis(source);
            triggerClick = SteamVR_Actions.drp_triggerClick.GetState(source);

            system = SteamVR_Actions.drp_system.GetState(source); // dosen't work  
        }
    }

    void Start() {
        // reset variables

        transforms = new List<Transform>();
        essentialTransforms = new List<EssentialTransform>();

        headset = new Headset();
        headsetCopy = new Headset(); // for viewing in inspector
        controllers = new List<Controller>();
        controllersCopy = new List<Controller>(); // for viewing in inspector

        // necessary for the buttons to be read
        SteamVR_ActionSet actionSetDRP = SteamVR_Input.GetActionSetFromPath("/actions/drp");
        actionSetDRP.Activate(SteamVR_Input_Sources.Any, 0, false);


        for (int index = 0; index < names.Count; index++)
        {
            // populate transforms
            transforms.Add(GameObject.Find(names[index]).GetComponent<Transform>());
            essentialTransforms.Add(new EssentialTransform(transforms[index], names[index], index));

            if (index > 0) // controllers
            {
                controllers.Add(new Controller(names[index], index, transforms[index], essentialTransforms[index]));
            }
        }
    }

    void Update()
    {
        // get transforms
        foreach (EssentialTransform essentialTranform in essentialTransforms)
        {
            essentialTranform.Update(transforms[essentialTranform.index]);
        }

        headset.Read(); // read headset and populate transform

        // read controllers and populate transforms
        foreach (Controller controller in controllers)
        {
            controller.Read();
        }

        // to implement - requires changes to the analyses

        //// get controller closest to the headset as the active pointer
        //if (Vector3.Distance(controllers[0].essentialTransform.position, headset.essentialTransform.position) <
        //    Vector3.Distance(controllers[1].essentialTransform.position, headset.essentialTransform.position))
        //{
        //    PointerGlobal.pointerToUse = PointerSystem.Pointer.PointerID.left;
        //}
        //else
        //{
        //    PointerGlobal.pointerToUse = PointerSystem.Pointer.PointerID.right;
        //}

        AttentionTracker.PointerGlobal.pointerToUse = PointerSystem.Pointer.PointerID.right;

        pointerToUse = AttentionTracker.PointerGlobal.pointerToUse; // for viewing in the inspector
        headsetCopy = headset;
        controllersCopy = controllers; // for viewing in the inspector
    }

    public static bool IsButtonPressed(GameOptions.ResponseButton responseButton, int? index = null)
    {
        bool isButtonPressed = false;

        switch (responseButton)
        {
            case GameOptions.ResponseButton.trackpad:
                if (index == null)
                    isButtonPressed = controllers[0].trackpadClick | controllers[1].trackpadClick;
                else
                    isButtonPressed = controllers[(int)index].trackpadClick;
                break;
            case GameOptions.ResponseButton.trigger:
                if (index == null)
                    isButtonPressed = controllers[0].triggerPull == 1 | controllers[1].triggerPull == 1;
                else
                    isButtonPressed = controllers[(int)index].triggerPull == 1;
                break;
            default:
                break;
        }

        return isButtonPressed;
    }

    // to require users to lift the button
    public static bool AreButtonsUnpressed(GameOptions.ResponseButton responseButton)
    {
        bool buttonAreUnpressed = false;

        switch (responseButton)
        {
            case GameOptions.ResponseButton.trackpad:
                buttonAreUnpressed = !controllers[0].trackpadClick & !controllers[1].trackpadClick;
                break;
            case GameOptions.ResponseButton.trigger:
                buttonAreUnpressed = !controllers[0].triggerClick & !controllers[1].triggerClick;
                break;
            default:
                break;
        }
        return buttonAreUnpressed;
    }
}
